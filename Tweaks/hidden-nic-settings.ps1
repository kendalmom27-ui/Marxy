# Hidden NIC Advanced Settings
#
# These are the settings under Device Manager > [adapter] > Advanced tab -
# not exposed anywhere in normal Windows Settings. Property names vary by
# NIC vendor/driver, so this tries several known aliases for each setting
# on every active adapter and reports what was actually found and changed.
#
#   - Interrupt Moderation OFF: each packet is processed immediately
#     instead of being batched, lowering latency at the cost of slightly
#     higher CPU usage (more noticeable on older/weaker CPUs).
#   - Energy-Efficient Ethernet OFF: stops the adapter from dropping into
#     a low-power state during idle gaps, which can cause latency spikes.
#   - Flow Control OFF: real tradeoff, not a pure win - can reduce latency,
#     but if your adapter's buffers get saturated under heavy load, you
#     may see more packet loss than with it on.

$targets = @{
    'Interrupt Moderation'        = 'Disabled'
    'Energy-Efficient Ethernet'   = 'Disabled'
    'Energy Efficient Ethernet'   = 'Disabled'
    'Advanced EEE'                = 'Disabled'
    'Green Ethernet'              = 'Disabled'
    'Flow Control'                = 'Disabled'
}

$adapters = Get-NetAdapter | Where-Object { $_.Status -eq 'Up' }
$changed = 0
$skipped = 0

foreach ($adapter in $adapters) {
    $props = Get-NetAdapterAdvancedProperty -Name $adapter.Name -ErrorAction SilentlyContinue

    foreach ($displayName in $targets.Keys) {
        $match = $props | Where-Object { $_.DisplayName -eq $displayName }
        if ($match) {
            try {
                Set-NetAdapterAdvancedProperty -Name $adapter.Name -DisplayName $displayName `
                    -DisplayValue $targets[$displayName] -ErrorAction Stop
                $changed++
            }
            catch {
                $skipped++
            }
        }
    }
}

Write-Output "Changed: $changed properties. Not present on this hardware: $skipped."

if ($changed -eq 0) {
    Write-Error "No matching advanced properties were found on any adapter - your NIC driver may not expose these under the names this script checks."
    exit 1
}

exit 0
