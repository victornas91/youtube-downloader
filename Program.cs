using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Linq;

class Program
{
    private static readonly HttpClient httpClient = new HttpClient();

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== YouTube Search & Download ===");

        // Pedir frase-chave para pesquisa
        Console.Write("🔍 Digite a frase-chave para pesquisar no YouTube: ");
        string searchQuery = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            Console.WriteLine("❌ Frase-chave inválida!");
            return;
        }

        // Pesquisar vídeos
        var videos = await SearchVideos(searchQuery);

        if (videos == null || videos.Count == 0)
        {
            Console.WriteLine("❌ Nenhum vídeo encontrado!");
            return;
        }

        // Mostrar lista de vídeos
        Console.WriteLine("\n📺 Vídeos encontrados:");
        for (int i = 0; i < videos.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {videos[i].Title}");
            Console.WriteLine($"   Duração: {videos[i].Duration} | Canal: {videos[i].Channel}");
            Console.WriteLine();
        }

        // Selecionar vídeo
        Console.Write($"🎯 Selecione o vídeo (1-{videos.Count}): ");
        if (int.TryParse(Console.ReadLine(), out int selection) && selection >= 1 && selection <= videos.Count)
        {
            var selectedVideo = videos[selection - 1];
            Console.WriteLine($"\n📌 Selecionado: {selectedVideo.Title}");

            // Escolher tipo de download
            Console.WriteLine("\n📥 O que deseja baixar?");
            Console.WriteLine("1. 🎬 Vídeo (MP4)");
            Console.WriteLine("2. 🎵 Apenas Áudio (MP3)");
            Console.Write("Escolha uma opção (1-2): ");

            if (int.TryParse(Console.ReadLine(), out int downloadType) && downloadType >= 1 && downloadType <= 2)
            {
                // Modificar URL para HTTP
                string httpUrl = ConvertToHttpUrl(selectedVideo.Url);

                if (downloadType == 1)
                {
                    Console.WriteLine("\n🎬 Iniciando download do vídeo...");
                    await DownloadVideoWithMultipleMethods(httpUrl, selectedVideo.Title);
                }
                else
                {
                    Console.WriteLine("\n🎵 Iniciando download do áudio...");
                    await DownloadAudioWithMultipleMethods(httpUrl, selectedVideo.Title);
                }
            }
            else
            {
                Console.WriteLine("❌ Opção de download inválida!");
            }
        }
        else
        {
            Console.WriteLine("❌ Seleção inválida!");
        }
    }

    static string ConvertToHttpUrl(string originalUrl)
    {
        // Converte URL do YouTube para HTTP
        if (originalUrl.Contains("https://"))
        {
            return originalUrl.Replace("https://", "http://");
        }
        else if (!originalUrl.StartsWith("http://"))
        {
            return "http://" + originalUrl;
        }
        return originalUrl;
    }

    static async Task<List<YouTubeVideo>> SearchVideos(string query)
    {
        try
        {
            Console.WriteLine("🔎 Pesquisando vídeos...");

            // Usar yt-dlp para pesquisar vídeos
            var processInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $@"--flat-playlist --print ""%(title)s||%(duration)s||%(channel)s||%(id)s"" ""ytsearch10:{query}""",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"Aviso: {error}");
            }

            var videos = new List<YouTubeVideo>();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    var video = new YouTubeVideo
                    {
                        Title = parts[0].Trim(),
                        Duration = FormatDuration(parts[1]),
                        Channel = parts[2].Trim(),
                        VideoId = parts[3].Trim(),
                        Url = $"http://www.youtube.com/watch?v={parts[3].Trim()}"
                    };
                    videos.Add(video);
                }
            }

            return videos;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro na pesquisa: {ex.Message}");
            return new List<YouTubeVideo>();
        }
    }

    static string FormatDuration(string duration)
    {
        if (int.TryParse(duration, out int seconds))
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            return timeSpan.Hours > 0 ?
                   $"{timeSpan.Hours}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}" :
                   $"{timeSpan.Minutes}:{timeSpan.Seconds:D2}";
        }
        return duration;
    }

    static async Task DownloadVideoWithMultipleMethods(string url, string videoTitle)
    {
        var methods = new List<DownloadMethod>
        {
            new DownloadMethod {
                Name = "yt-dlp com cookies (HTTP) - MP4",
                Command = "yt-dlp",
                Arguments = $"--cookies-from-browser chrome -f \"best[height<=480][ext=mp4]\" \"{url}\""
            },
            new DownloadMethod {
                Name = "yt-dlp com headers customizados (HTTP) - MP4",
                Command = "yt-dlp",
                Arguments = $"--user-agent \"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36\" --referer \"http://www.youtube.com/\" -f \"best[height<=480][ext=mp4]\" \"{url}\""
            },
            new DownloadMethod {
                Name = "yt-dlp modo simples (HTTP) - MP4",
                Command = "yt-dlp",
                Arguments = $"-f \"best[height<=480][ext=mp4]\" \"{url}\""
            },
            new DownloadMethod {
                Name = "yt-dlp com extractor alternativo (HTTP) - MP4",
                Command = "yt-dlp",
                Arguments = $"--extractor-args youtube:player-client=android,web --throttled-rate 1M --retries 10 -f \"best[height<=480][ext=mp4]\" \"{url}\""
            },
            new DownloadMethod {
                Name = "yt-dlp formato MP4 genérico (HTTP)",
                Command = "yt-dlp",
                Arguments = $"-f \"best[ext=mp4]\" \"{url}\""
            }
        };

        Console.WriteLine($"\n📥 Iniciando download do vídeo: {videoTitle}");
        Console.WriteLine($"🔗 URL: {url}");

        bool success = false;
        foreach (var method in methods)
        {
            Console.WriteLine($"\n🔄 Tentando: {method.Name}");
            success = await ExecuteCommand(method.Command, method.Arguments);
            if (success)
            {
                Console.WriteLine($"✅ Download do vídeo concluído com: {method.Name}");
                break;
            }
            await Task.Delay(3000);
        }

        if (!success)
        {
            Console.WriteLine("\n❌ Todos os métodos de vídeo falharam. Tentando estratégias alternativas...");
            await TryAlternativeVideoMethods(url);
        }
    }

    static async Task DownloadAudioWithMultipleMethods(string url, string videoTitle)
    {
        var methods = new List<DownloadMethod>
        {
            new DownloadMethod {
                Name = "yt-dlp áudio MP3 (com cookies)",
                Command = "yt-dlp",
                Arguments = $"--cookies-from-browser chrome -f \"bestaudio\" --extract-audio --audio-format mp3 --audio-quality 0 \"{url}\""
            },
            new DownloadMethod {
                Name = "yt-dlp áudio MP3 (headers customizados)",
                Command = "yt-dlp",
                Arguments = $"--user-agent \"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36\" --referer \"http://www.youtube.com/\" -f \"bestaudio\" --extract-audio --audio-format mp3 --audio-quality 0 \"{url}\""
            },
            new DownloadMethod {
                Name = "yt-dlp áudio MP3 (modo simples)",
                Command = "yt-dlp",
                Arguments = $"-f \"bestaudio\" --extract-audio --audio-format mp3 --audio-quality 0 \"{url}\""
            },
            new DownloadMethod {
                Name = "yt-dlp áudio M4A",
                Command = "yt-dlp",
                Arguments = $"-f \"bestaudio[ext=m4a]\" --extract-audio --audio-format m4a \"{url}\""
            },
            new DownloadMethod {
                Name = "yt-dlp áudio OPUS",
                Command = "yt-dlp",
                Arguments = $"-f \"bestaudio[ext=webm]\" --extract-audio --audio-format opus \"{url}\""
            }
        };

        Console.WriteLine($"\n📥 Iniciando download do áudio: {videoTitle}");
        Console.WriteLine($"🔗 URL: {url}");

        bool success = false;
        foreach (var method in methods)
        {
            Console.WriteLine($"\n🔄 Tentando: {method.Name}");
            success = await ExecuteCommand(method.Command, method.Arguments);
            if (success)
            {
                Console.WriteLine($"✅ Download do áudio concluído com: {method.Name}");
                break;
            }
            await Task.Delay(3000);
        }

        if (!success)
        {
            Console.WriteLine("\n❌ Todos os métodos de áudio falharam. Tentando estratégias alternativas...");
            await TryAlternativeAudioMethods(url);
        }
    }

    static async Task<bool> ExecuteCommand(string command, string arguments)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);

            // Capturar output em tempo real
            process.OutputDataReceived += (s, e) => {
                if (!string.IsNullOrEmpty(e.Data))
                    Console.WriteLine($"   {e.Data}");
            };

            process.ErrorDataReceived += (s, e) => {
                if (!string.IsNullOrEmpty(e.Data) && !e.Data.Contains("WARNING:"))
                    Console.WriteLine($"   ERRO: {e.Data}");
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Erro: {ex.Message}");
            return false;
        }
    }

    static async Task TryAlternativeVideoMethods(string url)
    {
        var alternativeMethods = new List<DownloadMethod>
        {
            new DownloadMethod {
                Name = "Vídeo resolução alternativa 360p",
                Command = "yt-dlp",
                Arguments = $"-f \"best[height<=360][ext=mp4]\" \"{url}\""
            },
            new DownloadMethod {
                Name = "Vídeo qualquer resolução MP4",
                Command = "yt-dlp",
                Arguments = $"-f \"best[ext=mp4]\" \"{url}\""
            },
            new DownloadMethod {
                Name = "Vídeo formato genérico",
                Command = "yt-dlp",
                Arguments = $"-f \"best\" \"{url}\""
            }
        };

        foreach (var method in alternativeMethods)
        {
            Console.WriteLine($"\n🔄 Tentando alternativa de vídeo: {method.Name}");
            bool success = await ExecuteCommand(method.Command, method.Arguments);
            if (success)
            {
                Console.WriteLine($"✅ Alternativa de vídeo bem-sucedida: {method.Name}");
                return;
            }
            await Task.Delay(2000);
        }

        ShowTroubleshootingTips();
    }

    static async Task TryAlternativeAudioMethods(string url)
    {
        var alternativeMethods = new List<DownloadMethod>
        {
            new DownloadMethod {
                Name = "Áudio formato WAV",
                Command = "yt-dlp",
                Arguments = $"-f \"bestaudio\" --extract-audio --audio-format wav \"{url}\""
            },
            new DownloadMethod {
                Name = "Áudio formato AAC",
                Command = "yt-dlp",
                Arguments = $"-f \"bestaudio\" --extract-audio --audio-format aac \"{url}\""
            },
            new DownloadMethod {
                Name = "Áudio sem conversão (formato original)",
                Command = "yt-dlp",
                Arguments = $"-f \"bestaudio\" \"{url}\""
            }
        };

        foreach (var method in alternativeMethods)
        {
            Console.WriteLine($"\n🔄 Tentando alternativa de áudio: {method.Name}");
            bool success = await ExecuteCommand(method.Command, method.Arguments);
            if (success)
            {
                Console.WriteLine($"✅ Alternativa de áudio bem-sucedida: {method.Name}");
                return;
            }
            await Task.Delay(2000);
        }

        ShowTroubleshootingTips();
    }

    static void ShowTroubleshootingTips()
    {
        Console.WriteLine("\n💡 Dicas para resolver problemas:");
        Console.WriteLine("1. Atualize o yt-dlp: yt-dlp -U");
        Console.WriteLine("2. Verifique sua conexão com a internet");
        Console.WriteLine("3. Tente usar VPN");
        Console.WriteLine("4. Teste em outro horário");
        Console.WriteLine("5. Para áudio, certifique-se de ter o FFmpeg instalado");
        Console.WriteLine("6. Para vídeo, verifique se há espaço em disco suficiente");
    }
}

public class DownloadMethod
{
    public string Name { get; set; }
    public string Command { get; set; }
    public string Arguments { get; set; }
}

public class YouTubeVideo
{
    public string Title { get; set; }
    public string Duration { get; set; }
    public string Channel { get; set; }
    public string VideoId { get; set; }
    public string Url { get; set; }
}
