using System.Reflection;
using MudBlazor;
using MudBlazor.Extensions.Components.ObjectEdit;
using MudBlazor.Extensions.Components.ObjectEdit.Options;

namespace KnowledgeMediaImporter.Configuration.MetaConfigurations;

public class ServiceSettingsMetaConfiguration : IObjectMetaConfiguration<ServiceSettings>
{
    public Task ConfigureAsync(ObjectEditMeta<ServiceSettings> meta)
    {
        meta.Properties(m => m.Knowledge.Realm).Ignore(); // Ignore realm DevOps can override it but it's not needed for the user

        meta.Property(m => m.Knowledge.Password).RenderWith<MudTextField<string>, string>(field => field.Value, field =>
        {
            field.InputType = InputType.Password;
        });
        
        meta.Properties(m => m.Knowledge.User, m => m.Knowledge.Password)
            .IgnoreIf(model => model.Knowledge.Login == LoginType.ApiKey);
        meta.Property(m => m.Knowledge.ApiKey)
            .IgnoreIf(model => model.Knowledge.Login == LoginType.User);

        var props = typeof(OpenAI.Models.Model).GetProperties(BindingFlags.Public | BindingFlags.Static);
        string[] items = props.Select(p => p.GetValue(typeof(OpenAI.Models.Model)).ToString()).ToArray();

        meta.Property(m => m.ChatGpt.Model).RenderWithMudAutocomplete(items);
        return Task.CompletedTask;
    }
}