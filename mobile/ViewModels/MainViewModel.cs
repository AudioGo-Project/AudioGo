using AudioGo.Helpers;
using AudioGo.Services;
using AudioGo.Services.Interfaces;
using AudioGo.ViewModels;
using Shared;

namespace AudioGo.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly SyncService _sync;
        private readonly IGeofenceService _geofence;
        private readonly IAudioService _audio;
        private readonly ILocationService _location;

        // ── State ──────────────────────────────────────────────────
        private List<POI> _pois = new();
        public List<POI> Pois
        {
            get => _pois;
            private set
            {
                SetProperty(ref _pois, value);
                OnPropertyChanged(nameof(NearbyPois));
                OnPropertyChanged(nameof(HasNearbyPois));
                OnPropertyChanged(nameof(NearbyEmpty));
            }
        }

        private POI? _activePoi;
        public POI? ActivePoi
        {
            get => _activePoi;
            private set
            {
                SetProperty(ref _activePoi, value);
                OnPropertyChanged(nameof(HasActivePoi));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set { SetProperty(ref _isLoading, value); }
        }

        private string _statusMessage = "Chờ GPS...";
        public string StatusMessage
        {
            get => _statusMessage;
            private set { SetProperty(ref _statusMessage, value); }
        }

        private string _currentLanguage = LanguageHelper.GetDeviceLanguageCode();
        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (SetProperty(ref _currentLanguage, value))
                    _ = ReloadPoisAsync();
            }
        }

        public bool IsAudioPlaying => _audio.IsPlaying;

        // ── UI helper computed properties for XAML ─────────────────
        public List<POI> NearbyPois => _pois;
        public bool HasNearbyPois => _pois.Count > 0;
        public bool NearbyEmpty => _pois.Count == 0 && !_isLoading;
        public bool HasActivePoi => _activePoi != null;

        // ── Commands ───────────────────────────────────────────────
        public System.Windows.Input.ICommand PlayPoiCommand => new Command<POI>(async poi =>
        {
            if (poi != null) await TriggerAudioAsync(poi);
        });

        public System.Windows.Input.ICommand OpenPoiDetailCommand => new Command<POI>(async poi =>
        {
            if (poi != null)
                await Shell.Current.GoToAsync($"{nameof(AudioGo_Mobile.Views.PoiDetailPage)}?poiId={poi.PoiId}");
        });

        public void ToggleAudio()
        {
            if (_audio.IsPlaying) _ = _audio.StopAsync();
            else if (_activePoi != null) _ = TriggerAudioAsync(_activePoi);
            OnPropertyChanged(nameof(IsAudioPlaying));
        }

        public void StopAudio()
        {
            _ = _audio.StopAsync();
            ActivePoi = null;
            OnPropertyChanged(nameof(IsAudioPlaying));
            OnPropertyChanged(nameof(HasActivePoi));
        }

        public MainViewModel(SyncService sync, IGeofenceService geofence,
                             IAudioService audio, ILocationService location)
        {
            _sync = sync;
            _geofence = geofence;
            _audio = audio;
            _location = location;

            _geofence.PoiTriggered += OnPoiTriggered;
            _location.LocationUpdated += OnLocationUpdated;
        }

        public async Task InitAsync()
        {
            IsLoading = true;
            StatusMessage = "Đang tải dữ liệu...";
            try
            {
                Pois = await _sync.GetPoisAsync(CurrentLanguage);
                if (Pois.Count == 0)
                {
                    // Fallback Mock Data
                    Pois = new List<POI>
                    {
                        new POI { PoiId = "mock-poi-1", Title = "Ốc Oanh Vĩnh Khánh", Description = "Quán ốc nổi tiếng với các món ốc hương rang muối ớt, sò điệp nướng mỡ hành.", Latitude = 10.7601, Longitude = 106.7025, LogoUrl = "hero_bg.jpg", LanguageCode = CurrentLanguage },
                        new POI { PoiId = "mock-poi-2", Title = "Bánh Canh Cua Vĩnh Khánh", Description = "Quán bánh canh cua nước cốt dừa đặc trưng miền Tây.", Latitude = 10.7610, Longitude = 106.7030, LogoUrl = "hero_bg.jpg", LanguageCode = CurrentLanguage },
                        new POI { PoiId = "mock-poi-3", Title = "Nhà thờ Xóm Chiếu", Description = "Nhà thờ cổ với kiến trúc độc đáo tại trung tâm quận 4.", Latitude = 10.7625, Longitude = 106.7051, LogoUrl = "splash_bg.jpg", LanguageCode = CurrentLanguage }
                    };
                }
                
                await _geofence.StartMonitoringAsync(Pois);
                await _location.StartAsync();
                StatusMessage = $"Đang theo dõi {Pois.Count} điểm";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task ReloadPoisAsync()
        {
            IsLoading = true;
            StatusMessage = "Đang chuyển ngôn ngữ...";
            try
            {
                await _audio.StopAsync();
                Pois = await _sync.GetPoisAsync(CurrentLanguage);
                await _geofence.StartMonitoringAsync(Pois);
                StatusMessage = $"Đang theo dõi {Pois.Count} điểm ({CurrentLanguage})";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Lỗi: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task StopAsync()
        {
            await _geofence.StopMonitoringAsync();
            await _location.StopAsync();
            await _audio.StopAsync();
        }

        /// <summary>Called from MapPage mini-play FAB to (re)play the active POI audio.</summary>
        public async Task TriggerAudioAsync(POI poi)
        {
            if (!string.IsNullOrEmpty(poi.AudioUrl))
                await _audio.PlayFileAsync(poi.AudioUrl);
            else if (!string.IsNullOrEmpty(poi.Description))
                await _audio.SpeakAsync(poi.Description, poi.LanguageCode);
            OnPropertyChanged(nameof(IsAudioPlaying));
        }

        private void OnLocationUpdated(object? sender, (double Lat, double Lon) loc)
            => _geofence.OnLocationUpdated(loc.Lat, loc.Lon);

        private async void OnPoiTriggered(object? sender, POI poi)
        {
            ActivePoi = poi;
            StatusMessage = $"Đang phát: {poi.Title}";
            OnPropertyChanged(nameof(IsAudioPlaying));

            if (!string.IsNullOrEmpty(poi.AudioUrl))
                await _audio.PlayFileAsync(poi.AudioUrl);
            else if (!string.IsNullOrEmpty(poi.Description))
                await _audio.SpeakAsync(poi.Description, poi.LanguageCode);

            OnPropertyChanged(nameof(IsAudioPlaying));
        }
    }
}
