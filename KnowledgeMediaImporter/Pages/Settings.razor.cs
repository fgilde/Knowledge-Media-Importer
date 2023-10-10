using KnowledgeMediaImporter.Configuration;
using KnowledgeMediaImporter.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using MudBlazor;
using MudBlazor.Extensions;
using Newtonsoft.Json;
using Nextended.Core.Extensions;

namespace KnowledgeMediaImporter.Pages;

public partial class Settings
{
    [Inject] private IOptionsMonitor<ServiceSettings> serviceSettingsMonitor { get; set; }
    private ServiceSettings? _serviceSettings;

    private string UserSettingsFile => Path.Combine(HostingEnvironment.ContentRootPath, Constants.AppSettingsUserFile);
   
    protected override Task OnInitializedAsync()
    {
        _serviceSettings = serviceSettingsMonitor.CurrentValue.Clone();

        // Register change notification
        serviceSettingsMonitor.OnChange(HandleSettingsChange);
        return base.OnInitializedAsync();
    }

    private void HandleSettingsChange(ServiceSettings updatedSettings)
    {
        _serviceSettings = null;
        InvokeAsync(StateHasChanged);
        _serviceSettings = updatedSettings.Clone();
        InvokeAsync(StateHasChanged);
    }


    private async Task SaveSettingsAsync(EditContext context)
    {
        var services = ServiceProvider.GetServices<IServiceSettingsValidation>().ToList();

        var resultsWithService = await Task.WhenAll(services.Select(async s => (ServiceName: s.GetType().Name, Result: await s.ValidateServiceSettingsAsync(_serviceSettings))));

        if (resultsWithService.Any(r => !r.Result.IsValid))
        {
            var errorMessages = resultsWithService
                .Where(r => !r.Result.IsValid)
                .SelectMany(r => r.Result.Errors, (r, error) => $"{r.ServiceName}:\n - {error}");

            var message = $"Could not save settings, some are invalid: {string.Join(Environment.NewLine + Environment.NewLine, errorMessages)}";

            SnackBar.Add(message, Severity.Error);
        }
        else
        {
            await File.WriteAllTextAsync(UserSettingsFile, JsonConvert.SerializeObject(new { Services = _serviceSettings }, Formatting.Indented));
            SnackBar.Add("Settings saved!", Severity.Info);
        }
    }


    private async Task DeleteAsync()
    {
        if (!File.Exists(UserSettingsFile))
            return;
        var res = await DialogService.ShowConfirmationDialogAsync("Restore default", "Are you sure you want to restore the default settings? This will overwrite any changes you've ever made here.", icon: Icons.Material.Filled.RestorePage);
        if (res)
            File.Delete(UserSettingsFile);
    }

}