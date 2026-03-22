using AudioGo.ViewModels;

namespace AudioGo_Mobile.Views;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel _vm;

    public MapPage(MapViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitAsync();
    }

    private async void OnSearchTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//Search");

    private void OnFilterTapped(object? sender, TappedEventArgs e)
    {
        // TODO: toggle filter panel
    }

    private void OnLocateMeTapped(object? sender, TappedEventArgs e)
    {
        if (_vm.UserLocation != null)
        {
            MainMap.MoveToRegion(Microsoft.Maui.Maps.MapSpan.FromCenterAndRadius(
                _vm.UserLocation, Microsoft.Maui.Maps.Distance.FromKilometers(1.0)));
        }
    }
}
