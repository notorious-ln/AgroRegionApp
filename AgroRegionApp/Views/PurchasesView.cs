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
            _btnEdit.Click += (s, e) => ShowEditDialog();

            _btnDelete = UiControls.CreateButton("✕  Удалить", false, 95, danger: true);
            _btnDelete.Location = new Point(238, 4);
            _btnDelete.Enabled = false;
            _btnDelete.Click += (s, e) => DeleteSelected();

            _btnDocs = UiControls.CreateButton("📄  Документы", false, 110);
            _btnDocs.Location = new Point(335, 4);
            _btnDocs.Enabled = false;
            _btnDocs.Click += (s, e) => ShowDocumentsDialog();

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
            _btnDelete.Enabled = has && _selected.StatusName != "Исполнен";
            _btnDocs.Enabled = has && _selected.CanGenerateDocuments;
        }

        private void ShowEditDialog()
        {
            if (_selected == null)
                return;

            var order = _selected;
            var statuses = PurchaseService.GetStatuses();
            var items = PurchaseMockData.GetItems(order.Id);

            var txtNumber = UiControls.CreateFieldBox(true);
            txtNumber.Text = order.OrderNumber;
            var txtDate = UiControls.CreateFieldBox(true);
            txtDate.Text = order.Date.ToString("dd.MM.yyyy");
            var txtSupplier = UiControls.CreateFieldBox(true);
            txtSupplier.Text = order.SupplierName;
            var txtItems = UiControls.CreateFieldBox(true);
            txtItems.Text = string.Join("; ", items.Select(i => $"{i.ProductName} — {i.QtyTons} т"));
            var cmbStatus = CreateComboBox();
            cmbStatus.DataSource = statuses.ToList();
            cmbStatus.DisplayMember = "Name";
            cmbStatus.ValueMember = "Id";
            cmbStatus.SelectedValue = order.StatusId;

            using (var form = new ModalForm($"Изменение заказа {order.OrderNumber}", 440, 300))
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
                    AddFormRow(table, 2, "Поставщик:", txtSupplier);
                    AddFormRow(table, 3, "Позиции:", txtItems);
                    AddFormRow(table, 4, "Статус:", cmbStatus);
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
                    try
                    {
                        PurchaseService.UpdateOrder(order.Id, (byte)cmbStatus.SelectedValue);
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

            if (_selected.StatusName == "Исполнен")
            {
                MessageBox.Show("Исполненный заказ удалить нельзя.", AppBranding.SystemTitle,
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
                PurchaseService.DeleteOrder(_selected.Id);
                _selected = null;
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowCreateDialog()
        {
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
                orderBox.Height = 150;

                var combo = new ComboBox();
                var supplierPicker = CounterpartyDialogs.CreatePickerRow(form, combo, isCustomer: false);
                supplierPicker.Location = new Point(110, 78);
                supplierPicker.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                void ResizeSupplierPicker()
                {
                    supplierPicker.Width = Math.Max(220, orderBox.ClientSize.Width - 122);
                }
                orderBox.Resize += (s, e) => ResizeSupplierPicker();
                orderBox.Layout += (s, e) => ResizeSupplierPicker();

                var txtComment = UiControls.CreateFieldBox();
                AddLabeledField(orderBox, 24, "Номер:", $"ЗЗ-{nextNum:D5} (авто)", true);
                AddLabeledField(orderBox, 52, "Дата:", DateTime.Today.ToString("dd.MM.yyyy"), true);
                var lblSupplier = UiControls.CreateFieldLabel("Поставщик:", 90);
                lblSupplier.Location = new Point(12, 80);
                orderBox.Controls.Add(lblSupplier);
                orderBox.Controls.Add(supplierPicker);
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

                    if (!CounterpartyDialogs.HasSelection(combo))
                    {
                        MessageBox.Show("Выберите поставщика или добавьте нового.", AppBranding.SystemTitle);
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
                MessageBox.Show(
                    "Документы доступны для заказов со статусом «Оформлен» или «Исполнен».",
                    AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                AddDialogSection(stack, CreateAlertPanel("ℹ",
                    $"Статус заказа: {order.StatusName}. Комплект документов готов к формированию.",
                    AppTheme.HintBg, AppTheme.HintBorder, Color.FromArgb(29, 78, 216)));

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
                        var files = DocumentService.GeneratePurchasePackage(order, selectedDocs, outputPath);
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
                input.Location = new Point(110, y - 2);
                if (input.Width <= 0)
                    input.Width = 300;
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
