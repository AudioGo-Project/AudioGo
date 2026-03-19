using AudioGo.Services.Interfaces;
using Plugin.Maui.Audio;

namespace AudioGo.Services
{
    /// <summary>
    /// Phát audio: TTS (Text-to-Speech) hoặc file audio từ URL/local path.
    /// Xếp hàng (queue) để tránh overlap khi nhiều POI trigger cùng lúc.
    /// </summary>
    public class AudioService : IAudioService
    {
        private readonly IAudioManager _audioManager;
        private readonly Queue<Func<CancellationToken, Task>> _queue = new();
        private CancellationTokenSource _cts = new();
        private bool _isProcessing;
        private IAudioPlayer? _player;

        public AudioService(IAudioManager audioManager)
        {
            _audioManager = audioManager;
        }

        public bool IsPlaying { get; private set; }

        public async Task SpeakAsync(string text, string languageCode = "vi")
        {
            Enqueue(async ct =>
            {
                IsPlaying = true;
                var locale = (await TextToSpeech.Default.GetLocalesAsync())
                    .FirstOrDefault(l => l.Language.StartsWith(languageCode));
                await TextToSpeech.Default.SpeakAsync(text,
                    new SpeechOptions { Locale = locale }, ct);
                IsPlaying = false;
            });
        }

        /// <summary>
        /// Phát audio cho 1 POI theo thứ tự ưu tiên:
        ///   1. LocalAudioPath (file đã download → phát được offline, 0ms latency)
        ///   2. AudioUrl      (stream HTTP khi online, 2-5s)
        ///   3. Device TTS   (MAUI SpeechSynthesis, fallback cuối nếu không có audio)
        /// </summary>
        public Task PlayPoiAudioAsync(
            string? localAudioPath,
            string? audioUrl,
            string? fallbackText,
            string languageCode = "vi")
        {
            return Enqueue(async ct =>
            {
                IsPlaying = true;
                try
                {
                    // Tier 1 — local file (offline)
                    if (!string.IsNullOrEmpty(localAudioPath) && File.Exists(localAudioPath))
                    {
                        await PlayStreamAsync(File.OpenRead(localAudioPath), ct);
                        return;
                    }

                    // Tier 2 — HTTP stream (online)
                    if (!string.IsNullOrEmpty(audioUrl) &&
                        Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
                        var data = await http.GetByteArrayAsync(audioUrl, ct);
                        await PlayStreamAsync(new MemoryStream(data), ct);
                        return;
                    }

                    // Tier 3 — device TTS fallback
                    if (!string.IsNullOrEmpty(fallbackText))
                    {
                        var locale = (await TextToSpeech.Default.GetLocalesAsync())
                            .FirstOrDefault(l => l.Language.StartsWith(
                                languageCode.Split('-')[0], StringComparison.OrdinalIgnoreCase));
                        await TextToSpeech.Default.SpeakAsync(
                            fallbackText, new SpeechOptions { Locale = locale }, ct);
                    }
                }
                finally
                {
                    IsPlaying = false;
                }
            });
        }

        public Task PlayFileAsync(string urlOrPath)
        {
            Enqueue(async ct =>
            {
                IsPlaying = true;
                try
                {
                    DisposePlayer();
                    Stream stream;
                    if (urlOrPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
                        var data = await http.GetByteArrayAsync(urlOrPath, ct);
                        stream = new MemoryStream(data);
                    }
                    else
                    {
                        stream = File.OpenRead(urlOrPath);
                    }

                    await PlayStreamAsync(stream, ct);
                }
                finally
                {
                    IsPlaying = false;
                }
            });
            return Task.CompletedTask;
        }

        private async Task PlayStreamAsync(Stream stream, CancellationToken ct)
        {
            DisposePlayer();
            _player = _audioManager.CreatePlayer(stream);
            var tcs = new TaskCompletionSource();
            _player.PlaybackEnded += (s, e) => tcs.TrySetResult();
            ct.Register(() => { _player.Stop(); tcs.TrySetCanceled(); });
            _player.Play();
            await tcs.Task;
        }

        public async Task StopAsync()
        {
            await _cts.CancelAsync();
            _queue.Clear();
            DisposePlayer();
            _cts = new CancellationTokenSource();
            IsPlaying = false;
        }

        private void DisposePlayer()
        {
            if (_player is not null)
            {
                _player.Stop();
                _player.Dispose();
                _player = null;
            }
        }

        private void Enqueue(Func<CancellationToken, Task> action)
        {
            _queue.Enqueue(action);
            if (!_isProcessing)
                _ = ProcessQueueAsync();
        }

        private async Task ProcessQueueAsync()
        {
            _isProcessing = true;
            while (_queue.TryDequeue(out var action))
            {
                try { await action(_cts.Token); }
                catch (OperationCanceledException) { break; }
                catch { /* log lỗi playback */ }
            }
            _isProcessing = false;
        }
    }
}
