using System;
using System.IO.Ports;
using System.Management;

namespace ATCommandLine
{
    class Program
    {
        private static IATCommandLineHelper _helper;
        static void Main(string[] args)
        {
            _helper = new ATCommandLineHelper("---------------- Siemens M20 / M20 Terminal ----------------\n");
            _helper.ShowAvailableOptions();
            _helper.SetPortSettings();
            _helper.InitUserTerminal();
           
            Console.ReadLine();
        }
    }
}
