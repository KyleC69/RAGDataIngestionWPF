// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using System.Text;

using Newtonsoft.Json;

using RAGDataIngestionWPF.Core.Contracts.Services;




namespace RAGDataIngestionWPF.Core.Services;





public sealed class FileService : IFileService
    {
    public T Read<T>(string folderPath, string fileName)
        {
        var path = Path.Combine(folderPath, fileName);
        if (File.Exists(path))
            {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
            }

        return default;
        }








    public void Save<T>(string folderPath, string fileName, T content)
        {
        if (!Directory.Exists(folderPath))
            {
            _ = Directory.CreateDirectory(folderPath);
            }

        var fileContent = JsonConvert.SerializeObject(content);
        File.WriteAllText(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
        }








    public void Delete(string folderPath, string fileName)
        {
        if (fileName != null && File.Exists(Path.Combine(folderPath, fileName)))
            {
            File.Delete(Path.Combine(folderPath, fileName));
            }
        }
    }