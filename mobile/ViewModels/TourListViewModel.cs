using AudioGo.ViewModels;
using Shared.DTOs;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Windows.Input;

namespace AudioGo.ViewModels
{
    public class TourListViewModel : BaseViewModel
    {
        private readonly HttpClient _http;
        private string _baseUrl;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { SetProperty(ref _isLoading, value); OnPropertyChanged(nameof(HasTours)); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { SetProperty(ref _searchText, value); FilterTours(value); }
        }

        private List<TourRowVm> _allTours = new();
        public ObservableCollection<TourRowVm> Tours { get; } = new();

        public bool HasTours => !IsLoading && Tours.Count > 0;
        public string CountLabel => $"{Tours.Count} tour quanh bạn";

        public ICommand OpenTourCommand { get; }
        public ICommand RefreshCommand { get; }

        public TourListViewModel()
        {
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            // Fallback URL nếu chưa có DI — đổi khi có config
            _baseUrl = "http://10.0.2.2:5000/api/mobile";

            OpenTourCommand = new Command<string>(async tourId =>
            {
                if (!string.IsNullOrEmpty(tourId))
                    await Shell.Current.GoToAsync($"{nameof(AudioGo_Mobile.Views.TourDetailPage)}?tourId={tourId}");
            });

            RefreshCommand = new Command(async () => await LoadToursAsync());
        }

        public async Task LoadToursAsync(string lang = "vi")
        {
            IsLoading = true;
            try
            {
                var result = await _http.GetFromJsonAsync<List<TourSummaryDto>>(
                    $"{_baseUrl}/tours?lang={lang}");

                _allTours = result?.Select(t => new TourRowVm(t)).ToList() ?? new();
                FilterTours(SearchText);
            }
            catch
            {
                // Fallback: giữ danh sách mock nếu API chưa chạy
                if (_allTours.Count == 0)
                    LoadMockData();
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(HasTours));
                OnPropertyChanged(nameof(CountLabel));
            }
        }

        private void FilterTours(string query)
        {
            Tours.Clear();
            var filtered = string.IsNullOrWhiteSpace(query)
                ? _allTours
                : _allTours.Where(t => t.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

            foreach (var t in filtered)
                Tours.Add(t);
        }

        private void LoadMockData()
        {
            _allTours = new List<TourRowVm>
            {
                new TourRowVm(new TourSummaryDto("mock-1", "Tour Hải Sản Vĩnh Khánh",
                    "Khám phá tinh hoa ẩm thực hải sản nổi tiếng ở khu phố Vĩnh Khánh.",
                    8, null, DateTime.Today)),
                new TourRowVm(new TourSummaryDto("mock-2", "Vĩnh Khánh — Ký Ức Phố Cảng",
                    "Hành trình ngược thời gian về lịch sử hình thành khu phố Vĩnh Khánh.",
                    12, null, DateTime.Today)),
            };
        }
    }

    /// <summary>Row VM cho CollectionView (có computed labels).</summary>
    public class TourRowVm
    {
        private readonly TourSummaryDto _dto;

        public TourRowVm(TourSummaryDto dto) => _dto = dto;

        public string TourId       => _dto.TourId;
        public string Name         => _dto.Name;
        public string Description  => _dto.Description;
        public string? ThumbnailUrl => _dto.ThumbnailUrl;
        public string PoiCountLabel => $"📍 {_dto.PoiCount} điểm";
        public string CreatedAtLabel => _dto.CreatedAt.ToString("dd/MM/yyyy");
    }
}
