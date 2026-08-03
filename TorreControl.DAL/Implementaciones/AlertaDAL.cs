using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Configuration;
using System.Configuration;
using TorreControl.BEL;

namespace TorreControl.DAL
{
    public class AlertaDAL : IAlertaDAL
    {
        #region IAlertaDAL Implementations

        /// <summary>
        /// Obtiene el tipo de alerta desde TC_TipoAlerta según su código mediante el SP TC_SP_ObtenerTipoAlerta
        /// </summary>
        /// <param name="codigoTipoAlerta"></param>
        /// <returns></returns>
        public TipoAlertaBEL ObtenerTipoAlerta(string codigoTipoAlerta)
        {
            var databaseSetting = (DatabaseSettings)ConfigurationManager.GetSection("dataConfiguration");
            Database db = DatabaseFactory.CreateDatabase(databaseSetting.DefaultDatabase);

            DbCommand cmd = db.GetStoredProcCommand("TC_SP_ObtenerTipoAlerta");
            db.AddInParameter(cmd, "@Codigo", DbType.String, codigoTipoAlerta);

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        return new TipoAlertaBEL
                        {
                            IdTipoAlerta = reader["IdTipoAlerta"] != DBNull.Value ? Convert.ToInt32(reader["IdTipoAlerta"]) : 0,
                            Codigo = reader["Codigo"] != DBNull.Value ? reader["Codigo"].ToString().Trim() : null,
                            Nombre = reader["Nombre"] != DBNull.Value ? reader["Nombre"].ToString().Trim() : null,
                            Area = reader["Area"] != DBNull.Value ? reader["Area"].ToString().Trim() : null,
                            Activo = reader["Activo"] != DBNull.Value && Convert.ToBoolean(reader["Activo"]),
                            PublicaGrupoWzap = reader["PublicaGrupoWzap"] != DBNull.Value && Convert.ToBoolean(reader["PublicaGrupoWzap"])
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

        /// <summary>
        /// Inserta un nuevo evento de alerta en TC_Evento mediante el SP TC_SP_InsertarEvento y retorna el ID generado
        /// </summary>
        /// <param name="evento"></param>
        /// <returns></returns>
        public int InsertarEvento(EventoBEL evento)
        {
            var databaseSetting = (DatabaseSettings)ConfigurationManager.GetSection("dataConfiguration");
            Database db = DatabaseFactory.CreateDatabase(databaseSetting.DefaultDatabase);

            DbCommand cmd = db.GetStoredProcCommand("TC_SP_InsertarEvento");
            db.AddInParameter(cmd, "@IdTipoAlerta", DbType.Int32, evento.IdTipoAlerta);
            db.AddInParameter(cmd, "@Payload", DbType.String, evento.Payload);
            db.AddInParameter(cmd, "@Estado", DbType.String, evento.Estado);
            db.AddInParameter(cmd, "@FechaOcurrencia", DbType.DateTime, evento.FechaOcurrencia);
            db.AddInParameter(cmd, "@OrigenSistema", DbType.String, evento.OrigenSistema);
            db.AddInParameter(cmd, "@Severidad", DbType.String, (object)evento.Severidad ?? DBNull.Value);
            db.AddInParameter(cmd, "@DescripcionBreve", DbType.String, (object)evento.DescripcionBreve ?? DBNull.Value);
            db.AddOutParameter(cmd, "@IdEvento", DbType.Int32, 4);

            try
            {
                db.ExecuteNonQuery(cmd);
                return Convert.ToInt32(db.GetParameterValue(cmd, "@IdEvento"));
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

        /// <summary>
        /// Obtiene la lista de responsables activos asignados a un tipo de alerta mediante el SP TC_SP_ObtenerResponsables
        /// </summary>
        /// <param name="idTipoAlerta"></param>
        /// <returns></returns>
        public List<ResponsableBEL> ObtenerResponsables(int idTipoAlerta)
        {
            var returnValue = new List<ResponsableBEL>();

            var databaseSetting = (DatabaseSettings)ConfigurationManager.GetSection("dataConfiguration");
            Database db = DatabaseFactory.CreateDatabase(databaseSetting.DefaultDatabase);

            DbCommand cmd = db.GetStoredProcCommand("TC_SP_ObtenerResponsables");
            db.AddInParameter(cmd, "@IdTipoAlerta", DbType.Int32, idTipoAlerta);

            try
            {
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        returnValue.Add(new ResponsableBEL
                        {
                            IdResponsable = reader["IdResponsable"] != DBNull.Value ? Convert.ToInt32(reader["IdResponsable"]) : 0,
                            IdTipoAlerta = idTipoAlerta,
                            Nombre = reader["Nombre"] != DBNull.Value ? reader["Nombre"].ToString().Trim() : null,
                            Telefono = reader["Telefono"] != DBNull.Value ? reader["Telefono"].ToString().Trim() : null,
                            Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString().Trim() : null,
                            Activo = reader["Activo"] != DBNull.Value && Convert.ToBoolean(reader["Activo"])
                        });
                    }
                }
                return returnValue;
            }
            catch
            {
                throw;
            }
            finally
            {
                cmd?.Dispose();
                returnValue = null;
            }
        }

        #endregion

        #region IDisposable Implementation

        ~AlertaDAL()
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
