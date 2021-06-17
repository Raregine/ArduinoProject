using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Devices;

namespace Company.Function
{
    public static class MyTestTrigger
    {
        private static ServiceClient serviceClient;
        private readonly static string serviceConnectionString = "HostName=dev-iotsolution-iothub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ntT8zdq6mr2HV5iFwyOJeLrgUN5k71XSixzJsrrQwss=";
        private readonly static string deviceId = "TriggerTestDevice";

        [FunctionName("MyTestTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            

            string temp = req.Query["temperature"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            temp = temp ?? data?.name;


             // Create a ServiceClient to communicate with service-facing endpoint on your hub.
            serviceClient = ServiceClient.CreateFromConnectionString(serviceConnectionString);
            InvokeMethod(Convert.ToDouble(temp)).GetAwaiter().GetResult();

            string responseMessage = string.IsNullOrEmpty(temp)
                ? "This HTTP triggered function executed successfully."
                : $"The min Temp will be set to {temp}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        private static async Task InvokeMethod(double minTempSetting)
        {
            try
            {
                var methodInvocation = new CloudToDeviceMethod("SetFanState") { ResponseTimeout = TimeSpan.FromSeconds(30) };
                string payload = JsonConvert.SerializeObject(minTempSetting);

                methodInvocation.SetPayloadJson(payload);

                // Invoke the direct method asynchronously and get the response from the simulated device.
                var response = await serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);

                if (response.Status == 200)
                {
                    ConsoleHelper.WriteGreenMessage("Direct method invoked: " + response.GetPayloadAsJson());
                }
                else
                {
                    ConsoleHelper.WriteRedMessage("Direct method failed: " + response.GetPayloadAsJson());
                }
            }
            catch
            {
                ConsoleHelper.WriteRedMessage("Direct method failed: timed-out");
            }
        }
    }

    internal static class ConsoleHelper
    {
        internal static void WriteColorMessage(string text, ConsoleColor clr)
        {
            Console.ForegroundColor = clr;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        internal static void WriteGreenMessage(string text)
        {
            WriteColorMessage(text, ConsoleColor.Green);
        }

        internal static void WriteRedMessage(string text)
        {
            WriteColorMessage(text, ConsoleColor.Red);
        }
    }
}
