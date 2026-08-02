# Reduce TCP ACK Delay
# Loops through every network adapter's registry key and sets:
#   TcpAckFrequency = 1  -> don't delay ACKs
#   TCPNoDelay      = 1  -> disable Nagle's algorithm
# Exits with a non-zero code on failure so the calling .bat can detect it.

try {
    $ifaces = Get-ChildItem 'HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces' -ErrorAction Stop

    foreach ($i in $ifaces) {
        New-ItemProperty -Path $i.PSPath -Name TcpAckFrequency -PropertyType DWord -Value 1 -Force -ErrorAction Stop | Out-Null
        New-ItemProperty -Path $i.PSPath -Name TCPNoDelay -PropertyType DWord -Value 1 -Force -ErrorAction Stop | Out-Null
    }

    Write-Output "Updated $($ifaces.Count) adapter(s)."
    exit 0
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}
