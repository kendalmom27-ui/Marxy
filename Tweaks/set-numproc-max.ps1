# Set Boot Processor Count to Detected Maximum (BCD numproc)
#
# msconfig's "Number of processors" dropdown bakes in whatever count was
# detected when you ticked it - a hardcoded number that does not track the
# hardware afterward. This detects the real logical processor count at run
# time instead, so it stays correct across machines rather than shipping
# one fixed number to everybody.
#
# Sums across sockets so multi-socket systems report their true total.

try {
    $count = (Get-CimInstance Win32_Processor | Measure-Object -Property NumberOfLogicalProcessors -Sum).Sum

    if (-not $count -or $count -lt 1) {
        Write-Error "Could not determine the logical processor count."
        exit 1
    }

    # If numproc is ALREADY set below the hardware's real count, Windows only
    # enumerates the capped number of processors - which means the count we
    # just detected would itself be that stale cap, and we'd silently re-lock
    # it. Surface that instead of hiding it.
    $existing = bcdedit /enum "{current}" | Select-String '^numproc\s+(\d+)'
    if ($existing) {
        $existingValue = [int]($existing.Matches[0].Groups[1].Value)
        if ($existingValue -ne $count) {
            Write-Output "NOTE: numproc was already set to $existingValue while Windows reports $count logical processor(s)."
            Write-Output "If $count looks lower than your CPU's real thread count, run 'bcdedit /deletevalue numproc', reboot, then run this again to pick up the true maximum."
        }
    }

    & bcdedit /set "{current}" numproc $count | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "bcdedit failed to set numproc."
        exit 1
    }

    Write-Output "Set numproc to $count."
    exit 0
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}
