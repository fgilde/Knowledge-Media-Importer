using KnowledgeMediaImporter.Configuration;
using MudBlazor;
using MudBlazor.Extensions.Components.ObjectEdit;
using MudBlazor.Extensions.Components.ObjectEdit.Options;

namespace KnowledgeMediaImporter.MetaConfigurations;

public class ServiceSettingsMetaConfiguration: IObjectMetaConfiguration<ServiceSettings>
{
    public Task ConfigureAsync(ObjectEditMeta<ServiceSettings> meta)
    {
        meta.Property(m => m.Knowledge.Password).RenderWith<MudTextField<string>, string>(field => field.Value, field =>
        {
            field.InputType = InputType.Password;
        });
        return Task.CompletedTask;
    }
}