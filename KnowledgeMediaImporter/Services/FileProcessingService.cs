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
    public void Cancel(Progress run)
    {
        if (run.Cancellation is { IsCancellationRequested: false })
            run.Cancellation.Cancel();
        else
            RemoveProgress(run);
    }

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
        progress.Changed += (_, _) => FileProgressesChanged?.Invoke(this, progress);
        FileProgresses.Add(progress);

        cts.Token.Register(() => OnCancel(progress));
        
        var service = FindImporter(file);

        if (service == null)
        {
            progress.Failed($"No importer registered for content type {file.ContentType}");
            return;
        }
        
        try
        {
            string text = await service.GetKnowledgeTextAsync(file.Data, cts.Token, progress.WithRange(0, 30));
            var result = await GptService.PrepareContentAsync(text, cts.Token, progress.WithRange(30, 60));
            if(result == default) 
                return;
            result.Content = await service.AfterPrepareAsync(result.Content, cts.Token);
            await SabioService.CreateArticleAsync(new CreateArticleOptions(result.Title, result.Content, file, targetSettings, progress.WithRange(60, 90), cts.Token));
            if (!cts.IsCancellationRequested)
                progress.WithoutRange().Done();
        }
        finally
        {
            cts.Dispose();
            progress.Cancellation = null;
        }
    }

    private void RemoveProgress(Progress progress)
    {
        if(FileProgresses.Contains(progress))
            FileProgresses.Remove(progress);
    }

    private void OnCancel(Progress progress)
    {
        (progress as IProgressUpdate).Update("Cancelled", 100);
        RemoveProgress(progress);
    }


    private IImportService? FindImporter(IUploadableFile file)
    {
        return ServiceProvider.GetServices<IImportService>().FirstOrDefault(s => s.CanHandle(file.ContentType));
    }
}