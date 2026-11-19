using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Wholesale.WinForms.Forms;
using Wholesale.WinForms.Security;

namespace Wholesale.WinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            var vi = new CultureInfo("vi-VN");
            CultureInfo.DefaultThreadCurrentCulture = vi;
            CultureInfo.DefaultThreadCurrentUICulture = vi;
            Thread.CurrentThread.CurrentCulture = vi;
            Thread.CurrentThread.CurrentUICulture = vi;

            ApplicationConfiguration.Initialize();
            Application.ThreadException += (s, e) =>
                MessageBox.Show(e.Exception.Message, "Unexpected error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            while (true)
            {
                Session.Clear();
                using (var login = new LoginForm())
                {
                    if (login.ShowDialog() != DialogResult.OK) return;   // closed / cancelled -> exit
                }
                using (var main = new MainForm())
                {
                    Application.Run(main);
                    if (!main.LoggedOut) return;                          // window closed -> exit; Logout -> show login again
                }
            }
        }
    }
}
