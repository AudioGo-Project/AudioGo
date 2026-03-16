using AudioGo.ViewModels;

namespace AudioGo_Mobile.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _vm;

    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Load data on start if empty
        if (_vm.Pois == null || !_vm.Pois.Any())
        {
            await _vm.InitAsync();
        }
    }
}
