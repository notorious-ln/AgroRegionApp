using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AgroRegionApp.Data;
using AgroRegionApp.UI;

namespace AgroRegionApp.Views
{
    internal sealed class SalesView : UserControl
    {
        private readonly DataGridView _grid;
        private Panel _detailPanel;
        private TextBox _txtSearch;
        private ComboBox _cmbStatus;
        private Label _lblCustomer;
        private Label _lblProduct;
        private Label _lblWarehouse;
        private Label _lblPrice;
        private Label _lblStatus;
        private Button _btnDocs;
        private Button _btnStocks;

        private List<SalesOrderRow> _allOrders = new List<SalesOrderRow>();

        public SalesView()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.ContentBg;

            Controls.Add(BuildDetailPanel());
            Controls.Add(_grid = BuildGrid());
            Controls.Add(BuildToolbar());

            LoadData();
        }

        private Panel BuildToolbar()
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = AppTheme.ContentBg };

            var btnCreate = UiControls.CreateButton("＋  Создать заказ", true, 130);
            btnCreate.Location = new Point(0, 4);
            btnCreate.Enabled = false;

            var btnEdit = UiControls.CreateButton("✏  Изменить", false, 100);
            btnEdit.Location = new Point(136, 4);
            btnEdit.Enabled = false;

            var btnDelete = UiControls.CreateButton("✕  Удалить", false, 95, danger: true);
            btnDelete.Location = new Point(238, 4);
            btnDelete.Enabled = false;

            _btnDocs = UiControls.CreateButton("📄  Документы", false, 110);
            _btnDocs.Location = new Point(335, 4);
            _btnDocs.Enabled = false;

            _btnStocks = UiControls.CreateButton("🔍  Запрос остатков", false, 140);
            _btnStocks.Location = new Point(448, 4);
            _btnStocks.Enabled = false;

            _txtSearch = UiControls.CreateFieldBox();
            _txtSearch.Width = 200;
            _txtSearch.Location = new Point(680, 5);
            _txtSearch.TextChanged += (s, e) => ApplyFilter();

            _cmbStatus = new ComboBox
            {
                Location = new Point(888, 5),
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTheme.FontUi
            };
            _cmbStatus.Items.AddRange(new object[]
            {
                "Все статусы", "Новый", "Подтверждён", "Готов к отгрузке", "Отгружен"
            });
            _cmbStatus.SelectedIndex = 0;
            _cmbStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            panel.Controls.Add(btnCreate);
            panel.Controls.Add(btnEdit);
            panel.Controls.Add(btnDelete);
            panel.Controls.Add(_btnDocs);
            panel.Controls.Add(_btnStocks);
            panel.Controls.Add(_txtSearch);
            panel.Controls.Add(_cmbStatus);
            panel.Resize += (s, e) =>
            {
                _cmbStatus.Left = panel.Width - _cmbStatus.Width;
                _txtSearch.Left = _cmbStatus.Left - _txtSearch.Width - 8;
            };
            return panel;
        }

        private DataGridView BuildGrid()
        {
            var grid = UiControls.CreateGrid();
            grid.Dock = DockStyle.Top;
            grid.Height = 208;
            grid.SelectionChanged += (s, e) => OnSelectionChanged();
            GridHelper.ApplyStatusColumnFormatting(grid, "Статус");
            return grid;
        }

        private Panel BuildDetailPanel()
        {
            _detailPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0), Visible = false };

            var box = UiControls.CreateGroupBox("Детали заказа");
            box.Dock = DockStyle.Left;
            box.Width = 440;

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 5,
                AutoSize = true,
                Padding = new Padding(4)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            _lblCustomer = AddField(table, 0, "Покупатель:");
            _lblProduct = AddField(table, 1, "Культура:");
            _lblWarehouse = AddField(table, 2, "Склад:");
            _lblPrice = AddField(table, 3, "Цена (₽/кг):");
            _lblStatus = AddField(table, 4, "Статус:");

            box.Controls.Add(table);
            _detailPanel.Controls.Add(box);
            return _detailPanel;
        }

        private static Label AddField(TableLayoutPanel table, int row, string caption)
        {
            table.Controls.Add(UiControls.CreateFieldLabel(caption, 100), 0, row);
            var lbl = new Label
            {
                AutoSize = true,
                Font = AppTheme.FontUi,
                ForeColor = AppTheme.TextBody,
                Margin = new Padding(0, 4, 0, 4)
            };
            table.Controls.Add(lbl, 1, row);
            return lbl;
        }

        private void LoadData()
        {
            try
            {
                _allOrders = SalesService.GetOrders();
                ApplyFilter();
                _detailPanel.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить заказы:\n" + ex.Message, AppBranding.SystemTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ApplyFilter()
        {
            var search = _txtSearch?.Text?.Trim().ToLowerInvariant() ?? "";
            var status = _cmbStatus?.SelectedItem?.ToString() ?? "Все статусы";

            var filtered = _allOrders.Where(o =>
            {
                var statusOk = status == "Все статусы" || o.StatusName == status;
                var textOk = string.IsNullOrEmpty(search)
                    || o.CustomerName.ToLowerInvariant().Contains(search)
                    || ("ЗП-" + o.Id.ToString("D5")).Contains(search);
                return statusOk && textOk;
            });

            _grid.DataSource = filtered.Select(o => new
            {
                Номер = $"ЗП-{o.Id:D5}",
                Дата = o.Date.ToString("dd.MM.yyyy"),
                Покупатель = o.CustomerName,
                Культура = o.ProductName,
                Склад = o.WarehouseName,
                Колво = o.StockQuantity + " т",
                Сумма = (o.PricePerKg * o.StockQuantity).ToString("N0") + " ₽",
                Статус = o.StatusName
            }).ToList();

            if (_grid.Columns.Contains("Колво"))
                _grid.Columns["Колво"].HeaderText = "Кол-во";
        }

        private void OnSelectionChanged()
        {
            if (_grid.CurrentRow == null)
                return;

            var search = _txtSearch?.Text?.Trim().ToLowerInvariant() ?? "";
            var status = _cmbStatus?.SelectedItem?.ToString() ?? "Все статусы";
            var filtered = _allOrders.Where(order =>
            {
                var statusOk = status == "Все статусы" || order.StatusName == status;
                var textOk = string.IsNullOrEmpty(search)
                    || order.CustomerName.ToLowerInvariant().Contains(search)
                    || ("ЗП-" + order.Id.ToString("D5")).Contains(search);
                return statusOk && textOk;
            }).ToList();

            var index = _grid.CurrentRow.Index;
            if (index >= filtered.Count)
                return;

            var selected = filtered[index];
            _lblCustomer.Text = selected.CustomerName;
            _lblProduct.Text = selected.ProductName;
            _lblWarehouse.Text = selected.WarehouseName;
            _lblPrice.Text = selected.PricePerKg.ToString("N2");
            _lblStatus.Text = selected.StatusName;
            _detailPanel.Visible = true;

            var canDocs = selected.StatusName == "Подтверждён" || selected.StatusName == "Готов к отгрузке";
            _btnDocs.Enabled = canDocs;
            _btnStocks.Enabled = true;
        }
    }
}
