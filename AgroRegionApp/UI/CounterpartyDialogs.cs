using System;
using System.Drawing;
using System.Windows.Forms;
using AgroRegionApp.Data;
using AgroRegionApp;

namespace AgroRegionApp.UI
{
    internal static class CounterpartyDialogs
    {
        public static void BindCustomers(ComboBox combo)
        {
            combo.DataSource = ReferenceService.GetCustomers();
            combo.DisplayMember = nameof(CustomerRow.Name);
            combo.ValueMember = nameof(CustomerRow.Id);
        }

        public static void BindSuppliers(ComboBox combo)
        {
            combo.DataSource = ReferenceService.GetSuppliers();
            combo.DisplayMember = nameof(SupplierRow.Name);
            combo.ValueMember = nameof(SupplierRow.Id);
        }

        public static Control CreatePickerRow(IWin32Window owner, ComboBox combo, bool isCustomer)
        {
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.Font = AppTheme.FontUi;
            combo.Dock = DockStyle.Fill;
            combo.Margin = new Padding(0, 2, 6, 2);

            if (isCustomer)
                BindCustomers(combo);
            else
                BindSuppliers(combo);

            var btnAdd = UiControls.CreateButton("+ Новый", false, 0);
            btnAdd.Margin = new Padding(0, 2, 0, 2);
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Click += (s, e) =>
            {
                if (isCustomer)
                    TryAddCustomer(owner, combo);
                else
                    TryAddSupplier(owner, combo);
            };

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(combo, 0, 0);
            panel.Controls.Add(btnAdd, 1, 0);
            return panel;
        }

        public static bool TryAddCustomer(IWin32Window owner, ComboBox combo)
        {
            var txtName = UiControls.CreateFieldBox();
            var txtInn = UiControls.CreateFieldBox();
            var txtPhone = UiControls.CreateFieldBox();
            var txtEmail = UiControls.CreateFieldBox();
            var txtAddress = UiControls.CreateFieldBox();

            if (!ShowCounterpartyForm(owner, "Новый покупатель", 420, 300,
                    ("Наименование:", txtName),
                    ("ИНН:", txtInn),
                    ("Телефон:", txtPhone),
                    ("E-mail:", txtEmail),
                    ("Адрес:", txtAddress)))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Укажите наименование покупателя.", AppBranding.SystemTitle);
                return false;
            }

            try
            {
                var id = ReferenceService.CreateCustomer(
                    txtName.Text, txtPhone.Text, txtEmail.Text, txtAddress.Text, txtInn.Text);
                BindCustomers(combo);
                combo.SelectedValue = id;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        public static bool TryAddSupplier(IWin32Window owner, ComboBox combo)
        {
            var txtName = UiControls.CreateFieldBox();
            var txtPhone = UiControls.CreateFieldBox();
            var txtEmail = UiControls.CreateFieldBox();

            if (!ShowCounterpartyForm(owner, "Новый поставщик", 400, 240,
                    ("Наименование:", txtName),
                    ("Телефон:", txtPhone),
                    ("E-mail:", txtEmail)))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Укажите наименование поставщика.", AppBranding.SystemTitle);
                return false;
            }

            try
            {
                var id = ReferenceService.CreateSupplier(txtName.Text, txtPhone.Text, txtEmail.Text);
                BindSuppliers(combo);
                combo.SelectedValue = id;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppBranding.SystemTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        public static bool HasSelection(ComboBox combo)
        {
            return combo.SelectedValue != null && combo.Items.Count > 0;
        }

        private static bool ShowCounterpartyForm(
            IWin32Window owner,
            string title,
            int width,
            int height,
            params (string Label, Control Field)[] rows)
        {
            using (var form = new ModalForm(title, width, height))
            {
                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    RowCount = 2,
                    ColumnCount = 1
                };
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var table = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    ColumnCount = 2,
                    Padding = new Padding(4, 4, 4, 0)
                };
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

                for (var i = 0; i < rows.Length; i++)
                {
                    table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    table.RowCount = i + 1;

                    var lbl = UiControls.CreateFieldLabel(rows[i].Label, 110);
                    lbl.Margin = new Padding(0, 6, 8, 2);

                    var field = rows[i].Field;
                    field.Dock = DockStyle.Fill;
                    field.Margin = new Padding(0, 4, 0, 2);
                    field.MinimumSize = new Size(180, 24);

                    table.Controls.Add(lbl, 0, i);
                    table.Controls.Add(field, 1, i);
                }

                var footer = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    Height = 34,
                    Padding = new Padding(0, 8, 0, 0)
                };
                var btnCancel = UiControls.CreateButton("Отмена", false, 90);
                btnCancel.DialogResult = DialogResult.Cancel;
                var btnSave = UiControls.CreateButton("Сохранить", true, 100);
                btnSave.Click += (s, e) =>
                {
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                };
                footer.Controls.Add(btnCancel);
                footer.Controls.Add(btnSave);

                layout.Controls.Add(table, 0, 0);
                layout.Controls.Add(footer, 0, 1);
                form.BodyPanel.Controls.Add(layout);

                return form.ShowDialog(owner) == DialogResult.OK;
            }
        }
    }
}
