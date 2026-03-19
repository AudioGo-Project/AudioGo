namespace AudioGo_Mobile.Views;

public partial class CreateTourPage : ContentPage
{
    public CreateTourPage()
    {
        InitializeComponent();
    }

    private async void OnCancelClicked(object? sender, TappedEventArgs e)
    {
        bool confirm = await DisplayAlertAsync(
            "Huỷ tạo tour",
            "Bạn có muốn huỷ không? Dữ liệu chưa lưu sẽ mất.",
            "Huỷ tour", "Tiếp tục");

        if (confirm)
            await Shell.Current.GoToAsync("..");
    }
}
