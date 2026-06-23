using System.Drawing;
using System.Windows.Forms;

namespace AgroRegionApp.UI
{
    internal static class GridHelper
    {
        public static void ApplyStatusColumnFormatting(DataGridView grid, string columnName)
        {
            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.ColumnIndex >= grid.Columns.Count)
                    return;

                var column = grid.Columns[e.ColumnIndex];
                if (column.Name != columnName && column.HeaderText != columnName)
                    return;

                var text = e.Value?.ToString();
                if (string.IsNullOrEmpty(text))
                    return;

                var style = GetStatusStyle(text);
                var row = grid.Rows[e.RowIndex];
                e.CellStyle.BackColor = style.Back;
                e.CellStyle.ForeColor = style.Fore;
                if (row.Selected)
                {
                    e.CellStyle.SelectionBackColor = AppTheme.GridSelect;
                    e.CellStyle.SelectionForeColor = Color.White;
                }
                else
                {
                    e.CellStyle.SelectionBackColor = style.Back;
                    e.CellStyle.SelectionForeColor = style.Fore;
                }
            };
        }

        private static (Color Back, Color Fore) GetStatusStyle(string status)
        {
            switch (status)
            {
                case "Покупатель":
                case "Новый":
                case "Оформлен":
                    return (Color.FromArgb(219, 234, 254), Color.FromArgb(29, 78, 216));
                case "Поставщик":
                    return (Color.FromArgb(254, 249, 195), Color.FromArgb(133, 77, 14));
                case "Подтверждён":
                    return (Color.FromArgb(254, 249, 195), Color.FromArgb(133, 77, 14));
                case "Готов к отгрузке":
                    return (Color.FromArgb(254, 243, 199), Color.FromArgb(146, 64, 14));
                case "Отгружен":
                case "Исполнен":
                case "Принят на склад":
                    return (Color.FromArgb(220, 252, 231), Color.FromArgb(21, 128, 61));
                case "Отменён":
                    return (Color.FromArgb(254, 226, 226), Color.FromArgb(220, 38, 38));
                case "В наличии":
                    return (Color.FromArgb(220, 252, 231), Color.FromArgb(21, 128, 61));
                case "Мало":
                    return (Color.FromArgb(254, 243, 199), Color.FromArgb(146, 64, 14));
                case "Нет в наличии":
                    return (Color.FromArgb(254, 226, 226), Color.FromArgb(220, 38, 38));
                case "Есть долг":
                case "Оплачено":
                    return (Color.FromArgb(243, 244, 246), AppTheme.TextBody);
                default:
                    return (Color.FromArgb(243, 244, 246), AppTheme.TextBody);
            }
        }
    }
}
