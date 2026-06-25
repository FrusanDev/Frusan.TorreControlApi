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
