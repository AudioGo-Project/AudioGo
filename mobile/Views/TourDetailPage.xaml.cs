using AudioGo.ViewModels;

namespace AudioGo_Mobile.Views;

[QueryProperty(nameof(TourId), "tourId")]
public partial class TourDetailPage : ContentPage
{
    private readonly TourDetailViewModel _vm;

    public string? TourId
    {
        get => _vm.TourName; // only used as setter from navigation
        set
        {
            if (!string.IsNullOrEmpty(value))
                _ = _vm.LoadAsync(value);
        }
    }

    public TourDetailPage(TourDetailViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Nếu chưa có tourId (vào trực tiếp không qua navigate), load tour demo
        if (string.IsNullOrEmpty(_vm.TourName))
            await _vm.LoadAsync("tour-1");
    }

    private async void OnBackClicked(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
