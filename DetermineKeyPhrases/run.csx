#r "Newtonsoft.Json"
using Newtonsoft.Json;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;

public static async Task<IActionResult> Run(HttpRequest request, ILogger log)
{
    log.LogInformation("DetermineLanguage function processed a request.");    
    string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
    dynamic bodyData = JsonConvert.DeserializeObject(requestBody);
    using (HttpClient client = new HttpClient())
    {
        client.BaseAddress = new Uri(System.Environment.GetEnvironmentVariable("EndpointUrl", EnvironmentVariableTarget.Process) + "/", UriKind.Absolute);
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", System.Environment.GetEnvironmentVariable("EndpointKey", EnvironmentVariableTarget.Process));
        string postBody = JsonConvert.SerializeObject(
            new
            {
                Documents = new[]
                {
                    new
                    {
                        Id = Guid.NewGuid().ToString().ToLower(),
                        Language = bodyData.language,
                        Text = bodyData.text
                    }
                }
            }
        );
        HttpResponseMessage response = await client.PostAsync($"keyPhrases", new StringContent(postBody, Encoding.UTF8, "application/json"));
        dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
        string[] keyPhrases = result.documents[0].keyPhrases.ToObject<string[]>();
        return new OkObjectResult(
            new 
            {
                keyPhrases = keyPhrases,
                language = bodyData.language,
                text = bodyData.text
            }
        );
    }
}
