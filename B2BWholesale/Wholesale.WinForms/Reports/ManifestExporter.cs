using System;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Wholesale.WinForms.Reports
{
    public static class ManifestExporter
    {
        /// <summary>Saves the manifest as a PDF file using the built-in "Microsoft Print to PDF" printer.</summary>
        public static bool TryExportPdf(ManifestDocument doc, string path, out string error)
        {
            error = null;
            try
            {
                doc.PrinterSettings.PrinterName = "Microsoft Print to PDF";
                if (!doc.PrinterSettings.IsValid)
                {
                    error = "The 'Microsoft Print to PDF' printer was not found on this computer. Use Print preview > Print and choose any PDF printer.";
                    return false;
                }
                doc.PrinterSettings.PrintToFile = true;
                doc.PrinterSettings.PrintFileName = path;
                doc.Print();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static void Preview(IWin32Window owner, ManifestDocument doc)
        {
            using (var dlg = new PrintPreviewDialog())
            {
                dlg.Document = doc;
                dlg.Width = 1000;
                dlg.Height = 800;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ShowDialog(owner);
            }
        }

        public static void PrintWithDialog(IWin32Window owner, ManifestDocument doc)
        {
            using (var dlg = new PrintDialog())
            {
                dlg.Document = doc;
                dlg.UseEXDialog = true;
                if (dlg.ShowDialog(owner) == DialogResult.OK) doc.Print();
            }
        }
    }
}
