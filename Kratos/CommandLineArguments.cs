using CommandLine;

namespace Kratos
{
    public class CommandLineArguments
    {
        [Option('t', "token", HelpText = "Your bot's token", Required = false)]
        public string Token { get; set; }
    }
}
