using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace ATCommandLine
{
    public class ATCommandLineHelper : IATCommandLineHelper
    {
        private SerialPort _serialPort;
        private string[] _portsNames;
        private string[] _parities;
        private string[] _stopBits;
        private string[] _handShakes;
        private string _deviceName;

        /// <summary>
        /// Instantiates new serialPort
        /// </summary>
        public ATCommandLineHelper(string deviceName)
        {
            _deviceName = deviceName;
            _serialPort = new SerialPort();

            AppDomain.CurrentDomain.ProcessExit += currentDomain_ProcessExit;
        }

        public void ShowAvailableOptions()
        {
            Console.WriteLine($"*{DateTime.Now.ToString("dd.MM.yyyy HH:mm")}");
            Console.WriteLine(_deviceName);

            _portsNames = SerialPort.GetPortNames();
            _parities = Enum.GetNames(typeof(Parity));
            _stopBits = Enum.GetNames(typeof(StopBits));
            _handShakes = Enum.GetNames(typeof(Handshake));

            if (!_portsNames.Any())
                return;

            Console.WriteLine("\n*Available options");

            Console.WriteLine("\n Ports names:");
            foreach (var port in _portsNames)
                Console.WriteLine($"> {port}");

            Console.WriteLine("\n> Parities: ");
            foreach (var parity in _parities)
                Console.WriteLine($"> {parity}");

            Console.WriteLine("\n> StopBits: ");
            foreach (var bit in _stopBits)
                Console.WriteLine($"> {bit}");

            Console.WriteLine("\n> HandShakes: ");
            foreach (var shake in _handShakes)
                Console.WriteLine($"> {shake}");
        }

        public void SetPortSettings()
        {
            if (!_portsNames.Any())
                return;

            _serialPort = new SerialPort();
            Console.WriteLine("\n*User options below\n");

            _serialPort.PortName = SpecifyPortName();
            _serialPort.BaudRate = SpecifyBaudRate(_serialPort.BaudRate);
            _serialPort.Parity = SpecifyParity(_serialPort.Parity);
            _serialPort.DataBits = SpecifyDataBits(_serialPort.DataBits);
            _serialPort.StopBits = SpecifyStopBits(_serialPort.StopBits);
            _serialPort.Handshake = SpecifyHandShake(_serialPort.Handshake);
        }

        public void InitUserTerminal()
        {
            if (!_portsNames.Any())
            {
                Console.WriteLine("\n*No COM ports were found on this system.\n");
                return;
            }

            Console.WriteLine("\n*Call eg: call [mobileNumber]");
            Console.WriteLine("*Sms eg: sendSms [mobileNumber] [message]");
            Console.WriteLine("*Custom command eg: custom [command]");
            Console.WriteLine("\n*User commands entered below\n");
            try
            {
                _serialPort.Open();

                _serialPort.WriteLine("AT+CLIP=1");
                _serialPort.WriteLine("AT+CMGF=1");
                _serialPort.WriteLine("AT+CNMI=3,1"); // TODO - will see

                _serialPort.DataReceived += _serialPort_DataReceived;
                InitCommandWindow();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Console.WriteLine(ex.ToString());
            }
        }

        private void InitCommandWindow()
        {
            try
            {
                Console.Write("> ");
                var command = Console.ReadLine();
                var commandSplitted = command.Split(" ");

                var commandType = commandSplitted.FirstOrDefault();
                var option1 = commandSplitted.ElementAtOrDefault(1); // customCommand or mobileNumber
                var option2 = commandSplitted.ElementAtOrDefault(2); // smsMessage

                switch (commandType)
                {
                    case "call":
                        InitCallProcedure(option1);
                        break;

                    case "sendSms":

                        InitSmsProcedure(option1, option2);
                        break;

                    case "custom":

                        InitCustomCommand(option1);
                        break;

                    case "exit":
                        Environment.Exit(0);
                        break;

                    default:
                        Console.WriteLine("*Incorrect command, please check your manual");
                        InitCommandWindow();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            InitCommandWindow();
        }

        private string SpecifyPortName()
        {
            Console.Write("> Specify port name: ");
            var portName = Console.ReadLine();

            if (!_portsNames.Contains(portName))
                return SpecifyPortName();

            return portName;
        }

        private int SpecifyBaudRate(int defaultValue)
        {
            Console.Write($"> BaudRate({defaultValue}): ");
            var newValue = Console.ReadLine();
            int resultValue = 0;

            try
            {
                resultValue = string.IsNullOrWhiteSpace(newValue)
                ? defaultValue
                : Convert.ToInt32(newValue);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return SpecifyBaudRate(defaultValue);
            }

            if (resultValue < 1)
                return SpecifyBaudRate(defaultValue);

            return resultValue;
        }

        private Parity SpecifyParity(Parity defaultParity)
        {
            string newParity = null;

            Console.Write($"> Parity({defaultParity}): ");
            newParity = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(newParity))
                return defaultParity;

            if (!_parities.Contains(newParity))
                return SpecifyParity(defaultParity);

            return (Parity)Enum.Parse(typeof(Parity), newParity, false);
        }

        private int SpecifyDataBits(int defaultValue)
        {
            Console.Write($"> StopBits({defaultValue})[5-8]: ");
            var newValue = Console.ReadLine();
            int resultValue = 0;

            try
            {
                resultValue = string.IsNullOrWhiteSpace(newValue)
                ? defaultValue
                : Convert.ToInt32(newValue);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return SpecifyDataBits(defaultValue);
            }

            if (resultValue < 5 || resultValue > 8)
                return SpecifyDataBits(defaultValue);

            return resultValue;
        }

        private StopBits SpecifyStopBits(StopBits defaultBits)
        {
            string newStopBits = null;

            Console.Write($"> StopBits({defaultBits}): ");
            newStopBits = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(newStopBits))
                return defaultBits;

            if (!_stopBits.Contains(newStopBits))
                return SpecifyStopBits(defaultBits);

            return (StopBits)Enum.Parse(typeof(StopBits), newStopBits, false);
        }

        private Handshake SpecifyHandShake(Handshake defaultShake)
        {
            string newShake = null;

            Console.Write($"> HandShake({defaultShake}): ");
            newShake = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(newShake))
                return defaultShake;

            if (!_handShakes.Contains(newShake))
                return SpecifyHandShake(defaultShake);

            return (Handshake)Enum.Parse(typeof(Handshake), newShake, false);
        }

        private void InitCallProcedure(string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
            {
                Console.WriteLine("*Incorrect mobile number");
                InitCommandWindow();
            }
            _serialPort.WriteLine($"AT+CHUP"); // hanging up calls if any
            Thread.Sleep(1000);
            _serialPort.WriteLine($"ATD{mobileNumber};");
        }

        private void InitSmsProcedure(string mobileNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber) || string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine("*Incorrect sms number or/and message");
                InitCommandWindow();
            }

            _serialPort.WriteLine($"AT+CHUP"); // hanging up calls if any
            Thread.Sleep(1000);

            _serialPort.WriteLine($"AT+CMGS=\"{mobileNumber}\"");
            Thread.Sleep(1000);
            _serialPort.WriteLine(message);
        }

        private void InitCustomCommand(string customCommand)
        {
            if (string.IsNullOrWhiteSpace(customCommand))
            {
                Console.WriteLine("*Incorrect custom command");
                InitCommandWindow();
            }

            _serialPort.WriteLine($"{customCommand}");
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;
            var indata = serialPort.ReadExisting();
            Console.Write("*Data Received:");
            Console.WriteLine(indata);
        }

        private void currentDomain_ProcessExit(object sender, EventArgs e)
        {
            _serialPort?.Close();
            _serialPort?.Dispose();
            _serialPort = null;
        }
    }
}