using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace Kratos
{
    public class CommandLineArguments
    {
        [Option('t', "token", HelpText = "Your bot's token", Required = false)]
        public string Token { get; set; }

        [Option('l', "logtofile", HelpText = "Log the console output to a file as well", Default = false)]
        public bool LogToFile { get; set; }
    }
}
