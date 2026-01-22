using ImGuiNET;
using NAudio.Wave;
using Raylib_cs;
using rlImGui_cs;
using System.Numerics;
using static ImGuiNET.ImGui;
using static Raylib_cs.Raylib;
using Newtonsoft.Json;

internal class MusicPlayer() : Viewport("Music Player") {

    private const string Url = "https://lofi.stream.laut.fm/lofi";
    
    private IWavePlayer? _waveOut;
    private MediaFoundationReader? _reader;
    private VisualizationSampleProvider? _visProvider;
    
    private bool _isPlaying, _isConnecting;
    private float _volume = 0.05f;
    
    private RenderTexture2D _rt;
    private bool _rtInit;
    
    private const int FftSize = 512;
    private const int VisualBins = 48;
    
    private readonly float[]
        _fft = new float[VisualBins],
        _samples = new float[FftSize];
    
    private int _sampleIdx;
    private readonly Lock _lock = new();
    private float _hueOffset, _smoothEnergy;

    private static string GetPath() => PathUtil.BestPath("Layouts/MusicPlayer.json", out var path) ? path : PathUtil.ExeRelative("Layouts/MusicPlayer.json");

    public void Load() {
        
        var path = GetPath();
        
        if (!File.Exists(path)) return;
        
        try {
            
            var settings = JsonConvert.DeserializeObject<MusicPlayerSettings>(File.ReadAllText(path));
            _volume = settings.Volume;
            if (settings.IsPlaying) Play();
            
        } catch { /**/ }
    }

    public void Save() {
        
        var path = GetPath();
        var dir = Path.GetDirectoryName(path);
        
        if (dir != null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        
        var settings = new MusicPlayerSettings {
            
            Volume = _volume,
            IsPlaying = _isPlaying
        };
        
        File.WriteAllText(path, JsonConvert.SerializeObject(settings, Formatting.Indented));
    }

    private struct MusicPlayerSettings {
        
        public float Volume;
        public bool IsPlaying;
    }

    private void Play() {
        
        if (_isPlaying || _waveOut != null || _isConnecting) return;
        
        _isConnecting = true;
        
        Task.Run(() => {
            
            try {
                
                _reader = new MediaFoundationReader(Url);
                _visProvider = new VisualizationSampleProvider(_reader.ToSampleProvider());
                
                _visProvider.OnSample += s => {
                    
                    lock(_lock) { if(_sampleIdx < FftSize) _samples[_sampleIdx++] = s; }
                };
                
                _waveOut = new WaveOutEvent { Volume = _volume };
                _waveOut.Init(_visProvider);
                _waveOut.Play();
                _isPlaying = true;
            }
            
            catch (Exception) { Cleanup(); }
            
            finally { _isConnecting = false; }
        });
    }

    private void Stop() {
        
        _waveOut?.Stop();
        Cleanup();
    }
    
    private void Cleanup() {
        
        _waveOut?.Dispose();
        _waveOut = null;
        
        _reader?.Dispose();
        _reader = null;
        
        _isPlaying = false;
    }

    protected override void OnDraw() {
        
        Spacing();
        BeginGroup();
        
        if (_isConnecting) {
            
            const float radius = 10;
            
            var pos = GetCursorScreenPos() + new Vector2(15, 12);
            var color = GetColorU32(ImGuiCol.Text);
            var startAngle = (float)Raylib.GetTime() * 5.0f;
            
            GetWindowDrawList().PathArcTo(pos, radius, startAngle, startAngle + 4.5f, 10);
            GetWindowDrawList().PathStroke(color, ImDrawFlags.None, 2.0f);
            Dummy(new Vector2(30, 24));
            
        } else {
            
            PushFont(Fonts.ImFontAwesomeNormal);
            
            if (!_isPlaying && Button(Icons.FaPlay, new Vector2(30, 24)))Play();
            if (_isPlaying && Button(Icons.FaStop, new Vector2(30, 24))) Stop();
                
            PopFont();
        }
        
        SameLine();
        
        SetNextItemWidth(GetContentRegionAvail().X - 10);
        
        if (SliderFloat("##Vol", ref _volume, 0, 1, "VOL %.2f")) {
            
            _volume = MathF.Round(_volume / 0.05f) * 0.05f;
            _waveOut?.Volume = _volume;
        }
        
        EndGroup();

        var winSize = GetContentRegionAvail();
        if (winSize.X < 50 || winSize.Y < 50) return;

        if (!_rtInit || _rt.Texture.Width != (int)winSize.X || _rt.Texture.Height != (int)winSize.Y) {
            
            if (_rtInit) UnloadRenderTexture(_rt);
            
            _rt = LoadRenderTexture((int)winSize.X, (int)winSize.Y);
            _rtInit = true;
        }

        UpdateFft();
        
        BeginTextureMode(_rt);
        ClearBackground(new Color(15, 15, 18, 255));
        DrawVisualizer((int)winSize.X, (int)winSize.Y);
        EndTextureMode();

        rlImGui.ImageRenderTexture(_rt);
    }

    private void UpdateFft() {
        
        const int step = FftSize / VisualBins;
        
        float[] snap;
        
        lock(_lock) { 
            
            snap = (float[])_samples.Clone(); 
            _sampleIdx = 0; 
        }
        
        var currentEnergy = 0f;
        
        var dt = GetIO().DeltaTime;

        for (var i = 0; i < VisualBins; i++) {
            
            var sum = 0f;
            
            for(var j=0; j<step; j++) sum += Math.Abs(snap[Math.Clamp(i*step+j, 0, FftSize-1)]);
            
            var avg = sum / step;
            
            var lerpSpeed = avg > _fft[i] ? 10.0f : 5.0f; 
            _fft[i] = Raymath.Lerp(_fft[i], avg, dt * lerpSpeed);
            
            currentEnergy += _fft[i];
        }
        
        var avgEnergy = currentEnergy / VisualBins;
        _smoothEnergy = Raymath.Lerp(_smoothEnergy, avgEnergy, dt * 2.0f);
        _hueOffset += (10.0f + _smoothEnergy * 100.0f) * dt;
    }

    private void DrawVisualizer(int w, int h) {
        
        const float padding = 1.5f;
        
        var barWidth = (float)w / VisualBins;
        
        for (var i = 0; i < VisualBins; i++) {

            var val = Math.Clamp(_fft[i] * h * 1.0f, 2, h * 0.5f);
            
            var t = (float)i / VisualBins;
            var colorTop = ColorFromHSV((t * 40.0f + _hueOffset) % 360, 0.5f, 0.8f);
            var colorBottom = ColorFromHSV((t * 40.0f + _hueOffset + 30) % 360, 0.6f, 0.4f);
            
            var rect = new Rectangle(i * barWidth + padding, h - val, barWidth - padding * 2, val);
            DrawRectangleGradientV((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height, colorTop, colorBottom);
            
            DrawRectangle((int)rect.X, (int)rect.Y, (int)rect.Width, 1, Fade(Color.White, 0.4f));
        }
    }

    private class VisualizationSampleProvider(ISampleProvider source) : ISampleProvider {
        
        public WaveFormat WaveFormat => source.WaveFormat;
        
        public event Action<float>? OnSample;
        
        public int Read(float[] buffer, int offset, int count) {
            
            var read = source.Read(buffer, offset, count);
            
            for (var i = 0; i < read; i++)
                OnSample?.Invoke(buffer[offset + i]);
            
            return read;
        }
    }
}
