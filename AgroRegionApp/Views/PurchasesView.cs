using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AgroRegionApp.Data;
using AgroRegionApp.Models;
using AgroRegionApp.UI;

namespace AgroRegionApp.Views
{
    internal sealed class PurchasesView : UserControl
    {
        private readonly AuthenticatedUser _user;
        private readonly DataGridView _ordersGrid;
        private readonly DataGridView _historyGrid;
        private readonly DataGridView _historyGridSplit;
        private readonly DataGridView _itemsGrid;
        private readonly Panel _bottomPanel;
        private readonly Panel _historyOnlyPanel;
        private readonly Panel _splitPanel;
        private readonly GroupBox _detailBox;
        private Label _lblSupplier;
        private Label _lblPhone;
        private Label _lblEmail;
        private Label _lblStatus;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnDocs;

        private List<PurchaseOrderRow> _orders = new List<PurchaseOrderRow>();
        private PurchaseOrderRow _selected;

        public PurchasesView(AuthenticatedUser user)
        {
            _user = user;
            Dock = DockStyle.Fill;
            BackColor = AppTheme.ContentBg;

            var toolbar = BuildToolbar();
            _ordersGrid = UiControls.CreateGrid();
            _ordersGrid.Dock = DockStyle.Top;
            _ordersGrid.Height = 176;
            _ordersGrid.SelectionChanged += OrdersGridOnSelectionChanged;
            GridHelper.ApplyStatusColumnFormatting(_ordersGrid, "Статус");

            _historyGrid = UiControls.CreateGrid();
            _historyGrid.Dock = DockStyle.Fill;

            _historyOnlyPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
            var historyBoxFull = UiControls.CreateGroupBox("История закупок (ретроспектива цен)");
            historyBoxFull.Dock = DockStyle.Fill;
            historyBoxFull.Controls.Add(_historyGrid);
            _historyOnlyPanel.Controls.Add(historyBoxFull);

            _splitPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0), Visible = false };

            _detailBox = UiControls.CreateGroupBox("Состав заказа");
            _detailBox.Dock = DockStyle.Left;
            _detailBox.Width = 360;

            var fields = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 4,
                AutoSize = true,
                Padding = new Padding(4)
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 4; i++)
                fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _lblSupplier = AddField(fields, 0, "Поставщик:");
            _lblPhone = AddField(fields, 1, "Телефон:");
            _lblEmail = AddField(fields, 2, "E-mail:");
            _lblStatus = AddField(fields, 3, "Статус:");

            _itemsGrid = UiControls.CreateGrid();
            _itemsGrid.Dock = DockStyle.Fill;
            _itemsGrid.Height = 100;

            var detailInner = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) };
            detailInner.Controls.Add(_itemsGrid);
            detailInner.Controls.Add(fields);

            _detailBox.Controls.Add(detailInner);

            var historyBoxSplit = UiControls.CreateGroupBox("История закупок (ретроспектива цен)");
            historyBoxSplit.Dock = DockStyle.Fill;
            _historyGridSplit = UiControls.CreateGrid();
            _historyGridSplit.Dock = DockStyle.Fill;
            historyBoxSplit.Controls.Add(_historyGridSplit);

            _splitPanel.Controls.Add(historyBoxSplit);
            _splitPanel.Controls.Add(_detailBox);

            _bottomPanel = new Panel { Dock = DockStyle.Fill };
            _bottomPanel.Controls.Add(_splitPanel);
            _bottomPanel.Controls.Add(_historyOnlyPanel);

            Controls.Add(_bottomPanel);
            Controls.Add(_ordersGrid);
            Controls.Add(toolbar);

            LoadHistory();
            LoadData();
        }

        private Panel BuildToolbar()
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = AppTheme.ContentBg };

            var btnCreate = UiControls.CreateButton("＋  Создать заказ", true, 130);
            btnCreate.Location = new Point(0, 4);
            btnCreate.Click += (s, e) => ShowCreateDialog();

            _btnEdit = UiControls.CreateButton("✏  Изменить", false, 100);
            _btnEdit.Location = new Point(136, 4);
            _btnEdit.Enabled = false;

            _btnDelete = UiControls.CreateButton("✕  Удалить", false, 95, danger: true);
            _btnDelete.Location = new Point(238, 4);
            _btnDelete.Enabled = false;

            _btnDocs = UiControls.CreateButton("📄  Документы", false, 110);
            _btnDocs.Location = new Point(335, 4);
            _btnDocs.Enabled = false;

            panel.Controls.Add(btnCreate);
            panel.Controls.Add(_btnEdit);
            panel.Controls.Add(_btnDelete);
            panel.Controls.Add(_btnDocs);
            return panel;
        }

        private static Label AddField(TableLayoutPanel table, int row, string caption)
        {
            table.Controls.Add(UiControls.CreateFieldLabel(caption, 85), 0, row);
            var value = new Label
            {
                AutoSize = true,
                Font = AppTheme.FontUi,
                ForeColor = AppTheme.TextBody,
                Margin = new Padding(0, 4, 0, 4)
            };
            table.Controls.Add(value, 1, row);
            return value;
        }

        private void LoadData()
        {
            try
            {
                _ordersGrid.SelectionChanged -= OrdersGridOnSelectionChanged;
                _orders = PurchaseService.GetOrders();
                UiControls.BindGrid(_ordersGrid, _orders.Select(o => new PurchaseOrderGridRow
                {
                    Номер = $"ЗЗ-{o.Id:D5}",
                    Дата = o.Date.ToString("dd.MM.yyyy"),
                    Поставщик = o.SupplierName,
                    Позиции = PurchaseMockData.GetItemCount(o.Id) + " поз.",
                    Сумма = PurchaseMockData.GetOrderTotal(o.Id).ToString("N0") + " ₽",
                    Статус = o.StatusName
                }).ToList());

                LoadHistory();
                _selected = null;
                ShowBottomMode(false);
                UpdateToolbarState();
                _ordersGrid.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить заказы:\n" + ex.Message, AppBranding.SystemTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                _ordersGrid.SelectionChanged += OrdersGridOnSelectionChanged;
            }
        }

        private void LoadHistory()
        {
            var rows = PurchaseMockData.GetHistory().Select(h => new HistoryGridRow
            {
                Дата = h.Date.ToString("dd.MM.yyyy"),
                Поставщик = h.SupplierName,
                Культура = h.ProductName,
                Колво = h.QtyTons,
                Цена = h.PricePerTon.ToString("N0")
            }).ToList();

            UiControls.BindGrid(_historyGrid, rows.ToList());
            UiControls.BindGrid(_historyGridSplit, rows.ToList());
            FormatHistoryGrid(_historyGrid);
            FormatHistoryGrid(_historyGridSplit);
        }

        private static void FormatHistoryGrid(DataGridView grid)
        {
            if (grid.Columns.Count == 0)
                return;

            SetHeader(grid, "Колво", "Кол-во (т)");
            SetHeader(grid, "Цена", "Цена ₽/т");
        }

        private static void SetHeader(DataGridView grid, string columnName, string header)
        {
            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column.Name == columnName || column.DataPropertyName == columnName)
                    column.HeaderText = header;
            }
        }

        private void OrdersGridOnSelectionChanged(object sender, EventArgs e)
        {
            if (_ordersGrid.SelectedRows.Count == 0)
            {
                _selected = null;
                ShowBottomMode(false);
                UpdateToolbarState();
                return;
            }

            OnOrderSelected();
        }

        private void OnOrderSelected()
        {
            if (_ordersGrid.CurrentRow == null || _ordersGrid.CurrentRow.Index < 0)
                return;

            var index = _ordersGrid.CurrentRow.Index;
            if (index >= _orders.Count)
                return;

            _selected = _orders[index];
            _detailBox.Text = $"СОСТАВ ЗАКАЗА ЗЗ-{_selected.Id:D5}".ToUpperInvariant();
            _lblSupplier.Text = _selected.SupplierName;
            _lblPhone.Text = _selected.SupplierPhone;
            _lblEmail.Text = _selected.SupplierEmail;
            _lblStatus.Text = _selected.StatusName;

            UiControls.BindGrid(_itemsGrid, PurchaseMockData.GetItems(_selected.Id).Select(i => new ItemGridRow
            {
                Культура = i.ProductName,
                Колво = i.QtyTons,
                Цена = i.PricePerTon.ToString("N0"),
                Сумма = i.Total.ToString("N0")
            }).ToList());
            SetHeader(_itemsGrid, "Колво", "Кол-во (т)");
            SetHeader(_itemsGrid, "Цена", "Цена ₽/т");
            SetHeader(_itemsGrid, "Сумма", "Сумма ₽");

            ShowBottomMode(true);
            UpdateToolbarState();
        }

        private void ShowBottomMode(bool withDetail)
        {
            _historyOnlyPanel.Visible = !withDetail;
            _splitPanel.Visible = withDetail;
        }

        private void UpdateToolbarState()
        {
            var has = _selected != null;
            _btnEdit.Enabled = has;
            _btnDelete.Enabled = has;
            _btnDocs.Enabled = has;
        }

        private void ShowCreateDialog()
        {
            var suppliers = PurchaseService.GetSuppliers();
            if (suppliers.Count == 0)
            {
                MessageBox.Show("В справочнике нет поставщиков.", AppBranding.SystemTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var nextNum = (_orders.Count > 0 ? _orders.Max(o => o.Id) : 0) + 1;

            using (var form = new ModalForm("Новый заказ на закупку", 480, 460))
            {
                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    RowCount = 4,
                    ColumnCount = 1
                };
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                layout.Controls.Add(UiControls.CreateInfoBar(
                    "Система показывает историю закупок и текущие остатки для принятия решения об объёме.",
                    AppTheme.HintBg, AppTheme.HintBorder, Color.FromArgb(29, 78, 216)), 0, 0);

                var orderBox = UiControls.CreateGroupBox("Заказ на закупку");
                orderBox.Dock = DockStyle.Top;
                orderBox.Height = 130;

                var combo = new ComboBox
                {
                    Location = new Point(110, 24),
                    Width = 300,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = AppTheme.FontUi
                };
                combo.DataSource = suppliers.ToList();
                combo.DisplayMember = "Name";
                combo.ValueMember = "Id";

                var txtComment = UiControls.CreateFieldBox();
                txtComment.Location = new Point(110, 84);
                txtComment.Width = 300;
                AddLabeledField(orderBox, 24, "Номер:", $"ЗЗ-{nextNum:D5} (авто)", true);
                AddLabeledField(orderBox, 52, "Дата:", DateTime.Today.ToString("dd.MM.yyyy"), true);
                AddLabeledField(orderBox, 80, "Поставщик:", null, false, combo);
                AddLabeledField(orderBox, 108, "Комментарий:", null, false, txtComment);

                var itemsBox = UiControls.CreateGroupBox("Перечень товаров");
                itemsBox.Dock = DockStyle.Fill;

                var itemsGrid = UiControls.CreateGrid();
                itemsGrid.Dock = DockStyle.Top;
                itemsGrid.Height = 72;
                itemsGrid.AllowUserToAddRows = false;
                itemsGrid.DataSource = new List<object>
                {
                    new { Культура = "Пшеница 3 кл.", Колво = 50, Цена = "4 800" },
                    new { Культура = "Ячмень фуражный", Колво = 30, Цена = "4 000" }
                };

                var rowBtns = new FlowLayoutPanel
                {
                    Dock = DockStyle.Top,
                    Height = 32,
                    FlowDirection = FlowDirection.LeftToRight,
                    Padding = new Padding(0, 4, 0, 0)
                };
                rowBtns.Controls.Add(UiControls.CreateButton("＋  Строку", false, 90));
                rowBtns.Controls.Add(UiControls.CreateButton("−  Строку", false, 90, danger: true));

                itemsBox.Controls.Add(rowBtns);
                itemsBox.Controls.Add(itemsGrid);

                var footer = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    Height = 34
                };
                var btnCancel = UiControls.CreateButton("Отмена", false, 90);
                btnCancel.DialogResult = DialogResult.Cancel;
                var btnSave = UiControls.CreateButton("💾  Сохранить", true, 110);
                btnSave.Click += (s, e) =>
                {
                    if (!_user.EmployeeId.HasValue)
                    {
                        MessageBox.Show("У учётной записи не привязан сотрудник.", AppBranding.SystemTitle);
                        return;
                    }

                    var newId = PurchaseService.CreateOrder(
                        (int)combo.SelectedValue,
                        _user.EmployeeId.Value,
                        PurchaseService.GetDefaultStatusId());
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                };
                footer.Controls.Add(btnCancel);
                footer.Controls.Add(btnSave);

                layout.Controls.Add(orderBox, 0, 1);
                layout.Controls.Add(itemsBox, 0, 2);
                layout.Controls.Add(footer, 0, 3);
                form.BodyPanel.Controls.Add(layout);

                if (form.ShowDialog(FindForm()) == DialogResult.OK)
                    LoadData();
            }
        }

        private sealed class PurchaseOrderGridRow
        {
            public string Номер { get; set; }
            public string Дата { get; set; }
            public string Поставщик { get; set; }
            public string Позиции { get; set; }
            public string Сумма { get; set; }
            public string Статус { get; set; }
        }

        private sealed class HistoryGridRow
        {
            public string Дата { get; set; }
            public string Поставщик { get; set; }
            public string Культура { get; set; }
            public int Колво { get; set; }
            public string Цена { get; set; }
        }

        private sealed class ItemGridRow
        {
            public string Культура { get; set; }
            public int Колво { get; set; }
            public string Цена { get; set; }
            public string Сумма { get; set; }
        }

        private static void AddLabeledField(Control parent, int y, string label, string value, bool readOnly, Control input = null)
        {
            var lbl = UiControls.CreateFieldLabel(label, 90);
            lbl.Location = new Point(12, y);
            parent.Controls.Add(lbl);

            if (input != null)
            {
                parent.Controls.Add(input);
                return;
            }

            var box = UiControls.CreateFieldBox(readOnly);
            box.Location = new Point(110, y - 2);
            box.Width = 300;
            box.Text = value ?? "";
            parent.Controls.Add(box);
        }
    }
}
