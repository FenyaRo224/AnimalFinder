using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Supabase.Gotrue;
using AnimalFinder.Models;

namespace AnimalFinder.Services
{
    public class AuthService
    {
        private readonly Supabase.Client _client = SupabaseService.Client;
        private readonly UserService _userService = new UserService();
        private readonly StorageService _storage = new StorageService();

        // Файл для временного хранения данных регистрации до подтверждения email
        private static string PendingFilePath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AnimalFinder", "pending_profiles.json");

        private record PendingProfile(string Email, string DisplayName, string Phone, string LocalPhotoPath);

        public async Task<bool> RegisterWithProfileAsync(string email, string password, string displayName, string phone, string avatarPath)
        {
            try
            {
                var options = new Supabase.Gotrue.SignUpOptions
                {
                    RedirectTo = "https://example.com/auth/confirmed"
                };

                var session = await _client.Auth.SignUp(email, password, options);

                if (session?.User == null)
                {
                    MessageBox.Show("Регистрация не завершилась: сервер не вернул пользователя.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false; // ← было
                }

                // Сохраняем данные локально до подтверждения email
                SavePendingProfile(new PendingProfile(
                    email.Trim().ToLowerInvariant(),
                    displayName?.Trim() ?? "",
                    phone?.Trim() ?? "",
                    avatarPath ?? ""
                ));

                MessageBox.Show(
                    "Аккаунт создан! Проверьте почту и подтвердите email.\n\nПосле подтверждения войдите в приложение — ваш профиль (имя, телефон, фото) будет автоматически загружен.",
                    "Регистрация успешна",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return true; // ← успех
            }
            catch (Exception ex)
            {
                // === ТВОЙ СТАРЫЙ ОБРАБОТЧИК ОШИБОК (вставь сюда весь свой код из catch, который был раньше) ===
                // (всё, что у тебя было: логирование, парсинг ошибок, MessageBox и т.д.)
                // Я вставлю минимальную рабочую версию, но ты можешь вернуть свой красивый обработчик

                System.Diagnostics.Debug.WriteLine("Register error: " + ex.ToString());

                string message = ex.Message.ToLowerInvariant();
                if (message.Contains("already") || message.Contains("duplicate"))
                    MessageBox.Show("Этот email уже зарегистрирован.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else if (message.Contains("rate_limit"))
                    MessageBox.Show("Слишком много попыток. Подождите минуту.", "Лимит", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    MessageBox.Show("Ошибка регистрации. Проверьте интернет и данные.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false; // ← ЭТО САМОЕ ВАЖНОЕ! Добавь в конец catch!
            }
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var session = await _client.Auth.SignIn(email, password);
                if (session?.User == null)
                {
                    MessageBox.Show("Неверный email или пароль!", "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                var userId = session.User.Id;

                // ВОТ ГЛАВНОЕ ИСПРАВЛЕНИЕ:
                // Берём email ПРЯМО из Supabase — он точно в том же виде, как был при регистрации!
                var emailFromSupabase = session.User.Email?.Trim().ToLowerInvariant() ?? "";

                await EnsureProfileFromPendingAsync(emailFromSupabase, userId);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Login error: " + ex);
                MessageBox.Show("Не удалось войти. Проверьте интернет и данные.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void SavePendingProfile(PendingProfile p)
        {
            MessageBox.Show($"Сохраняю pending для: {p.Email}\nИмя: {p.DisplayName}\nПуть: {PendingFilePath}");
            try
            {
                // Жёстко задаём путь — больше никаких ??
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AnimalFinder");
                Directory.CreateDirectory(folder); // создаём папку, если нет

                var filePath = Path.Combine(folder, "pending_profiles.json");

                Dictionary<string, PendingProfile> map = new();

                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    if (!string.IsNullOrWhiteSpace(json))
                        map = JsonSerializer.Deserialize<Dictionary<string, PendingProfile>>(json) ?? new();
                }

                map[p.Email] = p;

                var jsonToWrite = JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, jsonToWrite);

                System.Diagnostics.Debug.WriteLine($"Pending profile saved for {p.Email} → {filePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CRITICAL: SavePendingProfile FAILED: " + ex.ToString());
                MessageBox.Show("Не удалось сохранить временные данные профиля. Проверьте права доступа.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private PendingProfile? PopPendingProfile(string email)
        {
            try
            {
                if (!File.Exists(PendingFilePath)) return null;

                var json = File.ReadAllText(PendingFilePath);
                if (string.IsNullOrWhiteSpace(json)) return null;

                var map = JsonSerializer.Deserialize<Dictionary<string, PendingProfile>>(json) ?? new();

                // ИЩЕМ ПО НОРМАЛИЗОВАННОМУ КЛЮЧУ — вот и всё!
                var normalizedKey = email.Trim().ToLowerInvariant();

                if (!map.TryGetValue(normalizedKey, out var profile))
                    return null;

                // Удаляем запись
                map.Remove(normalizedKey);
                File.WriteAllText(PendingFilePath, JsonSerializer.Serialize(map));

                System.Diagnostics.Debug.WriteLine($"Pending профиль найден и удалён: {normalizedKey}");
                return profile;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PopPendingProfile error: " + ex);
                return null;
            }
        }

        private async Task EnsureProfileFromPendingAsync(string email, string userId)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userId)) return;

            var pending = PopPendingProfile(email);
            if (pending == null)
            {
                return;
            }

            string avatarUrl = "";

            if (!string.IsNullOrEmpty(pending.LocalPhotoPath) && File.Exists(pending.LocalPhotoPath))
            {
                var ext = Path.GetExtension(pending.LocalPhotoPath);
                var fileName = $"photos/{userId}{ext}";
                try
                {
                    var url = await _storage.UploadFileAsync(pending.LocalPhotoPath, "photos", fileName);
                    if (!string.IsNullOrEmpty(url))
                        avatarUrl = url;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Avatar upload from pending failed: " + ex);
                }
            }

            var user = new AnimalFinder.Models.User
            {
                Id = userId,
                Email = email,
                DisplayName = pending.DisplayName,
                Phone = pending.Phone,
                AvatarUrl = avatarUrl
            };

            var ok = await _userService.UpsertAsync(user);

            if (ok)
            {
                System.Diagnostics.Debug.WriteLine($"Профиль успешно создан/обновлён для пользователя {userId}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ОШИБКА: не удалось сохранить профиль для пользователя {userId}");
            }
        }

        public async Task LogoutAsync()
        {
            try { await _client.Auth.SignOut(); }
            catch { }
        }

        public AnimalFinder.Models.User? GetCurrentUser()
        {
            var u = _client.Auth.CurrentUser;
            if (u == null) return null;
            var id = u.Id;
            if (string.IsNullOrEmpty(id)) return null;

            return new AnimalFinder.Models.User
            {
                Id = id,
                Email = u.Email ?? string.Empty
            };
        }
    }
}