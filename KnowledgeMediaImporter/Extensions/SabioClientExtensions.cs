using KnowledgeMediaImporter.Configuration;
using SABIO.ClientApi.Core;
using SABIO.ClientApi.Core.Api;

namespace KnowledgeMediaImporter.Extensions;

public static class SabioClientExtensions
{
    public static Task LoginAsync(this SabioClient client, KnowledgeSettings settings)
    {
        return settings.Login == LoginType.ApiKey
            ? client.Api<AuthenticationApi>().LoginAsync(settings.ApiKey) 
            : client.Api<AuthenticationApi>().LoginAsync(settings.User, settings.Password);
    }
}