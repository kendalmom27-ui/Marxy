# Creates a System Restore Point at app startup, before any tweaks can be
# applied - a safety net so testers always have a way back if something
# goes wrong. Handles two expected non-error outcomes honestly instead of
# reporting them as failures:
#   - System Protection was off -> we turn it on first
#   - A restore point already exists from the last 24 hours (Windows only
#     allows one per day, even via this exact command) -> reported as
#     "skipped", not a failure

$driveLetter = $env:SystemDrive + "\"

try {
    Enable-ComputerRestore -Drive $driveLetter -ErrorAction Stop
}
catch {
    Write-Output "STATUS:PROTECTION_FAILED"
    Write-Error $_.Exception.Message
    exit 2
}

try {
    Checkpoint-Computer -Description "Ras Tweaks - Startup Restore Point" -RestorePointType MODIFY_SETTINGS -ErrorAction Stop
    Write-Output "STATUS:CREATED"
    exit 0
}
catch {
    if ($_.Exception.Message -match "already been created within the past 24 hours") {
        Write-Output "STATUS:SKIPPED_RECENT"
        exit 0
    }
    Write-Output "STATUS:FAILED"
    Write-Error $_.Exception.Message
    exit 1
}
