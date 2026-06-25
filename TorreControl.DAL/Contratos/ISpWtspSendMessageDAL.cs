using System;

namespace TorreControl.DAL
{
    public interface ISpWtspSendMessageDAL : IDisposable
    {
        void EnviarMensajeWhatsapp(string grupoId, string body);
    }
}
