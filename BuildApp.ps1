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
$binWindows = New-Item -Path "bin" -Name "win64" -ItemType "directory"
$binMac = New-Item -Path "bin" -Name "macOS" -ItemType "directory"

$elements = Get-ChildItem -Path "app" -Directory

for ($i=0; $i -lt $elements.Count; $i++) {
    $element = $elements[$i]

    $mainPath = Join-Path $element.FullName "main.go"

    if (Test-Path $mainPath) {
        Push-Location -Path $element.FullName

        go build
        env GOOS=windows GOARCH=amd64 go build

        Move-Item -Path $element.PSChildName -Destination $binMac.FullName
        Move-Item -Path "$($element.PSChildName).exe" -Destination $binWindows.FullName

        Pop-Location
    }
}

# zip the windows version
Push-Location $binWindows.FullName
$windowsZipName = "OnChromeWin64.$($Version).zip"
zip -r $windowsZipName . -x ".*" -x "__MACOSX"
Pop-Location

# Make the MacOS Bundle
$appPath = Join-Path $binMac "OnChrome.app"
$bundleTemplatePath = Join-Path "app" "MacOSBundleStructure"
if (Test-Path $appPath){
    Remove-Item -LiteralPath $appPath -Force -Recurse
}
Copy-Item -Path $bundleTemplatePath -Recurse -Destination $appPath
Copy-Item -Path "$($binMac.FullName)/Menu" -Destination "$($appPath)/Contents/MacOS/"
Copy-Item -Path "$($binMac.FullName)/FirefoxEndpoint" -Destination "$($appPath)/Contents/MacOS/"

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
Copy-Item -Path "$($binWindows.FullName)/$($windowsZipName)" -Destination $dist.FullName
