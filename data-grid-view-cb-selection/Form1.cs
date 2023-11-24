using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace data_grid_view_cb_selection
{
    public partial class Form1 : Form
    {
        Dictionary<ProductType, MyProduct> ProductDefinitions;
        private DataGridView dataGridView1;
        public Form1() => InitializeComponent();
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ProductDefinitions = new Dictionary<ProductType, MyProduct>
            {
                { ProductType.CPU, new MyProduct{ Id = 0, Name = nameof(ProductType.CPU), Price = 660.0 }},
                { ProductType.Monitor, new MyProduct{ Id = 1, Name = nameof(ProductType.Monitor), Price = 150.0 }},
                { ProductType.Mouse, new MyProduct { Id = 2, Name = nameof(ProductType.Mouse), Price = 5.0 }},
            };
            blDatasource = new BindingList<MyClass>
            {
                new MyClass{ 
                    Id = 0, 
                    ProductId = ProductType.CPU,
                    Price = ProductDefinitions[ProductType.CPU].Price,
                    Quantity = 1 },
                new MyClass{ 
                    Id = 1,
                    ProductId = ProductType.Monitor,
                    Price = ProductDefinitions[ProductType.Monitor].Price,
                    Quantity = 1 },
                new MyClass{ 
                    Id = 2, 
                    ProductId = ProductType.Mouse,
                    Price = ProductDefinitions[ProductType.Mouse].Price,
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
            cbColumn.DataSource = Enum.GetValues(typeof(ProductType));
            cbColumn.DataPropertyName = nameof(MyClass.ProductId);

            dataGridView1.EditingControlShowing += dataGridView1_EditingControlShowing;
            blDatasource.ListChanged += (sender, e) =>
            {
                switch (e.ListChangedType)
                {
                    case ListChangedType.ItemChanged:
                        switch (e.PropertyDescriptor?.Name)
                        {
                            case nameof(MyClass.ProductId):
                                MyClass item = blDatasource[e.NewIndex];
                                var def = ProductDefinitions[item.ProductId];
                                item.Price = def.Price;
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            };
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
#if DEBUG
                    // Verify that all the bindings did what they should.
                    var item = blDatasource[dgv.CurrentCell.RowIndex];
                    Debug.WriteLine($"ComboBox value: {cb.Text}");
                    Debug.WriteLine($"Item value    : {item.ProductId}");
                    Debug.Assert(cb.SelectedItem.Equals(item.ProductId));
#endif
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
        public string? Name { get; set; }
        public double Price { get; set; }
    }
}
