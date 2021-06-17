using Microsoft.Azure.Devices.Client;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;

namespace TriggerTestDevice
{
    class Program
    {
        private static DeviceClient s_deviceClient;
        private static readonly TransportType s_transportType = TransportType.Mqtt;
        private static string s_connectionString = "HostName=dev-iotsolution-iothub.azure-devices.net;DeviceId=TriggerTestDevice;SharedAccessKey=qRVNUAETfPKdaAOmKGcskh49vr8ZFvoey/9gs3lnCPs=";

        private static double minTemperature = 20;
        static async Task Main(string[] args)
        {

            // Connect to the IoT hub using the MQTT protocol
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, s_transportType);
            // Create a handler for the direct method call
            s_deviceClient.SetMethodHandlerAsync("SetFanState", SetFanState, null).Wait();

            // Set up a condition to quit the sample
            Console.WriteLine("Press control-C to exit.");
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            // Run the telemetry loop
            await SendDeviceToCloudMessagesAsync(cts.Token);

            // SendDeviceToCloudMessagesAsync is designed to run until cancellation has been explicitly requested by Console.CancelKeyPress.
            // As a result, by the time the control reaches the call to close the device client, the cancellation token source would
            // have already had cancellation requested.
            // Hence, if you want to pass a cancellation token to any subsequent calls, a new token needs to be generated.
            // For device client APIs, you can also call them without a cancellation token, which will set a default
            // cancellation timeout of 4 minutes: https://github.com/Azure/azure-iot-sdk-csharp/blob/64f6e9f24371bc40ab3ec7a8b8accbfb537f0fe1/iothub/device/src/InternalClient.cs#L1922
            await s_deviceClient.CloseAsync();

            s_deviceClient.Dispose();
            Console.WriteLine("Device simulator finished.");
        }

        // Async method to send simulated telemetry
        private static async Task SendDeviceToCloudMessagesAsync(CancellationToken ct)
        {
            // Initial telemetry values
            //double minTemperature = 20;
            double minHumidity = 60;
            var rand = new Random();

            while (!ct.IsCancellationRequested)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;


                // Create JSON message
                string messageBody = JsonSerializer.Serialize(
                    new
                    {
                        RoomTemperature = currentTemperature,
                        humidity = currentHumidity,
                    });
                using var message = new Message(Encoding.ASCII.GetBytes(messageBody))
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8",
                };

                // Add a custom application property to the message.
                // An IoT hub can filter on these properties without access to the message body.
                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                // Send the telemetry message
                await s_deviceClient.SendEventAsync(message);
                Console.WriteLine($"{DateTime.Now} > Sending message: {messageBody}");

                await Task.Delay(1000);
            }
        }

        // Handle the direct method call
        private static Task<MethodResponse> SetFanState(MethodRequest methodRequest, object userContext)
        {

            var data = Encoding.UTF8.GetString(methodRequest.Data);
            
            // Remove quotes from data.
            data = data.Replace("\"", "");
            minTemperature = Convert.ToDouble(data); 
            ConsoleHelper.WriteGreenMessage("Min Temp set to: " + data);
            string result = "{\"result\":\"Executed\"}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
            // if (cheeseCave.FanState == StateEnum.Failed)
            // {
            //     // Acknowledge the direct method call with a 400 error message.
            //     string result = "{\"result\":\"Fan failed\"}";
            //     ConsoleHelper.WriteRedMessage("Direct method failed: " + result);
            //     return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 400));
            // }
            // else
            // {
            //     try
            //     {
            //         var data = Encoding.UTF8.GetString(methodRequest.Data);

            //         // Remove quotes from data.
            //         data = data.Replace("\"", "");

            //         // Parse the payload, and trigger an exception if it's not valid.
            //         cheeseCave.UpdateFan((StateEnum)Enum.Parse(typeof(StateEnum), data));
            //         ConsoleHelper.WriteGreenMessage("Fan set to: " + data);

            //         // Acknowledge the direct method call with a 200 success message.
            //         string result = "{\"result\":\"Executed direct method: " + methodRequest.Name + "\"}";
            //         return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
            //     }
            //     catch
            //     {
            //         // Acknowledge the direct method call with a 400 error message.
            //         string result = "{\"result\":\"Invalid parameter\"}";
            //         ConsoleHelper.WriteRedMessage("Direct method failed: " + result);
            //         return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 400));
            //     }
            // }
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


