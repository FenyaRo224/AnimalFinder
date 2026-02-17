using System;
using System.Windows.Forms;
using AnimalFinder.Services;

namespace AnimalFinder
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SupabaseService.InitializeAsync().GetAwaiter().GetResult();

            Application.Run(new MainForm());
        }
    }
}