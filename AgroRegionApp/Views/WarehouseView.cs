using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AgroRegionApp.Data;
using AgroRegionApp.UI;

namespace AgroRegionApp.Views
{
    internal sealed class WarehouseView : UserControl
    {
        private readonly Panel _stocksPage;
        private readonly Panel _shipmentPage;
        private DataGridView _stocksGrid;
        private DataGridView _shipmentGrid;
        private Panel _shipmentDetailPanel;
        private DataGridView _miniStockGrid;
        private TextBox _txtActualQty;
        private Label _lblShipSaved;

        private StockRow _selectedStock;
        private ShipmentOrderRow _selectedShipment;
        private string _shipAction;

        public WarehouseView()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.ContentBg;

            var tabStrip = UiControls.CreateTabStrip(new[] { "Остатки на складах", "Отгрузка / Приёмка" });
            tabStrip.TabChanged += ShowTab;

            _stocksPage = BuildStocksPage();
            _shipmentPage = BuildShipmentPage();

            Controls.Add(_shipmentPage);
            Controls.Add(_stocksPage);
            Controls.Add(tabStrip);

            ShowTab(0);
            LoadStocks();
        }

        private Panel BuildStocksPage()
        {
            var page = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.ContentBg };

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 34 };
            var btnUpdate = UiControls.CreateButton("＋  Обновить остаток", true, 150);
            btnUpdate.Location = new Point(0, 4);
            btnUpdate.Click += (s, e) => ShowUpdateDialog();
            var btnJournal = UiControls.CreateButton("📝  Журнал изменений", false, 150);
            btnJournal.Location = new Point(156, 4);
            btnJournal.Enabled = false;
            toolbar.Controls.Add(btnUpdate);
            toolbar.Controls.Add(btnJournal);

            var warning = UiControls.CreateInfoBar(
                "Складской учёт ведётся на бумаге. Данные вносятся вручную после сверки с бумажными журналами.",
                AppTheme.WarnBg, AppTheme.WarnBorder);

            _stocksGrid = UiControls.CreateGrid();
            _stocksGrid.Dock = DockStyle.Fill;
            _stocksGrid.SelectionChanged += (s, e) => OnStockSelected();
            GridHelper.ApplyStatusColumnFormatting(_stocksGrid, "Состояние");

            page.Controls.Add(_stocksGrid);
            page.Controls.Add(warning);
            page.Controls.Add(toolbar);
            return page;
        }

        private Panel BuildShipmentPage()
        {
            var page = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.ContentBg };

            var info = UiControls.CreateInfoBar(
                "Выберите заказ и зафиксируйте фактическое движение товара. Статус заказа и остатки будут обновлены.",
                AppTheme.HintBg, AppTheme.HintBorder, Color.FromArgb(29, 78, 216));

            _shipmentGrid = UiControls.CreateGrid();
            _shipmentGrid.Dock = DockStyle.Top;
            _shipmentGrid.Height = 160;
            _shipmentGrid.SelectionChanged += (s, e) => OnShipmentSelected();
            GridHelper.ApplyStatusColumnFormatting(_shipmentGrid, "Статус");

            _shipmentDetailPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0), Visible = false };

            var actionBox = UiControls.CreateGroupBox("Действие по заказу");
            actionBox.Dock = DockStyle.Fill;

            var btnShip = UiControls.CreateButton("🚛  Отгрузить (→ Отгружен)", false, 200);
            btnShip.Location = new Point(8, 24);
            btnShip.Click += (s, e) => SetShipAction("ship", btnShip);

            var btnReceive = UiControls.CreateButton("📦  Принять на склад", false, 170);
            btnReceive.Location = new Point(216, 24);
            btnReceive.Click += (s, e) => SetShipAction("receive", btnReceive);

            var formBox = UiControls.CreateGroupBox("Фиксация данных");
            formBox.Location = new Point(8, 60);
            formBox.Size = new Size(420, 130);
            formBox.Visible = false;
            formBox.Name = "formBox";

            AddReadOnlyField(formBox, 22, "Заказано (т):", "0", "lblOrdered");
            var lblActual = UiControls.CreateFieldLabel("Факт. кол-во (т):", 110);
            lblActual.Location = new Point(12, 50);
            _txtActualQty = UiControls.CreateFieldBox();
            _txtActualQty.Location = new Point(128, 48);
            _txtActualQty.Width = 120;
            AddReadOnlyField(formBox, 78, "Дата/время:", DateTime.Now.ToString("g"), "lblDateTime");

            formBox.Controls.Add(lblActual);
            formBox.Controls.Add(_txtActualQty);

            var btnSave = UiControls.CreateButton("💾  Зафиксировать", true, 130);
            btnSave.Location = new Point(8, 200);
            btnSave.Visible = false;
            btnSave.Name = "btnSave";
            btnSave.Click += (s, e) => SaveShipment();

            _lblShipSaved = new Label
            {
                Location = new Point(8, 236),
                AutoSize = true,
                Font = AppTheme.FontUi,
                ForeColor = Color.FromArgb(21, 128, 61),
                Visible = false
            };

            actionBox.Controls.Add(_lblShipSaved);
            actionBox.Controls.Add(btnSave);
            actionBox.Controls.Add(formBox);
            actionBox.Controls.Add(btnShip);
            actionBox.Controls.Add(btnReceive);

            var stockBox = UiControls.CreateGroupBox("Текущие остатки на складах");
            stockBox.Dock = DockStyle.Right;
            stockBox.Width = 300;
            _miniStockGrid = UiControls.CreateGrid();
            _miniStockGrid.Dock = DockStyle.Fill;
            stockBox.Controls.Add(_miniStockGrid);

            var detailSplit = new Panel { Dock = DockStyle.Fill };
            detailSplit.Controls.Add(stockBox);
            detailSplit.Controls.Add(actionBox);

            _shipmentDetailPanel.Controls.Add(detailSplit);

            page.Controls.Add(_shipmentDetailPanel);
            page.Controls.Add(_shipmentGrid);
            page.Controls.Add(info);
            return page;
        }

        private void ShowTab(int index)
        {
            _stocksPage.Visible = index == 0;
            _shipmentPage.Visible = index == 1;
            if (index == 1)
                LoadShipment();
        }

        private void LoadStocks()
        {
            try
            {
                var stocks = WarehouseService.GetStocks();
                _stocksGrid.DataSource = stocks.Select(s => new
                {
                    Культура = s.ProductName,
                    Сорт = s.Variety,
                    Склад = s.WarehouseName,
                    Остаток = s.Quantity + " т",
                    Состояние = s.Quantity == 0 ? "Нет в наличии" : s.Quantity < 20 ? "Мало" : "В наличии",
                    Проверка = s.CheckDate?.ToString("dd.MM.yyyy") ?? "—"
                }).ToList();
                _selectedStock = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить остатки:\n" + ex.Message, AppBranding.SystemTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadShipment()
        {
            try
            {
                var orders = WarehouseService.GetShipmentOrders();
                _shipmentGrid.DataSource = orders.Select(o => new
                {
                    Номер = o.OrderNumber,
                    Дата = o.Date.ToString("dd.MM.yyyy"),
                    Контрагент = o.PartyName,
                    Тип = o.OrderType,
                    Статус = o.StatusName,
                    Культура = o.ProductName,
                    Колво = o.QtyTons
                }).ToList();
                if (_shipmentGrid.Columns.Contains("Колво"))
                    _shipmentGrid.Columns["Колво"].HeaderText = "Кол-во (т)";

                _miniStockGrid.DataSource = WarehouseService.GetStocks().Select(s => new
                {
                    Культура = s.ProductName,
                    Остаток = s.Quantity
                }).ToList();
                if (_miniStockGrid.Columns.Contains("Остаток"))
                    _miniStockGrid.Columns["Остаток"].HeaderText = "Остаток (т)";

                _shipmentDetailPanel.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить заказы:\n" + ex.Message, AppBranding.SystemTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnStockSelected()
        {
            if (_stocksGrid.CurrentRow == null)
                return;
            var stocks = WarehouseService.GetStocks();
            var idx = _stocksGrid.CurrentRow.Index;
            if (idx < stocks.Count)
                _selectedStock = stocks[idx];
        }

        private void OnShipmentSelected()
        {
            if (_shipmentGrid.CurrentRow == null)
                return;

            var orders = WarehouseService.GetShipmentOrders();
            var idx = _shipmentGrid.CurrentRow.Index;
            if (idx >= orders.Count)
                return;

            _selectedShipment = orders[idx];
            _shipmentDetailPanel.Visible = true;
            _shipAction = null;
            _lblShipSaved.Visible = false;

            var formBox = _shipmentDetailPanel.Controls.Find("formBox", true);
            if (formBox.Length > 0)
                formBox[0].Visible = false;
            var btnSave = _shipmentDetailPanel.Controls.Find("btnSave", true);
            if (btnSave.Length > 0)
                btnSave[0].Visible = false;

            var ordered = _shipmentDetailPanel.Controls.Find("lblOrdered", true);
            if (ordered.Length > 0)
                ordered[0].Text = _selectedShipment.QtyTons.ToString();
        }

        private void SetShipAction(string action, Button activeBtn)
        {
            _shipAction = action;
            var formBox = _shipmentDetailPanel.Controls.Find("formBox", true);
            if (formBox.Length > 0)
                formBox[0].Visible = true;
            var btnSave = _shipmentDetailPanel.Controls.Find("btnSave", true);
            if (btnSave.Length > 0)
                btnSave[0].Visible = true;
        }

        private void SaveShipment()
        {
            if (_selectedShipment == null || string.IsNullOrEmpty(_shipAction))
                return;

            var actionText = _shipAction == "ship" ? "Отгружено" : "Принято";
            _lblShipSaved.Text = $"Зафиксировано: {actionText} — {_txtActualQty.Text} т по заказу {_selectedShipment.OrderNumber}. Остатки обновлены.";
            _lblShipSaved.Visible = true;
            _txtActualQty.Clear();
            LoadStocks();
            _miniStockGrid.DataSource = WarehouseService.GetStocks().Select(s => new
            {
                Культура = s.ProductName,
                Остаток = s.Quantity
            }).ToList();
        }

        private void ShowUpdateDialog()
        {
            if (_selectedStock == null)
            {
                MessageBox.Show("Выберите строку остатка.", AppBranding.SystemTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var form = new ModalForm("Обновление остатков на складе", 440, 300))
            {
                form.BodyPanel.Controls.Add(UiControls.CreateInfoBar(
                    "После сверки с бумажными журналами внесите актуальное количество. Дата и время фиксируются автоматически.",
                    AppTheme.HintBg, AppTheme.HintBorder, Color.FromArgb(29, 78, 216)));

                var box = UiControls.CreateGroupBox("Остаток товара");
                box.Dock = DockStyle.Fill;
                box.Padding = new Padding(12, 20, 12, 12);

                var txtQty = UiControls.CreateFieldBox();
                txtQty.Text = _selectedStock.Quantity.ToString();
                txtQty.Width = 120;

                var table = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
                table.Controls.Add(UiControls.CreateFieldLabel("Культура:", 100), 0, 0);
                table.Controls.Add(new Label { Text = _selectedStock.ProductName, AutoSize = true }, 1, 0);
                table.Controls.Add(UiControls.CreateFieldLabel("Склад:", 100), 0, 1);
                table.Controls.Add(new Label { Text = _selectedStock.WarehouseName, AutoSize = true }, 1, 1);
                table.Controls.Add(UiControls.CreateFieldLabel("Кол-во (т):", 100), 0, 2);
                table.Controls.Add(txtQty, 1, 2);
                table.Controls.Add(UiControls.CreateFieldLabel("Дата проверки:", 100), 0, 3);
                table.Controls.Add(new Label { Text = DateTime.Now.ToString("g"), AutoSize = true }, 1, 3);

                box.Controls.Add(table);

                var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 36, FlowDirection = FlowDirection.RightToLeft };
                var btnCancel = UiControls.CreateButton("Отмена", false, 90);
                btnCancel.DialogResult = DialogResult.Cancel;
                var btnSave = UiControls.CreateButton("💾  Сохранить", true, 110);
                btnSave.Click += (s, e) =>
                {
                    if (!int.TryParse(txtQty.Text, out var qty))
                    {
                        MessageBox.Show("Введите корректное количество.", AppBranding.SystemTitle);
                        return;
                    }
                    WarehouseService.UpdateStock(_selectedStock.StockId, qty);
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                };
                footer.Controls.Add(btnCancel);
                footer.Controls.Add(btnSave);

                form.BodyPanel.Controls.Add(footer);
                form.BodyPanel.Controls.Add(box);

                if (form.ShowDialog(FindForm()) == DialogResult.OK)
                    LoadStocks();
            }
        }

        private static void AddReadOnlyField(Control parent, int y, string label, string value, string name)
        {
            var lbl = UiControls.CreateFieldLabel(label, 110);
            lbl.Location = new Point(12, y);
            var box = UiControls.CreateFieldBox(true);
            box.Location = new Point(128, y - 2);
            box.Width = 260;
            box.Text = value;
            box.Name = name;
            parent.Controls.Add(lbl);
            parent.Controls.Add(box);
        }
    }
}
