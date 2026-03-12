using System.Text;
using System.Collections.Generic;
using Google.Cloud.SecretManager.V1;
using MsRenault.Aplicacao.Dtos;
using MsRenault.Aplicacao.Interfaces;
using Newtonsoft.Json;

namespace MsRenault.Aplicacao.Servicos;

public class SecretManager(ISecretManagerServiceClient client) : ISecretManager
{
    private readonly ISecretManagerServiceClient _client = client;

    public string ObterAppSettingsDosSecrets(string projectID, IEnumerable<SecretsDTO> secrets)
    {
        try
        {
            var listaSecrets = new StringBuilder();

            foreach (var secret in secrets)
            {
                string secretValue = "";

                SecretVersionName secretVersionName = new SecretVersionName(projectID, secret.NomeSecret, secret.VersaoSecret);
                var response = _client.AccessSecretVersion(secretVersionName);
                secretValue = response.Payload.Data.ToStringUtf8();

                if (listaSecrets.Length > 0)
                    listaSecrets.Append(", ");

                if (secret.ConfigBanco != null)
                    secretValue = MontarConnectionString(secret.ConfigBanco, secretValue);

                listaSecrets.Append(secretValue);
            }

            var retorno = $"{{{listaSecrets}}}";
            return retorno;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    public static string MontarConnectionString(BancoDTO banco, string secret)
    {
        StringBuilder listaBancoSecrets = new();
        listaBancoSecrets.Append("\"MongoDB\": {" + Environment.NewLine);
        listaBancoSecrets.Append("\"ConnectionString\": ");

        ConexaoBancoDTO? conexaoBanco = JsonConvert.DeserializeObject<ConexaoBancoDTO>(secret);
        if (conexaoBanco == null) return string.Empty;

        string secretBanco = string.Format(
            banco.ConexaoPattern,
            conexaoBanco.Username,
            conexaoBanco.Password,
            conexaoBanco.Host
        );

        listaBancoSecrets.Append("\"" + secretBanco + "\"" + Environment.NewLine + "}");
        return listaBancoSecrets.ToString();
    }
}
