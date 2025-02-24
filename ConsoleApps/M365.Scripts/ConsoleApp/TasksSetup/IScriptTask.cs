namespace ConsoleApp.TasksSetup
{
    /// <summary>
    /// This interface is being used for the setup of the keyed services and to have the same structure for the execution methode.
    /// </summary>
    internal interface IScriptTask
    {
        Task RunAsync();
    }
}
