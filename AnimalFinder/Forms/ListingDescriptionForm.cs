using AnimalFinder.Models;
using AnimalFinder.Services;
using System;
using System.IO;
using System.Windows.Forms;

namespace AnimalFinder
{
    public partial class ListingDescriptionForm : Form
    {
        private readonly PetListing _listing;
        private readonly string _localPhotoPath;
        private readonly StorageService _storage = new();

        private RichTextBox rtbDescription;
        private TextBox tbContacts;
        private Button btnPublish, btnBack;

        public ListingDescriptionForm(PetListing listing, string localPhotoPath)
        {
            _listing = listing;
            _localPhotoPath = localPhotoPath;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Описание и контакты — шаг 2";
            this.Size = new Size(580, 720);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            int y = 20;

            // Заголовок
            new Label
            {
                Text = "Подробное описание",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                AutoSize = true,
                Location = new Point(20, y),
                Parent = this
            };
            y += 50;

            rtbDescription = new RichTextBox
            {
                Left = 20,
                Top = y,
                Width = 520,
                Height = 180,
                Font = new Font("Segoe UI", 11F),
                Parent = this
            };
            y += 200;

            // Контакты для связи — ОБЯЗАТЕЛЬНО!
            new Label
            {
                Text = "Контакты для связи (обязательно)",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                AutoSize = true,
                Location = new Point(20, y),
                Parent = this
            };
            y += 40;

            tbContacts = new TextBox
            {
                Left = 20,
                Top = y,
                Width = 520,
                Height = 120,
                Multiline = true,
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.Gray,
                Text = "Например:\r\n+7 (999) 123-45-67 (Иван)\r\nTelegram: @ivan_help\r\nВКонтакте: vk.com/ivanov",
                Parent = this
            };

            // Подсказка исчезает при фокусе
            tbContacts.GotFocus += (s, e) =>
            {
                if (tbContacts.ForeColor == Color.Gray)
                {
                    tbContacts.Text = "";
                    tbContacts.ForeColor = Color.Black;
                }
            };

            tbContacts.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tbContacts.Text))
                {
                    tbContacts.Text = "Например:\r\n+7 (999) 123-45-67 (Иван)\r\nTelegram: @ivan_help\r\nВКонтакте: vk.com/ivanov";
                    tbContacts.ForeColor = Color.Gray;
                }
            };

            y += 150;

            // Кнопки
            btnPublish = new Button
            {
                Text = "Опубликовать объявление",
                Left = 20,
                Top = y,
                Width = 520,
                Height = 55,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Parent = this
            };
            btnPublish.Click += async (s, e) => await Publish_Click();

            btnBack = new Button
            {
                Text = "? Назад",
                Left = 20,
                Top = y + 70,
                Width = 520,
                Height = 45,
                Parent = this
            };
            btnBack.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.AcceptButton = btnPublish;
        }

        private async Task Publish_Click()
        {
            // Проверка контактов
            if (tbContacts.ForeColor == Color.Gray || string.IsNullOrWhiteSpace(tbContacts.Text.Trim()))
            {
                MessageBox.Show("Укажите контакты для связи!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _listing.Description = rtbDescription.Text.Trim();
            _listing.Contact = tbContacts.Text.Trim(); // ? сохраняем контакты

            if (!string.IsNullOrEmpty(_localPhotoPath))
            {
                var fileName = $"photos/{Guid.NewGuid()}{Path.GetExtension(_localPhotoPath)}";
                var url = await _storage.UploadFileAsync(_localPhotoPath, "photos", fileName);
                _listing.PhotoUrl = url;
            }

            await new PetListingService().CreateAsync(_listing);
            MessageBox.Show("Объявление успешно опубликовано!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}