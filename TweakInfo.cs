using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace RasTweaksCS
{
    /// <summary>One selectable preset value for a parameterized tweak (e.g. 0x26 hex).</summary>
    public class TweakOption : INotifyPropertyChanged
    {
        public string Label { get; set; } = "";     // shown on the chip, e.g. "0x26 hex"
        public string Value { get; set; } = "";     // passed to the script, e.g. "38"
        public bool Recommended { get; set; }
        public TweakInfo? Parent { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected))); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class TweakInfo
    {
        public string Key { get; set; } = "";
        public string Category { get; set; } = "";
        public string Label { get; set; } = "";
        public string Description { get; set; } = "";
        public string Risk { get; set; } = "";
        public string Warning { get; set; } = "";
        public bool Toggleable { get; set; }

        // GPU tweaks only: "Nvidia" or "AMD" to show under just that vendor's tab,
        // or "" (default) to show under both vendors. Ignored for non-GPU categories.
        public string Vendor { get; set; } = "";

        // When set, the card shows a preset-value picker and Apply runs the script
        // with the selected option's Value passed as an argument. Null = plain tweak.
        public List<TweakOption>? Options { get; set; }
    }

    public static class TweakRegistry
    {
        public static List<TweakInfo> GetAllTweaks()
        {
            var tweaks = new List<TweakInfo>
            {
                // Network Tweaks
                new TweakInfo
                {
                    Key = "nic-tcp",
                    Category = "Network",
                    Label = "NIC & TCP Tweaks",
                    Description = "Disable power-saving on NIC, tune TCP stack for lower latency.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "disable-ahci-link-power-management",
                    Category = "Network",
                    Label = "Disable AHCI Link Power Management",
                    Description = "The real mechanism behind DIPM/HIPM parking tweaks - keeps your storage link at full power instead of idling.",
                    Risk = "Caution",
                    Warning = "This is one setting, not several - the DIPM parking and HIPM parking tweaks seen elsewhere are actually the same underlying Windows mechanism."
                },
                new TweakInfo
                {
                    Key = "hidden-nic-settings",
                    Category = "Network",
                    Label = "Hidden NIC Advanced Settings",
                    Description = "Disables Interrupt Moderation, Energy-Efficient Ethernet, and Flow Control for lower latency.",
                    Risk = "Caution",
                    Warning = "Property names vary by NIC vendor, so not every adapter will have all of these settings."
                },
                new TweakInfo
                {
                    Key = "enable-msi-mode",
                    Category = "Network",
                    Label = "Enable GPU MSI Mode",
                    Description = "Switches your GPU to Message-Signaled Interrupts - zero UI exposure anywhere, registry-only.",
                    Risk = "Caution",
                    Warning = "This is a genuinely undocumented-in-Settings internal (only documented in Microsoft's driver-facing docs, not end-user docs)."
                },
                new TweakInfo
                {
                    Key = "enable-long-paths",
                    Category = "Network",
                    Label = "Enable Long Paths",
                    Description = "Removes the 260-character file path limit for apps that support it.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "disable-delivery-optimization",
                    Category = "Network",
                    Label = "Disable Delivery Optimization P2P",
                    Description = "Stops Windows Update from uploading your bandwidth to share updates with strangers on the internet.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "flush-dns",
                    Category = "Network",
                    Label = "Flush DNS Cache",
                    Description = "Clears the local DNS resolver cache.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "reduce-ack-delay",
                    Category = "Network",
                    Label = "Reduce TCP ACK Delay",
                    Description = "Disables delayed ACKs and Nagle's algorithm on all adapters for lower latency.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "disable-netbios",
                    Category = "Network",
                    Label = "Disable NetBIOS over TCP/IP",
                    Description = "Turns off the legacy NetBIOS name-resolution protocol on every adapter, reducing local network broadcast chatter and attack surface.",
                    Risk = "Caution",
                    Warning = "Some older LAN devices (printers, NAS boxes, game consoles) and networks without DNS-based name resolution rely on NetBIOS for discovery/file sharing by hostname - those may stop finding each other."
                },
                new TweakInfo
                {
                    Key = "disable-qos-bandwidth-reservation",
                    Category = "Network",
                    Label = "Release QoS Bandwidth Reservation",
                    Description = "Frees the 20% bandwidth Windows reserves by default for QoS-tagged traffic, so UDP game traffic isn't competing for a smaller pool.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "increase-udp-buffer-size",
                    Category = "Network",
                    Label = "Increase Socket Buffer Size",
                    Description = "Raises the default UDP/TCP socket buffer size from Windows' ~8KB default to 64KB, reducing dropped packets under bursty traffic (game netcode, voice chat, streaming).",
                    Risk = "Caution",
                    Warning = "Minor increase in per-socket kernel memory usage - negligible on any modern system, but not literally free."
                },

                // Power Tweaks
                new TweakInfo
                {
                    Key = "RAS+FUNSPLIOTS",
                    Category = "Power",
                    Label = "RAS+FUNSPLIOTS",
                    Description = "Imports and activates a pre-tuned custom power plan.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "max-cpu-performance",
                    Category = "Power",
                    Label = "Disable CPU Power Saving",
                    Description = "Disables C-states, power throttling, and core parking for max CPU responsiveness.",
                    Risk = "Caution",
                    Warning = "Real increase in power draw and heat, since the CPU stays at full speed instead of idling down."
                },
                new TweakInfo
                {
                    Key = "disable-hibernation",
                    Category = "Power",
                    Label = "Disable Hibernation",
                    Description = "Frees disk space used by hiberfil.sys.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "disable-usb-selective-suspend",
                    Category = "Power",
                    Label = "Disable USB Selective Suspend",
                    Description = "Stops USB devices (mice/keyboards) from being power-suspended between uses.",
                    Risk = "Caution",
                    Warning = "Minor increase in power draw, and on laptops running on battery, slightly faster battery drain."
                },
                new TweakInfo
                {
                    Key = "disable-pcie-aspm",
                    Category = "Power",
                    Label = "Disable PCIe Link State Power Management",
                    Description = "Keeps all PCIe devices (GPU, NVMe, network card) at full power instead of idling between bursts.",
                    Risk = "Caution",
                    Warning = "Real increase in power draw and heat across every PCIe device in your system."
                },
                new TweakInfo
                {
                    Key = "set-cooling-policy-active",
                    Category = "Power",
                    Label = "Set Cooling Policy to Active",
                    Description = "Tells the kernel to ramp fans up before throttling the CPU when it gets hot.",
                    Risk = "Caution",
                    Warning = "Increases fan noise, and on laptops running on battery, drains it faster."
                },
                new TweakInfo
                {
                    Key = "reveal-boost-mode",
                    Category = "Power",
                    Label = "Reveal Processor Boost Mode",
                    Description = "Unhides Windows' buried CPU boost policy setting (PERFBOOSTMODE) in Power Options.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "disable-power-event-processor",
                    Category = "Power",
                    Label = "Disable Power Event Processor",
                    Description = "Stops the kernel from dynamically re-evaluating CPU performance state and core parking in response to power-efficiency events (EventProcessorEnabled = 0).",
                    Risk = "Caution",
                    Warning = "Real increase in idle power draw and heat, since the CPU stops stepping down opportunistically. This value is also undocumented by Microsoft - sourced from community tweak packs, so its exact effect is less verified than the powercfg-based tweaks here."
                },
                new TweakInfo
                {
                    Key = "disable-modern-standby",
                    Category = "Power",
                    Label = "Disable Modern Standby",
                    Description = "Forces traditional S3 deep sleep instead of Modern Standby / S0 idle (PlatformAoAcOverride = 0), reducing standby battery drain and wake quirks.",
                    Risk = "Caution",
                    Warning = "Many modern thin laptops only support Modern Standby - on those, disabling it can make sleep behave oddly or not enter deep sleep. Best on desktops or machines that still expose S3. Restart required; to revert, delete the PlatformAoAcOverride value."
                },

                // Boot Tweaks
                new TweakInfo
                {
                    Key = "gaming-latency",
                    Category = "Boot",
                    Label = "Gaming Latency Tweaks",
                    Description = "Disables dynamic tick, tunes scheduler responsiveness, and reduces display timeouts.",
                    Risk = "Caution",
                    Warning = "This also disables Windows' Fast Startup feature, since Fast Startup relies on hibernation."
                },
                new TweakInfo
                {
                    Key = "disable-superfetch",
                    Category = "Boot",
                    Label = "Disable Superfetch",
                    Description = "Stops Windows from proactively preloading apps into RAM - zero UI exposure, registry+service only.",
                    Risk = "Caution",
                    Toggleable = true,
                    Warning = "Most useful on SSDs/NVMe where load times are already fast, so Superfetch's preloading benefit is small."
                },
                new TweakInfo
                {
                    Key = "set-boot-timeout",
                    Category = "Boot",
                    Label = "Set Boot Menu Timeout to 6s",
                    Description = "Shortens how long the Windows Boot Manager waits at the boot menu before starting the default OS (msconfig > Boot > Timeout).",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "set-numproc-max",
                    Category = "Boot",
                    Label = "Set Boot Processor Count to Max",
                    Description = "Sets the BCD numproc value to this machine's detected logical processor count (msconfig > Boot > Advanced > Number of processors).",
                    Risk = "Caution",
                    Warning = "Windows already uses every core when this is left unset, so this is effectively a no-op rather than a speedup - and it hardcodes a number, so it must be re-run after a CPU upgrade or it will cap you. Detected at run time rather than baked in, but if a lower cap was already set, that stale value is what gets detected."
                },

                // System Tweaks
                new TweakInfo
                {
                    Key = "privacy-debloat",
                    Category = "System",
                    Label = "Privacy & Telemetry Debloat",
                    Description = "Disables diagnostic tracing, tailored content, CEIP, and Application Impact Telemetry.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "disable-error-reporting",
                    Category = "System",
                    Label = "Disable Windows Error Reporting",
                    Description = "Stops the background service that reports app/system crashes to Microsoft.",
                    Risk = "Safe",
                    Toggleable = true
                },
                new TweakInfo
                {
                    Key = "cache-cleaner",
                    Category = "System",
                    Label = "Cache Cleaner",
                    Description = "Clear temp files and Prefetch cache. Does not touch Event Logs.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "visual-performance",
                    Category = "System",
                    Label = "Visual Effects: Best Performance",
                    Description = "Same as Windows' built-in Adjust for best performance option.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "reduce-menu-delay",
                    Category = "System",
                    Label = "Reduce Menu Show Delay",
                    Description = "Removes the built-in pause before cascading submenus appear (right-click menus, Control Panel).",
                    Risk = "Safe",
                    Toggleable = true
                },
                new TweakInfo
                {
                    Key = "faster-shutdown",
                    Category = "System",
                    Label = "Faster Shutdown",
                    Description = "Reduces how long Windows waits for unresponsive apps/services before force-closing them.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "exclude-driver-updates",
                    Category = "System",
                    Label = "Exclude Driver Updates",
                    Description = "Stops Windows Update from auto-installing GPU/chipset driver updates.",
                    Risk = "Safe",
                    Toggleable = true
                },
                new TweakInfo
                {
                    Key = "mouse-flat-curve",
                    Category = "System",
                    Label = "Flatten Mouse Curve",
                    Description = "Disables mouse acceleration for 1:1 raw movement.",
                    Risk = "Caution",
                    Warning = "This noticeably changes how your mouse feels (removes acceleration entirely)."
                },

                // Kernel Tweaks
                new TweakInfo
                {
                    Key = "increase-dpc-watchdog",
                    Category = "Kernel",
                    Label = "Increase DPC Watchdog Timeout",
                    Description = "Raises the kernel's DPC timeout threshold (KPRCB) to prevent false-positive crashes during heavy driver load.",
                    Risk = "Caution",
                    Warning = "If a driver genuinely hangs, your system will stay frozen longer before the kernel catches it."
                },
                new TweakInfo
                {
                    Key = "win32-priority-separation",
                    Category = "Kernel",
                    Label = "Set Win32 Priority Separation",
                    Description = "Tunes CPU scheduling to favor the active foreground app. Pick a preset below (0x26 is the common gaming value).",
                    Risk = "Caution",
                    Warning = "Background apps (streaming software, Discord, etc.) may feel less smooth while a foreground app is focused. Restart recommended.",
                    Options = new List<TweakOption>
                    {
                        new TweakOption { Label = "0x26 hex", Value = "38", Recommended = true, IsSelected = true },
                        new TweakOption { Label = "0x18 hex", Value = "24" },
                        new TweakOption { Label = "0x16 hex", Value = "22" },
                        new TweakOption { Label = "0x2A hex", Value = "42" }
                    }
                },
                new TweakInfo
                {
                    Key = "boost-game-priority-mmcss",
                    Category = "Kernel",
                    Label = "Boost Game Thread Priority (MMCSS)",
                    Description = "Raises the Games task category's CPU/GPU scheduling priority above stock Windows defaults.",
                    Risk = "Caution",
                    Warning = "Only affects threads that apps explicitly register under the Games MMCSS category - not all games do this."
                },
                new TweakInfo
                {
                    Key = "enable-global-timer-resolution",
                    Category = "Kernel",
                    Label = "Global Timer Resolution",
                    Description = "Restores system-wide timer resolution (GlobalTimerResolutionRequests = 1) so a high resolution requested by any app applies globally, as it did before Windows 11 scoped it per-process.",
                    Risk = "Caution",
                    Warning = "A global high timer resolution slightly raises power draw and limits deep CPU idle, since the system tick runs more often. Restart required; to revert, delete the value."
                },
                new TweakInfo
                {
                    Key = "disable-memory-compression",
                    Category = "Kernel",
                    Label = "Disable Memory Compression",
                    Description = "Frees CPU from compressing/decompressing inactive RAM pages, at the cost of using more RAM.",
                    Risk = "Caution",
                    Toggleable = true,
                    Warning = "Best suited to systems with 16GB+ RAM. On lower-RAM systems, this can make memory pressure situations worse."
                },
                new TweakInfo
                {
                    Key = "disable-paging-executive",
                    Category = "Kernel",
                    Label = "Keep Kernel & Drivers in RAM",
                    Description = "Prevents Windows from paging kernel-mode code to disk (DisablePagingExecutive).",
                    Risk = "Caution",
                    Toggleable = true,
                    Warning = "This also disables Windows' Fast Startup feature, since Fast Startup relies on hibernation."
                },
                new TweakInfo
                {
                    Key = "disable-cpu-mitigations",
                    Category = "Kernel",
                    Label = "Disable CPU Security Mitigations",
                    Description = "Disables Spectre/Meltdown OS-level mitigations for reduced CPU overhead.",
                    Risk = "Security",
                    Warning = "This is a real security tradeoff: your system becomes more vulnerable to CPU side-channel attacks."
                },
                new TweakInfo
                {
                    Key = "optimize-memory-for-programs",
                    Category = "Kernel",
                    Label = "Optimize Memory for Programs",
                    Description = "Sets LargeSystemCache=0 so RAM favors application memory over file system cache - the gaming-appropriate side of the old \"Programs vs. System Cache\" setting.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "enable-threaded-dpc",
                    Category = "Kernel",
                    Label = "Enable Threaded DPCs",
                    Description = "Lets deferred procedure calls (DPC) run as schedulable kernel threads instead of blocking interrupt context, a Vista-era fix for driver-caused audio/input stutter.",
                    Risk = "Caution",
                    Warning = "This is a real, settable value, but its actual effect on Windows 10/11 specifically is less certain than most tweaks here - the kernel's DPC handling has changed since this was introduced, and confirming a real latency improvement needs kernel-level measurement tooling, not just a registry check."
                },

                // GPU Tweaks
                new TweakInfo
                {
                    Key = "disable-mpo",
                    Category = "GPU",
                    Label = "Disable Multiplane Overlay (MPO)",
                    Description = "Fixes flickering, black screens, and stutter caused by Windows' display compositing feature - especially with G-Sync/FreeSync.",
                    Risk = "Caution",
                    Warning = "Some multi-monitor setups may see increased GPU usage since Windows can no longer compose overlays efficiently."
                },
                new TweakInfo
                {
                    Key = "force-game-mode",
                    Category = "GPU",
                    Label = "Force Game Mode Always On",
                    Description = "Locks Game Mode on at the registry/policy level - fixes it silently turning off mid-session.",
                    Risk = "Caution",
                    Warning = "Game Mode has inconsistent performance impact across different games and Windows versions."
                },
                new TweakInfo
                {
                    Key = "disable-game-dvr",
                    Category = "GPU",
                    Label = "Disable Game DVR / Xbox Game Bar",
                    Description = "Removes DirectX capture hook overhead from background recording and the Win+G overlay.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "clear-directx-shader-cache",
                    Category = "GPU",
                    Label = "Clear DirectX Shader Cache",
                    Description = "Clears stale/corrupted compiled shaders - most useful after a GPU driver update.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "enable-hags",
                    Category = "GPU",
                    Label = "Enable Hardware-Accelerated GPU Scheduling",
                    Description = "Lets the GPU manage its own command scheduling instead of routing through the CPU (HwSchMode).",
                    Risk = "Caution",
                    Toggleable = true,
                    Warning = "Not all GPUs support this feature, and on some older hardware it can actually reduce performance."
                },
                new TweakInfo
                {
                    Key = "increase-tdr-delay",
                    Category = "GPU",
                    Label = "Increase GPU Timeout Delay",
                    Description = "Raises the GPU driver-reset timeout from 2s to 8s to prevent false crashes during heavy load (TdrDelay).",
                    Risk = "Caution",
                    Warning = "Microsoft documents this key as intended for driver debugging, not end-user tuning."
                },
                new TweakInfo
                {
                    Key = "disable-fullscreen-optimizations",
                    Category = "GPU",
                    Label = "Disable Fullscreen Optimizations",
                    Description = "Forces true exclusive fullscreen for lower input lag and more consistent frame pacing.",
                    Risk = "Caution",
                    Warning = "This setting's behavior has been inconsistent across Windows updates since 1809."
                },
                new TweakInfo
                {
                    Key = "disable-gpu-energy-driver",
                    Category = "GPU",
                    Vendor = "Nvidia",
                    Label = "Disable GPU Energy Driver",
                    Description = "Disables the GpuEnergyDrv power-management driver service (Start = 4), a community NVIDIA tuning tweak.",
                    Risk = "Caution",
                    Warning = "Couldn't be verified on real NVIDIA hardware, and this service also exists on non-NVIDIA systems - so it's not strictly vendor-locked and its real effect is unconfirmed. To revert, set the service's Start value back to 3. Restart required."
                },
                new TweakInfo
                {
                    Key = "increase-tdr-ddi-delay",
                    Category = "GPU",
                    Label = "Increase GPU DDI Timeout",
                    Description = "Raises TdrDdiDelay to 8s, the documented companion to TdrDelay - so both driver-hang timeouts move together and don't cause false resets under heavy GPU load.",
                    Risk = "Caution",
                    Warning = "Like TdrDelay, if the GPU genuinely hangs your screen stays frozen a bit longer before Windows recovers it. Restart required. To revert, delete the TdrDdiDelay value."
                },
                new TweakInfo
                {
                    Key = "disable-gpu-preemption",
                    Category = "GPU",
                    Label = "Disable GPU Preemption",
                    Description = "Sets EnablePreemption = 0 so the GPU finishes each task before switching, a latency tweak some setups prefer for steadier frame delivery.",
                    Risk = "Caution",
                    Warning = "Community tweak, not officially documented. Genuine tradeoff: without preemption, frame pacing can get WORSE in some workloads. Experimental - test your games and revert (set EnablePreemption to 1) if it doesn't help. Restart required."
                },
                new TweakInfo
                {
                    Key = "disable-amd-ulps",
                    Category = "GPU",
                    Vendor = "AMD",
                    Label = "Disable ULPS",
                    Description = "Disables AMD's Ultra Low Power State (EnableUlps = 0) on Radeon adapters - a long-standing AMD tweak to reduce micro-stutter and wake-from-idle hitches.",
                    Risk = "Caution",
                    Warning = "Only affects AMD/Radeon GPUs (safely does nothing on others). Couldn't be verified on real AMD hardware here, so the effect is unconfirmed. To revert, set EnableUlps back to 1. Restart required."
                },

                // Aim Tweaks
                new TweakInfo
                {
                    Key = "aim-afd",
                    Category = "Aim",
                    Label = "AFD Aim Tweak",
                    Description = "Applies the AFD registry tweak for lower networking latency.",
                    Risk = "Safe"
                },
                new TweakInfo
                {
                    Key = "mouse-flat-curve",
                    Category = "Aim",
                    Label = "Flatten Mouse Curve",
                    Description = "Disables mouse acceleration for 1:1 raw movement in shooter games.",
                    Risk = "Caution",
                    Warning = "This noticeably changes how your mouse feels (removes acceleration entirely)."
                },
                new TweakInfo
                {
                    Key = "reduce-ack-delay",
                    Category = "Aim",
                    Label = "Reduce TCP ACK Delay",
                    Description = "Disables delayed ACKs and Nagle's algorithm for lower hit-reg latency.",
                    Risk = "Safe"
                }
            };

            // Back-link each preset option to its tweak, so a chip click can find and
            // update its siblings' selection state.
            foreach (var tweak in tweaks)
            {
                if (tweak.Options != null)
                {
                    foreach (var option in tweak.Options)
                    {
                        option.Parent = tweak;
                    }
                }
            }

            return tweaks;
        }
    }
}