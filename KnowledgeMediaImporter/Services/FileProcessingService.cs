using System.Collections.ObjectModel;
using KnowledgeMediaImporter.Contracts;
using KnowledgeMediaImporter.Model;
using Nextended.Core.Contracts;

namespace KnowledgeMediaImporter.Services;

public class FileProcessingService : IFileProcessingService
{
    private readonly IServiceProvider ServiceProvider;
    private readonly GptService GptService;
    private readonly SabioService SabioService;

    // Define a thread-safe observable collection for progress.
    public ObservableCollection<Progress> FileProgresses { get; } = new();

    public event EventHandler<Progress> FileProgressesChanged;

    public FileProcessingService(IServiceProvider serviceProvider, GptService gptService, SabioService sabioService)
    {
        ServiceProvider = serviceProvider;
        GptService = gptService;
        SabioService = sabioService;
    }

    public async Task ExecuteImportAsync(ImportJobConfiguration configuration)
    {
        await Task.WhenAll(configuration.Files.Select(f => ExecuteImportAsync(f, configuration.KnowledgeTargetSettings)).ToList());
    }

    private async Task ExecuteImportAsync(IUploadableFile file, KnowledgeTargetSettings targetSettings)
    {
        var cts = new CancellationTokenSource();

        var progress = new Progress(file, cts);
        FileProgresses.Add(progress);

        cts.Token.Register(() => OnCancel(progress));


        var service = FindImporter(file);

        if (service == null)
        {
            UpdateProgress(progress, $"No importer registered for content type {file.ContentType}", 1);
            return;
        }


        void OnStatusUpdated(string s, double d) => UpdateProgress(progress, s, d);

        try
        {
            string text = await service.GetKnowledgeTextAsync(file.Data, cts.Token, OnStatusUpdated);
            var result = await GptService.PrepareContentAsync(text, cts.Token, OnStatusUpdated);
            //(string Title, string Content) result = ("TEST TEXT", text);

            result.Content = await service.AfterPrepareAsync(result.Content, cts.Token);
            await SabioService.CreateArticleAsync(result.Title, result.Content, file.Path, targetSettings, cts.Token, OnStatusUpdated);

            if (!cts.IsCancellationRequested)
                progress.Text = "Done";
        }
        finally
        {
            cts.Dispose();
            RemoveProgress(progress);
        }
    }

    private void RemoveProgress(Progress progress)
    {
        if(FileProgresses.Contains(progress))
            FileProgresses.Remove(progress);
    }

    private void OnCancel(Progress progress)
    {
        UpdateProgress(progress, "Cancelled", 1);
        RemoveProgress(progress);
    }

    private void UpdateProgress(Progress progress, string text, double value)
    {
        progress.Text = text;
        progress.Value = (int)(value * 100);
        FileProgressesChanged?.Invoke(this, progress);
    }

    private IImportService? FindImporter(IUploadableFile file)
    {
        return ServiceProvider.GetServices<IImportService>().FirstOrDefault(s => s.CanHandle(file.ContentType));
    }
}