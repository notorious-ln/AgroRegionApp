using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using AgroRegionApp;

namespace AgroRegionApp.Data
{
    internal static class WordTemplateService
    {
        private const int WdReplaceAll = 2;
        private const int WdReplaceOne = 1;
        private const int WdFormatPdf = 17;
        private const int WdAlertsNone = 0;

        private const string DogovorDatePlaceholder = "«_____» _________ 202___г.";
        private const string BuyerPartyPattern = ", далее именуем*«Покупатель»";
        private const string SupplierPartyPattern = "и, *, далее именуемое «Поставщик»";

        public static bool IsAvailable => Type.GetTypeFromProgID("Word.Application") != null;

        public static string GetTemplatesDirectory()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Documents");
        }

        public static List<string> GeneratePdfBatch(
            DocumentContext context,
            IReadOnlyList<(string TemplateFile, string OutputBaseName)> documents,
            string outputFolder)
        {
            if (documents == null || documents.Count == 0)
                throw new ArgumentException("Не выбраны документы для формирования.");

            Directory.CreateDirectory(outputFolder);

            var created = new List<string>();
            foreach (var doc in documents)
            {
                created.Add(GenerateSinglePdf(context, doc.TemplateFile, outputFolder, doc.OutputBaseName));
            }

            return created;
        }

        private static string GenerateSinglePdf(
            DocumentContext context,
            string templateFileName,
            string outputFolder,
            string outputBaseName)
        {
            var templatePath = Path.Combine(GetTemplatesDirectory(), templateFileName);
            if (!File.Exists(templatePath))
                throw new FileNotFoundException("Не найден шаблон документа: " + templateFileName);

            var workDir = Path.Combine(Path.GetTempPath(), "AgroRegion", "Docs");
            Directory.CreateDirectory(workDir);

            var workDoc = Path.Combine(workDir, Guid.NewGuid().ToString("N") + ".doc");
            var pdfPath = Path.Combine(outputFolder, outputBaseName + ".pdf");
            File.Copy(templatePath, workDoc, true);

            dynamic word = null;
            dynamic doc = null;
            try
            {
                word = Activator.CreateInstance(Type.GetTypeFromProgID("Word.Application"));
                word.Visible = false;
                word.DisplayAlerts = WdAlertsNone;
                word.ScreenUpdating = false;

                doc = word.Documents.Open(
                    FileName: workDoc,
                    ConfirmConversions: false,
                    ReadOnly: false,
                    AddToRecentFiles: false,
                    Visible: false);

                ApplyTemplateData(doc, context, templateFileName);
                doc.SaveAs2(pdfPath, WdFormatPdf);
                doc.Close(SaveChanges: false);

                word.Quit(SaveChanges: false);
                word = null;
                doc = null;
            }
            catch (COMException ex)
            {
                throw new InvalidOperationException(
                    "Не удалось сформировать PDF через Word. Убедитесь, что Word установлен и не занят другим процессом.",
                    ex);
            }
            finally
            {
                ReleaseComObject(doc);
                ReleaseComObject(word);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                TryDelete(workDoc);
            }

            if (!File.Exists(pdfPath))
                throw new InvalidOperationException("Word не создал файл PDF: " + outputBaseName);

            return pdfPath;
        }

        private static void ApplyTemplateData(dynamic doc, DocumentContext order, string templateFileName)
        {
            ApplyCommon(doc, order);

            switch (templateFileName.ToUpperInvariant())
            {
                case "SCHETNAOPLATU.DOC":
                    ApplySchet(doc, order);
                    break;
                case "DOGOVORKUPLIPRODAZHI.DOC":
                    ApplyDogovorBuyerSide(doc, order);
                    break;
                case "DOGOVORPOSTAVSHIKA.DOC":
                    ApplyDogovorSupplierSide(doc, order);
                    break;
                case "TORG12.DOC":
                    ApplyTorg12(doc, order);
                    break;
                case "TTN.DOC":
                    ApplyTtn(doc, order);
                    break;
            }
        }

        private static void ApplySchet(dynamic doc, DocumentContext order)
        {
            ReplaceAll(doc,
                "Счет № __________ от ___.___.20___ г.",
                $"Счет № {order.OrderNumber} от {order.DateFormatted} г.");

            if (order.IsPurchase)
            {
                TrySetCell(doc, 1, 3, 2, "Оплата по реквизитам поставщика");
                TrySetCell(doc, 1, 6, 2, order.CounterpartyName);
            }
            else
            {
                TrySetCell(doc, 1, 1, 3, CompanyProfile.Bik);
                TrySetCell(doc, 1, 2, 3, CompanyProfile.CorrAccount);
                TrySetCell(doc, 1, 3, 2, CompanyProfile.BankName);
                TrySetCell(doc, 1, 5, 2, CompanyProfile.Inn);
                TrySetCell(doc, 1, 5, 3, CompanyProfile.Kpp);
                TrySetCell(doc, 1, 5, 4, CompanyProfile.BankAccount);
                TrySetCell(doc, 1, 6, 2, CompanyProfile.LegalName);
            }

            TrySetCell(doc, 3, 1, 2, order.SupplierLine);
            TrySetCell(doc, 3, 2, 2, order.BuyerLine);

            TrySetCell(doc, 4, 2, 2, order.ProductFull);
            TrySetCell(doc, 4, 2, 3, order.QuantityTons.ToString());
            TrySetCell(doc, 4, 2, 4, "т");
            TrySetCell(doc, 4, 2, 5, order.UnitPriceFormatted);
            TrySetCell(doc, 4, 2, 6, order.SumFormatted);

            TrySetCell(doc, 5, 1, 2, order.SumFormatted);
            TrySetCell(doc, 5, 2, 2, "без НДС");
            TrySetCell(doc, 5, 3, 2, order.SumFormatted);

            ReplaceAll(doc,
                "Всего наименований _____, на сумму ________________ руб.",
                $"Всего наименований {order.QuantityTons}, на сумму {order.SumFormatted} руб.");

            TrySetCell(doc, 6, 1, 1, CompanyProfile.DirectorTitle);
            TrySetCell(doc, 6, 1, 2, CompanyProfile.DirectorName);
        }

        private static void ApplyDogovorBuyerSide(dynamic doc, DocumentContext order)
        {
            ApplyDogovorCommon(doc, order);
            ReplaceWildcardAll(doc,
                SupplierPartyPattern,
                $"и, {order.CounterpartyName}, далее именуемое «Поставщик»");
            FillDogovorPartiesTable(doc, order);
        }

        private static void ApplyDogovorSupplierSide(dynamic doc, DocumentContext order)
        {
            ApplyDogovorCommon(doc, order);
            ReplaceWildcardAll(doc,
                BuyerPartyPattern,
                $", {order.CounterpartyName}, далее именуемое «Покупатель»");
            FillDogovorPartiesTable(doc, order);
        }

        private static void ApplyDogovorCommon(dynamic doc, DocumentContext order)
        {
            ReplaceAll(doc, "ПОСТАВКИ №", "ПОСТАВКИ № " + order.OrderNumber);
            ReplaceAll(doc, DogovorDatePlaceholder, FormatDogovorDate(order.Date));
            ReplaceAll(doc, "<_____> _________ 202___г.", FormatDogovorDate(order.Date));
            ReplaceAll(doc, "указать: вид Товара, единицы измерения, цена за единицу",
                $"{order.ProductFull}, т, {order.UnitPriceFormatted} {order.UnitPriceLabel}");
        }

        private static void FillDogovorPartiesTable(dynamic doc, DocumentContext order)
        {
            try
            {
                if (doc.Tables.Count < 1)
                    return;

                dynamic table = doc.Tables[1];
                if (table.Rows.Count < 2)
                    return;

                if (order.IsPurchase)
                {
                    TrySetCell(table, 2, 1, CompanyProfile.FullRequisites);
                    TrySetCell(table, 2, 2, order.CounterpartyLine);
                }
                else
                {
                    TrySetCell(table, 2, 1, order.CounterpartyLine);
                }
            }
            catch
            {
                // ignore
            }
        }

        private static void ApplyTorg12(dynamic doc, DocumentContext order)
        {
            if (order.IsPurchase)
            {
                TrySetCell(doc, 1, 4, 2, order.CounterpartyLine);
                TrySetCell(doc, 1, 6, 2, CompanyProfile.OrganizationLine);
                TrySetCell(doc, 1, 7, 2, order.CounterpartyLine);
                TrySetCell(doc, 1, 8, 2, CompanyProfile.OrganizationLine);
            }
            else
            {
                TrySetCell(doc, 1, 4, 2, CompanyProfile.OrganizationLine);
                TrySetCell(doc, 1, 6, 2, order.CounterpartyLine);
                TrySetCell(doc, 1, 7, 2, CompanyProfile.OrganizationLine);
                TrySetCell(doc, 1, 8, 2, order.CounterpartyLine);
            }
            TrySetCell(doc, 1, 9, 2, order.ContractLabel);
            TrySetCell(doc, 1, 9, 4, order.OrderNumber);
            TrySetCell(doc, 1, 10, 2, "Договор поставки");
            TrySetCell(doc, 1, 10, 4, order.DateFormatted);

            if (doc.Tables.Count >= 2)
            {
                TrySetCell(doc, 2, 1, 2, order.OrderNumber);
                TrySetCell(doc, 2, 1, 3, order.DateFormatted);
            }

            TrySetCell(doc, 3, 4, 2, order.ProductFull);
            TrySetCell(doc, 3, 4, 4, "т");
            TrySetCell(doc, 3, 4, 5, "168");
            TrySetCell(doc, 3, 4, 7, order.QuantityTons.ToString());
            TrySetCell(doc, 3, 4, 8, order.UnitPriceFormatted);
            TrySetCell(doc, 3, 4, 9, order.SumFormatted);
            TrySetCell(doc, 3, 4, 11, order.SumFormatted);

            var totalRow = FindTorg12TotalRow(doc);
            if (totalRow > 0)
            {
                TrySetCell(doc, 3, totalRow, 7, order.QuantityTons.ToString());
                TrySetCell(doc, 3, totalRow, 9, order.SumFormatted);
                TrySetCell(doc, 3, totalRow, 11, order.SumFormatted);
            }
        }

        private static int FindTorg12TotalRow(dynamic doc)
        {
            try
            {
                dynamic table = doc.Tables[3];
                for (var row = table.Rows.Count; row >= 1; row--)
                {
                    var text = GetCellText(table, row, 1);
                    if (text != null && text.IndexOf("Итого", StringComparison.OrdinalIgnoreCase) >= 0)
                        return row;
                }
            }
            catch
            {
                // ignore
            }

            return -1;
        }

        private static void ApplyTtn(dynamic doc, DocumentContext order)
        {
            TrySetCell(doc, 1, 3, 3, order.TtnNumber);
            TrySetCell(doc, 1, 4, 3, order.DateFormatted);
            if (order.IsPurchase)
            {
                TrySetCell(doc, 1, 5, 2, order.CounterpartyLine);
                TrySetCell(doc, 1, 6, 2, CompanyProfile.OrganizationLine);
                TrySetCell(doc, 1, 8, 2, CompanyProfile.OrganizationLine);
            }
            else
            {
                TrySetCell(doc, 1, 5, 2, CompanyProfile.OrganizationLine);
                TrySetCell(doc, 1, 6, 2, order.CounterpartyLine);
                TrySetCell(doc, 1, 8, 2, order.CounterpartyLine);
            }

            TrySetCell(doc, 2, 3, 6, order.ProductFull);
            TrySetCell(doc, 2, 3, 7, "т");
            TrySetCell(doc, 2, 3, 10, order.QuantityTons.ToString());
            TrySetCell(doc, 2, 3, 5, order.UnitPriceFormatted);
            TrySetCell(doc, 2, 3, 11, order.SumFormatted);

            ReplaceAll(doc, "Всего наименований", "Всего наименований " + order.QuantityTons);
            ReplaceWildcardAll(doc, "Масса груза (нетто)*", "Масса груза (нетто) " + order.QuantityTons + " т");
            ReplaceWildcardAll(doc, "Всего к оплате*", "Всего к оплате " + order.SumFormatted + " руб.");
            ReplaceWildcardAll(doc, "Всего отпущено на сумму*", "Всего отпущено на сумму " + order.SumFormatted + " руб.");
        }

        private static void ApplyCommon(dynamic doc, DocumentContext order)
        {
            ReplaceAll(doc, "<ИНН>", order.CounterpartyInnSafe);
            ReplaceAll(doc, "<Сумма>", order.SumFormatted);
            ReplaceAll(doc, "<Количество>", order.QuantityTons.ToString());
            ReplaceAll(doc, "<Цена>", order.UnitPriceFormatted);
            ReplaceAll(doc, "<Сорт>", order.ProductVariety ?? "—");
            ReplaceAll(doc, "<Склад>", order.WarehouseName ?? "—");
            ReplaceAll(doc, "<Культура>", order.ProductFull);
            ReplaceAll(doc, "<Поставщик>", order.IsPurchase ? (order.CounterpartyName ?? "—") : CompanyProfile.LegalName);
            ReplaceAll(doc, "<Покупатель>", order.IsPurchase ? CompanyProfile.LegalName : (order.CounterpartyName ?? "—"));
            ReplaceAll(doc, "<Договор>", order.OrderNumber);
            ReplaceAll(doc, "<Номер>", order.OrderNumber);
            ReplaceAll(doc, "<ТТН>", order.TtnNumber);
            ReplaceAll(doc, "<Счет>", order.OrderNumber);
            ReplaceAll(doc, "<_____>", order.OrderNumber);
        }

        private static string FormatDogovorDate(DateTime date)
        {
            var month = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.GetMonthName(date.Month);
            return $"«{date:dd}» {month} {date:yyyy}г.";
        }

        private static void TrySetCell(dynamic doc, int tableIndex, int row, int column, string text)
        {
            try
            {
                if (doc.Tables.Count < tableIndex)
                    return;

                SetCellText(doc.Tables[tableIndex].Cell(row, column), text);
            }
            catch
            {
                // ignore
            }
        }

        private static void TrySetCell(dynamic table, int row, int column, string text)
        {
            try
            {
                SetCellText(table.Cell(row, column), text);
            }
            catch
            {
                // ignore
            }
        }

        private static string GetCellText(dynamic table, int row, int column)
        {
            try
            {
                return (table.Cell(row, column).Range.Text as string)?.Replace("\a", "").Replace("\r", "").Trim();
            }
            catch
            {
                return null;
            }
        }

        private static void SetCellText(dynamic cell, string text)
        {
            cell.Range.Text = text ?? "";
        }

        private static void ReplaceAll(dynamic doc, string findText, string replaceText)
        {
            Replace(doc, findText, replaceText, WdReplaceAll, false);
        }

        private static void ReplaceOne(dynamic doc, string findText, string replaceText)
        {
            Replace(doc, findText, replaceText, WdReplaceOne, false);
        }

        private static void ReplaceWildcardAll(dynamic doc, string findText, string replaceText)
        {
            Replace(doc, findText, replaceText, WdReplaceAll, true);
        }

        private static void Replace(dynamic doc, string findText, string replaceText, int replaceMode, bool wildcards)
        {
            if (string.IsNullOrEmpty(findText))
                return;

            try
            {
                dynamic find = doc.Content.Find;
                find.ClearFormatting();
                find.Replacement.ClearFormatting();
                find.Execute(
                    FindText: findText,
                    MatchCase: false,
                    MatchWholeWord: false,
                    MatchWildcards: wildcards,
                    MatchSoundsLike: false,
                    MatchAllWordForms: false,
                    Forward: true,
                    Wrap: 1,
                    Format: false,
                    ReplaceWith: replaceText ?? "",
                    Replace: replaceMode);
            }
            catch
            {
                // ignore
            }
        }

        private static void ReleaseComObject(object comObject)
        {
            if (comObject == null)
                return;

            try
            {
                Marshal.ReleaseComObject(comObject);
            }
            catch
            {
                // ignore
            }
        }

        private static void TryDelete(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            try
            {
                File.Delete(path);
            }
            catch
            {
                // ignore
            }
        }
    }
}
