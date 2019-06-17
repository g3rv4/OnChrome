[CmdletBinding(DefaultParametersetName='None')]
param(
    [Parameter(Position=0,Mandatory=$True)][string] $Version,

    [Parameter(ParameterSetName='CodeSign',Mandatory=$false)][switch] $CodeSign,
    [Parameter(ParameterSetName='CodeSign',Mandatory=$true)][string] $CodeSignIdentity,
    [Parameter(ParameterSetName='CodeSign',Mandatory=$true)][string] $AppleAccountId,
    [Parameter(ParameterSetName='CodeSign',Mandatory=$true)][securestring] $AppleAccountPassword
)

if (Test-Path "bin"){
    Remove-Item -LiteralPath "bin" -Force -Recurse
}
New-Item -Path "." -Name "bin" -ItemType "directory" | Out-Null
$binMac = New-Item -Path "bin" -Name "macOS" -ItemType "directory"

Push-Location "app/OnChrome"
go build
Move-Item -Path OnChrome -Destination $binMac.FullName
Pop-Location

# Make the MacOS Bundle
$appPath = Join-Path $binMac "OnChrome.app"
$bundleTemplatePath = Join-Path "app" "MacOSBundleStructure"
if (Test-Path $appPath){
    Remove-Item -LiteralPath $appPath -Force -Recurse
}
Copy-Item -Path $bundleTemplatePath -Recurse -Destination $appPath
Copy-Item -Path "$($binMac.FullName)/OnChrome" -Destination "$($appPath)/Contents/MacOS/OnChromeExecutable"

# Update the version on Info.plist
$plistPath = Join-Path $appPath "Contents/Info.plist"
$plistContent = Get-Content -Path $plistPath
Set-Content -Path $plistPath -Value $plistContent.Replace("{Version}", $Version)

if ($CodeSign) {
    codesign -s $CodeSignIdentity -v --timestamp --deep --options runtime $appPath
    if ($LASTEXITCODE) {
        "Error code signing the app bundle"
        exit $LASTEXITCODE
    }
}

$dmgName = "OnChromeMacOS.$($Version).dmg"
if (Test-Path $dmgName){
    Remove-Item -LiteralPath $dmgName -Force
}

$dmgPath = Join-Path $binMac.FullName $dmgName
appdmg dmgspec.json $dmgPath

if ($CodeSign) {
    $timestamp = [int][double]::Parse((Get-Date -UFormat %s))
    $bundle = "me.onchro.$($Version)-$($timestamp)"

    "Sending the bundle $bundle to Apple for notarization"

    $applePwd = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($AppleAccountPassword))

    $response = (xcrun altool --notarize-app --primary-bundle-id $bundle --username $AppleAccountId --password $applePwd 2>&1 --file $dmgPath) | Out-String

    $id = [regex]::match($response,'RequestUUID = ([a-z0-9-]+)\b').Groups[1].Value

    if (!$id) {
        "Unexpected response: $response"
        exit 1
    }

    "Uploaded! Request id: $id"

    Do {
        "Waiting 10 seconds"
        Start-Sleep -Seconds 10

        "Retrieving status"
        $response = (xcrun altool --notarization-info $id -u $AppleAccountId -p $applePwd 2>&1) | Out-String

        $status = [regex]::match($response,'Status: ([a-z ]+)').Groups[1].Value

        "Status: $status"
    } While ($status -eq "in progress")

    if ($status -ne "success") {
        "ERROR: Unexpected status: $status"
        exit 1
    }

    xcrun stapler staple $dmgPath
}

$dist = New-Item -Path "bin" -Name "dist" -ItemType "directory"
Copy-Item -Path $dmgPath -Destination $dist.FullName
