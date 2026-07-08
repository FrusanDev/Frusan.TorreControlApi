using Newtonsoft.Json.Serialization;
using System.Net.Http.Formatting;
using System.Web.Http;
using TorreControlApi.Authorization;

namespace TorreControlApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.EnableCors();
            config.MessageHandlers.Add(new ApiKeyAuthHandler());
            config.Formatters.Clear();

            var jsonFormatter = new JsonMediaTypeFormatter();
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.Add(jsonFormatter);
        }
    }
}
