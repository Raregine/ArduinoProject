// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// INSERT using statements below here
using System;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Azure.EventHubs;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;

namespace CheeseCaveOperator
{
    class Program
    {
        // INSERT variables below here
        private static ServiceClient serviceClient;
        private readonly static string serviceConnectionString = "HostName=dev-iotsolution-iothub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=ntT8zdq6mr2HV5iFwyOJeLrgUN5k71XSixzJsrrQwss=";

        private readonly static string deviceId = "TriggerTestDevice";
        // INSERT Main method below here
        public static void Main(string[] args)
        {
            ConsoleHelper.WriteColorMessage("Cheese Cave Operator\n", ConsoleColor.Yellow);


            // INSERT create service client instance below here
            // Create a ServiceClient to communicate with service-facing endpoint on your hub.
            serviceClient = ServiceClient.CreateFromConnectionString(serviceConnectionString);
            InvokeMethod().GetAwaiter().GetResult();

        }

        // INSERT ReceiveMessagesFromDeviceAsync method below here

        // Handle invoking a direct method.
        private static async Task InvokeMethod()
        {
            try
            {
                var methodInvocation = new CloudToDeviceMethod("SetFanState") { ResponseTimeout = TimeSpan.FromSeconds(30) };
                string payload = JsonConvert.SerializeObject("On");

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

        // INSERT Device twins section below here
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