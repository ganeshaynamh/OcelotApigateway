using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polly;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace ClientDemo.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientDemoController : ControllerBase
    {
        [Route("getvalues")]
        [HttpGet]
        public async Task<IActionResult> getvalues()
        {
            var result = await GetApiCall("http://localhost:64883/api/values");
            
            return Ok(JsonConvert.DeserializeObject<List<string>>(result));
        }
        private Task<string> GetApiCall(string url)
        {
            HttpClient httpClient = new HttpClient();
            var retry = Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.NotFound)
                .OrResult(r => r.StatusCode == HttpStatusCode.BadGateway)
                .OrResult(r => r.StatusCode == HttpStatusCode.InternalServerError)
            .Or<Exception>()
            .WaitAndRetry(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));


            HttpResponseMessage httpResponse = retry.Execute(() => {
                return httpClient.GetAsync(url).Result;

            });
            httpResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            string output = httpResponse.Content.ReadAsStringAsync().Result;

            return Task.FromResult(output);

        }
    }
   
}