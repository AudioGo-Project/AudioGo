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
            private set { SetProperty(ref _pois, value); }
        }

        private POI? _activePoi;
        public POI? ActivePoi
        {
            get => _activePoi;
            private set { SetProperty(ref _activePoi, value); }
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

        // ── Commands ────────────────────────────────────────────────
        public System.Windows.Input.ICommand InitCommand           { get; }
        public System.Windows.Input.ICommand SetVoiceCommand       { get; }
        public System.Windows.Input.ICommand CreateTourCommand     { get; }
        public System.Windows.Input.ICommand StartTourCommand      { get; }
        public System.Windows.Input.ICommand QrScanCommand         { get; }
        public System.Windows.Input.ICommand SearchCommand         { get; }

        public MainViewModel(SyncService sync, IGeofenceService geofence,
                             IAudioService audio, ILocationService location)
        {
            _sync = sync;
            _geofence = geofence;
            _audio = audio;
            _location = location;

            _geofence.PoiTriggered += OnPoiTriggered;
            _location.LocationUpdated += OnLocationUpdated;

            // Commands
            InitCommand = new Command(async () => await InitAsync());
            SetVoiceCommand = new Command<string>(v =>
            {
                // TODO: persist voice preference via Preferences
                Preferences.Default.Set("voice", v ?? "female");
            });
            CreateTourCommand = new Command(async () =>
                await Shell.Current.GoToAsync(nameof(AudioGo_Mobile.Views.CreateTourPage)));
            StartTourCommand = new Command<string>(async tourId =>
            {
                if (!string.IsNullOrEmpty(tourId))
                    await Shell.Current.GoToAsync(
                        $"{nameof(AudioGo_Mobile.Views.TourDetailPage)}?tourId={tourId}");
            });
            QrScanCommand = new Command(async () =>
                await Shell.Current.GoToAsync(nameof(AudioGo_Mobile.Views.QrScanPage)));
            SearchCommand = new Command(async () =>
                await Shell.Current.GoToAsync("//Search"));
        }

        public async Task InitAsync()
        {
            IsLoading = true;
            StatusMessage = "Đang tải dữ liệu...";
            try
            {
                Pois = await _sync.GetPoisAsync(CurrentLanguage);
                await _geofence.StartMonitoringAsync(Pois);
                await _location.StartAsync();
                StatusMessage = $"Đang theo dõi {Pois.Count} điểm";
            }
            catch (Exception)
            {
                // API chưa chạy — dùng mock data để Home/Map không trắng
                Pois = GetMockPois();
                StatusMessage = $"Offline — hiển thị {Pois.Count} điểm mẫu";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private static List<POI> GetMockPois() => new()
        {
            new POI { PoiId="poi-1", Title="Hải Sản Bã Tư",           Latitude=100.7295, Longitude=106.6921, ActivationRadius=5, Description="Tôm hùm, ghẹ rang muối tươi ngon",       Categories=new(){"🦐 Hải sản"}, LanguageCode="vi" },
            new POI { PoiId="poi-2", Title="Bún Bò Gánh Vĩnh Khánh", Latitude=100.7297, Longitude=106.6919, ActivationRadius=5, Description="Bún bò Huế chuẩn vị, mở từ 5 giờ sáng", Categories=new(){"🍜 Bún bò"},  LanguageCode="vi" },
            new POI { PoiId="poi-3", Title="Ốc Bà Ba Mươi",           Latitude=100.7293, Longitude=106.6924, ActivationRadius=5, Description="Ốc hương, ốc len xào dừa đặc sản",      Categories=new(){"🍢 Ốc"},      LanguageCode="vi" },
            new POI { PoiId="poi-4", Title="Cà Phê Cô Lan",           Latitude=100.7300, Longitude=106.6918, ActivationRadius=5, Description="Cà phê vợt pha kiểu miền Nam cổ điển",  Categories=new(){"☕ Cà phê"},   LanguageCode="vi" },
            new POI { PoiId="poi-5", Title="Miếu Ông Địa Vĩnh Khánh", Latitude=100.7291, Longitude=106.6930, ActivationRadius=5, Description="Di tích lịch sử cuối Vĩnh Khánh",        Categories=new(){"🏛️ Di tích"}, LanguageCode="vi" },
            new POI { PoiId="poi-6", Title="Bánh Mì Cô Hai",          Latitude=100.7299, Longitude=106.6925, ActivationRadius=5, Description="Bánh mì thịt nguội ăn kèm bì làm sẵn",  Categories=new(){"🥖 Bánh mì"}, LanguageCode="vi" },
        };

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
