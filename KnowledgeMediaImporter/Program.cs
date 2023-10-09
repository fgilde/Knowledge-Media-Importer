using KnowledgeMedia.Core;
using KnowledgeMediaImporter.Data;
using MudBlazor.Extensions;
using KnowledgeMedia.Core.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ServiceSettings>(builder.Configuration.GetSection("Services"));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddMudServicesWithExtensions();
builder.Services.AddScoped<IImportService, VideoImportService>();
builder.Services.AddScoped<IImportService, PdfImportService>();
builder.Services.AddScoped<IImportService, WordImportService>();
builder.Services.AddSingleton<SabioService>();
builder.Services.AddSingleton<GptService>();

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