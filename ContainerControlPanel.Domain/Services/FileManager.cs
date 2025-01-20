using ContainerControlPanel.Domain.Methods;
using ContainerControlPanel.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace ContainerControlPanel.Domain.Services;

/// <summary>
/// Class to manage files
/// </summary>
public class FileManager
{
    /// <summary>
    /// Searches for Docker Compose files in a directory
    /// </summary>
    /// <param name="directoryPath">Directory path</param>
    /// <param name="searchSubdirectories">Indicates whether to search subdirectories</param>
    /// <returns>Returns a list of <see cref="ComposeFile"/> objects</returns>
    /// <exception cref="Exception">Thrown when there is an error while searching for files</exception>
    public async static Task<List<ComposeFile>> SearchComposeFiles(string directoryPath, bool searchSubdirectories = true)
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

    /// <summary>
    /// Searches for image files in a directory
    /// </summary>
    /// <param name="directoryPath">Path to the directory</param>
    /// <param name="searchSubdirectories">Indicates whether to search subdirectories</param>
    /// <returns>Returns a list of <see cref="ImageFile"/> objects</returns>
    /// <exception cref="Exception">Thrown when there is an error while searching for files</exception>
    public async static Task<List<ImageFile>> SearchImageFiles(string directoryPath, bool searchSubdirectories = true)
    {
        List<ImageFile> imageFiles = new List<ImageFile>();

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory {directoryPath} does not exist.");
                return imageFiles;
            }

            var files = Directory.GetFiles(directoryPath, "*",
                searchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                if (Path.GetExtension(file) != ".tar")
                {
                    continue;
                }

                try
                {
                    imageFiles.Add(new ImageFile
                    {
                        FilePath = file,
                        FileName = Path.GetFileName(file),
                        Size = $"{new FileInfo(file).Length / (1024 * 1024)}MB"
                    });
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return imageFiles;
        }
        catch (Exception ex)
        {
            throw new Exception($"There was an error while searching for files: {ex.Message}");
        }
    }

    /// <summary>
    /// Writes the content to a file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="content">File content</param>
    /// <returns>Returns the result of the operation</returns>
    /// <exception cref="Exception">Thrown when there is an error while writing the file content</exception>
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

    /// <summary>
    /// Uploads a file
    /// </summary>
    /// <param name="directoryPath">Path to the directory</param>
    /// <param name="file">File to upload</param>
    /// <returns>Returns the result of the operation</returns>
    /// <exception cref="Exception">Thrown when there is an error while uploading the file</exception>
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

    /// <summary>
    /// Uploads a single chunk of a file
    /// </summary>
    /// <param name="directoryPath">Path to the directory</param>
    /// <param name="chunk">Chunk to upload</param>
    /// <returns>Returns the result of the operation</returns>
    /// <exception cref="Exception">Thrown when there is an error while uploading the chunk</exception>
    public async static Task UploadChunk(string directoryPath, IFormFile chunk)
    {
        try
        {
            using (var fileStream = new FileStream(directoryPath + $"\\{chunk.FileName}", FileMode.Create))
            {
                await chunk.CopyToAsync(fileStream);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"There was an error while uploading the chunk: {ex.Message}");
        }
    }

    /// <summary>
    /// Merges the chunks of a file into a single file
    /// </summary>
    /// <param name="directoryPath">Path to the directory</param>
    /// <param name="fileName">Name of the file</param>
    /// <returns>Returns the result of the operation</returns>
    /// <exception cref="Exception">Thrown when there is an error while merging the chunks</exception>
    public async static Task MergeChunks(string directoryPath, string fileName)
    {
        try
        {
            var chunkFiles = Directory.GetFiles(directoryPath, $"{fileName}.part*")
                .OrderBy(f => int.Parse(Path.GetFileName(f).Split("part")[1]))
                .ToArray();
            var filePath = Path.Combine(directoryPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);

            foreach (var chunkFile in chunkFiles)
            {
                using var chunkStream = new FileStream(chunkFile, FileMode.Open);
                await chunkStream.CopyToAsync(stream);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"There was an error while merging the chunks: {ex.Message}");
        }
        finally
        {
            foreach (var chunkFile in Directory.GetFiles(directoryPath, $"{fileName}.part*"))
            {
                File.Delete(chunkFile);
            }
        }
    }
}
