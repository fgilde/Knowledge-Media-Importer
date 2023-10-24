using MudBlazor.Extensions;
using KnowledgeMediaImporter;
using KnowledgeMediaImporter.Configuration;
using KnowledgeMediaImporter.Contracts;
using KnowledgeMediaImporter.Extensions;
using KnowledgeMediaImporter.Services;
using Nextended.Core.Extensions;
using Microsoft.Extensions.Options;
using SABIO.ClientApi.Core;
using KnowledgeMediaImporter.Configuration.MetaConfigurations;

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

builder.Services.AddScoped<Task<SabioClient>>(async p =>
{
    var cfg = p.GetRequiredService<IOptionsSnapshot<ServiceSettings>>().Value.Knowledge;
    var client = await SabioClient.CreateAsync(cfg.Url, cfg.Realm);
    if (!client.IsLoggedIn)
        await client.LoginAsync(cfg);
    return client.DisableAutomaticCaching();
});

//builder.Services.AddScoped<SabioClient>(p =>
//{
//    var clientTask = p.GetRequiredService<Task<SabioClient>>();
//    return clientTask.Result;  // Blocks until the task completes
//});



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