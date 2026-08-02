using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IOPath = System.IO.Path;

namespace RasTweaksCS
{
    public class TweakRunner
    {
        private readonly string _tweaksPath;

        public TweakRunner(string tweaksPath)
        {
            // If path is relative, make it relative to the executable directory
            if (!IOPath.IsPathRooted(tweaksPath))
            {
                var exeDir = AppDomain.CurrentDomain.BaseDirectory;
                _tweaksPath = IOPath.Combine(exeDir, tweaksPath);
            }
            else
            {
                _tweaksPath = tweaksPath;
            }
        }

        public async Task<TweakResult> RunTweakAsync(string tweakName)
        {
            var scriptPath = IOPath.Combine(_tweaksPath, $"{tweakName}.bat");

            if (!File.Exists(scriptPath))
            {
                return new TweakResult
                {
                    Success = false,
                    Message = $"Tweak file not found: {scriptPath}"
                };
            }

            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{scriptPath}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    // CRASH FIX: Prevent scripts from asking for input (hanging forever)
                    RedirectStandardInput = true
                };

                // CRASH FIX: Use timeout to prevent infinite hang
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                process.Start();

                // CRASH FIX: Close input stream immediately so scripts can't wait for input
                process.StandardInput.Close();

                // CRASH FIX: Read output with timeout
                var stdoutTask = process.StandardOutput.ReadToEndAsync();
                var stderrTask = process.StandardError.ReadToEndAsync();
                var waitTask = process.WaitForExitAsync(cts.Token);

                await Task.WhenAll(stdoutTask, stderrTask, waitTask);

                if (process.ExitCode == 0)
                {
                    return new TweakResult
                    {
                        Success = true,
                        Message = "Applied successfully",
                        Output = stdoutTask.Result
                    };
                }
                else
                {
                    return new TweakResult
                    {
                        Success = false,
                        Message = $"Script exited with code {process.ExitCode}",
                        Output = stderrTask.Result
                    };
                }
            }
            catch (OperationCanceledException)
            {
                // CRASH FIX: Script timed out
                return new TweakResult
                {
                    Success = false,
                    Message = "Tweak timed out after 30 seconds. The script may be stuck or require user input."
                };
            }
            catch (Exception ex)
            {
                return new TweakResult
                {
                    Success = false,
                    Message = $"Error running tweak: {ex.Message}"
                };
            }
        }

        public async Task<TweakResult> RunPowerShellScriptAsync(string scriptName, int timeoutSeconds = 60)
        {
            var scriptPath = IOPath.Combine(_tweaksPath, $"{scriptName}.ps1");

            if (!File.Exists(scriptPath))
            {
                return new TweakResult
                {
                    Success = false,
                    Message = $"Script not found: {scriptPath}"
                };
            }

            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                };

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

                process.Start();
                process.StandardInput.Close();

                var stdoutTask = process.StandardOutput.ReadToEndAsync();
                var stderrTask = process.StandardError.ReadToEndAsync();
                var waitTask = process.WaitForExitAsync(cts.Token);

                await Task.WhenAll(stdoutTask, stderrTask, waitTask);

                var stdout = stdoutTask.Result;

                return new TweakResult
                {
                    Success = process.ExitCode == 0,
                    Message = process.ExitCode == 0 ? "Completed" : (string.IsNullOrWhiteSpace(stderrTask.Result) ? $"Script exited with code {process.ExitCode}" : stderrTask.Result),
                    Output = stdout
                };
            }
            catch (OperationCanceledException)
            {
                return new TweakResult
                {
                    Success = false,
                    Message = "Script timed out."
                };
            }
            catch (Exception ex)
            {
                return new TweakResult
                {
                    Success = false,
                    Message = $"Error running script: {ex.Message}"
                };
            }
        }
    }

    public class TweakResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string Output { get; set; } = "";
    }
}