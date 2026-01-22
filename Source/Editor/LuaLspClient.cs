using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

internal class LuaLspClient(string serverPath) : IDisposable {
    
    private Process? _process;
    private Stream? _stream;
    
    private int _requestId = 1;
    
    // Non-BOM UTF8
    private readonly Encoding _utf8NoBom = new UTF8Encoding(false);
    
    public string Status { get; private set; } = "Not Started";
    public bool IsAlive => _process is { HasExited: false };

    private static readonly string[] TokenTypes = ["namespace", "type", "class", "enum", "interface", "struct", "typeParameter", "parameter", "variable", "property", "enumMember", "event", "function", "method", "macro", "keyword", "modifier", "comment", "string", "number", "regexp", "operator"];
    private static readonly string[] Formats = ["relative"];
    private static readonly string[] StringArray = ["self", "level", "cam", "renderSettings", "f2", "f3", "mt", "time", "kb", "mouse", "quat", "game", "color" ];

    public event Action<string, JToken>? NotificationReceived;
    public event Action<int, JToken>? ResponseReceived;
    public event Action? OnExited;

    public async Task Start() {
        
        if (!File.Exists(Path.GetFullPath(serverPath))) {
            
            Status = "Error: Exe Not Found";
            return;
        }

        var binDir = Path.GetDirectoryName(Path.GetFullPath(serverPath))!;
        
        _process = new Process {
            
            StartInfo = new ProcessStartInfo {
                
                FileName = Path.GetFullPath(serverPath),
                Arguments = $"-E \"{Path.Combine(binDir, "main.lua")}\"", 
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = binDir
            }
        };

        _process.EnableRaisingEvents = true;
        
        _process.Exited += (_, _) => {
            
            Status = "Error: Server Exited";
            OnExited?.Invoke();
        };

        if (!_process.Start()) return;
        
        // ReSharper disable once InconsistentlySynchronizedField
        _stream = _process.StandardInput.BaseStream;
        _Listen();

        await Task.Delay(500);

        SendRequest("initialize", new {
            
            processId = Environment.ProcessId,
            rootUri = new Uri(Config.Mod.Path).AbsoluteUri,
            rootPath = Config.Mod.Path,

            capabilities = new {
                
                textDocument = new {
                    
                    hover = new { },
                    
                    completion = new { completionItem = new { snippetSupport = true } },
                    
                    signatureHelp = new { 
                        signatureInformation = new { 
                            parameterInformation = new { labelOffsetSupport = true },
                            activeParameterSupport = true
                        }
                    },
                    
                    publishDiagnostics = new { },
                    
                    semanticTokens = new {
                        
                        requests = new { full = true },
                        tokenTypes = TokenTypes,
                        formats = Formats
                    },
                    
                    synchronization = new { didOpen = true, didChange = 1, didClose = true }
                }
            },
        });

        await Task.Delay(200);
        SendNotification("initialized", new { });

        // Library config
        SendNotification("workspace/didChangeConfiguration", new {
            
            settings = new {
                
                Lua = new {
                    
                    workspace = new {
                        
                        library = new[] { PathUtil.ModRelative("Temp") },
                        checkThirdParty = false
                    },
                    
                    diagnostics = new {
                        
                        enable = true,
                        globals = StringArray
                    }
                }
            }
        });
        
        Status = "Connected";
    }

    private void _Listen() {
        
        _ = Task.Run(Listen);
        _ = Task.Run(ListenError);
    }

    public int SendRequest(string method, object @params) {
        
        if (!IsAlive) return -1;
        
        var id = _requestId++;
        _ = SendMessage(new { jsonrpc = "2.0", id, method, @params });
        
        return id;
    }

    public void SendNotification(string method, object @params) {
        
        if (!IsAlive) return;
        _ = SendMessage(new { jsonrpc = "2.0", method, @params });
    }

    private Task SendMessage(object message) {
        
        if (_stream == null || !IsAlive) return Task.CompletedTask;
        
        try {
            
            var json = JsonConvert.SerializeObject(message);
            var body = _utf8NoBom.GetBytes(json);
            var header = Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");
            
            lock (_stream) {
                
                _stream.Write(header, 0, header.Length);
                _stream.Write(body, 0, body.Length);
                _stream.Flush();
            }
        }
        
        catch { /**/ }

        return Task.CompletedTask;
    }

    private void Listen() {
        
        var stream = _process?.StandardOutput.BaseStream;
        
        if (stream == null) return;
        
        while (IsAlive) {
            
            try {

                var header = "";
                
                while (true) {
                    
                    var b = stream.ReadByte();
                    if (b == -1) return;
                    header += (char)b;
                    if (header.EndsWith("\r\n\r\n")) break;
                }

                var match = System.Text.RegularExpressions.Regex.Match(header, @"Content-Length:\s*(\d+)");
                
                if (!match.Success) continue;
                
                var length = int.Parse(match.Groups[1].Value);
                var buffer = new byte[length];
                var totalRead = 0;
                
                while (totalRead < length) {
                    
                    var read = stream.Read(buffer, totalRead, length - totalRead);
                    if (read <= 0) break;
                    totalRead += read;
                }

                var json = _utf8NoBom.GetString(buffer);
                var obj = JObject.Parse(json);
                    
                if (obj["id"] != null)
                    ResponseReceived?.Invoke(obj["id"]!.Value<int>(), obj["result"] ?? obj["error"] ?? new JObject());
                
                else if (obj["method"] != null)
                    NotificationReceived?.Invoke(obj["method"]!.Value<string>()!, obj["params"] ?? new JObject());
            }
            
            catch { /**/ }
        }
    }

    private void ListenError() {
        
        try { while (IsAlive) _process?.StandardError.ReadLine(); } catch { /**/ }
    }

    public void Dispose() {
        
        try { if (IsAlive) _process?.Kill(); } catch { /**/ }
        _process?.Dispose();
    }
}
