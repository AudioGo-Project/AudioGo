namespace AudioGo_Mobile.Views;

[QueryProperty(nameof(TourId), "tourId")]
public partial class TourDetailPage : ContentPage
{
    public string? TourId { get; set; }

    public TourDetailPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // ViewModel sẽ được bind và load tour data từ TourId
        // TODO: inject TourDetailViewModel khi DI sẵn sàng
    }

    private async void OnBackClicked(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
