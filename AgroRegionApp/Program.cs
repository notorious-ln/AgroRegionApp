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

                try
                {
                    using (var mainForm = new MainForm(user))
                    {
                        mainForm.ShowDialog();
                        if (!mainForm.ReturnToLogin)
                            return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Не удалось открыть главное окно:\n\n" + ex.Message,
                        AppBranding.SystemTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
            }
        }
    }
}
