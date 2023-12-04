using KnowledgeMediaImporter.Shared;
using MudBlazor.Extensions;
using MudBlazor.Extensions.Components.ObjectEdit.Options;
using SABIO.ClientApi.Responses;

namespace KnowledgeMediaImporter.Configuration.MetaConfigurations;

internal static class RenderDataManager
{
    public class AppIds
    {
        public const string DropBox = "2ak2m6cfpdeb9f1";
        public const string Google = "787005879852-vkv0cduhl70u087pq4a8s2jtkdgv1n6s.apps.googleusercontent.com";
        public const string OneDrive = "55d00a29-1bb6-40bf-90a6-ecd689a52a51";
    }

    public static IServiceCollection AddMudExWithExtendedDefaults(this IServiceCollection services)
    {
        RegisterDefaults();
        return services.AddMudServicesWithExtensions(c => c.WithoutAutomaticCssLoading().EnableDropBoxIntegration(AppIds.DropBox)
            .EnableGoogleDriveIntegration(AppIds.Google)
            .EnableOneDriveIntegration(AppIds.OneDrive));
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