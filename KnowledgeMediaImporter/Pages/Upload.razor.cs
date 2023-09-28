using KnowledgeMediaImporter.Data;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Extensions;
using MudBlazor.Extensions.Components;
using Nextended.Core.Contracts;

namespace KnowledgeMediaImporter.Pages;

public partial class Upload
{
    [Inject] private IServiceProvider ServiceProvider { get; set; }
    [Inject] private IDialogService DialogService { get; set; }
    
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
            _ = ExecuteImport(FindImporter(res.Component.UploadRequest), res.Component.UploadRequest);
        }
    }

    private async Task ExecuteImport(IImportService service, IUploadableFile file)
    {
        if (service == null)
            throw new NotSupportedException("Ham wa nicht");
        string text = await service.CreateKnowledgeTextAsync(file.Data);
        // TODO: hämmer text to sabio
    }

    private IImportService FindImporter(IUploadableFile file)
    {
        return ServiceProvider.GetServices<IImportService>().FirstOrDefault(s => s.CanHandle(file.ContentType));
    }
}