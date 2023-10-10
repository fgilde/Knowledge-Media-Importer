using KnowledgeMediaImporter.Configuration;
using KnowledgeMediaImporter.Model;

namespace KnowledgeMediaImporter.Contracts;

public interface IServiceSettingsValidation
{
    public Task<ServiceValidationResult> ValidateServiceSettingsAsync(ServiceSettings? serviceSettings);
}