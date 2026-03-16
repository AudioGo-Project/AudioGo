using AudioGo.ViewModels;

namespace AudioGo_Mobile.Views;

public partial class PoiDetailPage : ContentPage
{
    private readonly PoiDetailViewModel _vm;

    public PoiDetailPage(PoiDetailViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    private async void OnPlayPauseTapped(object? sender, TappedEventArgs e)
    {
        await _vm.TogglePlayPauseAsync();
        
        // Update icon button locally
        if (PlayPauseBtn != null)
        {
            PlayPauseBtn.Text = _vm.IsPlaying ? "⏸" : "▶";
        }
    }

    private void OnSkipBackClicked(object? sender, EventArgs e)
    {
        // TODO: Skip back audio
    }

    private void OnSkipForwardClicked(object? sender, EventArgs e)
    {
        // TODO: Skip forward audio
    }

    private async void OnStopClicked(object? sender, EventArgs e)
    {
        await _vm.StopAudioAsync();
        if (PlayPauseBtn != null)
        {
            PlayPauseBtn.Text = "▶";
        }
    }
}
