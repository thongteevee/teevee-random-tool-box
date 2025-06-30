using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Win32;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ThongKhongToolBox.Views
{
    public partial class FormatterPage : Page
    {
        private string _selectedFile = string.Empty;

        private static readonly HashSet<string> _ssnHeaders = new(StringComparer.OrdinalIgnoreCase)
        {
            "SSN", "SOCIAL SECURITY NUMBER"
        };

        public FormatterPage()
        {
            InitializeComponent();
        }

        private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "Select an Excel File"
            };
            if (dlg.ShowDialog() == true)
            {
                _selectedFile = dlg.FileName;
                LabelFileName.Content = _selectedFile;
            }
        }

        private async void BtnProcessFile_Click(object sender, RoutedEventArgs e)
        {
            var origBrush = BtnProcessFile.Background;
            BtnProcessFile.IsEnabled = false;
            BtnProcessFile.Background = new SolidColorBrush(Colors.Gray);

            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.Value = 0;

            var progress = new Progress<int>(pct => ProgressBar.Value = pct);

            try
            {
                if (string.IsNullOrWhiteSpace(_selectedFile) ||
                    !File.Exists(_selectedFile))
                {
                    MessageBox.Show("Select a valid Excel file first.",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }

                var folder = Path.GetDirectoryName(_selectedFile)
                             ?? throw new InvalidOperationException("Cannot determine output folder.");

                string outFile = Path.Combine(
                    folder,
                    $"Formatted_{Path.GetFileName(_selectedFile)}"
                );

                await Task.Run(() =>
                    ProcessExcelFile(_selectedFile, outFile, progress)
                ).ConfigureAwait(false);

                MessageBox.Show($"Done! Saved to:\n{outFile}",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    BtnProcessFile.IsEnabled = true;
                    BtnProcessFile.Background = origBrush;
                    ProgressBar.Visibility = Visibility.Collapsed;
                });
            }
        }

        private static void ProcessExcelFile(
    string inputFile,
    string outputFile,
    IProgress<int> progress)
        {
            using var workbook = new XLWorkbook(inputFile);
            var sheets = workbook.Worksheets.ToList();

            // Count rows for progress
            int totalRows = sheets
                .Select(ws => ws.RangeUsed()?.RowsUsed().Count() - 1 ?? 0)
                .Where(n => n > 0)
                .Sum();
            if (totalRows == 0) totalRows = 1;

            int processed = 0;

            foreach (var ws in sheets)
            {
                var used = ws.RangeUsed();
                if (used == null) continue;

                var dateCols = ws.Row(1)
                                 .CellsUsed()
                                 .Where(c => c.GetString()
                                               .IndexOf("date",
                                                        StringComparison.OrdinalIgnoreCase) >= 0)
                                 .Select(c => c.Address.ColumnNumber)
                                 .ToList();

                int ssnCol = ws.Row(1)
                               .CellsUsed()
                               .FirstOrDefault(c => _ssnHeaders.Contains(c.GetString().Trim()))
                               ?.Address.ColumnNumber ?? -1;

                foreach (var row in used.RowsUsed().Skip(1))
                {
                    foreach (int col in dateCols)
                    {
                        var cell = row.Cell(col);
                        var txt = cell.GetString().Trim();
                        if (string.IsNullOrEmpty(txt)
                            || txt.Equals("null", StringComparison.OrdinalIgnoreCase))
                        {
                            cell.Value = "";
                        }
                        cell.Style.NumberFormat.Format = "MM-dd-yyyy";
                    }

                    if (ssnCol != -1)
                    {
                        var ssnCell = row.Cell(ssnCol);
                        var sb = new StringBuilder();
                        foreach (char c in ssnCell.GetString())
                            if (char.IsDigit(c))
                                sb.Append(c);

                        if (long.TryParse(sb.ToString(), out _))
                        {
                            ssnCell.Style.NumberFormat.Format = "0000000000";
                        }
                    }

                    processed++;
                    progress.Report(processed * 100 / totalRows);
                }

                ws.Columns().AdjustToContents();
            }

            if (File.Exists(outputFile))
                File.Delete(outputFile);
            workbook.SaveAs(outputFile);
            progress.Report(100);
        }

        private static bool TryParseSqlDateString(string input, out DateTimeOffset dt)
        {
            string[] formats =
            {
                "yyyy-MM-dd HH:mm:ss.fff",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd",
                "MM-dd-yyyy",
                "M/d/yyyy h:mm",
                "mm:ss.0",
            };
            return DateTimeOffset.TryParseExact(
                input,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal,
                out dt
            );
        }
    }
}