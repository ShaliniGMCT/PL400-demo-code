using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xrm.Tools.WebAPI;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Xrm.Tools.WebAPI.Requests;
using System.Dynamic;
using Xrm.Tools.WebAPI.Results;
using System.Collections.Generic;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            CRMWebAPI api = GetCRMWebAPI(log).Result;
            dynamic whoami = api.ExecuteFunction("WhoAmI").Result;
            log.LogInformation($"UserId: {whoami.UserId}");
            

            //var employees = GetEmployees(api).Result;
            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(whoami);


        }

        private static async Task<CRMWebAPI> GetCRMWebAPI(ILogger log)
        {
            var clientID = Environment.GetEnvironmentVariable("cdsclientid", EnvironmentVariableTarget.Process);
            var clientSecret = Environment.GetEnvironmentVariable("cdsclientsecret", EnvironmentVariableTarget.Process);
            var crmBaseUrl = Environment.GetEnvironmentVariable("cdsurl", EnvironmentVariableTarget.Process);
            var crmurl = crmBaseUrl + "/api/data/v9.2/";

            AuthenticationParameters ap = await AuthenticationParameters.CreateFromUrlAsync(new Uri(crmurl));

            var clientcred = new ClientCredential(clientID, clientSecret);

            // CreateFromUrlAsync returns endpoint while AuthenticationContext expects authority
            // workaround is to downgrade adal to v3.19 or to strip the tail
            var auth = ap.Authority.Replace("/oauth2/authorize", "");
            var authContext = new AuthenticationContext(auth);

            var authenticationResult = await authContext.AcquireTokenAsync(crmBaseUrl, clientcred);

            return new CRMWebAPI(crmurl, authenticationResult.AccessToken);

            CRMWebAPI api = GetCRMWebAPI(log).Result;
            dynamic whoami = api.ExecuteFunction("WhoAmI").Result;
            log.LogInformation($"UserID: {whoami.UserId}");
        }

       /* private static Task<CRMGetListResult<ExpandoObject>> GetEmployees(CRMWebAPI api)
        {
            var fetchxml= @"<fetch version=""1.0"" mapping=""logical"">
   < entity name = ""kg_employee"" >
  
      < attribute name = ""kg_basicsalary"" />
   
       < attribute name = ""kg_departmentassigned"" />
    
        < attribute name = ""kg_firstname"" />
     
         < attribute name = ""kg_jobtitle"" />
      
          < attribute name = ""kg_name"" />
             </ entity >
           </ fetch > ";

            var employees = api.GetList<ExpandoObject>("kg_employee", QueryOptions: new CRMGetListOptions()
            {
                FetchXml = fetchxml
            });
            return employees;
        }   */  
    }
}
