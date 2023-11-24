# DataGridView ComboBox Selection

One issue in `dataGridView1_EditingControlShowing` is that you `+=` the event every time it's shown without `-=` first, so make sure there's only one event subscribed at a time. But the main "clue" to what you say about the item "reverting" seems to be that (when I run your code) the DGV is still in Edit mode after the selecting the new value in the ComboBox.

[![stuck in edit][1]][1]

That's easy to remedy. Use the `SelectionChangeCommitted` event to commit the edit.

```
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
        });
    }
}
```
[![tracking][2]][2]

___

#### Binding Reference

Here's a reference for setting up `DataGridView` properly and implementing `INotifyPropertyChanged` in your `MyClass`.

```
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

        dataGridView1.CellValueChanged += (sender, e) =>
        {
            if (dataGridView1.Columns[nameof(MyClass.ProductId)].Index == e.ColumnIndex)
            {
                dataGridView1.EndEdit();
            }
        };
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
}
```

**MyClass**

```
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
```

  [1]: https://i.stack.imgur.com/oDodE.png
  [2]: https://i.stack.imgur.com/xtaug.png