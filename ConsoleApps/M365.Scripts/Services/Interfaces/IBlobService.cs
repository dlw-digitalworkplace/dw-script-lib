namespace Services.Interfaces
{
    public interface IBlobService
    {
        List<string> GetFileContent(string containerName, string filename);
    }
}
