using System;
using System.ServiceProcess;
using System.Text.RegularExpressions;

namespace hcGate
{
    static class Program
    {
        static void Main(string[] args)
        {
            hcGate service = new hcGate();

            if (args.Length > 0)
            {
                if (Regex.IsMatch(args[0], "^[/|-]?console$", RegexOptions.IgnoreCase))
                {
                    service.ConsoleMode = true;
                }
                else if (Regex.IsMatch(args[0], "^[/|-]?autonomous$", RegexOptions.IgnoreCase))
                {
                    service.ConsoleMode = true;
                    service.Autonomous = true;
                }
            }

            service.ReadSettings();

            if (!service.ConsoleMode)
            {
                ServiceBase.Run(service);
            }
            else
            {
                service.ConsoleStart();
                Console.ReadLine();
            }
        }
    }
}