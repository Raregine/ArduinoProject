using System;
using System.IO.Ports;
using System.Threading;

namespace ReadSerial
{
    public class Program
    {
        static bool _continue;
        // all of the options for a serial device  
        // ---- can be sent through the constructor of the SerialPort class  
        // ---- PortName = "COM1", Baud Rate = 19200, Parity = None,  
        // ---- Data Bits = 8, Stop Bits = One, Handshake = None  
        static SerialPort _serialPort = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
        public static void Main()
        {
            Thread readThread = new Thread(Read);
            _serialPort.Open();
            _continue = true;
           readThread.Start();

          

        }

        public static void Read()
        {
            while (_continue)
            {
                try
                {
                    string message = _serialPort.ReadLine();
                    Console.WriteLine(message);
                }
                catch (TimeoutException) { }
            }
        }

    }
}
