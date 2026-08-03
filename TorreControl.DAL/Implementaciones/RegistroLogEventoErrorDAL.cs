using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;
using TorreControl.BEL;

namespace TorreControl.DAL
{
    public class RegistroLogEventoErrorDAL : IRegistroLogEventoErrorDAL
    {
        #region IRegistroLogEventoErrorDAL Implementation

        /// <summary>
        /// Inserta un registro en la tabla de log de errores compartida (mismo SP que usan
        /// los demás procesos de fondo de Frusan, ej. Hangfire de EmailInformativoApi)
        /// </summary>
        /// <param name="registroLogEvento"></param>
        public void InsertarRegistroLogEvento(RegistroLogEventoBEL registroLogEvento)
        {
            var databaseSetting = (DatabaseSettings)ConfigurationManager.GetSection("dataConfiguration");
            Database db = DatabaseFactory.CreateDatabase(databaseSetting.DefaultDatabase);

            DbCommand cmd = db.GetStoredProcCommand("prdTermografoInformativo_InsertarRegistroLogEventoError");
            db.AddInParameter(cmd, "@fechaHora", DbType.DateTime, registroLogEvento.FechaHora);
            db.AddInParameter(cmd, "@servidor", DbType.String, registroLogEvento.Servidor);
            db.AddInParameter(cmd, "@nombreTarea", DbType.String, registroLogEvento.NombreTarea);
            db.AddInParameter(cmd, "@errorDescripcion", DbType.String, registroLogEvento.ErrorDescripcion);

            try
            {
                db.ExecuteNonQuery(cmd);
            }
            catch
            {
                throw;
            }
            finally
            {
                cmd?.Dispose();
            }
        }

        #endregion

        #region IDisposable Implementation

        ~RegistroLogEventoErrorDAL()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Libera los recursos administrados utilizados por la instancia
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libera los recursos administrados y no administrados según el parámetro disposing
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion
    }
}
