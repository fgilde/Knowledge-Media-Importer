using KnowledgeMediaImporter.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace KnowledgeMediaImporter.Pages;

public partial class Settings
{
    [Inject] private IOptions<ServiceSettings> _serviceSettings { get; set; }
    [Inject] private IWebHostEnvironment _hostingEnvironment { get; set; }
    
    private string UserSettingsFile => Path.Combine(_hostingEnvironment.ContentRootPath, Constants.AppSettingsUserFile);

    private async Task SaveSettingsAsync(ServiceSettings settings)
    {
        await File.WriteAllTextAsync(UserSettingsFile, JsonConvert.SerializeObject(new { Services = settings }, Formatting.Indented));
    }

}