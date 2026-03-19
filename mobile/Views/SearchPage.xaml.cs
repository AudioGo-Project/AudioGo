namespace AudioGo_Mobile.Views;

public partial class SearchPage : ContentPage
{
    public SearchPage()
    {
        InitializeComponent();
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        // Relay text sang ViewModel nếu có ViewModel binding
        // Hiện dùng code-behind cho simplicity
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Focus vào search entry khi page xuất hiện
        SearchEntry?.Focus();
    }
}
