using System;
using System.Linq;
using System.Windows.Forms;
using AgroRegionApp.Data;
using AgroRegionApp.UI;

namespace AgroRegionApp.Views
{
    internal sealed class ReferencesView : UserControl
    {
        private readonly DataGridView _grid;
        private readonly Panel _detailPanel;
        private readonly Label _lblDetail;
        private int _activeTab;

        public ReferencesView()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.ContentBg;

            var tabStrip = UiControls.CreateTabStrip(new[] { "Контрагенты", "Номенклатура" });
            tabStrip.TabChanged += index => { _activeTab = index; LoadData(); };

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 34,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = AppTheme.ContentBg
            };
            var btnRefresh = UiControls.CreateButton("Обновить", false, 90);
            btnRefresh.Click += (s, e) => LoadData();
            toolbar.Controls.Add(btnRefresh);

            _grid = UiControls.CreateGrid();
            _grid.Dock = DockStyle.Top;
            _grid.Height = 240;
            _grid.SelectionChanged += GridOnSelectionChanged;

            _detailPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0), Visible = false };
            var box = UiControls.CreateGroupBox("Реквизиты");
            box.Dock = DockStyle.Fill;
            _lblDetail = new Label
            {
                Dock = DockStyle.Fill,
                Font = AppTheme.FontUi,
                ForeColor = AppTheme.TextBody,
                Padding = new Padding(8)
            };
            box.Controls.Add(_lblDetail);
            _detailPanel.Controls.Add(box);

            Controls.Add(_detailPanel);
            Controls.Add(_grid);
            Controls.Add(toolbar);
            Controls.Add(tabStrip);

            LoadData();
        }

        private void GridOnSelectionChanged(object sender, EventArgs e) => OnSelectionChanged();

        private void LoadData()
        {
            _grid.SelectionChanged -= GridOnSelectionChanged;
            try
            {
                _detailPanel.Visible = false;
                _grid.DataSource = null;
                _grid.ClearSelection();

                if (_activeTab == 0)
                {
                    var customers = ReferenceService.GetCustomers();
                    var suppliers = ReferenceService.GetSuppliers();
                    _grid.DataSource = customers.Select(c => new
                    {
                        Тип = "Покупатель",
                        Наименование = c.Name,
                        Телефон = c.Phone,
                        Email = c.Email,
                        Адрес = c.Address
                    }).Concat(suppliers.Select(s => new
                    {
                        Тип = "Поставщик",
                        Наименование = s.Name,
                        Телефон = s.Phone,
                        Email = s.Email,
                        Адрес = "—"
                    })).ToList();
                }
                else
                {
                    _grid.DataSource = ReferenceService.GetProducts().Select(p => new
                    {
                        Наименование = p.Name,
                        Сорт = p.Variety,
                        Сезонность = p.Seasonality
                    }).ToList();
                }
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

        private void OnSelectionChanged()
        {
            if (_grid.CurrentRow == null || _grid.CurrentRow.Index < 0)
                return;

            var row = _grid.CurrentRow;
            if (_activeTab == 0)
            {
                if (row.Cells.Count < 5)
                    return;

                _lblDetail.Text =
                    $"Тип: {row.Cells[0].Value}\r\n" +
                    $"Наименование: {row.Cells[1].Value}\r\n" +
                    $"Телефон: {row.Cells[2].Value}\r\n" +
                    $"E-mail: {row.Cells[3].Value}\r\n" +
                    $"Адрес: {row.Cells[4].Value}";
            }
            else
            {
                if (row.Cells.Count < 3)
                    return;

                _lblDetail.Text =
                    $"Наименование: {row.Cells[0].Value}\r\n" +
                    $"Сорт: {row.Cells[1].Value}\r\n" +
                    $"Сезонность: {row.Cells[2].Value}";
            }

            _detailPanel.Visible = true;
        }
    }
}
