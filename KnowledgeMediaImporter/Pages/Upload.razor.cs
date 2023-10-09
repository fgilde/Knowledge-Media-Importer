using System.Collections.Concurrent;
using KnowledgeMedia.Core;
using KnowledgeMediaImporter.Data;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Extensions;
using MudBlazor.Extensions.Components;
using Nextended.Core.Contracts;

namespace KnowledgeMediaImporter.Pages;

public partial class Upload
{
    private bool _running;


    private ConcurrentDictionary<string, Progress> _progresses = new();
    
    [Inject] private IServiceProvider ServiceProvider { get; set; }
    [Inject] private IDialogService DialogService { get; set; }
    [Inject] private SabioService SabioService { get; set; }
    [Inject] private GptService GptService { get; set; }
    
    private async Task UploadClick()
    {
        var parameters = new DialogParameters
        {
            { nameof(MudExMessageDialog.Buttons), MudExDialogResultAction.OkCancel("Upload") },
            { nameof(MudExMessageDialog.Icon), Icons.Material.Filled.FileUpload }
        };
        var res =await DialogService.ShowComponentInDialogAsync<MudExUploadEdit<UploadableFile>>("Upload content", "Upload content files as zip or separate",
            uploadEdit =>
            {
                uploadEdit.AllowMultiple = true;
                uploadEdit.MinHeight = 400;
                uploadEdit.AutoExtractArchive = true;
                uploadEdit.MimeTypes = Array.Empty<string>();
                uploadEdit.MimeRestrictionType = RestrictionType.BlackList;
                uploadEdit.StreamUrlHandling = StreamUrlHandling.BlobUrl;
            }, parameters, options =>
            {
                options.Resizeable = true;
                options.FullWidth = true;
                options.MaxWidth = MaxWidth.Medium;
                //options.FullHeight = true;
            });
        if (!res.DialogResult.Canceled)
        {
            var files = res.Component.UploadRequests;
            var tasks = (from file in files select new{Importer = FindImporter(file), File = file} 
                into importer where importer != null 
                select ExecuteImport(importer.Importer, importer.File )).ToList();
            _running = true;
            StateHasChanged();
            await Task.WhenAll(tasks);
            _progresses?.Clear();
            _running = false;
        }
    }

    private async Task ExecuteImport(IImportService service, IUploadableFile file)
    {
        if (service == null)
        {
            await ShowUnsupportedInfoAsync(file);
            return;
        }

        try
        {
            var cts = new CancellationTokenSource();
            cts.Token.Register(() =>
            {
                _progresses.TryRemove(file.FileName, out _);
            });
            _progresses.TryAdd(file.FileName, new Progress(cts) {Text = "Initializing...", Value = 1});
            StateHasChanged();
            await Task.Delay(10);
            string text = await service.GetKnowledgeTextAsync(file.Data, cts.Token, (s,v) => UpdateStatus(file.FileName, s, v));
            var result = await GptService.PrepareContentAsync(text, cts.Token, (s,v) => UpdateStatus(file.FileName, s, v));
            result.Content = await service.AfterPrepareAsync(result.Content, cts.Token);
            await SabioService.CreateArticleAsync(result.Title, result.Content, cts.Token, (s,v) => UpdateStatus(file.FileName, s, v));
            
            if(!cts.IsCancellationRequested)
                UpdateStatus(file.FileName, "Done", 1);
            StateHasChanged();
        }
        finally
        {
            _progresses.TryRemove(file.FileName, out _);
        }
    }

    private async Task ShowUnsupportedInfoAsync(IUploadableFile file)
    {
        var options = new MessageBoxOptions
        {
            Title = "Not supported",
            Message = $"There is no importer registered for content type {file.ContentType}"
        };
        await DialogService.ShowMessageBoxEx(options);
    }
    

    private void UpdateStatus(string file, string text, double progress)
    {
        try
        {
            if (_progresses.TryGetValue(file, out var p))
            {
                p.Text = text;
                p.Value = (int)(progress * 100);
                StateHasChanged();
            }
        }
        catch 
        {
        }
    }

    private IImportService FindImporter(IUploadableFile file)
    {
        return ServiceProvider.GetServices<IImportService>().FirstOrDefault(s => s.CanHandle(file.ContentType));
    }

    private Task Cancel(KeyValuePair<string, Progress> run)
    {
        run.Value.Cancellation.Cancel();
        UpdateStatus(run.Key, "Cancelled", 1);
        return Task.CompletedTask;
    }
}