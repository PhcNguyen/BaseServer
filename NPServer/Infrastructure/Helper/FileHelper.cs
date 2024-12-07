﻿using NPServer.Infrastructure.Configuration.Default;
using System.IO;
using System.Text.Json;

namespace NPServer.Infrastructure.Helper
{
    /// <summary>
    /// Makes it easier to load and save files.
    /// </summary>
    public static class FileHelper
    {
        public static readonly string ServerRoot = PathConfig.Base;
        public static readonly string DataDirectory = PathConfig.DataDirectory;

        /// <summary>
        /// Returns a path relative to server root directory.
        /// </summary>
        public static string GetRelativePath(string filePath)
        {
            return Path.GetRelativePath(ServerRoot ?? "", filePath);
        }

        /// <summary>
        /// Deserializes a <typeparamref name="T"/> from a JSON file located at the specified path.
        /// </summary>
        public static T? DeserializeJson<T>(string path, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path), options);
        }

        /// <summary>
        /// Serializes a <typeparamref name="T"/> to JSON and saves it to the specified path.
        /// </summary>
        public static void SerializeJson<T>(string path, T @object, JsonSerializerOptions? options = null)
        {
            string? dirName = Path.GetDirectoryName(path);
            if (Directory.Exists(dirName) == false && dirName != null)
                Directory.CreateDirectory(dirName);

            string json = JsonSerializer.Serialize(@object, options);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Saves the provided <see cref="string"/> to a text file in the server root directory.
        /// </summary>
        public static void SaveTextFileToRoot(string fileName, string text)
        {
            File.WriteAllText(Path.Combine(ServerRoot, fileName), text);
        }

        public static bool CreateFileBackup(string filePath, int maxBackups)
        {
            if (maxBackups == 0)
                return false;

            if (File.Exists(filePath) == false)
                return false;

            // Cache backup file names for reuse.
            // NOTE: We can also reuse the same string array for multiple calls of this function,
            // but it's probably not going to be called often enough to be worth it.
            string[] backupPaths = new string[maxBackups];

            // Look for a free backup index
            int freeIndex = -1;
            for (int i = 0; i < maxBackups; i++)
            {
                // Backup path strings are created on demand so that we don't end up creating
                // a lot of unneeded strings when we don't have a lot of backup files.
                backupPaths[i] = $"{filePath}.bak{i}";

                if (File.Exists(backupPaths[i]) == false)
                {
                    freeIndex = i;
                    break;
                }
            }

            // Delete the oldest backup if there are no free spots
            if (freeIndex == -1)
            {
                freeIndex = maxBackups - 1;
                File.Delete(backupPaths[freeIndex]);
            }

            // Move files to the right until we free up index 0
            for (int i = freeIndex - 1; i >= 0; i--)
            {
                File.Move(backupPaths[i], backupPaths[i + 1]);
            }

            // Create our backup at index 0
            if (File.Exists(backupPaths[0]))
                return false;

            File.Copy(filePath, backupPaths[0]);

            return true;
        }
    }
}