using ContainerControlPanel.Domain.Methods;
using ContainerControlPanel.Domain.Models;

namespace ContainerControlPanel.Domain.Services;

public class FileManager
{
    public async static Task<List<ComposeFile>> SearchFiles(string directoryPath, bool searchSubdirectories = true)
    {
        List<ComposeFile> composeFiles = new List<ComposeFile>();

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory {directoryPath} does not exist.");
                return composeFiles;
            }

            var files = Directory.GetFiles(directoryPath, "*",
                searchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                try
                {
                    composeFiles.Add(new ComposeFile
                    {
                        FilePath = file,
                        FileName = Path.GetFileName(file),
                        FileContent = await File.ReadAllTextAsync(file),
                        ServiceNames = Parser.GetServiceNames(file)
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return composeFiles;
        }
        catch (Exception ex)
        {
            throw new Exception($"There was an error while searching for files: {ex.Message}");
        }
    }

    public async static Task WriteFileContent(string filePath, string content)
    {
        try
        {
            await File.WriteAllTextAsync(filePath, content);
        }
        catch (Exception ex)
        {
            throw new Exception($"There was an error while writing the file content: {ex.Message}");
        }
    }
}
