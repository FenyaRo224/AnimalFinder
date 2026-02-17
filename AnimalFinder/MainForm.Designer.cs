namespace AnimalFinder
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // Удалены лишние элементы — теперь всё создаётся программно
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // Только статус — всё остальное создаётся программно
            this.statusStrip = new StatusStrip();
            this.statusLabel = new ToolStripStatusLabel
            {
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Загрузка..."
            };
            this.statusStrip.Items.Add(statusLabel);

            this.ClientSize = new Size(1200, 800);
            this.Controls.Add(statusStrip);
            this.Text = "Animal Finder - Сервис поиска животных";
            this.Load += MainForm_Load;
        }

        // Добавьте обработчик события Load
        private void MainForm_Load(object sender, EventArgs e)
        {
            // Здесь можно добавить инициализацию, если требуется
        }
    }
}