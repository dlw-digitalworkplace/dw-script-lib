using Services.Interfaces;

namespace Services.Services
{
    public class LocalFileService : ILocalFileService
    {
        /// <summary>
        /// Read all lines from a file located on the given filepath and returns it as a list of text (1 line is 1 list item).
        /// </summary>
        /// <param name="filePathWithExtension"></param>
        /// <returns></returns>
        public List<string> ReadLinesFromFile(string filePathWithExtension)
        {
            var returnList = new List<string>();
            try
            {
                using (var reader = new StreamReader(filePathWithExtension))
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        returnList.Add(line);
                        line = reader.ReadLine();
                    }
                    reader.Close();
                }
                return returnList;
            }
            catch (Exception ex)
            {
                throw new FileLoadException($"Something went wrong while loading the file from ${filePathWithExtension}", ex);
            }
        }
    }
}
