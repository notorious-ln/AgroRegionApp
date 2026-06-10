using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AgroRegionApp.Data;
using AgroRegionApp.Models;
using AgroRegionApp.UI;

namespace AgroRegionApp.Views
{
    internal sealed class SalesView : UserControl
    {
        private readonly AuthenticatedUser _user;
        private readonly DataGridView _grid;
        private Panel _detailPanel;
        private TextBox _txtSearch;
        private ComboBox _cmbStatus;
        private TextBox _txtDetailNumber;
        private TextBox _txtDetailDate;
        private TextBox _txtDetailBuyer;
        private TextBox _txtDetailInn;
        private TextBox _txtDetailProduct;
        private TextBox _txtDetailVariety;
        private TextBox _txtDetailWarehouse;
        private TextBox _txtDetailPrice;
        private TextBox _txtDetailQty;
        private TextBox _txtDetailSum;
        private TextBox _txtDebtBuyer;
        private TextBox _txtDebtPhone;
        private TextBox _txtDebtAmount;
        private Panel _debtAlertPanel;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnDocs;
        private Button _btnStocks;

        private List<SalesOrderRow> _allOrders = new List<SalesOrderRow>();
        private SalesOrderRow _selected;

        public SalesView(AuthenticatedUser user)
        {
            _user = user;
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
            btnCreate.Click += (s, e) => ShowCreateDialog();

            _btnEdit = UiControls.CreateButton("✏  Изменить", false, 100);
            _btnEdit.Location = new Point(136, 4);
            _btnEdit.Enabled = false;
            _btnEdit.Click += (s, e) => ShowEditDialog();

            _btnDelete = UiControls.CreateButton("✕  Удалить", false, 95, danger: true);
            _btnDelete.Location = new Point(238, 4);
            _btnDelete.Enabled = false;
            _btnDelete.Click += (s, e) => DeleteSelected();

            _btnDocs = UiControls.CreateButton("📄  Документы", false, 110);
            _btnDocs.Location = new Point(335, 4);
            _btnDocs.Enabled = false;
            _btnDocs.Click += (s, e) => ShowDocumentsDialog();

            _btnStocks = UiControls.CreateButton("🔍  Запрос остатков", false, 140);
            _btnStocks.Location = new Point(448, 4);
            _btnStocks.Enabled = false;
            _btnStocks.Click += (s, e) => ShowStocksDialog();

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
            panel.Controls.Add(_btnEdit);
            panel.Controls.Add(_btnDelete);
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
            _detailPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 8, 0, 0),
                Visible = false
            };

            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0)
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 268));
            row.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var orderBox = UiControls.CreateGroupBox("Детали заказа");
            orderBox.Dock = DockStyle.Top;
            orderBox.AutoSize = true;
            orderBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            orderBox.Margin = new Padding(0, 0, 8, 0);
            orderBox.Padding = new Padding(10, 8, 10, 10);

            var fields = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 4,
                RowCount = 5,
                Padding = new Padding(0, 4, 0, 0)
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            for (var i = 0; i < 5; i++)
                fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _txtDetailNumber = AddDetailField(fields, 0, 0, "Номер:");
            _txtDetailDate = AddDetailField(fields, 0, 2, "Дата:");
            _txtDetailBuyer = AddDetailField(fields, 1, 0, "Покупатель:");
            _txtDetailInn = AddDetailField(fields, 1, 2, "ИНН:");
            _txtDetailProduct = AddDetailField(fields, 2, 0, "Культура:");
            _txtDetailVariety = AddDetailField(fields, 2, 2, "Сорт:");
            _txtDetailWarehouse = AddDetailField(fields, 3, 0, "Склад:");
            _txtDetailPrice = AddDetailField(fields, 3, 2, "Цена (₽/кг):");
            _txtDetailQty = AddDetailField(fields, 4, 0, "Кол-во (т):");
            _txtDetailSum = AddDetailField(fields, 4, 2, "Сумма (₽):");

            orderBox.Controls.Add(fields);

            var debtBox = UiControls.CreateGroupBox("Дебиторская задолженность покупателя");
            debtBox.Dock = DockStyle.Top;
            debtBox.AutoSize = true;
            debtBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            debtBox.Padding = new Padding(10, 8, 10, 10);

            var debtFields = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(0, 4, 0, 0)
            };
            debtFields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            debtFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            for (var i = 0; i < 3; i++)
                debtFields.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _txtDebtBuyer = AddDetailField(debtFields, 0, 0, "Покупатель:");
            _txtDebtPhone = AddDetailField(debtFields, 1, 0, "Телефон:");
            _txtDebtAmount = AddDetailField(debtFields, 2, 0, "Задолженность:");

            _debtAlertPanel = new Panel { Dock = DockStyle.Top, Height = 36, Margin = new Padding(0, 8, 0, 0) };

            debtBox.Controls.Add(debtFields);
            debtBox.Controls.Add(_debtAlertPanel);

            row.Controls.Add(orderBox, 0, 0);
            row.Controls.Add(debtBox, 1, 0);
            _detailPanel.Controls.Add(row);
            return _detailPanel;
        }

        private static TextBox AddDetailField(TableLayoutPanel table, int row, int labelCol, string caption)
        {
            var lbl = UiControls.CreateFieldLabel(caption, 90);
            lbl.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            lbl.Margin = new Padding(0, 6, 6, 2);

            var box = UiControls.CreateFieldBox(true);
            box.Dock = DockStyle.Fill;
            box.Margin = new Padding(0, 4, labelCol == 0 ? 12 : 0, 2);

            table.Controls.Add(lbl, labelCol, row);
            table.Controls.Add(box, labelCol + 1, row);
            return box;
        }

        private static void SetDebtAlert(Panel panel, bool hasDebt)
        {
            panel.Controls.Clear();
            panel.Paint -= DebtAlertPaint;

            Color bg, border, fore;
            string icon, text;
            if (hasDebt)
            {
                bg = AppTheme.WarnBg;
                border = AppTheme.WarnBorder;
                fore = Color.FromArgb(146, 64, 14);
                icon = "⚠";
                text = "Имеется дебиторская задолженность";
            }
            else
            {
                bg = AppTheme.SuccessBg;
                border = AppTheme.SuccessBorder;
                fore = AppTheme.SuccessText;
                icon = "✔";
                text = "Задолженности нет";
            }

            panel.BackColor = bg;
            panel.Tag = border;
            panel.Paint += DebtAlertPaint;
            panel.Controls.Add(new Label
            {
                Text = icon,
                Location = new Point(10, 9),
                AutoSize = true,
                Font = AppTheme.FontUi,
                ForeColor = fore,
                BackColor = Color.Transparent
            });
            panel.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(28, 8),
                AutoSize = true,
                Font = AppTheme.FontUi,
                ForeColor = fore,
                BackColor = Color.Transparent
            });
        }

        private static void DebtAlertPaint(object sender, PaintEventArgs e)
        {
            var panel = (Panel)sender;
            var border = panel.Tag as Color? ?? AppTheme.Border;
            using (var pen = new Pen(border))
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        }

        private void LoadData()
        {
            try
            {
                _allOrders = SalesService.GetOrders();
                _selected = null;
                ApplyFilter();
                _detailPanel.Visible = false;
                UpdateToolbarState();
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
                Колво = o.QuantityTons + " т",
                Сумма = o.OrderTotal.ToString("N0") + " ₽",
                Статус = o.StatusName
            }).ToList();

            if (_grid.Columns.Contains("Колво"))
                _grid.Columns["Колво"].HeaderText = "Кол-во";
        }

        private void OnSelectionChanged()
        {
            if (_grid.CurrentRow == null)
                return;

            var filtered = GetFilteredOrders();
            var index = _grid.CurrentRow.Index;
            if (index >= filtered.Count)
                return;

            _selected = filtered[index];
            var selected = _selected;
            _txtDetailNumber.Text = selected.OrderNumber;
            _txtDetailDate.Text = selected.Date.ToString("dd.MM.yyyy");
            _txtDetailBuyer.Text = selected.CustomerName;
            _txtDetailInn.Text = string.IsNullOrEmpty(selected.CustomerInn) ? "—" : selected.CustomerInn;
            _txtDetailProduct.Text = selected.ProductName;
            _txtDetailVariety.Text = selected.ProductVariety;
            _txtDetailWarehouse.Text = selected.WarehouseName;
            _txtDetailPrice.Text = selected.PricePerKg.ToString("G29");
            _txtDetailQty.Text = selected.QuantityTons.ToString();
            _txtDetailSum.Text = selected.OrderTotal.ToString("N0");

            _txtDebtBuyer.Text = selected.CustomerName;
            _txtDebtPhone.Text = string.IsNullOrEmpty(selected.CustomerPhone) ? "—" : selected.CustomerPhone;
            _txtDebtAmount.Text = selected.CustomerDebt.ToString("N0") + " ₽";
            SetDebtAlert(_debtAlertPanel, selected.CustomerDebt > 0);

            _detailPanel.Visible = true;
            UpdateToolbarState();
        }

        private void UpdateToolbarState()
        {
            var has = _selected != null;
            _btnEdit.Enabled = has;
            _btnDelete.Enabled = has && _selected.StatusName != "Отгружен";
            _btnDocs.Enabled = has && _selected.CanGenerateDocuments;
            _btnStocks.Enabled = has;
        }

        private List<SalesOrderRow> GetFilteredOrders()
        {
            var search = _txtSearch?.Text?.Trim().ToLowerInvariant() ?? "";
            var status = _cmbStatus?.SelectedItem?.ToString() ?? "Все статусы";
            return _allOrders.Where(order =>
            {
                var statusOk = status == "Все статусы" || order.StatusName == status;
                var textOk = string.IsNullOrEmpty(search)
                    || order.CustomerName.ToLowerInvariant().Contains(search)
                    || order.OrderNumber.Contains(search);
                return statusOk && textOk;
            }).ToList();
        }

        private void ShowCreateDialog()
        {
            var stocks = WarehouseService.GetStocks();
            if (stocks.Count == 0)
            {
                MessageBox.Show("Нет данных об остатках на складе.", AppBranding.SystemTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var nextNum = (_allOrders.Count > 0 ? _allOrders.Max(o => o.Id) : 0) + 1;

            using (var form = new ModalForm("Новый заказ на продажу", 500, 580))
            {
                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    RowCount = 5,
                    ColumnCount = 1,
                    Padding = new Padding(0)
                };
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var comboBuyer = CreateComboBox();
                var buyerPicker = CounterpartyDialogs.CreatePickerRow(form, comboBuyer, isCustomer: true);

                var txtNumber = UiControls.CreateFieldBox(true);
                txtNumber.Text = $"ЗП-{nextNum:D5} (авто)";
                var txtDate = UiControls.CreateFieldBox(true);
                txtDate.Text = DateTime.Today.ToString("dd.MM.yyyy");

                var requisitesBox = CreateFieldsGroup("Реквизиты (Заказ_на_продажу)", table =>
                {
                    AddFormRow(table, 0, "Номер:", txtNumber);
                    AddFormRow(table, 1, "Дата:", txtDate);
                    AddFormRow(table, 2, "Покупатель:", buyerPicker);
                });

                var stockOptions = stocks.Select(s => new StockOption
                {
                    StockId = s.StockId,
                    Quantity = s.Quantity,
                    Display = $"{s.ProductName} — {s.WarehouseName} ({s.Quantity} т)"
                }).ToList();

                var comboStock = CreateComboBox();
                comboStock.DataSource = stockOptions;
                comboStock.DisplayMember = "Display";
                comboStock.ValueMember = "StockId";

                var txtQty = UiControls.CreateFieldBox();
                var txtPrice = UiControls.CreateFieldBox();
                AttachPlaceholder(txtQty, "напр. 50");
                AttachPlaceholder(txtPrice, "напр. 5.00");

                var stockBox = CreateFieldsGroup("Номенклатура и остаток (Остаток_товара → Товар, Склад)", table =>
                {
                    AddFormRow(table, 0, "Остаток / Склад:", comboStock);
                    AddFormRow(table, 1, "Кол-во (т):", txtQty);
                    AddFormRow(table, 2, "Цена (₽/кг):", txtPrice);
                });

                var checkBox = UiControls.CreateGroupBox("Проверка остатков на складе");
                checkBox.Dock = DockStyle.Top;
                checkBox.AutoSize = true;
                checkBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                checkBox.Margin = new Padding(0, 0, 0, 8);
                checkBox.Padding = new Padding(10, 8, 10, 10);

                var checkInner = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    ColumnCount = 1,
                    RowCount = 4,
                    Padding = new Padding(0)
                };
                for (var i = 0; i < 4; i++)
                    checkInner.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var infoAlert = CreateAlertPanel("ℹ",
                    "Складской учёт ведётся на бумаге. Уточните остатки у руководителя склада и внесите данные вручную.",
                    AppTheme.HintBg, AppTheme.HintBorder, Color.FromArgb(29, 78, 216));

                var txtConfirm = UiControls.CreateFieldBox();
                AttachPlaceholder(txtConfirm, "напр.: Пшеница — 120 т на 09.06.2026");

                var confirmTable = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    ColumnCount = 2,
                    Margin = new Padding(0, 6, 0, 0)
                };
                confirmTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));
                confirmTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                confirmTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                AddFormRow(confirmTable, 0, "Подтверждение:", txtConfirm);

                var chkConfirmed = new CheckBox
                {
                    Text = "Остатки подтверждены руководителем склада",
                    AutoSize = true,
                    Font = AppTheme.FontUi,
                    ForeColor = AppTheme.TextBody,
                    Margin = new Padding(0, 6, 0, 0)
                };

                var warnAlert = CreateAlertPanel("⚠",
                    "Система зафиксирует необходимость уточнения у склада",
                    AppTheme.WarnBg, AppTheme.WarnBorder, Color.FromArgb(146, 64, 14));
                warnAlert.Margin = new Padding(0, 6, 0, 0);

                chkConfirmed.CheckedChanged += (s, e) => warnAlert.Visible = !chkConfirmed.Checked;

                infoAlert.Dock = DockStyle.Fill;
                confirmTable.Dock = DockStyle.Fill;
                chkConfirmed.Dock = DockStyle.Fill;
                warnAlert.Dock = DockStyle.Fill;
                checkInner.Controls.Add(infoAlert, 0, 0);
                checkInner.Controls.Add(confirmTable, 0, 1);
                checkInner.Controls.Add(chkConfirmed, 0, 2);
                checkInner.Controls.Add(warnAlert, 0, 3);
                checkBox.Controls.Add(checkInner);

                var footer = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    Height = 34,
                    Padding = new Padding(0, 8, 0, 0)
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

                    if (!CounterpartyDialogs.HasSelection(comboBuyer))
                    {
                        MessageBox.Show("Выберите покупателя или добавьте нового.", AppBranding.SystemTitle);
                        return;
                    }

                    if (!TryParseInt(txtQty, "напр. 50", out var quantityTons))
                    {
                        MessageBox.Show("Укажите корректное количество (т).", AppBranding.SystemTitle);
                        return;
                    }

                    if (!TryParseDecimal(txtPrice, "напр. 5.00", out var price))
                    {
                        MessageBox.Show("Укажите корректную цену (₽/кг).", AppBranding.SystemTitle);
                        return;
                    }

                    var selectedStock = comboStock.SelectedItem as StockOption;
                    if (selectedStock != null && quantityTons > selectedStock.Quantity)
                    {
                        MessageBox.Show(
                            $"Недостаточно остатка на складе. Доступно: {selectedStock.Quantity} т.",
                            AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {
                        SalesService.CreateOrder(
                            (int)comboBuyer.SelectedValue,
                            _user.EmployeeId.Value,
                            (int)comboStock.SelectedValue,
                            quantityTons,
                            price,
                            chkConfirmed.Checked,
                            SalesService.GetDefaultStatusId());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    form.DialogResult = DialogResult.OK;
                    form.Close();
                };
                footer.Controls.Add(btnCancel);
                footer.Controls.Add(btnSave);

                layout.Controls.Add(requisitesBox, 0, 0);
                layout.Controls.Add(stockBox, 0, 1);
                layout.Controls.Add(checkBox, 0, 2);
                layout.Controls.Add(new Panel { Dock = DockStyle.Fill }, 0, 3);
                layout.Controls.Add(footer, 0, 4);
                form.BodyPanel.Controls.Add(layout);

                if (form.ShowDialog(FindForm()) == DialogResult.OK)
                    LoadData();
            }
        }

        private void ShowEditDialog()
        {
            if (_selected == null)
                return;

            var order = _selected;
            var statuses = SalesService.GetStatuses();

            var txtNumber = UiControls.CreateFieldBox(true);
            txtNumber.Text = order.OrderNumber;
            var txtDate = UiControls.CreateFieldBox(true);
            txtDate.Text = order.Date.ToString("dd.MM.yyyy");
            var txtBuyer = UiControls.CreateFieldBox(true);
            txtBuyer.Text = order.CustomerName;
            var txtProduct = UiControls.CreateFieldBox(true);
            txtProduct.Text = $"{order.ProductName} — {order.WarehouseName}";
            var cmbStatus = CreateComboBox();
            cmbStatus.DataSource = statuses.ToList();
            cmbStatus.DisplayMember = "Name";
            cmbStatus.ValueMember = "Id";
            cmbStatus.SelectedValue = order.StatusId;
            var txtQty = UiControls.CreateFieldBox();
            txtQty.Text = order.QuantityTons.ToString();
            var txtPrice = UiControls.CreateFieldBox();
            txtPrice.Text = order.PricePerKg.ToString("G29");

            using (var form = new ModalForm($"Изменение заказа {order.OrderNumber}", 440, 340))
            {
                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    RowCount = 2,
                    ColumnCount = 1
                };
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var box = CreateFieldsGroup("Параметры заказа", table =>
                {
                    AddFormRow(table, 0, "Номер:", txtNumber);
                    AddFormRow(table, 1, "Дата:", txtDate);
                    AddFormRow(table, 2, "Покупатель:", txtBuyer);
                    AddFormRow(table, 3, "Позиция:", txtProduct);
                    AddFormRow(table, 4, "Статус:", cmbStatus);
                    AddFormRow(table, 5, "Кол-во (т):", txtQty);
                    AddFormRow(table, 6, "Цена (₽/кг):", txtPrice);
                });

                var footer = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    Height = 34,
                    Padding = new Padding(0, 8, 0, 0)
                };
                var btnCancel = UiControls.CreateButton("Отмена", false, 90);
                btnCancel.DialogResult = DialogResult.Cancel;
                var btnSave = UiControls.CreateButton("💾  Сохранить", true, 110);
                btnSave.Click += (s, e) =>
                {
                    if (!int.TryParse(txtQty.Text.Trim(), out var qty) || qty <= 0)
                    {
                        MessageBox.Show("Укажите корректное количество (т).", AppBranding.SystemTitle);
                        return;
                    }

                    if (!decimal.TryParse(txtPrice.Text.Trim().Replace(",", "."),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out var price))
                    {
                        MessageBox.Show("Укажите корректную цену (₽/кг).", AppBranding.SystemTitle);
                        return;
                    }

                    try
                    {
                        SalesService.UpdateOrder(order.Id, order.StockId, (byte)cmbStatus.SelectedValue, qty, price);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    form.DialogResult = DialogResult.OK;
                    form.Close();
                };

                footer.Controls.Add(btnCancel);
                footer.Controls.Add(btnSave);
                layout.Controls.Add(box, 0, 0);
                layout.Controls.Add(footer, 0, 1);
                form.BodyPanel.Controls.Add(layout);

                if (form.ShowDialog(FindForm()) == DialogResult.OK)
                    LoadData();
            }
        }

        private void DeleteSelected()
        {
            if (_selected == null)
                return;

            if (_selected.StatusName == "Отгружен")
            {
                MessageBox.Show("Отгруженный заказ удалить нельзя.", AppBranding.SystemTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var answer = MessageBox.Show(
                $"Удалить заказ {_selected.OrderNumber}?",
                AppBranding.SystemTitle,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (answer != DialogResult.Yes)
                return;

            try
            {
                SalesService.DeleteOrder(_selected.Id);
                _selected = null;
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowDocumentsDialog()
        {
            if (_selected == null)
                return;

            if (!WordTemplateService.IsAvailable)
            {
                MessageBox.Show(
                    "Для формирования документов по шаблонам требуется установленный Microsoft Word.",
                    AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_selected.CanGenerateDocuments)
            {
                if (_selected.StatusName != "Подтверждён" && _selected.StatusName != "Готов к отгрузке")
                {
                    MessageBox.Show(
                        "Документы доступны для заказов со статусом «Подтверждён» или «Готов к отгрузке».",
                        AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Для формирования документов необходимо подтвердить остатки на складе.",
                        AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return;
            }

            var order = _selected;
            var docItems = new[]
            {
                new DocItem("Счёт на оплату", true),
                new DocItem("Договор купли-продажи", true),
                new DocItem("Товарная накладная (ТОРГ-12)", true),
                new DocItem("ТТН (при необходимости)", false)
            };

            using (var form = new ModalForm($"Документы по заказу {order.OrderNumber}", 440, 400))
            {
                var root = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    RowCount = 2,
                    ColumnCount = 1
                };
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var stack = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    ColumnCount = 1,
                    Padding = new Padding(0)
                };
                stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

                var infoAlert = CreateAlertPanel("ℹ",
                    $"Статус заказа: {order.StatusName}. Комплект документов готов к формированию.",
                    AppTheme.HintBg, AppTheme.HintBorder, Color.FromArgb(29, 78, 216));
                AddDialogSection(stack, infoAlert);

                var docsBox = UiControls.CreateGroupBox("Состав комплекта");
                docsBox.AutoSize = true;
                docsBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                docsBox.Padding = new Padding(10, 10, 10, 10);
                docsBox.Margin = new Padding(0);

                var checksTable = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    ColumnCount = 1,
                    Padding = new Padding(4, 2, 0, 0)
                };
                checksTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                var checkRow = 0;
                foreach (var item in docItems)
                {
                    var chk = new CheckBox
                    {
                        Text = item.Label,
                        Checked = item.DefaultChecked,
                        AutoSize = true,
                        Font = AppTheme.FontUi,
                        ForeColor = AppTheme.TextBody,
                        Margin = new Padding(0, 2, 0, 2),
                        Dock = DockStyle.Top
                    };
                    item.CheckBox = chk;
                    checksTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    checksTable.Controls.Add(chk, 0, checkRow++);
                    checksTable.RowCount = checkRow;
                }
                docsBox.Controls.Add(checksTable);
                AddDialogSection(stack, docsBox);

                var cmbFormat = CreateComboBox();
                cmbFormat.Items.AddRange(new object[] { "PDF", "DOCX" });
                cmbFormat.SelectedIndex = 0;
                AddDialogSection(stack, CreateFieldsGroup("Параметры вывода", table =>
                {
                    AddFormRow(table, 0, "Формат вывода:", cmbFormat);
                }));

                var outputPath = DocumentService.GetDefaultOutputFolder(order.OrderNumber);
                var txtPath = UiControls.CreateFieldBox(true);
                txtPath.Text = outputPath + Path.DirectorySeparatorChar;
                AddDialogSection(stack, CreateFieldsGroup("Сохранение", table =>
                {
                    AddFormRow(table, 0, "Путь:", txtPath);
                }), 0);

                var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
                scroll.Controls.Add(stack);

                var footer = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    Height = 34,
                    Padding = new Padding(0, 8, 0, 0)
                };
                var btnClose = UiControls.CreateButton("Закрыть", false, 90);
                btnClose.DialogResult = DialogResult.Cancel;
                var btnGenerate = UiControls.CreateButton("📄  Сформировать", true, 120);
                btnGenerate.Click += (s, e) =>
                {
                    var selectedDocs = docItems.Where(d => d.CheckBox.Checked).Select(d => d.Label).ToList();
                    if (selectedDocs.Count == 0)
                    {
                        MessageBox.Show("Выберите хотя бы один документ.", AppBranding.SystemTitle);
                        return;
                    }

                    if (!string.Equals(cmbFormat.SelectedItem?.ToString(), "PDF", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("В текущей версии поддерживается только формат PDF.", AppBranding.SystemTitle);
                        return;
                    }

                    try
                    {
                        var files = DocumentService.GenerateSalesPackage(order, selectedDocs, outputPath);
                        MessageBox.Show(
                            $"Сформировано документов: {files.Count}\nПапка:\n{outputPath}",
                            AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Process.Start("explorer.exe", outputPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                };

                footer.Controls.Add(btnClose);
                footer.Controls.Add(btnGenerate);
                root.Controls.Add(scroll, 0, 0);
                root.Controls.Add(footer, 0, 1);
                form.BodyPanel.Controls.Add(root);
                form.ShowDialog(FindForm());
            }
        }

        private static void AddDialogSection(TableLayoutPanel stack, Control section, int bottomMargin = 8)
        {
            section.Dock = DockStyle.Fill;
            section.Margin = new Padding(0, 0, 0, bottomMargin);
            var row = stack.RowCount;
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            stack.Controls.Add(section, 0, row);
            stack.RowCount = row + 1;
        }

        private void ShowStocksDialog()
        {
            if (_selected == null)
                return;

            var order = _selected;
            var stockQty = order.StockId > 0 ? SalesService.GetStockQuantity(order.StockId) : 0;

            using (var form = new ModalForm("Запрос остатков на складе", 440, 360))
            {
                var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var content = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
                var stack = new FlowLayoutPanel
                {
                    Dock = DockStyle.Top,
                    FlowDirection = FlowDirection.TopDown,
                    AutoSize = true,
                    WrapContents = false,
                    Width = 400
                };

                stack.Controls.Add(CreateAlertPanel("ℹ",
                    "Складской учёт ведётся на бумаге. Запрос фиксируется в системе с датой/временем проверки.",
                    AppTheme.HintBg, AppTheme.HintBorder, Color.FromArgb(29, 78, 216)));

                var grid = UiControls.CreateGrid();
                grid.Height = 56;
                grid.Width = 392;
                grid.DataSource = new List<object>
                {
                    new
                    {
                        Культура = order.ProductName,
                        Требуется = order.QuantityTons + " т",
                        Остаток = stockQty + " т"
                    }
                };
                var gridBox = UiControls.CreateGroupBox("Запрошенные позиции");
                gridBox.Width = 392;
                gridBox.Controls.Add(grid);
                stack.Controls.Add(gridBox);

                var txtFact = UiControls.CreateFieldBox();
                AttachPlaceholder(txtFact, "напр.: Пшеница — 85 т, подтверждено 09.06.2026");
                var txtTime = UiControls.CreateFieldBox(true);
                txtTime.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                var responseBox = CreateFieldsGroup("Ответ руководителя склада", table =>
                {
                    AddFormRow(table, 0, "Фактически:", txtFact);
                    AddFormRow(table, 1, "Время проверки:", txtTime);
                });
                responseBox.Width = 392;
                stack.Controls.Add(responseBox);
                content.Controls.Add(stack);

                var footer = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    Height = 34,
                    Padding = new Padding(0, 8, 0, 0)
                };
                var btnCancel = UiControls.CreateButton("Отмена", false, 90);
                btnCancel.DialogResult = DialogResult.Cancel;
                var btnConfirm = UiControls.CreateButton("✔  Подтвердить", true, 120);
                btnConfirm.Click += (s, e) =>
                {
                    if (txtFact.Text.Contains("напр.:"))
                    {
                        MessageBox.Show("Укажите фактические остатки.", AppBranding.SystemTitle);
                        return;
                    }

                    SalesService.ConfirmStock(order.Id);
                    MessageBox.Show("Остатки подтверждены. Запрос зафиксирован.", AppBranding.SystemTitle);
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                };

                footer.Controls.Add(btnCancel);
                footer.Controls.Add(btnConfirm);
                layout.Controls.Add(content, 0, 0);
                layout.Controls.Add(footer, 0, 1);
                form.BodyPanel.Controls.Add(layout);

                if (form.ShowDialog(FindForm()) == DialogResult.OK)
                    LoadData();
            }
        }

        private sealed class DocItem
        {
            public DocItem(string label, bool defaultChecked)
            {
                Label = label;
                DefaultChecked = defaultChecked;
            }

            public string Label { get; }
            public bool DefaultChecked { get; }
            public CheckBox CheckBox { get; set; }
        }

        private sealed class StockOption
        {
            public int StockId { get; set; }
            public int Quantity { get; set; }
            public string Display { get; set; }
        }

        private static ComboBox CreateComboBox()
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTheme.FontUi,
                Height = 24,
                IntegralHeight = false
            };
        }

        private static GroupBox CreateFieldsGroup(string title, Action<TableLayoutPanel> build)
        {
            var box = UiControls.CreateGroupBox(title);
            box.Dock = DockStyle.Top;
            box.AutoSize = true;
            box.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            box.Margin = new Padding(0);
            box.Padding = new Padding(10, 10, 10, 10);

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(0, 2, 0, 0)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            build(table);
            box.Controls.Add(table);
            return box;
        }

        private static void AddFormRow(TableLayoutPanel table, int row, string labelText, Control field)
        {
            while (table.RowStyles.Count <= row)
            {
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                table.RowCount = row + 1;
            }

            var lbl = UiControls.CreateFieldLabel(labelText, 110);
            lbl.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            lbl.Margin = new Padding(0, 6, 8, 2);
            lbl.BackColor = Color.Transparent;

            field.Dock = DockStyle.Fill;
            field.Margin = new Padding(0, 4, 0, 2);
            if (!(field is TableLayoutPanel))
                field.MinimumSize = new Size(200, 24);

            table.Controls.Add(lbl, 0, row);
            table.Controls.Add(field, 1, row);
        }

        private static Panel CreateAlertPanel(string icon, string text, Color bg, Color border, Color fore)
        {
            var panel = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = bg,
                Padding = new Padding(10, 8, 10, 8),
                MinimumSize = new Size(0, 36),
                Dock = DockStyle.Top
            };
            panel.Paint += (s, e) =>
            {
                using (var pen = new Pen(border))
                    e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            var lbl = new Label
            {
                Text = icon + "  " + text,
                AutoSize = true,
                Font = AppTheme.FontUi,
                ForeColor = fore,
                BackColor = Color.Transparent,
                Dock = DockStyle.Top,
                MaximumSize = new Size(420, 0)
            };
            panel.Controls.Add(lbl);
            panel.Resize += (s, e) => lbl.MaximumSize = new Size(Math.Max(120, panel.ClientSize.Width), 0);
            return panel;
        }

        private static void AttachPlaceholder(TextBox box, string placeholder)
        {
            box.ForeColor = AppTheme.TextLight;
            box.Text = placeholder;
            box.GotFocus += (s, e) =>
            {
                if (box.Text == placeholder)
                {
                    box.Text = "";
                    box.ForeColor = AppTheme.TextBody;
                }
            };
            box.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(box.Text))
                {
                    box.Text = placeholder;
                    box.ForeColor = AppTheme.TextLight;
                }
            };
        }

        private static bool TryParseInt(TextBox box, string placeholder, out int value)
        {
            value = 0;
            var text = box.Text?.Trim() ?? "";
            if (text == placeholder || string.IsNullOrEmpty(text))
                return false;

            return int.TryParse(text.Replace(" ", ""), out value) && value > 0;
        }

        private static bool TryParseDecimal(TextBox box, string placeholder, out decimal value)
        {
            value = 0;
            var text = box.Text?.Trim() ?? "";
            if (text == placeholder || string.IsNullOrEmpty(text))
                return false;

            return decimal.TryParse(text.Replace(" ", "").Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out value);
        }
    }
}
