using System;
using System.Windows.Forms;
using OfficeOpenXml;

namespace GestionEventos
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Licencia EPPlus para uso no comercial / educativo
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}