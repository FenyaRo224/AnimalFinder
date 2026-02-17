using System;
using System.IO;
using System.Threading.Tasks;

namespace AnimalFinder.Services
{
    public class StorageService
    {
        private readonly Supabase.Client _client = SupabaseService.Client;

        public async Task<string?> UploadFileAsync(string localFilePath, string bucketName, string remotePath)
        {
            try
            {
                var fileBytes = await File.ReadAllBytesAsync(localFilePath);

                var fileOptions = new Supabase.Storage.FileOptions
                {
                    Upsert = true,
                    ContentType = GetContentType(localFilePath)
                };

                // Загружаем в бакет "photos"
                await _client.Storage
                    .From(bucketName)
                    .Upload(fileBytes, remotePath, fileOptions);

                // Возвращаем публичную ссылку
                var url = _client.Storage
                    .From(bucketName)
                    .GetPublicUrl(remotePath);

                return url;
            }
            catch (Exception ex)
            {
                // Полный стек в Output для диагностики
                System.Diagnostics.Debug.WriteLine("UploadFileAsync error: " + ex.ToString());

                MessageBox.Show($"Ошибка загрузки фото: {ex.Message}\nПроверьте, что бакет '{bucketName}' существует и публичный!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private static string GetContentType(string path)
        {
            return Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }
    }
}