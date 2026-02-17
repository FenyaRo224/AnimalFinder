using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AnimalFinder.Services;

namespace AnimalFinder
{
    public partial class LoginForm : Form
    {
        private TextBox tbEmail, tbPassword, tbDisplayName, tbPhone;
        private PictureBox pbAvatar;
        private Button btnChooseAvatar, btnAction, btnCancel;
        private string avatarPath = "";
        private readonly bool _isRegister;

        public LoginForm(bool isRegister = false)
        {
            _isRegister = isRegister;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = _isRegister ? "Регистрация" : "Вход";
            this.Size = new Size(420, _isRegister ? 620 : 320);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            // Перехватываем нажатия клавиш формой
            this.KeyPreview = true;
            this.KeyDown += LoginForm_KeyDown;

            int y = 20;

            // Заголовок
            new Label
            {
                Text = _isRegister ? "Создать аккаунт" : "Вход в аккаунт",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                AutoSize = true,
                Location = new Point(30, y),
                Parent = this
            };
            y += 60;

            // Email
            new Label { Text = "Email:", Location = new Point(30, y), Parent = this };
            tbEmail = new TextBox { Left = 30, Top = y + 25, Width = 340, Parent = this };
            y += 70;

            // Пароль
            new Label { Text = "Пароль:", Location = new Point(30, y), Parent = this };
            tbPassword = new TextBox { Left = 30, Top = y + 25, Width = 340, PasswordChar = '*', Parent = this };
            y += 70;

            if (_isRegister)
            {
                // Имя
                new Label { Text = "Ваше имя:", Location = new Point(30, y), Parent = this };
                tbDisplayName = new TextBox { Left = 30, Top = y + 25, Width = 340, Parent = this };
                y += 70;

                // Телефон
                new Label { Text = "Телефон (+7XXXXXXXXXX):", Location = new Point(30, y), Parent = this };
                tbPhone = new TextBox { Left = 30, Top = y + 25, Width = 340, Text = "+7", Parent = this };
                y += 70;

                // Аватарка
                new Label { Text = "Аватарка:", Location = new Point(30, y), Parent = this };
                pbAvatar = new PictureBox
                {
                    Size = new Size(120, 120),
                    Location = new Point(30, y + 25),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BorderStyle = BorderStyle.FixedSingle,
                    Parent = this
                };

                btnChooseAvatar = new Button
                {
                    Text = "Выбрать фото",
                    Left = 160,
                    Top = y + 65,
                    Width = 140,
                    Parent = this
                };
                btnChooseAvatar.Click += (s, e) =>
                {
                    using var ofd = new OpenFileDialog { Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif" };
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        avatarPath = ofd.FileName;
                        pbAvatar.Image = Image.FromFile(avatarPath);
                    }
                };
                y += 160;
            }

            btnAction = new Button
            {
                Text = _isRegister ? "Зарегистрироваться" : "Войти",
                Left = 30,
                Top = y,
                Width = 340,
                Height = 50,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Parent = this
            };
            btnAction.Click += BtnAction_Click;

            btnCancel = new Button
            {
                Text = "Отмена",
                Left = 30,
                Top = y + 60,
                Width = 340,
                Height = 40,
                Parent = this
            };
            btnCancel.Click += (s, e) => this.Close();

            this.AcceptButton = btnAction;
        }

        // Секрет: Shift + F — автозаполнение (только на форме регистрации)
        private void LoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_isRegister) return;

            if (e.Shift && e.KeyCode == Keys.F)
            {
                try
                {
                    tbEmail.Text = "dosytamurza@yandex.ru";
                    tbPassword.Text = "Test1234!";
                    if (tbDisplayName != null) tbDisplayName.Text = "1sdfsdfasdfadfsa";
                    if (tbPhone != null) tbPhone.Text = "+79098098998";

                    // Попытка загрузить тестовый аватар из папки с приложением
                    var candidate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? "", "test_avatar.jpg");
                    if (File.Exists(candidate) && pbAvatar != null)
                    {
                        avatarPath = candidate;
                        try { pbAvatar.Image = Image.FromFile(candidate); }
                        catch { /* молча игнорируем ошибки чтения файла */ }
                    }

                    System.Diagnostics.Debug.WriteLine("Autofill applied (Shift+F).");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Autofill error: " + ex);
                }

                e.Handled = true;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Shift | Keys.F))
            {
                tbEmail.Text = "dosytamurza@yandex.ru";
                tbPassword.Text = "123123";
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async void BtnAction_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbEmail.Text) || string.IsNullOrWhiteSpace(tbPassword.Text))
            {
                MessageBox.Show("Заполните email и пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_isRegister)
            {
                if (string.IsNullOrWhiteSpace(tbDisplayName.Text))
                {
                    MessageBox.Show("Введите имя!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!Regex.IsMatch(tbPhone.Text, @"^(\+7|8)\d{10}$"))
                {
                    MessageBox.Show("Телефон должен быть в формате +7XXXXXXXXXX", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            btnAction.Enabled = false;
            btnAction.Text = "Подождите...";

            var auth = new AuthService();
            var success = _isRegister
                ? await auth.RegisterWithProfileAsync(
                    tbEmail.Text.Trim(),
                    tbPassword.Text,
                    tbDisplayName?.Text.Trim() ?? "",
                    tbPhone?.Text.Trim() ?? "",
                    avatarPath)
                : await auth.LoginAsync(tbEmail.Text.Trim(), tbPassword.Text);

            if (success)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                btnAction.Enabled = true;
                btnAction.Text = _isRegister ? "Зарегистрироваться" : "Войти";
            }
        }
    }
}