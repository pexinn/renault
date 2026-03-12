using System.Collections.Generic;
using MsRenault.Aplicacao.Dtos;

namespace MsRenault.Aplicacao.Interfaces;

public interface ISecretManager
{
    string ObterAppSettingsDosSecrets(string projectID, IEnumerable<SecretsDTO> secretsList);
}
