using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using IOPath = System.IO.Path;

namespace RasTweaksCS
{
    public class UpdateInfo
    {
        public required Version Version { get; set; }
        public required string DownloadUrl { get; set; }
        public long Size { get; set; }
    }

    public static class UpdateChecker
    {
        private const string RepoOwner = "kendalmom27-ui";
        private const string RepoName = "Marxy";

        public static Version CurrentVersion =>
            Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);

        public static async Task<UpdateInfo?> CheckForUpdateAsync()
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RasTweaksCS-UpdateChecker", "1.0"));

            var url = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
            var json = await http.GetStringAsync(url);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var tagName = root.TryGetProperty("tag_name", out var tagProp) ? tagProp.GetString() : null;
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return null;
            }

            var versionString = tagName.TrimStart('v', 'V');
            if (!Version.TryParse(versionString, out var remoteVersion))
            {
                return null;
            }

            if (remoteVersion <= CurrentVersion)
            {
                return null;
            }

            if (!root.TryGetProperty("assets", out var assets))
            {
                return null;
            }

            foreach (var asset in assets.EnumerateArray())
            {
                var name = asset.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
                if (name == null || !name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var downloadUrl = asset.TryGetProperty("browser_download_url", out var urlProp) ? urlProp.GetString() : null;
                if (string.IsNullOrWhiteSpace(downloadUrl))
                {
                    continue;
                }

                var size = asset.TryGetProperty("size", out var sizeProp) ? sizeProp.GetInt64() : 0;

                return new UpdateInfo
                {
                    Version = remoteVersion,
                    DownloadUrl = downloadUrl,
                    Size = size
                };
            }

            return null;
        }

        public static async Task DownloadUpdateAsync(string url, string destPath, IProgress<double> progress)
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RasTweaksCS-UpdateChecker", "1.0"));

            using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = File.Create(destPath);

            var buffer = new byte[81920];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalRead += bytesRead;

                if (totalBytes > 0)
                {
                    progress.Report((double)totalRead / totalBytes * 100.0);
                }
            }
        }

        /// <summary>
        /// A running exe can't overwrite itself on Windows, so this writes a small
        /// helper batch script that waits for this process to actually exit (by PID,
        /// not a fixed delay), swaps the downloaded exe into place, relaunches it,
        /// then deletes itself. Caller must shut down immediately after calling this
        /// so the file lock on the current exe is released.
        /// </summary>
        public static void ApplyUpdateAndRestart(string newExePath)
        {
            var currentExePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(currentExePath))
            {
                return;
            }

            var pid = Environment.ProcessId;
            var scriptPath = IOPath.Combine(IOPath.GetTempPath(), "RasTweaksCS_apply_update.bat");

            var script = "@echo off\r\n" +
                         ":waitloop\r\n" +
                         $"tasklist /FI \"PID eq {pid}\" | find \"{pid}\" >nul\r\n" +
                         "if not errorlevel 1 (\r\n" +
                         "    timeout /t 1 /nobreak >nul\r\n" +
                         "    goto waitloop\r\n" +
                         ")\r\n" +
                         $"move /y \"{newExePath}\" \"{currentExePath}\" >nul 2>&1\r\n" +
                         $"start \"\" \"{currentExePath}\"\r\n" +
                         "(goto) 2>nul & del \"%~f0\"\r\n";

            File.WriteAllText(scriptPath, script);

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{scriptPath}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }
    }
}
