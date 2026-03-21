using System;
using System.IO;
using System.Windows.Forms;
using OfficeOpenXml;

namespace GestionEventos
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                // Licencia EPPlus para uso no comercial / educativo
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += (_, e) => RegistrarError("thread", e.Exception);
                AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                    RegistrarError("domain", e.ExceptionObject as Exception);

                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                string logPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "startup-error.log");
                File.WriteAllText(logPath, ex.ToString());

                MessageBox.Show(
                    "No se pudo iniciar la aplicación.\n\n" +
                    ex.Message +
                    "\n\nSe generó log en:\n" + logPath,
                    "Error de inicio",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void RegistrarError(string tipo, Exception? ex)
        {
            string msg = ex?.ToString() ?? "Error no identificado.";
            string logPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "startup-error.log");

            File.WriteAllText(logPath,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {tipo}\n{msg}");

            MessageBox.Show(
                "Se produjo un error en ejecución.\n\n" +
                (ex?.Message ?? "Error no identificado") +
                "\n\nSe generó log en:\n" + logPath,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}