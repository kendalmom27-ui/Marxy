# Disable NetBIOS over TCP/IP
# Loops through every network adapter's own NetBT registry subkey and sets
# NetbiosOptions = 2 (disable NetBIOS over TCP/IP). This is the exact
# registry mechanism behind the GUI checkbox: adapter Properties >
# TCP/IPv4 > Advanced > WINS tab > "Disable NetBIOS over TCP/IP".
# (0 = use DHCP server setting/default, 1 = force enable, 2 = force disable.)
# Exits with a non-zero code on failure so the calling .bat can detect it.

try {
    $ifaces = Get-ChildItem 'HKLM:\SYSTEM\CurrentControlSet\Services\NetBT\Parameters\Interfaces' -ErrorAction Stop

    foreach ($i in $ifaces) {
        New-ItemProperty -Path $i.PSPath -Name NetbiosOptions -PropertyType DWord -Value 2 -Force -ErrorAction Stop | Out-Null
    }

    Write-Output "Updated $($ifaces.Count) adapter(s)."
    exit 0
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}
