using Google.Cloud.SecretManager.V1;

namespace MsRenault.Aplicacao.Interfaces;

public interface ISecretManagerServiceClient
{
    AccessSecretVersionResponse AccessSecretVersion(SecretVersionName secretVersionName);
}
