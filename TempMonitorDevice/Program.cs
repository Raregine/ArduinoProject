using Microsoft.Azure.Devices.Client;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;

namespace TempMonitorDevice
{
    class Program
    {
        private static DeviceClient s_deviceClient;
        private static readonly TransportType s_transportType = TransportType.Mqtt;
        private static string s_connectionString = "HostName=iotc-c8d20ea0-4237-44fa-ae01-ad55d74e34af.azure-devices.net;DeviceId=1z1gyquvu0r;SharedAccessKey=8wL1iICwr0G587ecoYA9e6vI+ZMUfvzsUKBYWjJ6yk4=";
        static SerialPort _serialPort = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
        static async Task Main(string[] args)
        {

            //Thread readThread = new Thread(Read);
            _serialPort.Open();
            //readThread.Start();

            // Connect to the IoT hub using the MQTT protocol

            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, s_transportType);

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
            double minTemperature = 20;
            double minHumidity = 60;
            var rand = new Random();

            while (!ct.IsCancellationRequested)
            {
                //double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;

                double currentTemperature = Read();
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

        public static double Read()
        {
            double reading; 
            string message = _serialPort.ReadLine();
            Console.WriteLine(message);
            reading = Convert.ToDouble(message);

            return reading; 

        }


    }
}


