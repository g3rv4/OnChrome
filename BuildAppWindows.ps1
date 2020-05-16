[CmdletBinding(DefaultParametersetName='None')]
param(
    [Parameter(Position=0,Mandatory=$True)][string] $Version
)

if (Test-Path "bin"){
    Remove-Item -LiteralPath "bin" -Force -Recurse
}
New-Item -Path "." -Name "bin" -ItemType "directory" | Out-Null
$bin = New-Item -Path "bin" -Name "win64" -ItemType "directory"

Copy-Item -Path "app/WindowsVersionInfo/*" -Destination "app/OnChrome"

# adjust the version
$versionParts = $Version.Split('.')
$versionMajor = $versionParts[0]
$versionMinor = $versionParts[1]
$versionPatch = if ($versionParts.Length -gt 2) { $versionParts[2] } else { '0' }
$versionBuild = if ($versionParts.Length -gt 3) { $versionParts[3] } else { '0' }

$versionInfoContent = Get-Content -Path "app/OnChrome/versioninfo.json"
$versionInfoContent = $versionInfoContent.Replace("{versionMajor}", $versionMajor).Replace("{versionMinor}", $versionMinor).Replace("{versionPatch}", $versionPatch).Replace("{versionBuild}", $versionBuild)
Set-Content -Path "app/OnChrome/versioninfo.json" -Value $versionInfoContent

Push-Location "app/OnChrome"
go generate
go build
& "C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe" sign /fd SHA256 /t http://timestamp.comodoca.com OnChrome.exe
Move-Item -Path OnChrome.exe -Destination $bin.FullName
Pop-Location
