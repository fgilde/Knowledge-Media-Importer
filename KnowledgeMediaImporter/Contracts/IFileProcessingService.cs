using Nextended.Core.Contracts;
using System.Collections.ObjectModel;
using KnowledgeMediaImporter.Model;

namespace KnowledgeMediaImporter.Contracts;

public interface IFileProcessingService
{
    Task ExecuteImportAsync(ImportJobConfiguration configuration);
    ObservableCollection<Progress> FileProgresses { get; }
    event EventHandler<Progress> FileProgressesChanged;
}