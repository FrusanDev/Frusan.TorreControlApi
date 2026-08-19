using Microsoft.AspNet.SignalR;

namespace TorreControlApi.Hubs
{
    /// <summary>
    /// Hub de solo difusion para la Torre de Control: AlertaController llama a
    /// Clients.All.NuevaAlerta(idEvento) justo despues de insertar en TC_Evento (Post exitoso).
    /// No expone metodos invocables desde el cliente — los navegadores solo escuchan.
    /// </summary>
    public class AlertasHub : Hub
    {
    }
}
