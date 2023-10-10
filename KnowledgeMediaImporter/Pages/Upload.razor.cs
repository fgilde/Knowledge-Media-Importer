using System.Collections.Specialized;
using KnowledgeMediaImporter.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Extensions;
using MudBlazor.Extensions.Components;

namespace KnowledgeMediaImporter.Pages;

public partial class Upload
{

    private List<Progress>? _progresses;

    [Inject] private IFileProcessingService FileProcessingService { get; set; }
    [Inject] private IDialogService DialogService { get; set; }
    
    protected override Task OnInitializedAsync()
    {
        FileProcessingService.FileProgresses.CollectionChanged += FileProgressesOnCollectionChanged;
        FileProcessingService.FileProgressesChanged += OnProgressChanged;
        return base.OnInitializedAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if(firstRender)
            UpdateProgress();
        base.OnAfterRender(firstRender);
    }

    private void OnProgressChanged(object? sender, Progress e) => UpdateProgress();
    private void FileProgressesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateProgress();


    private void UpdateProgress()
    {
        _progresses = FileProcessingService.FileProgresses.ToList();
        StateHasChanged();
    }


    private async Task UploadClick()
    {
        var parameters = CreateDialogParameters();
        var res = await ShowUploadDialog(parameters);

        if (!res.DialogResult.Canceled)
        {
            StateHasChanged();
            _ = FileProcessingService.ExecuteImportAsync(res.Component.UploadRequests);
        }
    }


    private DialogParameters CreateDialogParameters()
    {
        return new DialogParameters
        {
            { nameof(MudExMessageDialog.Buttons), MudExDialogResultAction.OkCancel("Upload") },
            { nameof(MudExMessageDialog.Icon), Icons.Material.Filled.FileUpload }
        };
    }

    private Task<(DialogResult DialogResult, MudExUploadEdit<UploadableFile> Component)> ShowUploadDialog(DialogParameters parameters)
    {
        return DialogService.ShowComponentInDialogAsync<MudExUploadEdit<UploadableFile>>("Upload content", "Upload content files as zip or separate",
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
         });

    }

    private void Cancel(Progress run)
    {
        run.Cancellation.Cancel();
    }
}