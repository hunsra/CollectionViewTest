using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CollectionViewTest
{

    public partial class MainPage : ContentPage
    {
        private MainViewModel _vm;
        public MainPage()
        {
            InitializeComponent();

            // Get a reference to the main view model (set in XAML via the ViewModelLocator)
            _vm = BindingContext as MainViewModel;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            // If no items are in the Items collection...
            if (_vm != null && (_vm.Items == null || _vm.Items.Count == 0))
            {
                // Execute the GetDataCommand command
                await _vm.GetDataCommand.ExecuteAsync();
            }
        }
    }
}
