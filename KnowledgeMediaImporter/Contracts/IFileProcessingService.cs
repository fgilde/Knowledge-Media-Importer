using Nextended.Core.Contracts;
using System.Collections.ObjectModel;

namespace KnowledgeMediaImporter.Contracts;

public interface IFileProcessingService
{
    Task ExecuteImportAsync(IEnumerable<IUploadableFile> files);
    ObservableCollection<Progress> FileProgresses { get; }
    event EventHandler<Progress> FileProgressesChanged;
}