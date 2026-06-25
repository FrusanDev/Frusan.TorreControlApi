using System;
using System.Web.Http;
using TorreControl.BEL;
using TorreControl.BLL;

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
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost, Route("")]
        public IHttpActionResult Post([FromBody] IngresarAlertaRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CodigoTipoAlerta))
                return BadRequest("CodigoTipoAlerta es requerido.");

            try
            {
                int idEvento = this.alertaBLL.IngresarAlerta(request);
                return Ok(new { mensaje = "Alerta registrada correctamente.", idEvento });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
