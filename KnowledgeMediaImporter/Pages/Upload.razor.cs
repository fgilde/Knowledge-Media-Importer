using KnowledgeMedia.Core;
using KnowledgeMediaImporter.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Extensions;
using MudBlazor.Extensions.Components;
using Nextended.Core.Contracts;

namespace KnowledgeMediaImporter.Pages;

public partial class Upload
{
    private bool _running;
    public string ProgressText { get; set; }
    public int ProgressValue { get; set; }
    private string fileName;
    [Inject] private IJSRuntime JS { get; set; }
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
                uploadEdit.AllowMultiple = false;
                uploadEdit.MinHeight = 400;
                uploadEdit.AutoExtractZip = true;
                uploadEdit.MimeTypes = Array.Empty<string>();
                uploadEdit.MimeRestrictionType = MimeTypeRestrictionType.BlackList;
            }, parameters, options =>
            {
                options.Resizeable = true;
                options.FullWidth = true;
                options.MaxWidth = MaxWidth.Medium;
                //options.FullHeight = true;
            });
        if (!res.DialogResult.Canceled)
        {
            fileName = res.Component.UploadRequest.FileName;
            _ = ExecuteImport(FindImporter(res.Component.UploadRequest), res.Component.UploadRequest);
        }
    }

    private async Task ExecuteImport(IImportService service, IUploadableFile file)
    {
        if (service == null)
        {
            await ShowUnsupportedInfoAsync(file);
            return;
        }
        _running = true;
        StateHasChanged();
        string text = await service.GetKnowledgeTextAsync(file.Data, UpdateStatus);
        var result = await GptService.PrepareContentAsync(text, UpdateStatus);
        result.Content = await service.AfterPrepareAsync(result.Content);
        var url = await SabioService.CreateArticleAsync(result.Title, result.Content, UpdateStatus);
        UpdateStatus("Done", 1);
        _running = false;
        StateHasChanged();
        await JS.InvokeVoidAsync("window.open", url);
    }

    private async Task ShowUnsupportedInfoAsync(IUploadableFile file)
    {
        var mboxOptions = new MessageBoxOptions();
        mboxOptions.Title = "Not supported";
        mboxOptions.Message = $"There is no importer registered for content type {file.ContentType}";
        await DialogService.ShowMessageBoxEx(mboxOptions);
    }
    

    private void UpdateStatus(string text, double progress)
    {
        ProgressText = $"{fileName} - {text}";
        ProgressValue = (int)(progress * 100);
        StateHasChanged();
    }

    private IImportService FindImporter(IUploadableFile file)
    {
        return ServiceProvider.GetServices<IImportService>().FirstOrDefault(s => s.CanHandle(file.ContentType));
    }
}