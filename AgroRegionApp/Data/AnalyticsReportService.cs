using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using AgroRegionApp;

namespace AgroRegionApp.Data
{
    internal static class AnalyticsReportService
    {
        private const int WdFormatPdf = 17;
        private const int WdFormatDocumentDefault = 16;
        private const int WdAlertsNone = 0;

        private static readonly object WordLock = new object();
        private static dynamic _cachedWord;

        static AnalyticsReportService()
        {
            Application.ApplicationExit += (_, __) => ShutdownWord();
        }

        public static string GetDefaultOutputFolder(int year)
        {
            var preferred = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "AgroTorg", "Reports", "Analytics", year.ToString());
            Directory.CreateDirectory(preferred);
            return preferred;
        }

        public static (string WordPath, string PdfPath) GenerateWordAndPdf(AnalyticsSummary data, int year, string outputFolder)
        {
            if (!WordTemplateService.IsAvailable)
                throw new InvalidOperationException(
                    "Для формирования отчёта требуется установленный Microsoft Word.");

            Directory.CreateDirectory(outputFolder);
            var baseName = $"Analitika_{year}";
            var wordPath = Path.GetFullPath(Path.Combine(outputFolder, baseName + ".docx"));
            var pdfPath = Path.GetFullPath(Path.Combine(outputFolder, baseName + ".pdf"));

            TryDelete(wordPath);
            TryDelete(pdfPath);

            var workDir = Path.Combine(Path.GetTempPath(), "AgroRegion", "Reports");
            Directory.CreateDirectory(workDir);
            var tempHtml = Path.Combine(workDir, Guid.NewGuid().ToString("N") + ".htm");

            dynamic word = null;
            dynamic doc = null;
            var wordFromCache = false;
            try
            {
                var html = BuildWordHtml(data, year);
                File.WriteAllText(tempHtml, html, new UTF8Encoding(true));

                word = AcquireWord(out wordFromCache);
                doc = word.Documents.Open(
                    FileName: tempHtml,
                    ConfirmConversions: false,
                    ReadOnly: true,
                    AddToRecentFiles: false,
                    Visible: false);

                doc.SaveAs2(wordPath, WdFormatDocumentDefault);
                doc.SaveAs2(pdfPath, WdFormatPdf);
                doc.Close(false);
                doc = null;
            }
            catch (Exception ex)
            {
                if (wordFromCache)
                    InvalidateWordCache();

                var details = ex is COMException com ? $" (код {com.ErrorCode})" : "";
                throw new InvalidOperationException(
                    "Не удалось сформировать отчёт через Word" + details + ": " + ex.Message, ex);
            }
            finally
            {
                try
                {
                    if (doc != null)
                    {
                        doc.Close(false);
                        ReleaseComObject(doc);
                    }
                }
                catch { /* ignore */ }

                if (!wordFromCache)
                    ReleaseWord(word);

                TryDelete(tempHtml);
            }

            if (!File.Exists(wordPath))
                throw new InvalidOperationException("Word не создал файл DOCX: " + wordPath);
            if (!File.Exists(pdfPath))
                throw new InvalidOperationException("Word не создал файл PDF: " + pdfPath);

            return (wordPath, pdfPath);
        }

        public static string ExportToExcel(AnalyticsSummary data, int year, string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var html = BuildExcelHtml(data, year);
            File.WriteAllText(filePath, html, new UTF8Encoding(true));
            return filePath;
        }

        private static dynamic AcquireWord(out bool fromCache)
        {
            lock (WordLock)
            {
                if (_cachedWord != null)
                {
                    try
                    {
                        _ = (string)_cachedWord.Version;
                        fromCache = true;
                        return _cachedWord;
                    }
                    catch
                    {
                        _cachedWord = null;
                    }
                }

                var word = (dynamic)Activator.CreateInstance(Type.GetTypeFromProgID("Word.Application"));
                if (word == null)
                    throw new InvalidOperationException("Не удалось запустить Microsoft Word.");

                word.Visible = false;
                word.DisplayAlerts = WdAlertsNone;
                word.ScreenUpdating = false;
                try { word.Options.SavePropertiesPrompt = false; } catch { /* ignore */ }
                try { word.Options.UpdateLinksAtOpen = false; } catch { /* ignore */ }

                _cachedWord = word;
                fromCache = true;
                return word;
            }
        }

        private static void InvalidateWordCache()
        {
            lock (WordLock)
            {
                ReleaseWord(_cachedWord);
                _cachedWord = null;
            }
        }

        private static void ReleaseWord(dynamic word)
        {
            if (word == null) return;
            try { word.Quit(false); }
            catch { /* ignore */ }
            ReleaseComObject(word);
        }

        private static void ShutdownWord()
        {
            lock (WordLock)
            {
                ReleaseWord(_cachedWord);
                _cachedWord = null;
            }
        }

        private static string BuildWordHtml(AnalyticsSummary data, int year)
        {
            var sb = new StringBuilder(8192);
            sb.AppendLine("<html xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" xmlns=\"http://www.w3.org/TR/REC-html40\">");
            sb.AppendLine("<head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            sb.AppendLine("<!--[if gte mso 9]><xml><w:WordDocument><w:View>Print</w:View><w:DoNotOptimizeForBrowser/></w:WordDocument></xml><![endif]-->");
            sb.AppendLine("<style>");
            sb.AppendLine("@page Section1 { size: 595.3pt 841.9pt; margin: 35.43pt; }");
            sb.AppendLine("div.Section1 { page: Section1; }");
            sb.AppendLine("body, p, td, th, h2, h3 { font-family: 'Times New Roman', serif; font-size: 14pt; color: #000000; }");
            sb.AppendLine("p { text-align: justify; line-height: 150%; mso-line-height-rule: exactly; margin: 0 0 6pt 0; }");
            sb.AppendLine("h2 { font-size: 16pt; font-weight: bold; text-align: center; color: #000000; margin: 0 0 8pt 0; }");
            sb.AppendLine("h3 { font-size: 14pt; font-weight: bold; text-align: left; color: #000000; margin: 12pt 0 6pt 0; }");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; mso-table-layout-alt: fixed; }");
            sb.AppendLine("td, th { border: 1pt solid #000000; padding: 4pt; vertical-align: top; background: #FFFFFF; color: #000000; }");
            sb.AppendLine("th { font-weight: bold; text-align: center; }");
            sb.AppendLine("</style></head><body><div class=\"Section1\">");

            sb.AppendLine("<h2>АНАЛИТИЧЕСКИЙ ОТЧЁТ</h2>");
            sb.Append("<p style=\"text-align:center;color:#000000\">за ").Append(year).AppendLine(" год</p>");
            sb.Append("<p style=\"color:#000000\">Организация: ").Append(Esc(CompanyProfile.LegalName))
                .Append(". Дата формирования: ").Append(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).AppendLine(".</p>");

            AppendWordTable(sb, "1. Сводные показатели", new[] { "Показатель", "Значение" }, new[]
            {
                new[] { "Продажи за период", AnalyticsService.FormatMoneyShort(data.TotalSales) },
                new[] { "Закупки за период", AnalyticsService.FormatMoneyShort(data.TotalPurchases) },
                new[] { "Итого остатки", data.TotalStockTons + " т" },
                new[] { "Дебиторская задолженность", AnalyticsService.FormatMoney(data.TotalDebt) }
            });

            var salesRows = new List<string[]>();
            foreach (var m in data.SalesByMonth)
                salesRows.Add(new[] { m.MonthName, m.QuantityTons.ToString(), FormatNum(m.Amount) });
            AppendWordTable(sb, "2. Сводка продаж по месяцам", new[] { "Месяц", "Кол-во (т)", "Сумма (руб.)" }, salesRows);

            var cmpRows = new List<string[]>();
            for (var i = 0; i < data.SalesByMonth.Count; i++)
            {
                var s = data.SalesByMonth[i];
                var p = i < data.PurchasesByMonth.Count ? data.PurchasesByMonth[i] : new MonthlyAmountRow();
                var diff = s.Amount - p.Amount;
                cmpRows.Add(new[]
                {
                    s.MonthName,
                    FormatNum(s.Amount),
                    FormatNum(p.Amount),
                    (diff >= 0 ? "+" : "") + FormatNum(diff)
                });
            }
            AppendWordTable(sb, "3. Сравнение продаж и закупок",
                new[] { "Месяц", "Продажи (руб.)", "Закупки (руб.)", "Разница (руб.)" }, cmpRows);

            var stockRows = new List<string[]>();
            foreach (var s in data.Stocks)
            {
                stockRows.Add(new[]
                {
                    s.ProductName,
                    s.Warehouse1Tons.ToString(),
                    s.Warehouse2Tons.ToString(),
                    s.TotalTons.ToString(),
                    s.CheckDate?.ToString("dd.MM.yyyy") ?? "-",
                    s.Status
                });
            }
            AppendWordTable(sb, "4. Складские запасы",
                new[] { "Культура", "Склад 1 (т)", "Склад 2 (т)", "Итого (т)", "Дата проверки", "Состояние" },
                stockRows);

            var debtRows = new List<string[]>();
            foreach (var d in data.Debtors)
            {
                debtRows.Add(new[]
                {
                    d.CustomerName,
                    d.OrderCount.ToString(),
                    FormatNum(d.OrderTotal),
                    FormatNum(d.PaidAmount),
                    d.DebtAmount > 0 ? FormatNum(d.DebtAmount) : "-",
                    d.Status
                });
            }
            if (debtRows.Count == 0)
                debtRows.Add(new[] { "-", "0", "0", "0", "-", "Нет данных" });
            AppendWordTable(sb, "5. Дебиторская задолженность",
                new[] { "Покупатель", "Заказов", "Сумма заказов (руб.)", "Оплачено (руб.)", "Задолженность (руб.)", "Статус" },
                debtRows);

            sb.AppendLine("</div></body></html>");
            return sb.ToString();
        }

        private static void AppendWordTable(StringBuilder sb, string title, string[] headers, IList<string[]> rows)
        {
            sb.Append("<h3>").Append(Esc(title)).AppendLine("</h3>");
            sb.AppendLine("<table border=\"1\" cellspacing=\"0\" cellpadding=\"4\">");
            sb.Append("<tr>");
            foreach (var h in headers)
                sb.Append("<th style=\"color:#000000;background:#FFFFFF\">").Append(Esc(h)).Append("</th>");
            sb.AppendLine("</tr>");
            foreach (var row in rows)
            {
                sb.Append("<tr>");
                foreach (var cell in row)
                    sb.Append("<td style=\"color:#000000;background:#FFFFFF\">").Append(Esc(cell)).Append("</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table>");
        }

        private static string BuildExcelHtml(AnalyticsSummary data, int year)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
            sb.AppendLine("<head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            sb.AppendLine("<style>");
            sb.AppendLine("td,th{font-family:'Times New Roman';font-size:14pt;mso-number-format:\\@;}");
            sb.AppendLine("th{font-weight:bold;background:#eef2f7;}");
            sb.AppendLine("h2{font-family:'Times New Roman';font-size:16pt;}");
            sb.AppendLine("</style></head><body>");

            sb.Append("<h2>Аналитический отчёт за ").Append(year).AppendLine(" год</h2>");
            sb.Append("<p style=\"font-family:'Times New Roman';font-size:14pt\">");
            sb.Append(CompanyProfile.LegalName).Append(". Дата: ").Append(DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
            sb.AppendLine("</p>");

            AppendHtmlTable(sb, "Сводные показатели", new[] { "Показатель", "Значение" }, new[]
            {
                new[] { "Продажи за период", AnalyticsService.FormatMoneyShort(data.TotalSales) },
                new[] { "Закупки за период", AnalyticsService.FormatMoneyShort(data.TotalPurchases) },
                new[] { "Итого остатки", data.TotalStockTons + " т" },
                new[] { "Дебиторская задолженность", AnalyticsService.FormatMoney(data.TotalDebt) }
            });

            var salesBody = new List<string[]>();
            foreach (var m in data.SalesByMonth)
                salesBody.Add(new[] { m.MonthName, m.QuantityTons.ToString(), FormatNum(m.Amount) });
            AppendHtmlTable(sb, "Продажи по месяцам", new[] { "Месяц", "Кол-во (т)", "Сумма (₽)" }, salesBody.ToArray());

            var cmpBody = new List<string[]>();
            for (var i = 0; i < data.SalesByMonth.Count; i++)
            {
                var s = data.SalesByMonth[i];
                var p = i < data.PurchasesByMonth.Count ? data.PurchasesByMonth[i] : new MonthlyAmountRow();
                var diff = s.Amount - p.Amount;
                cmpBody.Add(new[] { s.MonthName, FormatNum(s.Amount), FormatNum(p.Amount), (diff >= 0 ? "+" : "") + FormatNum(diff) });
            }
            AppendHtmlTable(sb, "Продажи vs закупки", new[] { "Месяц", "Продажи (₽)", "Закупки (₽)", "Разница (₽)" }, cmpBody.ToArray());

            var stockBody = new List<string[]>();
            foreach (var s in data.Stocks)
                stockBody.Add(new[] { s.ProductName, s.Warehouse1Tons.ToString(), s.Warehouse2Tons.ToString(), s.TotalTons.ToString(), s.CheckDate?.ToString("dd.MM.yyyy") ?? "—", s.Status });
            AppendHtmlTable(sb, "Складские запасы", new[] { "Культура", "Склад №1 (т)", "Склад №2 (т)", "Итого (т)", "Дата проверки", "Состояние" }, stockBody.ToArray());

            var debtBody = new List<string[]>();
            foreach (var d in data.Debtors)
                debtBody.Add(new[] { d.CustomerName, d.OrderCount.ToString(), FormatNum(d.OrderTotal), FormatNum(d.PaidAmount), d.DebtAmount > 0 ? FormatNum(d.DebtAmount) : "—", d.Status });
            if (debtBody.Count == 0)
                debtBody.Add(new[] { "—", "0", "0", "0", "—", "Нет данных" });
            AppendHtmlTable(sb, "Дебиторская задолженность", new[] { "Покупатель", "Заказов", "Сумма заказов (₽)", "Оплачено (₽)", "Задолженность (₽)", "Статус" }, debtBody.ToArray());

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static void AppendHtmlTable(StringBuilder sb, string title, string[] headers, string[][] rows)
        {
            sb.Append("<h3 style=\"font-family:'Times New Roman';font-size:14pt;font-weight:bold\">")
                .Append(Esc(title)).AppendLine("</h3>");
            sb.AppendLine("<table border=\"1\" cellspacing=\"0\" cellpadding=\"4\">");
            sb.Append("<tr>");
            foreach (var h in headers)
                sb.Append("<th>").Append(Esc(h)).Append("</th>");
            sb.AppendLine("</tr>");
            foreach (var row in rows)
            {
                sb.Append("<tr>");
                foreach (var cell in row)
                    sb.Append("<td>").Append(Esc(cell)).Append("</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table><br/>");
        }

        private static string Esc(string value) =>
            (value ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

        private static string FormatNum(decimal value) =>
            value.ToString("N0", CultureInfo.GetCultureInfo("ru-RU")).Replace('\u00A0', ' ');

        private static void ReleaseComObject(object comObject)
        {
            if (comObject == null) return;
            try { Marshal.ReleaseComObject(comObject); }
            catch { /* ignore */ }
        }

        private static void TryDelete(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
            try { File.Delete(path); }
            catch { /* ignore */ }
        }
    }
}
