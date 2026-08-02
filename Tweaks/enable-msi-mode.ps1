# Enable MSI (Message-Signaled Interrupts) Mode for GPU
#
# This is a genuinely hidden Windows internal - there is no Settings page,
# Control Panel option, or Device Manager toggle for this anywhere. It's
# documented only in Microsoft's hardware DRIVER docs (dev-facing, not
# end-user facing): https://learn.microsoft.com/en-us/windows-hardware/drivers/kernel/enabling-message-signaled-interrupts-in-the-registry
#
# By default, most PCI devices (including GPUs) use legacy line-based
# interrupts, which can be shared between multiple devices and add
# latency. MSI mode gives the GPU its own dedicated interrupt vector
# instead, which can reduce DPC latency and frame-time stutter.
#
# The registry path is unique per device/system, so this finds the
# active GPU(s) dynamically rather than using a hardcoded path.

try {
    $gpus = Get-PnpDevice -Class Display -Status OK -ErrorAction Stop

    if (-not $gpus) {
        Write-Error "No active display (GPU) devices found."
        exit 1
    }

    $successCount = 0
    foreach ($gpu in $gpus) {
        $regPath = "HKLM:\SYSTEM\CurrentControlSet\Enum\$($gpu.InstanceId)\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties"

        if (-not (Test-Path $regPath)) {
            New-Item -Path $regPath -Force | Out-Null
        }

        New-ItemProperty -Path $regPath -Name "MSISupported" -PropertyType DWord -Value 1 -Force | Out-Null
        Write-Output "Enabled MSI mode for: $($gpu.FriendlyName)"
        $successCount++
    }

    if ($successCount -eq 0) {
        Write-Error "Could not enable MSI mode on any GPU."
        exit 1
    }

    exit 0
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}
