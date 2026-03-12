using Hangfire.Dashboard;

namespace MsRenault.API.Configuracoes;

public class AutorizacaoHangfire : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
       return true;
    }
}
