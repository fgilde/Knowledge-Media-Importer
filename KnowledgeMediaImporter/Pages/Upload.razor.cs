using System.Collections.Specialized;
using System.Text;
using KnowledgeMediaImporter.Configuration;
using KnowledgeMediaImporter.Contracts;
using KnowledgeMediaImporter.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using MudBlazor;
using MudBlazor.Extensions;
using MudBlazor.Extensions.Components;
using MudBlazor.Extensions.Components.ObjectEdit;
using MudBlazor.Extensions.Options;

namespace KnowledgeMediaImporter.Pages;

public partial class Upload
{
    [Inject] private IOptionsMonitor<ServiceSettings> serviceSettingsMonitor { get; set; }
    private List<Progress>? _progresses;
    private bool? SettingsWrong;

    protected override Task OnInitializedAsync()
    {
        FileProcessingService.FileProgresses.CollectionChanged += FileProgressesOnCollectionChanged;
        FileProcessingService.FileProgressesChanged += OnProgressChanged;
        serviceSettingsMonitor.OnChange(settings => _ = ValidateSettings());
        return base.OnInitializedAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            UpdateProgress();
            _=ValidateSettings();
        }

        base.OnAfterRender(firstRender);
    }

    private async Task ValidateSettings()
    {
        var services = ServiceProvider.GetServices<IServiceSettingsValidation>().ToList();

        var resultsWithService = await Task.WhenAll(services.Select(async s =>
            (ServiceName: s.GetType().Name, Result: await s.ValidateServiceSettingsAsync(serviceSettingsMonitor.CurrentValue))));

        SettingsWrong = resultsWithService.Any(r => !r.Result.IsValid);
        await InvokeAsync(StateHasChanged);
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
        var settings = await ShowSettingsEdit();
        if (settings != null)
        {
            StateHasChanged();
            _ = FileProcessingService.ExecuteImportAsync(settings);
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

    private async Task<ImportJobConfiguration?> ShowSettingsEdit()
    {
        var parameters = new DialogParameters
        {
            {nameof(MudExObjectEditDialog<ImportJobConfiguration>.DialogIcon), Icons.Material.Filled.ImportExport}
        };
        var settings = new ImportJobConfiguration();
        var result = await DialogService.EditObject(settings, "Specify Import settings", OnSubmit, DialogOptionsEx.SlideInFromRight, null, parameters);
        return !result.Cancelled ? result.Result : null;
    }

    private Task<string> OnSubmit(ImportJobConfiguration value, MudExObjectEditDialog<ImportJobConfiguration> dialog)
    {
        var errorMessages = new StringBuilder();

        if (value?.KnowledgeTargetSettings?.TargetBranches is not { Length: > 0 })
            errorMessages.AppendLine("You need to specify at least one target branch.");

        if (string.IsNullOrEmpty(value?.KnowledgeTargetSettings?.TargetTreeNodeId))
            errorMessages.AppendLine("You need to specify a tree node.");

        if (value?.Files is not { Count: > 0 })
            errorMessages.AppendLine("You need to specify at least one file.");

        return Task.FromResult(errorMessages.ToString().Trim());
    }


    private void Cancel(Progress run)
    {
        run.Cancellation.Cancel();
    }
}