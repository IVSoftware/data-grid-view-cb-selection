using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace data_grid_view_cb_selection
{
    public partial class Form1 : Form
    {
        List<MyProduct> ProductDefinitions;
        public Form1() => InitializeComponent();
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ProductDefinitions = new List<MyProduct>
            {
                new MyProduct{ Id = 0, Name = ProductType.CPU, Price = 660.0 },
                new MyProduct{ Id = 1, Name = ProductType.Monitor, Price = 150.0 },
                new MyProduct { Id = 2, Name = ProductType.Mouse, Price = 5.0 },
            };
            blDatasource = new BindingList<MyClass>
            {
                new MyClass{
                    Id = 0,
                    ProductId = ProductType.CPU,
                    Price = ProductDefinitions.First(_=>_.Name.Equals(ProductType.CPU)).Price,
                    Quantity = 1 },
                new MyClass{
                    Id = 1,
                    ProductId = ProductType.Monitor,
                    Price = ProductDefinitions.First(_=>_.Name.Equals(ProductType.Monitor)).Price,
                    Quantity = 1 },
                new MyClass{
                    Id = 2,
                    ProductId = ProductType.Mouse,
                    Price = ProductDefinitions.First(_=>_.Name.Equals(ProductType.Mouse)).Price,
                    Quantity = 1 },
            };
            dataGridView1.AutoGenerateColumns = true; // HIGHLY recommended
            dataGridView1.DataSource = blDatasource;
            DataGridViewColumn oldColumn = dataGridView1.Columns[nameof(MyClass.ProductId)];
            DataGridViewComboBoxColumn cbColumn = new DataGridViewComboBoxColumn
            {
                Name = oldColumn.Name,
                HeaderText = oldColumn.HeaderText,
            };
            int swapIndex = oldColumn.Index;
            dataGridView1.Columns.RemoveAt(swapIndex);
            dataGridView1.Columns.Insert(swapIndex, cbColumn);
            cbColumn.DataSource = ProductDefinitions;
            cbColumn.DisplayMember = "Name";
            cbColumn.DataPropertyName = nameof(MyClass.ProductId);
            dataGridView1.EditingControlShowing += dataGridView1_EditingControlShowing;
        }
        BindingList<MyClass> blDatasource;

        private void dataGridView1_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (sender is DataGridView dgv && e.Control is DataGridViewComboBoxEditingControl cb)
            {
                cb.SelectionChangeCommitted -= localOnSelectionChangeCommitted;
                cb.SelectionChangeCommitted += localOnSelectionChangeCommitted;
            }
            void localOnSelectionChangeCommitted(object? sender, EventArgs e)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    dgv.EndEdit();
                    if(sender is ComboBox cb)
                    {
                        MyProduct product = ProductDefinitions[cb.SelectedIndex];
                        blDatasource[dgv.CurrentCell.RowIndex].Price = product.Price;
                    }
                });
            }
        }
    }

    public enum ProductType
    {
        CPU,
        Monitor,
        Mouse,
    }

    [DebuggerDisplay("{Id} {ProductId}")]
    public class MyClass : INotifyPropertyChanged
    {
        public int Id
        {
            get => _id;
            set
            {
                if (!Equals(_id, value))
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }
        int _id = 0;
        public ProductType ProductId
        {
            get => _productId;
            set
            {
                if (!Equals(_productId, value))
                {
                    _productId = value;
                    OnPropertyChanged();
                }
            }
        }
        ProductType _productId = default;

        public double Price
        {
            get => _price;
            internal set
            {
                if (!Equals(_price, value))
                {
                    _price = value;
                    OnPropertyChanged();
                }
            }
        }
        double _price = default;

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (!Equals(_quantity, value))
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }
        int _quantity = default;

        public double Total
        {
            get => _total;
            internal set
            {
                if (!Equals(_total, value))
                {
                    _total = value;
                    OnPropertyChanged();
                }
            }
        }
        double _total = default;

        private void OnPropertyChanged([CallerMemberName]string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Total = Price * Quantity;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }


    [DebuggerDisplay("{Id} {Name} {Price}")]
    public class MyProduct
    {
        public int Id { get; set; }
        public ProductType Name { get; set; }
        public double Price { get; set; }
    }
}
