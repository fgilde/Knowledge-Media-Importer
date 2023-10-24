using KnowledgeMediaImporter.Shared;
using MudBlazor.Extensions;
using MudBlazor.Extensions.Components.ObjectEdit.Options;
using SABIO.ClientApi.Responses;

namespace KnowledgeMediaImporter.Configuration.MetaConfigurations;

internal static class RenderDataManager
{
    public static IServiceCollection AddMudExWithExtendedDefaults(this IServiceCollection services)
    {
        RegisterDefaults();
        return services.AddMudServicesWithExtensions(c => c.WithoutAutomaticCssLoading());
    }

    private static void RegisterDefaults()
    {
        // Custom Domain
        RenderDataDefaults.RegisterDefault<Branch[], IEnumerable<Branch>, BranchSelect>(s => s.SelectedValues);
        RenderDataDefaults.RegisterDefault<ICollection<Branch>, IEnumerable<Branch>, BranchSelect>(s => s.SelectedValues);
        RenderDataDefaults.RegisterDefault<IEnumerable<Branch>, BranchSelect>(s => s.SelectedValues);
        RenderDataDefaults.RegisterDefault<Group, GroupSelect>(s => s.Value);
        RenderDataDefaults.RegisterDefault<Group?, GroupSelect>(s => s.Value);
    }
}