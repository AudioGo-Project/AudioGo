using AudioGo.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Shared;

namespace AudioGo_Mobile.Views;

public partial class MapPage : ContentPage
{
    private readonly MapViewModel _vm;
    private readonly MainViewModel _main;
    private string? _activePinPoiId;
    private bool _sheetExpanded = true;
    private List<MapPoiItem> _allPoiItems = new();

    public MapPage(MapViewModel vm, MainViewModel main)
    {
        InitializeComponent();
        _vm = vm;
        _main = main;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Load pins từ danh sách POI hiện tại (hoặc mock nếu empty)
        _vm.LoadPois(_main.Pois);
        RefreshPins();
        LoadPoiListItems(_main.Pois);

        // Theo dõi POI active để hiển thị banner
        _main.PropertyChanged += OnMainPropertyChanged;
        _vm.PropertyChanged += OnVmPropertyChanged;

        // Yêu cầu vị trí hiện tại để căn giữa bản đồ
        await RequestInitialLocationAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _main.PropertyChanged -= OnMainPropertyChanged;
        _vm.PropertyChanged -= OnVmPropertyChanged;
    }

    private void RefreshPins()
    {
        MapControl.Pins.Clear();
        foreach (var pin in _vm.Pins)
        {
            pin.MarkerClicked += OnPinMarkerClicked;
            MapControl.Pins.Add(pin);
        }
    }

    private async void OnPinMarkerClicked(object? sender, PinClickedEventArgs e)
    {
        if (sender is Pin pin && pin.BindingContext is string poiId)
        {
            e.HideInfoWindow = false;
            _activePinPoiId = poiId;
        }
        await Task.CompletedTask;
    }

    private void OnMainPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainViewModel.ActivePoi)) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var poi = _main.ActivePoi;
            if (poi is null)
            {
                ActivePoiBanner.IsVisible = false;
                return;
            }
            ActivePoiTitle.Text = poi.Title;
            ActivePoiDesc.Text = poi.Description;
            _activePinPoiId = poi.PoiId;
            ActivePoiBanner.IsVisible = true;
        });
    }

    private void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MapViewModel.VisibleRegion)) return;
        if (_vm.VisibleRegion is { } region)
            MapControl.MoveToRegion(region);
    }

    private async void OnPoiBannerDetailClicked(object? sender, EventArgs e)
    {
        if (_activePinPoiId is null) return;
        await Shell.Current.GoToAsync($"{nameof(PoiDetailPage)}?poiId={_activePinPoiId}");
    }

    private async void OnQrScanClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(QrScanPage));
    }

    private async void OnLanguageClicked(object? sender, EventArgs e)
    {
        var selected = await DisplayActionSheetAsync(
            "Chọn ngôn ngữ thuyết minh",
            "Huỷ",
            null,
            "🇻🇳 Tiếng Việt",
            "🇬🇧 English",
            "🇨🇳 中文",
            "🇰🇷 한국어",
            "🇯🇵 日本語");

        var lang = selected switch
        {
            "🇻🇳 Tiếng Việt" => "vi",
            "🇬🇧 English"    => "en",
            "🇨🇳 中文"        => "zh-Hans",
            "🇰🇷 한국어"       => "ko",
            "🇯🇵 日本語"       => "ja",
            _ => null
        };

        if (lang is not null)
        {
            _main.CurrentLanguage = lang;
            // Pins sẽ refresh sau khi ReloadPoisAsync hoàn tất
            _main.PropertyChanged += OnPoisReloaded;
        }
    }

    private void OnPoisReloaded(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainViewModel.Pois)) return;
        _main.PropertyChanged -= OnPoisReloaded;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _vm.LoadPois(_main.Pois);
            RefreshPins();
        });
    }

    private void OnMiniPlayClicked(object? sender, EventArgs e)
    {
        if (_activePinPoiId is null) return;
        var poi = _main.ActivePoi;
        if (poi is not null)
            Task.Run(() => _main.TriggerAudioAsync(poi));
    }

    // ── Bottom sheet toggle ──────────────────────────────────────────
    private void OnBottomSheetHandleTapped(object? sender, TappedEventArgs e)
    {
        _sheetExpanded = !_sheetExpanded;
        PoiCollectionView.IsVisible = _sheetExpanded;
        ExpandIcon.Text = _sheetExpanded ? "▲" : "▼";
    }

    // ── Category chip filter ─────────────────────────────────────────
    private void OnCategoryChipTapped(object? sender, TappedEventArgs e)
    {
        var cat = (e.Parameter as string) ?? "all";
        var filtered = cat == "all"
            ? _allPoiItems
            : _allPoiItems.Where(p => p.Category == cat).ToList();
        PoiCollectionView.ItemsSource = filtered;
        PoiCountLabel.Text = $"{filtered.Count} địa điểm quanh đây";
    }

    // ── Search on map ────────────────────────────────────────────────
    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var q = e.NewTextValue?.ToLowerInvariant() ?? string.Empty;
        var filtered = string.IsNullOrWhiteSpace(q)
            ? _allPoiItems
            : _allPoiItems.Where(p => p.Title.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        PoiCollectionView.ItemsSource = filtered;
        PoiCountLabel.Text = $"{filtered.Count} kết quả";
    }

    // ── Locate me FAB ────────────────────────────────────────────────
    private async void OnLocateMeClicked(object? sender, EventArgs e)
        => await RequestInitialLocationAsync();

    // ── POI list interactions ────────────────────────────────────────
    private void OnPoiCardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string poiId)
        {
            _activePinPoiId = poiId;
            var item = _allPoiItems.FirstOrDefault(p => p.PoiId == poiId);
            if (item is null) return;
            ActivePoiTitle.Text = item.Title;
            ActivePoiDesc.Text = item.CategoryLabel;
            ActivePoiBanner.IsVisible = true;
        }
    }

    private void OnPoiPlayClicked(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string poiId)
        {
            _activePinPoiId = poiId;
            var poi = _main.Pois.FirstOrDefault(p => p.PoiId == poiId);
            if (poi is not null)
                Task.Run(() => _main.TriggerAudioAsync(poi));
        }
    }

    // ── Load POI items cho CollectionView ───────────────────────────
    private void LoadPoiListItems(IEnumerable<POI> pois)
    {
        _allPoiItems = pois.Select(p => new MapPoiItem
        {
            PoiId = p.PoiId,
            Title = p.Title,
            LogoUrl = p.LogoUrl,
            Category = p.Categories?.FirstOrDefault() ?? "all",
            CategoryLabel = p.Categories?.FirstOrDefault() ?? "🏛️ Địa điểm",
            DistanceLabel = "< 200m"
        }).ToList();

        // Fallback mock nếu API chưa có data
        if (_allPoiItems.Count == 0)
        {
            _allPoiItems = new List<MapPoiItem>
            {
                new() { PoiId="poi-1", Title="Hải Sản Bã Tư ", Category="seafood", CategoryLabel="🦐 Hải sản", DistanceLabel="50m" },
                new() { PoiId="poi-2", Title="Bún Bò Gánh Vĩnh Khánh", Category="noodle", CategoryLabel="🍜 Bún bò", DistanceLabel="120m" },
                new() { PoiId="poi-3", Title="Ốc Bà Ba Mười", Category="shellfish", CategoryLabel="🍢 Ốc", DistanceLabel="80m" },
                new() { PoiId="poi-4", Title="Cà Phê Cô Lan", Category="cafe", CategoryLabel="☕ Cà phê", DistanceLabel="200m" },
                new() { PoiId="poi-5", Title="Miếu Ông Địa Vĩnh Khánh", Category="heritage", CategoryLabel="🏛️ Di tích", DistanceLabel="150m" },
            };
        }

        PoiCollectionView.ItemsSource = _allPoiItems;
        PoiCountLabel.Text = $"{_allPoiItems.Count} địa điểm quanh đây";
    }

    private async Task RequestInitialLocationAsync()
    {
        try
        {
            var loc = await Geolocation.Default.GetLastKnownLocationAsync();
            if (loc is not null)
            {
                _vm.MoveTo(loc.Latitude, loc.Longitude);
                MapControl.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Location(loc.Latitude, loc.Longitude),
                    Distance.FromKilometers(1)));
            }
        }
        catch { /* GPS không khả dụng */ }
    }
}

/// <summary>Lightweight DTO cho MapPage bottom sheet list.</summary>
public class MapPoiItem
{
    public string PoiId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string Category { get; init; } = "all";
    public string CategoryLabel { get; init; } = string.Empty;
    public string DistanceLabel { get; init; } = string.Empty;
}
