using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace HeicConverter
{
    public static class Utils
    {
        // FutureAccessList has a limit of 1000 entries!!
        public static string RememberStorageItem(IStorageItem file, string token = null)
        {
            string itemToken = token == null ? $"{file.Name}_{Guid.NewGuid().ToString()}" : token;
            StorageApplicationPermissions.FutureAccessList.AddOrReplace(itemToken, file);
            return itemToken;
        }

        public static void ForgetFileToken(string token)
        {
            if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(token)) return;
            StorageApplicationPermissions.FutureAccessList.Remove(token);
        }

        public static void ClearFutureAccessList()
        {
            StorageApplicationPermissions.FutureAccessList.Clear();
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

        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int itemsPerSet)
        {
            var sourceList = source as List<T> ?? source.ToList();
            for (var index = 0; index < sourceList.Count; index += itemsPerSet)
            {
                yield return sourceList.Skip(index).Take(itemsPerSet);
            }
        }
    }
}
