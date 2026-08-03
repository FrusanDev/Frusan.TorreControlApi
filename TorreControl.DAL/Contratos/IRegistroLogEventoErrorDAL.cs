using System;
using TorreControl.BEL;

namespace TorreControl.DAL
{
    public interface IRegistroLogEventoErrorDAL : IDisposable
    {
        void InsertarRegistroLogEvento(RegistroLogEventoBEL registroLogEvento);
    }
}
