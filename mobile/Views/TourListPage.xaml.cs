using AudioGo.ViewModels;

namespace AudioGo_Mobile.Views;

public partial class TourListPage : ContentPage
{
    private readonly TourListViewModel _vm;

    public TourListPage(TourListViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_vm.Tours.Count == 0)
            await _vm.LoadToursAsync();
    }

    private async void OnCreateTourClicked(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CreateTourPage));
    }
}
