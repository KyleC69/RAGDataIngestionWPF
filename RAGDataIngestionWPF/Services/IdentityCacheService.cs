// 2026/03/08
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         IdentityCacheService.cs
//   Author: Kyle L. Crowder



using System.IO;
using System.Reflection;
using System.Security.Cryptography;

using RAGDataIngestionWPF.Core.Contracts.Services;




namespace RAGDataIngestionWPF.Services;





internal class IdentityCacheService : IIdentityCacheService
{
    private readonly Lock _fileLock = new();
    public static readonly string MsalCacheFileName = ".msalcache.bin3";
    public static readonly string MsalCacheFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{Assembly.GetExecutingAssembly().GetName().Name}";








    public byte[] ReadMsalToken()
    {
        lock (_fileLock)
        {
            string filePath = Path.Combine(MsalCacheFilePath, MsalCacheFileName);
            if (File.Exists(filePath))
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                return ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            }

            return default;
        }
    }








    public void SaveMsalToken(byte[] token)
    {
        lock (_fileLock)
        {
            if (!Directory.Exists(MsalCacheFilePath))
            {
                DirectoryInfo unused = Directory.CreateDirectory(MsalCacheFilePath);
            }

            byte[] encryptedData = ProtectedData.Protect(token, null, DataProtectionScope.CurrentUser);
            string filePath = Path.Combine(MsalCacheFilePath, MsalCacheFileName);
            File.WriteAllBytes(filePath, encryptedData);
        }
    }
}