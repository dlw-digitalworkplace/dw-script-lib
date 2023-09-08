using CommandLine;
using Manage_ExternalGraphConnector.Console;
using Manage_ExternalGraphConnector.Models;
using Manage_ExternalGraphConnector.Schemas;
using Manage_ExternalGraphConnector.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Models.ExternalConnectors;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;

IGraphService _graphService;
ExternalConnection _currentConnection = null;

// Check script options
var scriptOptions = Parser.Default.ParseArguments<ScriptOptions>(args).MapResult(
    ScriptOptions.RunOptionsAndReturnExitCode,
    ScriptOptions.HandleParseError
);
if (scriptOptions == null) { return; };

// Load app settings
var appConfig = LoadAppSettings(scriptOptions);

try
{
    // Initialize the graph service
    _graphService = new GraphService(appConfig.Get<Settings>());

    do
    {
        MenuChoice userChoice = DoMenuPrompt();

        switch (userChoice)
        {
            case MenuChoice.CreateConnection:
                await CreateConnectionAsync();
                break;
            case MenuChoice.ChooseExistingConnection:
                await SelectExistingConnectionAsync();
                break;
            case MenuChoice.DeleteConnection:
                await DeleteCurrentConnectionAsync();
                break;
            case MenuChoice.RegisterSchema:
                await RegisterSchemaAsync();
                break;
            case MenuChoice.ViewSchema:
                await GetSchemaAsync();
                break;
            case MenuChoice.Exit:
                Output.WriteLine("Goodbye...");
                return;
            case MenuChoice.Invalid:
            default:
                Output.WriteLine(Output.Warning, "Invalid choice! Please try again.");
                break;
        }
    } while (true);
}
catch (Exception ex)
{
    Output.WriteLine(Output.Error, "An unexpected exception occurred.");
    Output.WriteLine(Output.Error, ex.Message);
    Output.WriteLine(Output.Error, ex.StackTrace);
}

#region Action methods
async Task SelectExistingConnectionAsync()
{
    Output.WriteLine(Output.Info, "Getting existing connections...");
    try
    {
        // Get connections
        var connections = (await _graphService.GetExistingConnectionsAsync()).ToList();

        if (connections.Count <= 0)
        {
            Output.WriteLine(Output.Warning, "No connections exist. Please create a new connection.");
            return;
        }

        Output.WriteLine(Output.Info, "Choose one of the following connections:");
        int menuNumber = 1;
        foreach (var connection in connections)
        {
            Output.WriteLine($"{menuNumber++}. {connection.Name}");
        }

        ExternalConnection selectedConnection = null;

        do
        {
            try
            {
                Output.Write(Output.Info, "Selection: ");
                var choice = int.Parse(System.Console.ReadLine());

                if (choice > 0 && choice <= connections.Count)
                {
                    selectedConnection = connections[choice - 1];
                }
                else
                {
                    Output.WriteLine(Output.Warning, "Invalid choice.");
                }
            }
            catch (FormatException)
            {
                Output.WriteLine(Output.Warning, "Invalid choice.");
            }
        } while (selectedConnection == null);

        _currentConnection = selectedConnection;
    }
    catch (Exception ex)
    {
        Output.WriteLine(Output.Error, $"Error getting connections:");
        Output.WriteLine(Output.Error, ex.Message);
        return;
    }
}
async Task CreateConnectionAsync()
{
    var connectionId = PromptForInput("Enter a unique ID for the new connection", true);
    var connectionName = PromptForInput("Enter a name for the new connection", true);
    var connectionDescription = PromptForInput("Enter a description for the new connection", false);

    try
    {
        // Create the connection
        _currentConnection = await _graphService.CreateConnectionAsync(connectionId, connectionName, connectionDescription);
        Output.WriteLine(Output.Success, "New connection created");
        Output.WriteObject(Output.Info, _currentConnection);
    }
    catch (Exception ex)
    {
        Output.WriteLine(Output.Error, $"Error creating new connection:");
        Output.WriteLine(Output.Error, ex.Message);
        return;
    }
}
async Task RegisterSchemaAsync()
{
    if (_currentConnection == null)
    {
        Output.WriteLine(Output.Warning, "No connection selected. Please create a new connection or select an existing connection.");
        return;
    }

    // Select Schema
    var schemaAssebmlies = Assembly.GetExecutingAssembly().GetTypes().Where(t =>
        t.FullName.StartsWith("Manage_ExternalGraphConnector.Schemas") &&
        !t.Name.Equals(nameof(SchemaBase))
    );
    Output.WriteLine(Output.Info, "Choose one of the following schemas:");

    int menuNumber = 1;
    foreach (var schemaAssembly in schemaAssebmlies)
    {
        Output.WriteLine($"{menuNumber++}. {schemaAssembly.Name}");
    }

    Type assymbly = null;
    do
    {
        try
        {
            Output.Write(Output.Info, "Selection: ");
            var choice = int.Parse(Console.ReadLine());

            if (choice > 0 && choice <= schemaAssebmlies.Count())
            {
                assymbly = schemaAssebmlies.ElementAt(choice - 1);
            }
            else
            {
                Output.WriteLine(Output.Warning, "Invalid choice.");
            }
        }
        catch (FormatException)
        {
            Output.WriteLine(Output.Warning, "Invalid choice.");
        }
    } while (assymbly == null);

    var imecSchema = (SchemaBase)Activator.CreateInstance(assymbly);
    var schemaProps = imecSchema.GetSchemaProperties();

    Output.WriteLine(Output.Info, "Registering schema, this may take a moment...");

    try
    {
        // Register the schema
        var schema = new Schema
        {
            BaseType = "microsoft.graph.externalItem",
            Properties = schemaProps
        };

        await _graphService.RegisterSchemaAsync(_currentConnection.Id, schema);
        Output.WriteLine(Output.Success, "Schema registered");
    }
    catch (Exception ex)
    {
        Output.WriteLine(Output.Error, $"Error registering schema:");
        Output.WriteLine(Output.Error, ex.Message);
        return;
    }
}
async Task GetSchemaAsync()
{
    if (_currentConnection == null)
    {
        Output.WriteLine(Output.Warning, "No connection selected. Please create a new connection or select an existing connection.");
        return;
    }

    try
    {
        var schema = await _graphService.GetSchemaAsync(_currentConnection.Id);
        Output.WriteObject(Output.Info, schema);
    }
    catch (Exception ex)
    {
        Output.WriteLine(Output.Error, $"Error getting schema:");
        Output.WriteLine(Output.Error, ex.Message);
        return;
    }
}
async Task DeleteCurrentConnectionAsync()
{
    if (_currentConnection == null)
    {
        Output.WriteLine(Output.Warning, "No connection selected. Please create a new connection or select an existing connection.");
        return;
    }

    Output.WriteLine(Output.Warning, $"Deleting {_currentConnection.Name} - THIS CANNOT BE UNDONE");
    Output.WriteLine(Output.Warning, "Enter the connection name to confirm.");

    var input = Console.ReadLine();

    if (input != _currentConnection.Name)
    {
        Output.WriteLine(Output.Warning, "Canceled");
        return;
    }

    try
    {
        await _graphService.DeleteConnectionAsync(_currentConnection.Id);
        Output.WriteLine(Output.Success, $"{_currentConnection.Name} deleted");
        _currentConnection = null;
    }
    catch (Exception ex)
    {
        Output.WriteLine(Output.Error, $"Error deleting connection:");
        Output.WriteLine(Output.Error, ex.Message);
        return;
    }
}
#endregion

#region Private methods
MenuChoice DoMenuPrompt()
{
    Output.WriteLine(Output.Info, $"Current connection: {(_currentConnection == null ? "NONE" : _currentConnection.Name)}");
    Output.WriteLine(Output.Info, "Please choose one of the following options:");

    Output.WriteLine($"{Convert.ToInt32(MenuChoice.CreateConnection)}. Create a connection");
    Output.WriteLine($"{Convert.ToInt32(MenuChoice.ChooseExistingConnection)}. Select an existing connection");
    Output.WriteLine($"{Convert.ToInt32(MenuChoice.DeleteConnection)}. Delete current connection");
    Output.WriteLine($"{Convert.ToInt32(MenuChoice.RegisterSchema)}. Register schema for current connection");
    Output.WriteLine($"{Convert.ToInt32(MenuChoice.ViewSchema)}. View schema for current connection");
    Output.WriteLine($"{Convert.ToInt32(MenuChoice.Exit)}. Exit");

    try
    {
        var choice = int.Parse(Console.ReadLine());
        return (MenuChoice)choice;
    }
    catch (FormatException)
    {
        return MenuChoice.Invalid;
    }
}
string PromptForInput(string prompt, bool valueRequired)
{
    string response = null;

    do
    {
        Output.WriteLine(Output.Info, $"{prompt}:");
        response = Console.ReadLine();
        if (valueRequired && string.IsNullOrEmpty(response))
        {
            Output.WriteLine(Output.Error, "You must provide a value");
        }
    } while (valueRequired && string.IsNullOrEmpty(response));

    return response;
}
IConfigurationRoot LoadAppSettings(ScriptOptions scriptOptions)
{
    if (scriptOptions.Debug)
    {
        // Add from user secrets in debug
        return new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();
    }
    else
    {
        var settings = new Settings
        {
            AzureAd = new()
            {
                ClientId = scriptOptions.ClientId,
                ClientSecret = scriptOptions.ClientSecret,
                TenantId = scriptOptions.TenantId
            }
        };
        return new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(settings)))).Build();
    }
}
#endregion