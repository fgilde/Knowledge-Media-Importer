using MudBlazor.Extensions;
using KnowledgeMediaImporter;
using KnowledgeMediaImporter.Configuration;
using KnowledgeMediaImporter.Contracts;
using KnowledgeMediaImporter.Extensions;
using KnowledgeMediaImporter.Services;
using Nextended.Core.Extensions;
using KnowledgeMediaImporter.MetaConfigurations;
using Microsoft.Extensions.Options;
using SABIO.ClientApi.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile(Constants.AppSettingsFile, optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile(Constants.AppSettingsUserFile, optional: true, reloadOnChange: true);
builder.Services.Configure<ServiceSettings>(builder.Configuration.GetSection("Services"));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddMudExWithExtendedDefaults();

if(builder.Configuration.GetValue<bool>("Dummy"))
    builder.Services.AddScoped<IFileProcessingService, DummyProcessingService>();
else
    builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();

builder.Services.AddScoped<VideoAnalyzer>();
builder.Services.RegisterAllImplementationsOf<IImportService>(lifeTime: ServiceLifetime.Scoped);
builder.Services.RegisterAllImplementationsOf<IServiceSettingsValidation>(lifeTime: ServiceLifetime.Scoped);


builder.Services.AddScoped<SabioService>();
builder.Services.AddScoped<GptService>();

builder.Services.AddScoped<SabioClient>(p =>
{
    var cfg = p.GetRequiredService<IOptionsSnapshot<ServiceSettings>>().Value.Knowledge;
    var result = new SabioClient(cfg.Url, cfg.Realm);
    if (!result.IsLoggedIn)
        _ = result.LoginAsync(cfg);
    return result.DisableAutomaticCaching();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();