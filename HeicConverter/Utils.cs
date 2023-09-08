using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace HeicConverter
{
    public static class Utils
    {
        public static string RememberStorageItem(IStorageItem file, string token = null)
        {
            string itemToken = token == null ? $"{file.Name}_{Guid.NewGuid().ToString()}" : token;
            StorageApplicationPermissions.FutureAccessList.AddOrReplace(itemToken, file);
            return itemToken;
        }

        public static async Task<StorageFile> GetFileForToken(string token)
        {
            if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(token)) return null;
            return await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
        }

        public static async Task<StorageFolder> GetFolderForToken(string token)
        {
            if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(token)) return null;
            return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
        }
    }
}
