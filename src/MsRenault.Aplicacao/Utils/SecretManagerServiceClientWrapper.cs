using Google.Cloud.SecretManager.V1;
using MsRenault.Aplicacao.Interfaces;

namespace MsRenault.Aplicacao.Utils;

public class SecretManagerServiceClientWrapper(SecretManagerServiceClient client) : ISecretManagerServiceClient
{
    private readonly SecretManagerServiceClient _client = client;

    public AccessSecretVersionResponse AccessSecretVersion(SecretVersionName secretVersionName)
    {
        return _client.AccessSecretVersion(secretVersionName);
    }
}
