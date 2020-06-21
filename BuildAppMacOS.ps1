[CmdletBinding(DefaultParametersetName='None')]
param(
    [Parameter(Position=0,Mandatory=$True)][string] $Version,

    [Parameter(ParameterSetName='CodeSign',Mandatory=$false)][switch] $CodeSign,
    [Parameter(ParameterSetName='CodeSign',Mandatory=$true)][string] $CodeSignIdentity,
    [Parameter(ParameterSetName='CodeSign',Mandatory=$true)][string] $CodeSignPackageIdentity,
    [Parameter(ParameterSetName='CodeSign',Mandatory=$true)][string] $AppleAccountId,
    [Parameter(ParameterSetName='CodeSign',Mandatory=$true)][securestring] $AppleAccountPassword
)

function copyScript
{
    param(
        [Parameter(Mandatory=$True)][string] $Source,
        [Parameter(Mandatory=$True)][string] $Dest
    )

    New-Item -ItemType File -Path $Dest -Force
    Copy-Item $Source $Dest -Force
    chmod u+x $Dest
}

$binMac = Join-Path (pwd) bin
if (Test-Path $binMac){
    Remove-Item -LiteralPath "bin" -Force -Recurse
}
New-Item -Path "." -Name "bin" -ItemType "directory" | Out-Null
$binMac = New-Item -Path "bin" -Name "macOS" -ItemType "directory"

$macOsInstallerFilesPath = Join-Path (pwd) "app" "MacOSPackages"

$installerPath = Join-Path $binMac "Installers"
copyScript -Source "$($macOsInstallerFilesPath)/installer-postinstall" -Dest "$($installerPath)/Installer/scripts/postinstall"
copyScript -Source "$($macOsInstallerFilesPath)/uninstaller-postinstall" -Dest "$($installerPath)/Uninstaller/scripts/postinstall"

Push-Location "app/OnChrome"
dotnet publish -c Release -r osx-x64 -o "$($installerPath)/Installer/payload/usr/local/share/OnChrome"
Pop-Location

if ($CodeSign) {
    Push-Location "$($installerPath)/Installer/payload/usr/local/share/OnChrome"
    codesign -s $CodeSignIdentity -v --timestamp --options runtime *.dylib
    if ($LASTEXITCODE) {
        "Error code signing the dylib libraries"
        exit $LASTEXITCODE
    }

    codesign -s $CodeSignIdentity -v --timestamp --options runtime --entitlements "$($macOsInstallerFilesPath)/entitlements.plist" OnChrome
    if ($LASTEXITCODE) {
        "Error code signing the app"
        exit $LASTEXITCODE
    }
    Pop-Location
}

Push-Location $installerPath

pkgbuild --root Installer/payload --scripts Installer/scripts --identifier me.onchro OnChrome.unsigned.pkg
pkgbuild --nopayload --scripts Uninstaller/scripts --identifier me.onchro.uninstall OnChrome.Uninstall.unsigned.pkg

if ($CodeSign) {
    productsign --sign $CodeSignPackageIdentity --timestamp OnChrome.unsigned.pkg OnChrome.pkg
    productsign --sign $CodeSignPackageIdentity --timestamp OnChrome.Uninstall.unsigned.pkg OnChrome.Uninstall.pkg
}

Pop-Location

function staple {
    param (
        [Parameter(Mandatory=$True)][string] $File,
        [Parameter(Mandatory=$True)][string] $Bundle
    )

    $timestamp = [int][double]::Parse((Get-Date -UFormat %s))
    $bundle = "$($Bundle).$($Version)-$($timestamp)"

    "Sending the bundle $bundle to Apple for notarization"

    $applePwd = ConvertFrom-SecureString $AppleAccountPassword -AsPlainText

    $response = (xcrun altool --notarize-app --primary-bundle-id $bundle --username $AppleAccountId --password $applePwd 2>&1 --file $File) | Out-String

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
        $response
        exit 1
    }

    xcrun stapler staple $File
}

if ($CodeSign) {
    staple -File "$($installerPath)/OnChrome.pkg" -Bundle me.onchro
    staple -File "$($installerPath)/OnChrome.Uninstall.pkg" -Bundle me.onchro.uninstall
}

# $dist = New-Item -Path "bin" -Name "dist" -ItemType "directory"
# Copy-Item -Path $dmgPath -Destination $dist.FullName
