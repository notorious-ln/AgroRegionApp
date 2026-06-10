using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AgroRegionApp.Data
{
    internal static class DocumentService
    {
        private static readonly Dictionary<string, string> SalesTemplateByDocumentName =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "Счёт на оплату", "SchetNaOplatu.doc" },
                { "Договор купли-продажи", "DogovorPostavshika.doc" },
                { "Товарная накладная (ТОРГ-12)", "Torg12.doc" },
                { "ТТН (при необходимости)", "TTN.doc" }
            };

        private static readonly Dictionary<string, string> PurchaseTemplateByDocumentName =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "Счёт на оплату", "SchetNaOplatu.doc" },
                { "Договор купли-продажи", "DogovorKupliProdazhi.doc" },
                { "Товарная накладная (ТОРГ-12)", "Torg12.doc" },
                { "ТТН (при необходимости)", "TTN.doc" }
            };

        public static string GetDefaultOutputFolder(string orderNumber)
        {
            var preferred = Path.Combine(@"C:\АгроТорг", "Документы", orderNumber);
            try
            {
                Directory.CreateDirectory(preferred);
                return preferred;
            }
            catch
            {
                var fallback = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "АгроТорг", "Документы", orderNumber);
                Directory.CreateDirectory(fallback);
                return fallback;
            }
        }

        public static List<string> GenerateSalesPackage(
            SalesOrderRow order,
            IEnumerable<string> documentNames,
            string outputFolder)
        {
            EnsureWordAvailable();
            var context = DocumentContext.FromSales(order);
            return GeneratePackage(context, documentNames, SalesTemplateByDocumentName, outputFolder);
        }

        public static List<string> GeneratePurchasePackage(
            PurchaseOrderRow order,
            IEnumerable<string> documentNames,
            string outputFolder)
        {
            EnsureWordAvailable();
            var context = DocumentContext.FromPurchase(order);
            return GeneratePackage(context, documentNames, PurchaseTemplateByDocumentName, outputFolder);
        }

        private static List<string> GeneratePackage(
            DocumentContext context,
            IEnumerable<string> documentNames,
            IReadOnlyDictionary<string, string> templateMap,
            string outputFolder)
        {
            var documents = documentNames
                .Where(name => templateMap.ContainsKey(name))
                .Select(name => (templateMap[name], SanitizeFileName(name)))
                .ToList();

            if (documents.Count == 0)
                throw new InvalidOperationException("Не удалось сопоставить выбранные документы с шаблонами.");

            return WordTemplateService.GeneratePdfBatch(context, documents, outputFolder);
        }

        private static void EnsureWordAvailable()
        {
            if (!WordTemplateService.IsAvailable)
            {
                throw new InvalidOperationException(
                    "Для формирования документов по шаблонам требуется установленный Microsoft Word.");
            }
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
