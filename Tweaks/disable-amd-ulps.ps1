# Disable AMD ULPS (Ultra Low Power State)
#
# ULPS puts an AMD GPU into a deep low-power state when idle. It's a long-
# standing AMD community tweak to disable it (EnableUlps = 0) to reduce
# micro-stutter, wake-from-idle hitches, and (historically) coil whine.
#
# The setting lives in each display adapter's registry key under the Display
# class GUID. This scans those keys and only touches ones whose driver is
# actually AMD/Radeon/ATI - so it does nothing on NVIDIA/Intel systems.
#
# Could not be verified on real AMD hardware (test machine has an Intel iGPU);
# the AMD-detection + write logic is verified to run cleanly, but the effect
# on an actual Radeon is unconfirmed. Restart required.
#
# TO REVERT: set EnableUlps back to 1 on the same adapter key(s).

$classPath = 'HKLM:\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}'
$changed = 0
$failed = 0

$keys = Get-ChildItem $classPath -ErrorAction SilentlyContinue |
    Where-Object { $_.PSChildName -match '^\d{4}$' }

foreach ($key in $keys) {
    # Read this adapter's driver info; skip the key entirely if we can't read it.
    $props = Get-ItemProperty -Path $key.PSPath -ErrorAction SilentlyContinue
    if ($null -eq $props) { continue }

    # Match real AMD identifiers only. Word boundaries matter: a naive 'ATI'
    # substring falsely matches "Intel Corpor(ati)on", so anchor it.
    $desc = "$($props.DriverDesc) $($props.ProviderName)"
    if ($desc -notmatch 'Advanced Micro Devices|Radeon|\bAMD\b|\bATI\b') { continue }

    try {
        New-ItemProperty -Path $key.PSPath -Name 'EnableUlps' -PropertyType DWord -Value 0 -Force -ErrorAction Stop | Out-Null
        $changed++
    }
    catch {
        $failed++
    }
}

if ($failed -gt 0 -and $changed -eq 0) {
    Write-Error "Found AMD adapter(s) but couldn't write EnableUlps (registry access denied)."
    exit 1
}

if ($changed -eq 0) {
    Write-Output "No AMD/Radeon display adapter found - nothing to change (this tweak only affects AMD GPUs)."
}
else {
    Write-Output "Set EnableUlps=0 on $changed AMD adapter(s)."
}
exit 0
