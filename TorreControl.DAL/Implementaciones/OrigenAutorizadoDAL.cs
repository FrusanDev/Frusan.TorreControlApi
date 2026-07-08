using System;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;
using System.Configuration;
using TorreControl.BEL;

namespace TorreControl.DAL
{
    public class OrigenAutorizadoDAL : IOrigenAutorizadoDAL
    {
        #region IOrigenAutorizadoDAL Implementations

        /// <summary>
        /// Valida una API Key en texto plano contra TC_OrigenAutorizado mediante el SP TC_SP_ValidarOrigenAutorizado
        /// </summary>
        /// <param name="apiKeyPlano"></param>
        /// <returns></returns>
        public OrigenAutorizadoBEL ValidarApiKey(string apiKeyPlano)
        {
            var databaseSetting = (DatabaseSettings)ConfigurationManager.GetSection("dataConfiguration");
            Database db = DatabaseFactory.CreateDatabase(databaseSetting.DefaultDatabase);

            DbCommand cmd = db.GetStoredProcCommand("TC_SP_ValidarOrigenAutorizado");
            db.AddInParameter(cmd, "@ApiKeyPlano", DbType.String, apiKeyPlano);

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read() && reader["IdOrigenAutorizado"] != DBNull.Value)
                    {
                        return new OrigenAutorizadoBEL
                        {
                            IdOrigenAutorizado = Convert.ToInt32(reader["IdOrigenAutorizado"]),
                            Codigo = reader["Codigo"] != DBNull.Value ? reader["Codigo"].ToString().Trim() : null,
                            Activo = reader["Activo"] != DBNull.Value && Convert.ToBoolean(reader["Activo"])
                        };
                    }
                }
                return null;
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

        ~OrigenAutorizadoDAL()
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
