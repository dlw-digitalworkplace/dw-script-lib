using CommandLine;
using Manage_ExternalGraphConnector.Console;

namespace Manage_ExternalGraphConnector.Models
{
    public class ScriptOptions
    {
        [Option('c', "clientId", Required = false, HelpText = "The client id used to connect to the key vault")]
        public string ClientId { get; set; }

        [Option('s', "clientSecret", Required = false, HelpText = "The client secret to connect to the key vault")]
        public string ClientSecret { get; set; }

        [Option('t', "tenantId", Required = false, HelpText = "The Id of the tenant where the key vault is hosted")]
        public string TenantId { get; set; }

        [Option(Default = false, HelpText = "Use debug flag to load secrets from local secrets file instead of the Key Vault")]
        public bool Debug { get; set; }

        public static ScriptOptions? RunOptionsAndReturnExitCode(ScriptOptions options)
        {
            if (!options.Debug)
            {
                if (string.IsNullOrEmpty(options.ClientId))
                {
                    Output.WriteLine(Output.Error, "If not in debug the client id is required!");
                    return null;
                }
                else if (string.IsNullOrEmpty(options.ClientSecret))
                {
                    Output.WriteLine(Output.Error, "If not in debug the client secret name is required!");
                    return null;
                }
                else if (string.IsNullOrEmpty(options.TenantId))
                {
                    Output.WriteLine(Output.Error, "If not in debug the tenant id is required!");
                    return null;
                }
            }
            Output.WriteLine(Output.Success, "Successfully parsed input arguments");
            return options;
        }

        public static ScriptOptions? HandleParseError(IEnumerable<Error> errs)
        {
            return null;
        }
    }
}