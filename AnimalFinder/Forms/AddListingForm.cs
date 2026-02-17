using AnimalFinder.Models;
using AnimalFinder.Services;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnimalFinder
{
    public partial class AddListingForm : Form
    {
        private ComboBox cbType, cbSpecies, cbSize, cbGender, cbTemperament;
        private TextBox tbSpeciesCustom, tbPetName, tbBreed, tbLocation;
        private NumericUpDown nudYears, nudMonths;
        private CheckedListBox clbColors;
        private Button btnChoosePhoto, btnNext, btnCancel, btnGeo;
        private PictureBox pbPreview;

        public string LocalPhotoPath { get; private set; } = "";
        public PetListing ResultListing { get; private set; } = new PetListing();

        public AddListingForm()
        {
            InitializeComponent();
            this.Size = new Size(540, 1080);
            this.MinimumSize = new Size(540, 900);
            this.MaximumSize = new Size(600, 1200);
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void InitializeComponent()
        {
            this.Text = "Новое объявление — шаг 1";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            int y = 20;
            int labelX = 20;
            int controlX = 150;
            int shortWidth = 340;

            // Заголовок
            new Label
            {
                Text = "Создать объявление",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                AutoSize = true,
                Location = new Point(20, y),
                Parent = this
            };
            y += 55;

            // Тип
            new Label { Text = "Тип:", Location = new Point(labelX, y), Parent = this };
            cbType = new ComboBox { Left = controlX, Top = y - 3, Width = shortWidth, DropDownStyle = ComboBoxStyle.DropDownList, Parent = this };
            cbType.Items.AddRange(new[] { "lost — потерялся", "found — найден" });
            cbType.SelectedIndex = 0;
            y += 45;

            // Вид
            new Label { Text = "Вид:", Location = new Point(labelX, y), Parent = this };
            cbSpecies = new ComboBox { Left = controlX, Top = y - 3, Width = shortWidth, DropDownStyle = ComboBoxStyle.DropDownList, Parent = this };
            cbSpecies.Items.AddRange(new[] { "Собака", "Кошка", "Птица", "Грызун", "Рептилия", "Другое" });
            cbSpecies.SelectedIndex = 0;
            cbSpecies.SelectedIndexChanged += (s, e) => tbSpeciesCustom.Visible = cbSpecies.Text == "Другое";
            tbSpeciesCustom = new TextBox { Left = controlX, Top = y + 25, Width = shortWidth, Visible = false, Parent = this };
            y += 60;

            // Имя питомца
            new Label { Text = "Имя питомца:", Location = new Point(labelX, y), Parent = this };
            tbPetName = new TextBox { Left = controlX, Top = y - 3, Width = shortWidth, Parent = this };
            y += 45;

            // Порода
            new Label { Text = "Порода:", Location = new Point(labelX, y), Parent = this };
            tbBreed = new TextBox { Left = controlX, Top = y - 3, Width = shortWidth, Parent = this };
            y += 45;

            // Возраст + Размер + Пол
            new Label { Text = "Возраст:", Location = new Point(labelX, y), Parent = this };
            nudYears = new NumericUpDown { Left = controlX, Top = y - 3, Width = 70, Maximum = 30, Value = 1, Parent = this };
            nudMonths = new NumericUpDown { Left = controlX + 115, Top = y - 3, Width = 70, Maximum = 11, Parent = this };
            new Label { Text = "лет", Left = controlX, Top = y + 25, Width = 70, TextAlign = ContentAlignment.MiddleCenter, Parent = this };
            new Label { Text = "мес.", Left = controlX + 115, Top = y + 25, Width = 70, TextAlign = ContentAlignment.MiddleCenter, Parent = this };

            new Label { Text = "Размер:", Location = new Point(labelX + 250, y), Parent = this };
            cbSize = new ComboBox { Left = labelX + 250 + 70, Top = y - 3, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Parent = this };
            cbSize.Items.AddRange(new[] { "Маленький", "Средний", "Большой" });
            cbSize.SelectedIndex = 0;

            new Label { Text = "Пол:", Location = new Point(labelX + 400, y), Parent = this };
            cbGender = new ComboBox { Left = labelX + 400 + 50, Top = y - 3, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Parent = this };
            cbGender.Items.AddRange(new[] { "Мальчик", "Девочка", "Неизвестно" });
            cbGender.SelectedIndex = 0;
            y += 70;

            // Цвет
            new Label { Text = "Цвет (можно несколько):", Location = new Point(labelX, y), Parent = this };
            clbColors = new CheckedListBox
            {
                Left = controlX,
                Top = y + 25,
                Width = shortWidth,
                Height = 80,
                CheckOnClick = true,
                Parent = this
            };
            clbColors.Items.AddRange(new[] { "Белый", "Чёрный", "Рыжий", "Серый", "Коричневый", "Пятнистый", "Трёхцветный", "Другое" });
            y += 110;

            // Характер
            new Label { Text = "Характер:", Location = new Point(labelX, y), Parent = this };
            cbTemperament = new ComboBox { Left = controlX, Top = y - 3, Width = shortWidth, DropDownStyle = ComboBoxStyle.DropDownList, Parent = this };
            cbTemperament.Items.AddRange(new[]
            {
                "Спокойный", "Игривый", "Активный", "Ласковый", "Независимый",
                "Пугливый", "Дружелюбный", "Защитник", "Ленивый", "Любопытный"
            });
            cbTemperament.SelectedIndex = 0;
            y += 50;

            // Местоположение + ГЕОЛОКАЦИЯ
            new Label { Text = "Где видели / потеряли:", Location = new Point(labelX, y), Parent = this };
            tbLocation = new TextBox { Left = controlX, Top = y - 3, Width = shortWidth, Parent = this };

            btnGeo = new Button
            {
                Text = "Моё местоположение",
                Left = controlX + shortWidth + 20,
                Top = y - 3,
                Width = 160,
                Height = 34,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Parent = this
            };
            btnGeo.Click += async (s, e) =>
            {
                btnGeo.Enabled = false;
                btnGeo.Text = "Определяю...";

                try
                {
                    // Простая геолокация через IP — работает ВСЕГДА на любом Windows
                    using var client = new System.Net.WebClient();
                    string json = client.DownloadString("http://ip-api.com/json/");

                    dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                    if (data.status == "success")
                    {
                        double lat = data.lat;
                        double lon = data.lon;
                        string city = data.city ?? "Неизвестно";

                        tbLocation.Text = $"Город: {city}, Широта: {lat:F6}, Долгота: {lon:F6}";
                        MessageBox.Show($"Геолокация определена!\nГород: {city}\nКоординаты: {lat:F6}, {lon:F6}",
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось определить местоположение.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch
                {
                    MessageBox.Show("Нет интернета или сервис недоступен.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                finally
                {
                    btnGeo.Text = "Моё местоположение";
                    btnGeo.Enabled = true;
                }
            };

            y += 70;

            // Фото
            new Label { Text = "Фото:", Location = new Point(labelX, y), Parent = this };
            pbPreview = new PictureBox
            {
                Left = controlX,
                Top = y + 20,
                Size = new Size(160, 120),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                Parent = this
            };
            btnChoosePhoto = new Button
            {
                Text = "Выбрать фото",
                Left = controlX + 180,
                Top = y + 60,
                Width = 140,
                Parent = this
            };
            btnChoosePhoto.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog { Filter = "Фото|*.jpg;*.jpeg;*.png" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    LocalPhotoPath = ofd.FileName;
                    pbPreview.ImageLocation = LocalPhotoPath;
                }
            };
            y += 160;

            // Кнопки
            btnNext = new Button
            {
                Text = "Дальше →",
                Left = 20,
                Top = y,
                Width = 480,
                Height = 50,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Parent = this
            };
            btnNext.Click += BtnNext_Click;

            btnCancel = new Button
            {
                Text = "Отмена",
                Left = 20,
                Top = y + 70,
                Width = 480,
                Height = 40,
                Parent = this
            };
            btnCancel.Click += (s, e) => this.Close();

            this.AcceptButton = btnNext;
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbPetName.Text) || string.IsNullOrWhiteSpace(tbLocation.Text))
            {
                MessageBox.Show("Заполните имя питомца и местоположение!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var colors = string.Join(" + ", clbColors.CheckedItems.Cast<string>());
            if (string.IsNullOrEmpty(colors)) colors = "Не указан";

            string species = cbSpecies.Text == "Другое"
                ? (string.IsNullOrWhiteSpace(tbSpeciesCustom.Text) ? "Неизвестно" : tbSpeciesCustom.Text.Trim())
                : cbSpecies.Text;

            int ageMonths = (int)nudYears.Value * 12 + (int)nudMonths.Value;

            ResultListing = new PetListing
            {
                Id = Guid.NewGuid().ToString(),
                ListingType = cbType.Text.Contains("lost") ? "lost" : "found",
                Species = species,
                Breed = tbBreed.Text.Trim(),
                Color = colors,
                Age = ageMonths > 0 ? ageMonths : null,
                Size = cbSize.SelectedItem?.ToString(),
                Gender = cbGender.SelectedItem?.ToString(),
                Location = tbLocation.Text.Trim(),
                PetName = tbPetName.Text.Trim(),
                Temperament = cbTemperament.SelectedItem?.ToString() ?? "",
                UserId = SupabaseService.Client.Auth.CurrentUser?.Id ?? "",
                CreatedAt = DateTime.UtcNow
            };

            using var descriptionForm = new ListingDescriptionForm(ResultListing, LocalPhotoPath);
            if (descriptionForm.ShowDialog() == DialogResult.OK)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}