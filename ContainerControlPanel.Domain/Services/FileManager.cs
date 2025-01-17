using ContainerControlPanel.Domain.Methods;
using ContainerControlPanel.Domain.Models;
using Microsoft.AspNetCore.Http;

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
                if (Path.GetExtension(file) != ".yml")
                {
                    continue;
                }

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

    public async static Task UploadFile(string directoryPath, IFormFile file)
    {
        try
        {
            var filePath = Path.Combine(directoryPath, file.FileName);
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            await File.WriteAllTextAsync(filePath, content);
        }
        catch (Exception ex)
        {
            throw new Exception($"There was an error while uploading the file: {ex.Message}");
        }
    }
}
