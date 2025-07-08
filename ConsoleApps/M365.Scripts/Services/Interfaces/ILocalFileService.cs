namespace Services.Interfaces
{
    public interface ILocalFileService
    {
        List<string> ReadLinesFromFile(string filePathWithExtension);
    }
}
