using System;
using DiscordRPC;

namespace RasTweaksCS
{
    /// <summary>
    /// Discord Rich Presence - shows "Playing RasTweaks / Optimizing Windows" on the
    /// user's Discord profile while the app is open, with a Join Discord button.
    ///
    /// Uses the pure-managed DiscordRichPresence library (no native dll), so it bundles
    /// cleanly into the single-file exe. If Discord isn't running, the client just sits
    /// idle and connects if/when Discord appears - it never throws or blocks, and the
    /// whole thing is wrapped defensively so presence can never affect the app itself.
    /// </summary>
    internal sealed class DiscordPresence : IDisposable
    {
        private const string ApplicationId = "1533214369658441832";
        private const string InviteUrl = "https://discord.gg/v5Hy39pxe";

        private DiscordRpcClient? _client;

        public void Start()
        {
            try
            {
                _client = new DiscordRpcClient(ApplicationId);
                _client.Initialize();

                _client.SetPresence(new RichPresence
                {
                    Details = "Optimizing Windows",
                    State = "RASX Tweaks",
                    Timestamps = Timestamps.Now, // drives the "elapsed" timer
                    Assets = new Assets
                    {
                        // Shows an image IF an asset named "logo" was uploaded under
                        // Rich Presence > Art Assets in the Discord dev portal.
                        // If not uploaded, no image shows - the text still works fine.
                        LargeImageKey = "logo",
                        LargeImageText = "RasTweaks"
                    },
                    Buttons = new[]
                    {
                        new Button { Label = "Join Discord", Url = InviteUrl }
                    }
                });
            }
            catch
            {
                // Presence is a nice-to-have; never let it interfere with the app.
                _client?.Dispose();
                _client = null;
            }
        }

        public void Dispose()
        {
            try
            {
                _client?.ClearPresence();
                _client?.Dispose();
            }
            catch { }
            finally
            {
                _client = null;
            }
        }
    }
}
