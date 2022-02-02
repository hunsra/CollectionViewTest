using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace CollectionViewTest
{
    public class VewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class ItemViewModel : VewModelBase
    {
        private string _value1;
        private string _value2;

        public string Value1
        {
            get => _value1;

            set
            {
                _value1 = value;
                RaisePropertyChanged();
            }
        }

        public string Value2
        {
            get => _value2;

            set
            {
                _value2 = value;
                RaisePropertyChanged();
            }
        }

        public ItemViewModel Instance
        {
            get => this;
        }

        public ItemViewModel()
        {
        }
    }

    public class MainViewModel : VewModelBase
    {
        private static Random random = new Random();
        private bool _isBusy;
        private ObservableCollection<ItemViewModel> _items;
        private ItemViewModel _selectedItem;

        private static string RandomString()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, random.Next(10, 32)).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private bool CanClearData()
        {
            return !_isBusy && _items != null && _items.Count > 0;
        }

        private async Task ClearData()
        {
            if (CanClearData())
            {
                await Task.Run(() =>
                {
                    // Get rid of the collection
                    // This will cause Clear() to be called on the collection before assigning
                    // null to the property.  See Items property setter.
                    Items = null;
                });
            }
        }

        private bool CanGetData()
        {
            return !_isBusy;
        }

        private async Task GetData()
        {
            if (CanGetData())
            {
                IsBusy = true;

                await Task.Run(() =>
                {
                    // Create a new collection
                    Items = new ObservableCollection<ItemViewModel>();

                    // Generate random items and add to collection
                    int count = random.Next(100, 1000);
                    for (int i = 0; i < count; i++)
                    {
                        _items.Add(new ItemViewModel() { Value1 = $"{i}: {RandomString()}", Value2 = RandomString() });
                    }
                });

                IsBusy = false;
            }
        }

        private bool CanDelete(object item)
        {
            return !_isBusy && item is ItemViewModel ivm && ivm != null;
        }

        private async Task Delete(ItemViewModel item)
        {
            if (CanDelete(item))
            {
                await Task.Run(() =>
                {
                    if (_items.Contains(item))
                    {
                        _items.Remove(item);
                    }
                });
            }
        }

        public bool IsBusy
        {
            get => _isBusy;

            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    RaisePropertyChanged();
                    GetDataCommand.RaiseCanExecuteChanged();
                    ClearDataCommand.RaiseCanExecuteChanged();
                    DeleteCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<ItemViewModel> Items
        {
            get => _items;

            set
            {
                if (_items != value)
                {
                    if (_items != null)
                    {
                        // Clear the items in the collection.
                        // This seems to trigger the CollectionView
                        // to stop working properly.
                        _items.Clear();
                    }

                    _items = value;
                    RaisePropertyChanged();
                    ClearDataCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ItemViewModel SelectedItem
        {
            get => _selectedItem;

            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    RaisePropertyChanged();
                }
            }
        }

        public AsyncCommand GetDataCommand { get; private set; }

        public AsyncCommand ClearDataCommand { get; private set; }

        public AsyncCommand<ItemViewModel> DeleteCommand { get; private set; }

        public MainViewModel()
        {
            _isBusy = false;
            _items = null;
            _selectedItem = null;

            GetDataCommand = new AsyncCommand(GetData, CanGetData);
            ClearDataCommand = new AsyncCommand(ClearData, CanClearData);
            DeleteCommand = new AsyncCommand<ItemViewModel>(Delete, CanDelete);
        }
    }

    public static class ViewModelLocator
    {
        private static MainViewModel _mainViewModel;

        public static MainViewModel MainViewModel
        {
            get
            {
                if (_mainViewModel == null)
                {
                    _mainViewModel = new MainViewModel();
                }

                return _mainViewModel;
            }
        }

        static ViewModelLocator()
        {
            _mainViewModel = null;
        }
    }
}
