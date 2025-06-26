using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace USERSERVICE.Functions
{
    public class PingFunction
    {
        [Function("PingFunction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
            FunctionContext context)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("✅ Ping działa!");
            return response;
        }
    }
}
