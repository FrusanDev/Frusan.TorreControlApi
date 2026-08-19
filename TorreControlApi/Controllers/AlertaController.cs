using System;
using System.Web.Http;
using TorreControl.BEL;
using TorreControl.BLL;
using TorreControlApi.Authorization;

namespace TorreControlApi.Controllers
{
    [RoutePrefix("api/alertas")]
    public class AlertaController : ApiController
    {
        private readonly IAlertaBLL alertaBLL;

        /// <summary>
        /// Constructor que recibe la dependencia BLL mediante inyección de Unity
        /// </summary>
        /// <param name="alertaBLL"></param>
        public AlertaController(IAlertaBLL alertaBLL)
        {
            this.alertaBLL = alertaBLL;
        }

        /// <summary>
        /// Endpoint POST que recibe una alerta desde cualquier sistema origen y la registra en la Torre de Control
        /// </summary>
        /// <param name="request">Datos de la alerta a registrar: código de tipo de alerta, payload con los datos del evento y, opcionalmente, mensaje de WhatsApp propio del origen.</param>
        /// <returns>HTTP 200 con el id del evento creado, o 400 si el tipo de alerta no existe/no está activo.</returns>
        [HttpPost, Route("")]
        public IHttpActionResult Post([FromBody] IngresarAlertaRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CodigoTipoAlerta))
                return BadRequest("CodigoTipoAlerta es requerido.");

            try
            {
                string origenAutenticado = Request.Properties[ApiKeyAuthHandler.OrigenAutenticadoKey] as string;
                int idEvento = this.alertaBLL.IngresarAlerta(request, origenAutenticado);
                return Ok(new { mensaje = "Alerta registrada correctamente.", idEvento });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Endpoint GET que retorna los eventos registrados, filtrables por rango de fecha y estado.
        /// Sin paginación: si no se especifican fechas, se acota por defecto a los últimos 7 días
        /// para evitar traer todo el histórico a medida que crece el volumen
        /// </summary>
        /// <param name="fechaDesde">Fecha de inicio del rango a consultar (opcional). Si se omite junto con fechaHasta, se usan por defecto los últimos 7 días.</param>
        /// <param name="fechaHasta">Fecha de término del rango a consultar (opcional).</param>
        /// <param name="estado">Filtra por estado del evento: "No atendido", "Atendido" o "Cerrado" (opcional).</param>
        /// <returns>Lista de eventos registrados que cumplen el filtro.</returns>
        [HttpGet, Route("")]
        public IHttpActionResult Get(DateTime? fechaDesde = null, DateTime? fechaHasta = null, string estado = null)
        {
            try
            {
                var eventos = this.alertaBLL.ObtenerEventos(fechaDesde, fechaHasta, estado);
                return Ok(eventos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
