using System;
using System.Windows.Forms;
using AgroRegionApp.Forms;
using AgroRegionApp.Models;

namespace AgroRegionApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            while (true)
            {
                AuthenticatedUser user;
                using (var loginForm = new LoginForm())
                {
                    if (loginForm.ShowDialog() != DialogResult.OK)
                        return;
                    user = loginForm.AuthenticatedUser;
                }

                if (user == null)
                    return;

                using (var mainForm = new MainForm(user))
                {
                    mainForm.ShowDialog();
                    if (!mainForm.ReturnToLogin)
                        return;
                }
            }
        }
    }
}
