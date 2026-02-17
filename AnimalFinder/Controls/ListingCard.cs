using System;
using System.Drawing;
using System.Windows.Forms;
using AnimalFinder.Models;

namespace AnimalFinder.Controls
{
    public class ListingCard : UserControl
    {
        private readonly PictureBox _photo;
        private readonly Label _title;
        private readonly Label _desc;

        public PetListing Listing { get; }

        public ListingCard(PetListing listing)
        {
            Listing = listing ?? throw new ArgumentNullException(nameof(listing));
            Height = 120;
            BackColor = Color.White;
            Margin = new Padding(8);
            Padding = new Padding(8);
            DoubleBuffered = true;

            // Блок фото
            _photo = new PictureBox
            {
                Size = new Size(96, 96),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(8, 8),
                ImageLocation = listing.PhotoUrl
            };

            // Заголовок
            _title = new Label
            {
                AutoSize = false,
                Location = new Point(_photo.Right + 12, 10),
                Width = 360,
                Height = 28,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                Text = $"{(listing.ListingType == "lost" ? "Пропал" : "Найден")} {listing.Species} — {listing.Location}",
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Краткое описание
            _desc = new Label
            {
                AutoSize = false,
                Location = new Point(_photo.Right + 12, _title.Bottom + 4),
                Width = 360,
                Height = 56,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(80, 80, 80),
                Text = listing.Description,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Controls.Add(_photo);
            Controls.Add(_title);
            Controls.Add(_desc);

            // визуальная рамка
            Paint += ListingCard_Paint;

            // проброс клика с любого дочернего контрола
            foreach (Control c in Controls)
            {
                c.Click += (s, e) => OnClick(e);
            }

            Click += (s, e) => { /* чтобы контейнер реагировал на клик */ };
        }

        private void ListingCard_Paint(object? sender, PaintEventArgs e)
        {
            // тонкая тень/граница для карточки
            using var pen = new Pen(Color.FromArgb(230, 230, 230));
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }
}