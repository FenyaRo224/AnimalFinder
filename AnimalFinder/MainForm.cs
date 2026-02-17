using AnimalFinder.Models;
using AnimalFinder.Services;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using AnimalFinder.Models;
using AnimalFinder.Services;
namespace AnimalFinder
{
    public partial class MainForm : Form
    {
        private readonly PetListingService _listingService = new();
        private readonly StorageService _storageService = new();
        private readonly AuthService _authService = new();
        private List<PetListing> _currentListings = new();
        private Button btnAccount, btnAdd;
        private System.Windows.Forms.Timer autoRefreshTimer;
        private FlowLayoutPanel listingsPanel;
        private Panel detailPanel;
        private PictureBox pbBigPhoto;
        private Label lblTitle, lblInfo, lblDescription;
        private SplitContainer split;
        // Новые элементы верхней панели
        private TableLayoutPanel topBar;
        private FlowLayoutPanel leftTopPanel;
        private FlowLayoutPanel rightTopPanel;
        private TextBox txtSearch;
        private ComboBox cbTypeFilter;
        private ComboBox cbSortBy;
        private PictureBox pbAvatar;
        // Новая левая панель фильтров
        private Panel filtersPanel;
        private ComboBox cbBreedFilter, cbAgeFilter, cbColorFilter, cbSizeFilter, cbGenderFilter;
        public MainForm()
        {
            InitializeComponent();
            SetupModernLayout();
            CreateTopBar();
            UpdateAccountStatus();
        }
        private void SetupModernLayout()
        {
            this.Size = new Size(1400, 900);
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // ← нельзя растягивать
            this.BackColor = Color.FromArgb(250, 250, 252);

            // Главный контейнер — TableLayoutPanel
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240)); // Фильтры
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Карточки + детали

            // Фильтры слева
            filtersPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 246, 248),
                Padding = new Padding(12),
                AutoScroll = true
            };
            mainLayout.Controls.Add(filtersPanel, 0, 0);

            // Правая часть — карточки и детали
            var rightPanel = new Panel { Dock = DockStyle.Fill };

            // Карточки объявлений
            listingsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = Color.White,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            // Детальная информация — справа от карточек
            detailPanel = new Panel
            {
                Width = 500,
                Dock = DockStyle.Right,
                BackColor = Color.White,
                Padding = new Padding(30)
            };

            // Элементы детальной информации
            pbBigPhoto = new PictureBox
            {
                Size = new Size(300, 300),
                Location = new Point(30, 20),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };
            pbBigPhoto.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(pbBigPhoto.ImageLocation))
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(pbBigPhoto.ImageLocation) { UseShellExecute = true });
            };

            lblTitle = new Label
            {
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                AutoSize = true,
                Location = new Point(30, 340)
            };

            lblInfo = new Label
            {
                Font = new Font("Segoe UI", 12F),
                AutoSize = true,
                MaximumSize = new Size(440, 0),
                Location = new Point(30, 390)
            };

            lblDescription = new Label
            {
                Font = new Font("Segoe UI", 11F),
                AutoSize = true,
                MaximumSize = new Size(440, 0),
                Location = new Point(30, 500)
            };

            detailPanel.Controls.Add(pbBigPhoto);
            detailPanel.Controls.Add(lblTitle);
            detailPanel.Controls.Add(lblInfo);
            detailPanel.Controls.Add(lblDescription);

            rightPanel.Controls.Add(listingsPanel);
            rightPanel.Controls.Add(detailPanel);

            mainLayout.Controls.Add(rightPanel, 1, 0);

            this.Controls.Add(mainLayout);

            // Создаём фильтры
            CreateLeftFilters();
        }
        private void CreateLeftFilters()
        {
            filtersPanel.Controls.Clear();
            int y = 8;
            void AddLabel(string text)
            {
                var l = new Label { Text = text, Location = new Point(12, y), AutoSize = true, Parent = filtersPanel, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
                y += 26;
            }
            void AddCombo(ref ComboBox cb)
            {
                cb = new ComboBox { Left = 12, Top = y, Width = filtersPanel.Width - 32, DropDownStyle = ComboBoxStyle.DropDownList, Parent = filtersPanel };
                cb.Items.Add("Any");
                cb.SelectedIndex = 0;
                cb.SelectedIndexChanged += (s, e) => RefreshListingsView();
                y += 40;
            }
            AddLabel("BREED");
            AddCombo(ref cbBreedFilter);
            AddLabel("AGE");
            AddCombo(ref cbAgeFilter);
            AddLabel("COLOR");
            AddCombo(ref cbColorFilter);
            AddLabel("SIZE");
            AddCombo(ref cbSizeFilter);
            AddLabel("GENDER");
            AddCombo(ref cbGenderFilter);
            // немного отступа внизу
            var filler = new Panel { Height = 20, Dock = DockStyle.Bottom, Parent = filtersPanel };
        }
        private void PopulateFilterOptions()
        {
            if (_currentListings == null) return;
            void FillCombo(ComboBox cb, IEnumerable<string> vals)
            {
                var sel = cb.SelectedItem?.ToString() ?? "Any";
                cb.BeginUpdate();
                cb.Items.Clear();
                cb.Items.Add("Any");
                foreach (var v in vals.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x))
                    cb.Items.Add(v);
                cb.SelectedItem = cb.Items.Contains(sel) ? sel : "Any";
                cb.EndUpdate();
            }
            FillCombo(cbBreedFilter, _currentListings.Select(x => x.Breed ?? ""));
            FillCombo(cbColorFilter, _currentListings.Select(x => x.Color ?? ""));
            FillCombo(cbSizeFilter, _currentListings.Select(x => x.Size ?? ""));
            FillCombo(cbGenderFilter, _currentListings.Select(x => x.Gender ?? ""));
            // Age — простые диапазоны
            var ageOpts = new[] { "Any", "0-1", "1-5", "5-10", "10+" };
            cbAgeFilter.BeginUpdate();
            cbAgeFilter.Items.Clear();
            cbAgeFilter.Items.AddRange(ageOpts);
            cbAgeFilter.SelectedIndex = 0;
            cbAgeFilter.EndUpdate();
        }
        private void CreateTopBar()
        {
            topBar = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 70,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(10),
                BackColor = Color.WhiteSmoke
            };
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            leftTopPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Dock = DockStyle.Fill,
                WrapContents = false
            };
            rightTopPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Dock = DockStyle.Fill,
                WrapContents = false
            };
            txtSearch = new TextBox
            {
                Width = 320,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "Поиск по описанию, месту, породе..."
            };
            txtSearch.TextChanged += (s, e) => RefreshListingsView();
            cbTypeFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 140,
                Font = new Font("Segoe UI", 10F)
            };
            cbTypeFilter.Items.AddRange(new[] { "Все", "Пропал", "Найден" });
            cbTypeFilter.SelectedIndex = 0;
            cbTypeFilter.SelectedIndexChanged += (s, e) => RefreshListingsView();
            cbSortBy = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 160,
                Font = new Font("Segoe UI", 10F)
            };
            cbSortBy.Items.AddRange(new[] { "Nearest", "Дата ↓", "Дата ↑", "Тип", "Место" });
            cbSortBy.SelectedIndex = 1;
            cbSortBy.SelectedIndexChanged += (s, e) => RefreshListingsView();
            leftTopPanel.Controls.Add(txtSearch);
            leftTopPanel.Controls.Add(cbTypeFilter);
            leftTopPanel.Controls.Add(cbSortBy);
            btnAdd = new Button
            {
                Text = "Добавить",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 167, 69),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 40),
                Cursor = Cursors.Hand,
                Margin = new Padding(6)
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += btnAdd_Click;
            pbAvatar = new PictureBox
            {
                Size = new Size(40, 40),
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = new Padding(6),
                Cursor = Cursors.Hand,
                BorderStyle = BorderStyle.FixedSingle
            };
            btnAccount = new Button
            {
                Text = "Аккаунт",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 122, 204),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 40),
                Cursor = Cursors.Hand,
                Margin = new Padding(6)
            };
            var btnRefresh = new Button
            {
                Text = "Обновить ↻",
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += async (s, e) =>
            {
                btnRefresh.Enabled = false;
                btnRefresh.Text = "Загрузка...";
                await LoadListingsAsync();
                btnRefresh.Text = "Обновить ↻";
                btnRefresh.Enabled = true;
            };
            rightTopPanel.Controls.Add(btnRefresh);
            btnAccount.FlatAppearance.BorderSize = 0;

            btnAccount.Click += (s, e) => ShowAccountMenu();
            pbAvatar.Click += (s, e) => ShowAccountMenu();
            btnAccount.FlatAppearance.BorderSize = 0;
            rightTopPanel.Controls.Add(btnAdd);
            rightTopPanel.Controls.Add(pbAvatar);
            rightTopPanel.Controls.Add(btnAccount);
            topBar.Controls.Add(leftTopPanel, 0, 0);
            topBar.Controls.Add(new Panel(), 1, 0); // заполнитель
            topBar.Controls.Add(rightTopPanel, 2, 0);
            this.Controls.Add(topBar);
            this.Load += (s, e) =>
            {
                UpdateAccountStatus();
                _ = LoadListingsAsync();
            };
            this.Resize += (s, e) => RefreshListingsView();
        }
            private void ShowAccountMenu()
        {
            var user = _authService.GetCurrentUser();
            var menu = new ContextMenuStrip();

            if (user != null)
            {
                menu.Items.Add($"Вы вошли как: {user.Email}", null, null);
                menu.Items.Add(new ToolStripSeparator());
                menu.Items.Add("Выйти", null, async (s, a) =>
                {
                    await _authService.LogoutAsync();
                    UpdateAccountStatus();
                    await LoadListingsAsync();
                    MessageBox.Show("Вы успешно вышли из аккаунта", "Выход", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            }
            else
            {
                menu.Items.Add("Войти", null, (s, a) =>
                {
                    using var loginForm = new LoginForm(false);
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        UpdateAccountStatus();
                        _ = LoadListingsAsync();
                    }
                });

                menu.Items.Add("Регистрация", null, (s, a) =>
                {
                    using var loginForm = new LoginForm(true);
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        UpdateAccountStatus();
                        _ = LoadListingsAsync();
                    }
                });
            }

            // Показываем меню под кнопкой "Аккаунт"
            menu.Show(btnAccount, new Point(0, btnAccount.Height));
        }


        private async Task LoadListingsAsync()
        {
            try
            {
                var newListings = await _listingService.GetAllAsync();

                // ← ВОТ ГЛАВНОЕ ИСПРАВЛЕНИЕ!
                if (newListings == null) return;

                // Проверяем, изменились ли данные по ID
                bool hasChanges = _currentListings == null ||
                                 _currentListings.Count != newListings.Count ||
                                 !_currentListings.Select(x => x.Id).OrderBy(id => id)
                                                  .SequenceEqual(newListings.Select(x => x.Id).OrderBy(id => id));

                if (hasChanges)
                {
                    _currentListings = newListings;
                    PopulateFilterOptions();
                    RefreshListingsView(); // ← Очищает и заполняет карточки
                    statusLabel.Text = $"Активных объявлений: {_currentListings.Count}";
                }
                else
                {
                    statusLabel.Text = $"Активных объявлений: {_currentListings.Count}";
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Ошибка соединения";
                System.Diagnostics.Debug.WriteLine("Load error: " + ex);
            }
        }
        private IEnumerable<PetListing> ApplyFiltersAndSort(IEnumerable<PetListing> source)
        {
            if (source == null) return Enumerable.Empty<PetListing>();
            var q = source.AsQueryable();
            var term = txtSearch?.Text?.Trim();
            if (!string.IsNullOrEmpty(term))
            {
                var low = term.ToLowerInvariant();
                q = q.Where(x =>
                       (!string.IsNullOrEmpty(x.Description) && x.Description.ToLowerInvariant().Contains(low))
                    || (!string.IsNullOrEmpty(x.Location) && x.Location.ToLowerInvariant().Contains(low))
                    || (!string.IsNullOrEmpty(x.Breed) && x.Breed.ToLowerInvariant().Contains(low)));
            }
            var typeSel = cbTypeFilter?.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(typeSel) && typeSel != "Все")
            {
                var want = typeSel == "Пропал" ? "lost" : "found";
                q = q.Where(x => string.Equals(x.ListingType, want, StringComparison.OrdinalIgnoreCase));
            }
            // Левые фильтры
            if (cbBreedFilter != null && cbBreedFilter.SelectedItem is string bs && bs != "Any")
                q = q.Where(x => string.Equals(x.Breed ?? "", bs, StringComparison.OrdinalIgnoreCase));
            if (cbColorFilter != null && cbColorFilter.SelectedItem is string cs && cs != "Any")
                q = q.Where(x => string.Equals(x.Color ?? "", cs, StringComparison.OrdinalIgnoreCase));
            if (cbSizeFilter != null && cbSizeFilter.SelectedItem is string ss && ss != "Any")
                q = q.Where(x => string.Equals(x.Size ?? "", ss, StringComparison.OrdinalIgnoreCase));
            if (cbGenderFilter != null && cbGenderFilter.SelectedItem is string gs && gs != "Any")
                q = q.Where(x => string.Equals(x.Gender ?? "", gs, StringComparison.OrdinalIgnoreCase));
            if (cbAgeFilter != null && cbAgeFilter.SelectedItem is string ag && ag != "Any")
            {
                if (ag == "0-1") q = q.Where(x => (x.Age ?? 0) <= 1);
                else if (ag == "1-5") q = q.Where(x => (x.Age ?? 0) >= 1 && (x.Age ?? 0) <= 5);
                else if (ag == "5-10") q = q.Where(x => (x.Age ?? 0) >= 5 && (x.Age ?? 0) <= 10);
                else if (ag == "10+") q = q.Where(x => (x.Age ?? 0) >= 10);
            }
            var sortSel = cbSortBy?.SelectedItem?.ToString();
            return sortSel switch
            {
                "Дата ↑" => q.OrderBy(x => x.CreatedAt).ToList(),
                "Тип" => q.OrderBy(x => x.ListingType).ThenByDescending(x => x.CreatedAt).ToList(),
                "Место" => q.OrderBy(x => x.Location).ThenByDescending(x => x.CreatedAt).ToList(),
                _ => q.OrderByDescending(x => x.CreatedAt).ToList(),
            };
        }
        private void RefreshListingsView()
        {
            listingsPanel.Controls.Clear();

            var filtered = ApplyFiltersAndSort(_currentListings);

            foreach (var item in filtered)
            {
                var card = new Panel
                {
                    Width = 240,
                    Height = 320,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(12),
                    Cursor = Cursors.Hand,
                    Tag = item
                };

                var photo = new PictureBox
                {
                    Dock = DockStyle.Top,
                    Height = 180,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BorderStyle = BorderStyle.FixedSingle,
                    ImageLocation = item.PhotoUrl,
                    Tag = item
                };

                var title = new Label
                {
                    Text = item.PetName ?? "Без имени",
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 122, 204),
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 40,
                    Tag = item
                };

                var info = new Label
                {
                    Text = $"{item.Species}\n{item.Age} мес. • {item.Color}",
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.Gray,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 50,
                    Tag = item
                };

                var location = new Label
                {
                    Text = item.Location,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(0, 122, 204),
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Bottom,
                    Height = 30,
                    Tag = item
                };

                card.Controls.Add(location);
                card.Controls.Add(info);
                card.Controls.Add(title);
                card.Controls.Add(photo);

                // ← ВСТРОЕННЫЙ ShowDetail ПРЯМО ЗДЕСЬ!
                void ShowThisDetail()
                {
                    pbBigPhoto.ImageLocation = item.PhotoUrl;
                    lblTitle.Text = $"{item.PetName ?? "Без имени"} • {item.Species}";
                    lblInfo.Text =
                        $"Порода: {item.Breed ?? "не указано"}\n" +
                        $"Возраст: {item.Age} мес.\n" +
                        $"Пол: {item.Gender ?? "не указано"}\n" +
                        $"Размер: {item.Size ?? "не указано"}\n" +
                        $"Цвет: {item.Color}\n" +
                        $"Характер: {item.Temperament ?? "не указано"}\n" +
                        $"Место: {item.Location}\n" +
                        $"Дата: {item.CreatedAt:dd.MM.yyyy HH:mm}";

                    if (!string.IsNullOrWhiteSpace(item.Contact))
                    {
                        lblInfo.Text += $"\n\nКонтакт для связи:\n{item.Contact}";
                    }

                    lblDescription.Text = item.Description ?? "Описание отсутствует";
                }

                card.Click += (s, e) => ShowThisDetail();
                photo.Click += (s, e) => ShowThisDetail();
                title.Click += (s, e) => ShowThisDetail();
                info.Click += (s, e) => ShowThisDetail();
                location.Click += (s, e) => ShowThisDetail();

                listingsPanel.Controls.Add(card);
            }
        }

        private void RefreshListings(IEnumerable<PetListing> source)
        {
            listingsPanel.Controls.Clear();

            foreach (var item in source)
            {
                var card = new Panel
                {
                    Width = 240,
                    Height = 320,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(12),
                    Cursor = Cursors.Hand,
                    Tag = item
                };

                // Фото
                var photo = new PictureBox
                {
                    Dock = DockStyle.Top,
                    Height = 180,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BorderStyle = BorderStyle.FixedSingle,
                    ImageLocation = item.PhotoUrl,
                    Tag = item
                };

                // Имя питомца
                var title = new Label
                {
                    Text = item.PetName ?? "Без имени",
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 122, 204),
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 40,
                    Tag = item
                };

                // Информация
                var info = new Label
                {
                    Text = $"{item.Species}\n{item.Age} мес. • {item.Color}",
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.Gray,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 50,
                    Tag = item
                };

                // Место
                var location = new Label
                {
                    Text = item.Location,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(0, 122, 204),
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Bottom,
                    Height = 30,
                    Tag = item
                };

                card.Controls.Add(location);
                card.Controls.Add(info);
                card.Controls.Add(title);
                card.Controls.Add(photo);

                // Клик по любой части карточки
                card.Click += Card_Click;
                photo.Click += Card_Click;
                title.Click += Card_Click;
                info.Click += Card_Click;
                location.Click += Card_Click;

                listingsPanel.Controls.Add(card);
            }
        }
        private bool AreListsEqual(List<PetListing> a, List<PetListing> b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
                if (a[i].Id != b[i].Id) return false;
            return true;
        }
        private async void Card_Click(object sender, EventArgs e)
        {
            if (sender is Control ctrl && ctrl.Tag is PetListing item)
            {
                // Большое фото
                pbBigPhoto.ImageLocation = item.PhotoUrl;

                // Заголовок
                lblTitle.Text = $"{item.PetName ?? "Без имени"} • {item.Species}";

                // Основная информация
                string baseInfo =
                    $"Порода: {item.Breed ?? "не указано"}\n" +
                    $"Возраст: {item.Age} мес.\n" +
                    $"Пол: {item.Gender ?? "не указано"}\n" +
                    $"Размер: {item.Size ?? "не указано"}\n" +
                    $"Цвет: {item.Color}\n" +
                    $"Характер: {item.Temperament ?? "не указано"}\n" +
                    $"Место: {item.Location}\n" +
                    $"Дата: {item.CreatedAt:dd.MM.yyyy HH:mm}";

                // Добавляем контакты из поля contact (у тебя колонка называется contact)
                string contactText = "";
                if (!string.IsNullOrWhiteSpace(item.Contact))
                {
                    contactText = $"\n\nКонтакт для связи:\n{item.Contact}";
                }

                lblInfo.Text = baseInfo + contactText;

                // Описание
                lblDescription.Text = string.IsNullOrWhiteSpace(item.Description)
                    ? "Описание отсутствует"
                    : item.Description;
            }
        }
        private void ClearDetail()
        {
            pbBigPhoto.Image = null;
            lblTitle.Text = "Выберите объявление";
            lblInfo.Text = "";
            lblDescription.Text = "";
        }
        private void UpdateAccountStatus()
        {
            var user = _authService.GetCurrentUser();
            btnAccount.Text = user != null ? $"Выйти ({user.Email.Split('@')[0]})" : "Аккаунт";
            string avatarUrl = null;
            if (user != null)
            {
                var t = user.GetType();
                var p = t.GetProperty("AvatarUrl", BindingFlags.Public | BindingFlags.Instance)
                        ?? t.GetProperty("avatar_url", BindingFlags.Public | BindingFlags.Instance);
                if (p != null)
                {
                    avatarUrl = p.GetValue(user)?.ToString();
                }
            }
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                try
                {
                    pbAvatar.ImageLocation = avatarUrl;
                }
                catch
                {
                    pbAvatar.Image = null;
                }
            }
            else
            {
                pbAvatar.Image = null;
            }
        }
        private async void btnAdd_Click(object sender, EventArgs e)
        {
            if (_authService.GetCurrentUser() == null)
            {
                MessageBox.Show("Войдите в аккаунт!");
                return;
            }

            using var form = new AddListingForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                var listing = form.ResultListing;
                listing.UserId = _authService.GetCurrentUser()?.Id ?? "";

                if (!string.IsNullOrEmpty(form.LocalPhotoPath))
                {
                    var fileName = $"photos/{Guid.NewGuid()}{Path.GetExtension(form.LocalPhotoPath)}";
                    var url = await _storageService.UploadFileAsync(form.LocalPhotoPath, "photos", fileName);
                    listing.PhotoUrl = url;
                }

                await _listingService.CreateAsync(listing);
                MessageBox.Show("Объявление создано! Нажмите «Обновить», чтобы увидеть.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void btnAccount_Click(object sender, EventArgs e)
        {
            var user = _authService.GetCurrentUser();
            var menu = new ContextMenuStrip();

            if (user != null)
            {
                menu.Items.Add("Выйти", null, async (s, a) =>
                {
                    await _authService.LogoutAsync();
                    UpdateAccountStatus();
                    await LoadListingsAsync();
                });
            }
            else
            {
                menu.Items.Add("Войти", null, (s, a) =>
                {
                    using var loginForm = new LoginForm(false);
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        UpdateAccountStatus();
                        _ = LoadListingsAsync();
                    }
                });
            }

            menu.Show(btnAccount, new Point(0, btnAccount.Height));
        }

    }
}