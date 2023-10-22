using System.Reflection;
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

        var props = typeof(OpenAI.Models.Model).GetProperties(BindingFlags.Public | BindingFlags.Static);
        string[] items = props.Select(p => p.GetValue(typeof(OpenAI.Models.Model)).ToString()).ToArray();

        meta.Property(m => m.ChatGpt.Model).RenderWithMudAutocomplete(items);
        return Task.CompletedTask;
    }
}