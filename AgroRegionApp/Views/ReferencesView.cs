using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AgroRegionApp.Data;
using AgroRegionApp.UI;

namespace AgroRegionApp.Views
{
    internal sealed class ReferencesView : UserControl, INavigationScreen
    {
        private readonly DataGridView _grid;
        private readonly Panel _detailPanel;
        private Panel _cpToolbarExtras;
        private TextBox _txtSearch;
        private ComboBox _cmbType;
        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;

        private TextBox _txtCpName;
        private TextBox _txtCpType;
        private TextBox _txtCpInn;
        private TextBox _txtCpPhone;
        private TextBox _txtCpEmail;
        private TextBox _txtCpAddress;

        private TextBox _txtProdName;
        private TextBox _txtProdVariety;
        private TextBox _txtProdSeason;
        private TextBox _txtProdUnit;
        private TextBox _txtProdPrice;

        private List<CounterpartyEntry> _counterparties = new List<CounterpartyEntry>();
        private List<ProductRow> _products = new List<ProductRow>();
        private CounterpartyEntry _selectedCp;
        private ProductRow _selectedProduct;
        private int _activeTab;

        public ReferencesView()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.ContentBg;

            var tabStrip = UiControls.CreateTabStrip(new[] { "Контрагенты", "Номенклатура" });
            tabStrip.TabChanged += index =>
            {
                _activeTab = index;
                _selectedCp = null;
                _selectedProduct = null;
                LoadData();
            };

            var toolbar = BuildToolbar();

            _grid = UiControls.CreateGrid();
            _grid.Dock = DockStyle.Top;
            _grid.Height = 176;
            _grid.SelectionChanged += GridOnSelectionChanged;
            GridHelper.ApplyStatusColumnFormatting(_grid, "Тип");

            _detailPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 8, 0, 0),
                Visible = false
            };
            _detailPanel.Controls.Add(BuildProductDetailPanel());
            _detailPanel.Controls.Add(BuildCounterpartyDetailPanel());

            Controls.Add(_detailPanel);
            Controls.Add(_grid);
            Controls.Add(toolbar);
            Controls.Add(tabStrip);

        }

        public void OnNavigatedTo() => LoadData();

        private Panel BuildToolbar()
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = AppTheme.ContentBg };

            _btnAdd = UiControls.CreateButton("＋  Добавить", true, 110);
            _btnAdd.Location = new Point(0, 4);
            _btnAdd.Click += (s, e) => ShowAddDialog();

            _btnEdit = UiControls.CreateButton("✏  Изменить", false, 100);
            _btnEdit.Location = new Point(116, 4);
            _btnEdit.Enabled = false;
            _btnEdit.Click += (s, e) => ShowEditDialog();

            _btnDelete = UiControls.CreateButton("✕  Удалить", false, 95, danger: true);
            _btnDelete.Location = new Point(218, 4);
            _btnDelete.Enabled = false;
            _btnDelete.Click += (s, e) => DeleteSelected();

            _cpToolbarExtras = new Panel
            {
                Dock = DockStyle.Right,
                Width = 360,
                Height = 34,
                BackColor = AppTheme.ContentBg
            };

            _cmbType = new ComboBox
            {
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = AppTheme.FontUi,
                Location = new Point(220, 5)
            };
            _cmbType.Items.AddRange(new object[] { "Все типы", "Покупатели", "Поставщики" });
            _cmbType.SelectedIndex = 0;
            _cmbType.SelectedIndexChanged += (s, e) => ApplyFilter();

            _txtSearch = UiControls.CreateFieldBox();
            _txtSearch.Width = 210;
            _txtSearch.Location = new Point(0, 5);
            AttachPlaceholder(_txtSearch, "Поиск по названию, ИНН...");
            _txtSearch.TextChanged += (s, e) => ApplyFilter();

            _cpToolbarExtras.Controls.Add(_cmbType);
            _cpToolbarExtras.Controls.Add(_txtSearch);

            panel.Controls.Add(_cpToolbarExtras);
            panel.Controls.Add(_btnAdd);
            panel.Controls.Add(_btnEdit);
            panel.Controls.Add(_btnDelete);
            return panel;
        }

        private Control BuildCounterpartyDetailPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Visible = true, Name = "cpDetail" };
            var box = UiControls.CreateGroupBox("Реквизиты контрагента (Покупатель / Поставщик)");
            box.Dock = DockStyle.Fill;

            var fields = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(4, 8, 4, 4)
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            for (var i = 0; i < 3; i++)
                fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _txtCpName = AddDetailField(fields, 0, 0, "Наименование:");
            _txtCpType = AddDetailField(fields, 0, 2, "Тип:");
            _txtCpInn = AddDetailField(fields, 1, 0, "ИНН:");
            _txtCpPhone = AddDetailField(fields, 1, 2, "Телефон:");
            _txtCpEmail = AddDetailField(fields, 2, 0, "E-mail:");
            _txtCpAddress = AddDetailField(fields, 2, 2, "Адрес:");

            box.Controls.Add(fields);
            panel.Controls.Add(box);
            return panel;
        }

        private Control BuildProductDetailPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Visible = false, Name = "prodDetail" };
            var box = UiControls.CreateGroupBox("Карточка товара (Товар)");
            box.Dock = DockStyle.Fill;

            var fields = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(4, 8, 4, 4)
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            for (var i = 0; i < 3; i++)
                fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _txtProdName = AddDetailField(fields, 0, 0, "Наименование:");
            _txtProdVariety = AddDetailField(fields, 0, 2, "Сорт:");
            _txtProdSeason = AddDetailField(fields, 1, 0, "Сезонность:");
            _txtProdUnit = AddDetailField(fields, 1, 2, "Ед. изм.:");
            _txtProdPrice = AddDetailField(fields, 2, 0, "Баз. цена:");

            box.Controls.Add(fields);
            panel.Controls.Add(box);
            return panel;
        }

        private static TextBox AddDetailField(TableLayoutPanel table, int row, int labelCol, string caption)
        {
            var lbl = UiControls.CreateFieldLabel(caption, 100);
            lbl.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            lbl.Margin = new Padding(0, 6, 6, 2);

            var box = UiControls.CreateFieldBox(true);
            box.Dock = DockStyle.Fill;
            box.Margin = new Padding(0, 4, labelCol == 0 ? 12 : 0, 2);

            table.Controls.Add(lbl, labelCol, row);
            table.Controls.Add(box, labelCol + 1, row);
            return box;
        }

        private void LoadData()
        {
            _grid.SelectionChanged -= GridOnSelectionChanged;
            try
            {
                _selectedCp = null;
                _selectedProduct = null;
                _detailPanel.Visible = false;
                UpdateToolbarState();
                _cpToolbarExtras.Visible = _activeTab == 0;

                if (_activeTab == 0)
                {
                    _counterparties = ReferenceService.GetCounterparties();
                    ApplyFilter();
                }
                else
                {
                    _products = ReferenceService.GetProducts();
                    UiControls.BindGrid(_grid, _products.Select(p => new
                    {
                        Наименование = p.Name,
                        Сорт = p.Variety,
                        Сезонность = p.Seasonality,
                        ЕдИзм = p.Unit,
                        БазЦена = p.BasePrice.ToString("N0")
                    }).ToList());
                    FormatProductGrid();
                }

                _grid.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить справочники:\n" + ex.Message, AppBranding.SystemTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                _grid.SelectionChanged += GridOnSelectionChanged;
            }
        }

        private void ApplyFilter()
        {
            if (_activeTab != 0)
                return;

            var search = GetSearchText();
            var typeFilter = _cmbType?.SelectedItem?.ToString() ?? "Все типы";

            var filtered = _counterparties.Where(c =>
            {
                var typeOk = typeFilter == "Все типы"
                    || (typeFilter == "Покупатели" && c.IsCustomer)
                    || (typeFilter == "Поставщики" && !c.IsCustomer);
                var textOk = string.IsNullOrEmpty(search)
                    || c.Name.ToLowerInvariant().Contains(search)
                    || (c.Inn ?? "").Contains(search);
                return typeOk && textOk;
            }).ToList();

            UiControls.BindGrid(_grid, filtered.Select(c => new
            {
                Наименование = c.Name,
                Тип = c.Type,
                ИНН = string.IsNullOrEmpty(c.Inn) ? "—" : c.Inn,
                Телефон = string.IsNullOrEmpty(c.Phone) ? "—" : c.Phone,
                Email = string.IsNullOrEmpty(c.Email) ? "—" : c.Email
            }).ToList());

            if (_grid.Columns.Contains("Email"))
                _grid.Columns["Email"].HeaderText = "E-mail";
        }

        private void FormatProductGrid()
        {
            if (_grid.Columns.Contains("ЕдИзм"))
                _grid.Columns["ЕдИзм"].HeaderText = "Ед. изм.";
            if (_grid.Columns.Contains("БазЦена"))
                _grid.Columns["БазЦена"].HeaderText = "Баз. цена (₽/т)";
        }

        private string GetSearchText()
        {
            return (_txtSearch == null ? "" : GetFieldText(_txtSearch, "Поиск по названию, ИНН...")).ToLowerInvariant();
        }

        private List<CounterpartyEntry> GetFilteredCounterparties()
        {
            var search = GetSearchText();
            var typeFilter = _cmbType?.SelectedItem?.ToString() ?? "Все типы";
            return _counterparties.Where(c =>
            {
                var typeOk = typeFilter == "Все типы"
                    || (typeFilter == "Покупатели" && c.IsCustomer)
                    || (typeFilter == "Поставщики" && !c.IsCustomer);
                var textOk = string.IsNullOrEmpty(search)
                    || c.Name.ToLowerInvariant().Contains(search)
                    || (c.Inn ?? "").Contains(search);
                return typeOk && textOk;
            }).ToList();
        }

        private void GridOnSelectionChanged(object sender, EventArgs e)
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.Index < 0)
            {
                _selectedCp = null;
                _selectedProduct = null;
                _detailPanel.Visible = false;
                UpdateToolbarState();
                return;
            }

            var index = _grid.CurrentRow.Index;
            if (_activeTab == 0)
            {
                var filtered = GetFilteredCounterparties();
                if (index >= filtered.Count)
                    return;

                _selectedCp = filtered[index];
                _selectedProduct = null;
                ShowCounterpartyDetail(_selectedCp);
            }
            else
            {
                if (index >= _products.Count)
                    return;

                _selectedProduct = _products[index];
                _selectedCp = null;
                ShowProductDetail(_selectedProduct);
            }

            _detailPanel.Visible = true;
            UpdateToolbarState();
        }

        private void ShowCounterpartyDetail(CounterpartyEntry cp)
        {
            foreach (Control c in _detailPanel.Controls)
                c.Visible = c.Name == "cpDetail";

            _txtCpName.Text = cp.Name;
            _txtCpType.Text = cp.Type;
            _txtCpInn.Text = string.IsNullOrEmpty(cp.Inn) ? "—" : cp.Inn;
            _txtCpPhone.Text = string.IsNullOrEmpty(cp.Phone) ? "—" : cp.Phone;
            _txtCpEmail.Text = string.IsNullOrEmpty(cp.Email) ? "—" : cp.Email;
            _txtCpAddress.Text = string.IsNullOrEmpty(cp.Address) ? "—" : cp.Address;
        }

        private void ShowProductDetail(ProductRow product)
        {
            foreach (Control c in _detailPanel.Controls)
                c.Visible = c.Name == "prodDetail";

            _txtProdName.Text = product.Name;
            _txtProdVariety.Text = product.Variety;
            _txtProdSeason.Text = product.Seasonality;
            _txtProdUnit.Text = product.Unit;
            _txtProdPrice.Text = product.BasePrice.ToString("N0") + " ₽/т";
        }

        private void UpdateToolbarState()
        {
            var has = _activeTab == 0 ? _selectedCp != null : _selectedProduct != null;
            _btnEdit.Enabled = has;
            _btnDelete.Enabled = has;
        }

        private void ShowAddDialog()
        {
            if (_activeTab == 0)
                ShowCounterpartyDialog(false, null);
            else
                ShowProductDialog(false, null);
        }

        private void ShowEditDialog()
        {
            if (_activeTab == 0)
            {
                if (_selectedCp == null)
                    return;
                ShowCounterpartyDialog(true, _selectedCp);
            }
            else
            {
                if (_selectedProduct == null)
                    return;
                ShowProductDialog(true, _selectedProduct);
            }
        }

        private void DeleteSelected()
        {
            if (_activeTab == 0)
            {
                if (_selectedCp == null)
                    return;

                var answer = MessageBox.Show(
                    $"Удалить контрагента «{_selectedCp.Name}»?",
                    AppBranding.SystemTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (answer != DialogResult.Yes)
                    return;

                try
                {
                    if (_selectedCp.IsCustomer)
                        ReferenceService.DeleteCustomer(_selectedCp.Id);
                    else
                        ReferenceService.DeleteSupplier(_selectedCp.Id);
                    _selectedCp = null;
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                if (_selectedProduct == null)
                    return;

                var answer = MessageBox.Show(
                    $"Удалить товар «{_selectedProduct.Name}»?",
                    AppBranding.SystemTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (answer != DialogResult.Yes)
                    return;

                try
                {
                    ReferenceService.DeleteProduct(_selectedProduct.Id);
                    _selectedProduct = null;
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ShowCounterpartyDialog(bool isEdit, CounterpartyEntry existing)
        {
            var title = isEdit ? "Изменить контрагента" : "Добавить контрагента";

            var cmbType = CreateComboBox();
            cmbType.Items.AddRange(new object[] { "Покупатель", "Поставщик", "Оба" });
            cmbType.SelectedIndex = existing == null ? 0 : existing.IsCustomer ? 0 : 1;
            cmbType.Enabled = !isEdit;

            var txtName = UiControls.CreateFieldBox();
            var txtInn = UiControls.CreateFieldBox();
            var txtPhone = UiControls.CreateFieldBox();
            var txtEmail = UiControls.CreateFieldBox();
            var txtAddress = UiControls.CreateFieldBox();

            if (existing != null)
            {
                txtName.Text = existing.Name;
                txtInn.Text = existing.Inn;
                txtPhone.Text = existing.Phone;
                txtEmail.Text = existing.Email;
                txtAddress.Text = existing.Address;
            }
            else
            {
                AttachPlaceholder(txtName, "ООО «Название»");
                AttachPlaceholder(txtInn, "1234567890");
                AttachPlaceholder(txtPhone, "+7 495 ...");
                AttachPlaceholder(txtEmail, "info@company.ru");
                AttachPlaceholder(txtAddress, "г. Москва, ул. ...");
            }

            using (var form = new ModalForm(title, 440, 360))
            {
                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    RowCount = 2,
                    ColumnCount = 1
                };
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var box = CreateFieldsGroup("Покупатель / Поставщик", table =>
                {
                    AddFormRow(table, 0, "Тип:", cmbType);
                    AddFormRow(table, 1, "Наименование:", txtName);
                    AddFormRow(table, 2, "ИНН:", txtInn);
                    AddFormRow(table, 3, "Телефон:", txtPhone);
                    AddFormRow(table, 4, "E-mail:", txtEmail);
                    AddFormRow(table, 5, "Адрес:", txtAddress);
                });

                var footer = CreateDialogFooter(form, () =>
                {
                    var name = GetFieldText(txtName, "ООО «Название»");
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        MessageBox.Show("Укажите наименование.", AppBranding.SystemTitle);
                        return false;
                    }

                    var phone = GetFieldText(txtPhone, "+7 495 ...");
                    var email = GetFieldText(txtEmail, "info@company.ru");
                    var address = GetFieldText(txtAddress, "г. Москва, ул. ...");
                    var inn = GetFieldText(txtInn, "1234567890");

                    try
                    {
                        if (isEdit)
                        {
                            if (existing.IsCustomer)
                            {
                                ReferenceService.UpdateCustomer(existing.Id, name, phone, email, address, inn);
                            }
                            else
                            {
                                ReferenceService.UpdateSupplier(existing.Id, name, phone, email, inn, address);
                            }
                        }
                        else
                        {
                            var type = cmbType.SelectedItem?.ToString() ?? "Покупатель";
                            if (type == "Покупатель" || type == "Оба")
                                ReferenceService.CreateCustomer(name, phone, email, address, inn);
                            if (type == "Поставщик" || type == "Оба")
                                ReferenceService.CreateSupplier(name, phone, email, inn, address);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    return true;
                });

                layout.Controls.Add(box, 0, 0);
                layout.Controls.Add(footer, 0, 1);
                form.BodyPanel.Controls.Add(layout);

                if (form.ShowDialog(FindForm()) == DialogResult.OK)
                    LoadData();
            }
        }

        private void ShowProductDialog(bool isEdit, ProductRow existing)
        {
            var title = isEdit ? "Изменить позицию номенклатуры" : "Добавить позицию номенклатуры";

            var txtName = UiControls.CreateFieldBox();
            var txtVariety = UiControls.CreateFieldBox();
            var txtSeason = UiControls.CreateFieldBox();
            var cmbUnit = CreateComboBox();
            cmbUnit.Items.AddRange(new object[] { "т (тонна)", "кг", "ц (центнер)" });
            cmbUnit.SelectedIndex = 0;
            var txtPrice = UiControls.CreateFieldBox();

            if (existing != null)
            {
                txtName.Text = existing.Name;
                txtVariety.Text = existing.Variety;
                txtSeason.Text = existing.Seasonality;
                cmbUnit.SelectedItem = UnitToDisplay(existing.Unit);
                txtPrice.Text = existing.BasePrice.ToString();
            }
            else
            {
                AttachPlaceholder(txtName, "Пшеница 3 кл.");
                AttachPlaceholder(txtVariety, "Экстра");
                AttachPlaceholder(txtSeason, "Лето-2026");
                AttachPlaceholder(txtPrice, "5000");
            }

            using (var form = new ModalForm(title, 420, 340))
            {
                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    RowCount = 2,
                    ColumnCount = 1
                };
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var box = CreateFieldsGroup("Товар (с/х культура)", table =>
                {
                    AddFormRow(table, 0, "Наименование:", txtName);
                    AddFormRow(table, 1, "Сорт:", txtVariety);
                    AddFormRow(table, 2, "Сезонность:", txtSeason);
                    AddFormRow(table, 3, "Ед. изм.:", cmbUnit);
                    AddFormRow(table, 4, "Баз. цена (₽/т):", txtPrice);
                });

                var footer = CreateDialogFooter(form, () =>
                {
                    var name = GetFieldText(txtName, "Пшеница 3 кл.");
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        MessageBox.Show("Укажите наименование.", AppBranding.SystemTitle);
                        return false;
                    }

                    var variety = GetFieldText(txtVariety, "Экстра");
                    var season = GetFieldText(txtSeason, "Лето-2026");
                    var unit = DisplayToUnit(cmbUnit.SelectedItem?.ToString());
                    var priceText = GetFieldText(txtPrice, "5000");
                    if (!int.TryParse(priceText.Replace(" ", ""), out var price))
                        price = 0;

                    try
                    {
                        if (isEdit)
                        {
                            ReferenceService.UpdateProduct(existing.Id, name, variety, season, unit, price);
                        }
                        else
                        {
                            ReferenceService.CreateProduct(name, variety, season, unit, price);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    return true;
                });

                layout.Controls.Add(box, 0, 0);
                layout.Controls.Add(footer, 0, 1);
                form.BodyPanel.Controls.Add(layout);

                if (form.ShowDialog(FindForm()) == DialogResult.OK)
                    LoadData();
            }
        }

        private static string UnitToDisplay(string unit)
        {
            switch (unit)
            {
                case "кг": return "кг";
                case "ц": return "ц (центнер)";
                default: return "т (тонна)";
            }
        }

        private static string DisplayToUnit(string display)
        {
            if (display == "кг") return "кг";
            if (display == "ц (центнер)") return "ц";
            return "т";
        }

        private static FlowLayoutPanel CreateDialogFooter(ModalForm form, Func<bool> onSave)
        {
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
                if (!onSave())
                    return;
                form.DialogResult = DialogResult.OK;
                form.Close();
            };
            footer.Controls.Add(btnCancel);
            footer.Controls.Add(btnSave);
            return footer;
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

        private static string GetFieldText(TextBox box, string placeholder)
        {
            var text = box.Text?.Trim() ?? "";
            return text == placeholder ? "" : text;
        }
    }
}
