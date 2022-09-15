using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace refresh_dgv_from_query_every_minute
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        private BindingList<Record> DataSource { get; } = new BindingList<Record>();
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            initDataGridView();
            _ = startPolling(_cts.Token);
        }

        private async Task startPolling(CancellationToken token)
        {
            while(!token.IsCancellationRequested)
            {
                // This is a synchronous task in the background that can take as long as it needs to.
                List<Record> mockRecordset = mockSomeDatabaseQuery();
                // Marshal onto the UI thread for the update
                BeginInvoke((MethodInvoker)delegate 
                {
                    DataSource.Clear();
                    foreach (var record in mockRecordset)
                    {
                        DataSource.Add(record);
                    }
                    datagridview.Refresh();
                    if (_toggleColor = !_toggleColor)
                        datagridview.BackgroundColor = Color.PaleGreen;
                    else
                        datagridview.BackgroundColor = Color.PaleTurquoise;
                });
                // Reduced the time interval to make it more testable.
                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
        }
        private bool _toggleColor;

        private CancellationTokenSource _cts = new CancellationTokenSource();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                _cts.Cancel();
            }
            base.Dispose(disposing);
        }

        private List<Record> mockSomeDatabaseQuery()
        {
            var mockRecordset = new List<Record>();
            Random randomPriceGen = new Random();
            for (int i = 1; i <= 3; i++)
            {
                var price = i == 1 ? 1.0m : (decimal)randomPriceGen.NextDouble() * 100;
                mockRecordset.Add(new Record
                {
                    Description = $"Item {(char)('A' + (i - 1))}",
                    Quantity = i,
                    Price = price,
                });
            }
            return mockRecordset;
        }

        private void initDataGridView()
        {
            datagridview.DataSource = DataSource;
            datagridview.AllowUserToAddRows = false;
            DataSource.ListChanged += (sender, e) =>
            {
                if (e.ListChangedType == ListChangedType.ItemChanged)
                {
                    datagridview.Refresh();
                }
            };
            // Add dummy record to autogen the columns
            DataSource.Add(new Record());
            // Do a little column formatting
            foreach (DataGridViewColumn column in datagridview.Columns)
            {
                switch (column.Name)
                {
                    case nameof(Record.Description):
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        column.MinimumWidth = 120;
                        break;
                    case nameof(Record.Quantity):
                    case nameof(Record.Price):
                        column.DefaultCellStyle.Format = "F2";
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        break;
                    default:
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        break;
                }
            }
            // Remove the dummy record.
            DataSource.Clear();
        }
    }

    [Table("records")]
    internal class Record
    {
        [PrimaryKey]
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
