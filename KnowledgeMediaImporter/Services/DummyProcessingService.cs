using System;
using System.Collections.ObjectModel;
using KnowledgeMediaImporter.Contracts;
using KnowledgeMediaImporter.Model;
using Nextended.Core.Contracts;

namespace KnowledgeMediaImporter.Services;

public class DummyProcessingService : IFileProcessingService
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

    public DummyProcessingService(IServiceProvider serviceProvider, GptService gptService, SabioService sabioService)
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
            UpdateProgress(progress.Failed($"No importer registered for content type {file.ContentType}"));
            return;
        }


        void OnStatusUpdated(string s, double d) => UpdateProgress(progress, s, d);

        try
        {
            UpdateProgress(progress.Start().WriteLog("We start"));
            var rnd = new Random();
            await Task.Delay(rnd.Next(1000,2000));
            UpdateProgress(progress.WriteLog("tue echt gar nichts ausser warten"));
            UpdateProgress(progress.WriteLog("immer noch...."));
            OnStatusUpdated("Tu nichts", 0.3);
            await Task.Delay(rnd.Next(200, 2000));
            OnStatusUpdated("Tu immer noch nichts", 0.7);
            UpdateProgress(progress.WriteLog("tue echt gar nichts ausser warten"));

            await Task.Delay(rnd.Next(1000, 5000));
            OnStatusUpdated("Warte erstmal ab", 0.9);
            UpdateProgress(progress.WriteLog("... moin na wie gehts"));
            await Task.Delay(rnd.Next(1000, 3000));
            UpdateProgress(progress.WriteLog("Blah blah blub"));
            if (rnd.Next(1, 5) <= 2)
            {
                UpdateProgress(progress.Failed($"Kaputt"));
                return;
            }

            if (!cts.IsCancellationRequested)
                UpdateProgress(progress.Done());
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
        UpdateProgress(progress, "Cancelled", 1);
        RemoveProgress(progress);
    }

    private void UpdateProgress(Progress progress, double? value = null)
    {
        UpdateProgress(progress, progress.Text, value ?? progress.Value);
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