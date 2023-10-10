using MudBlazor.Extensions;
using KnowledgeMediaImporter;
using KnowledgeMediaImporter.Configuration;
using KnowledgeMediaImporter.Contracts;
using KnowledgeMediaImporter.Services;
using MudBlazor.Extensions.Components;
using Nextended.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile(Constants.AppSettingsFile, optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile(Constants.AppSettingsUserFile, optional: true, reloadOnChange: true);
builder.Services.Configure<ServiceSettings>(builder.Configuration.GetSection("Services"));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddMudServicesWithExtensions();
builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();
builder.Services.AddScoped<VideoAnalyzer>();
builder.Services.RegisterAllImplementationsOf<IImportService>(lifeTime: ServiceLifetime.Scoped);
builder.Services.RegisterAllImplementationsOf<IServiceSettingsValidation>(lifeTime: ServiceLifetime.Scoped);


builder.Services.AddScoped<SabioService>();
builder.Services.AddScoped<GptService>();

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