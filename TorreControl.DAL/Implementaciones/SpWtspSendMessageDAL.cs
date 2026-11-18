using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;

namespace TorreControl.DAL
{
    public class SpWtspSendMessageDAL : ISpWtspSendMessageDAL
    {
        #region ISpWtspSendMessageDAL Implementation

        public void EnviarMensajeWhatsapp(string grupoId, string body)
        {
            var databaseSetting = (DatabaseSettings)ConfigurationManager.GetSection("dataConfiguration");
            Database database = DatabaseFactory.CreateDatabase(databaseSetting.DefaultDatabase);

            DbCommand cmd = database.GetStoredProcCommand("SpWtspSendMessage");
            database.AddInParameter(cmd, "@GrupoId", DbType.String, grupoId);
            database.AddInParameter(cmd, "@Body", DbType.String, body);

            using (DbConnection connection = database.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                try
                {
                    database.ExecuteNonQuery(cmd, transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Envía un mensaje de WhatsApp a un grupo usando una instancia y token específicos,
        /// mediante el SP SpWtspSendMessageEx (@Instancia, @Token, @GrupoId, @Body)
        /// </summary>
        /// <param name="instancia">Instancia del proveedor de WhatsApp (p.ej. instance196600)</param>
        /// <param name="token">Token de la instancia (viene de appSettings, nunca del código)</param>
        /// <param name="grupoId">ID del grupo destino (xxxxxxxxxxxxxxxxxx@g.us)</param>
        /// <param name="body">Texto del mensaje</param>
        public void EnviarMensajeWhatsappEx(string instancia, string token, string grupoId, string body)
        {
            var databaseSetting = (DatabaseSettings)ConfigurationManager.GetSection("dataConfiguration");
            Database database = DatabaseFactory.CreateDatabase(databaseSetting.DefaultDatabase);

            DbCommand cmd = database.GetStoredProcCommand("SpWtspSendMessageEx");
            database.AddInParameter(cmd, "@Instancia", DbType.String, instancia);
            database.AddInParameter(cmd, "@Token", DbType.String, token);
            database.AddInParameter(cmd, "@GrupoId", DbType.String, grupoId);
            database.AddInParameter(cmd, "@Body", DbType.String, body);

            using (DbConnection connection = database.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();

                try
                {
                    database.ExecuteNonQuery(cmd, transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region IDisposable Implementation

        ~SpWtspSendMessageDAL()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion
    }
}
