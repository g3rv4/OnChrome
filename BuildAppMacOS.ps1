[CmdletBinding(DefaultParametersetName='None')]
param(
    [Parameter(Position=0,Mandatory=$True)][string] $Version,

    [Parameter(ParameterSetName='CodeSign',Mandatory=$false)][switch] $CodeSign,
    [Parameter(ParameterSetName='CodeSign',Mandatory=$true)][string] $CodeSignIdentity,
    [Parameter(ParameterSetName='CodeSign',Mandatory=$true)][string] $CodeSignPackageIdentity,
    [Parameter(ParameterSetName='CodeSign',Mandatory=$true)][string] $AppleAccountId,
    [Parameter(ParameterSetName='CodeSign',Mandatory=$true)][securestring] $AppleAccountPassword
)

function copyFileCreatingFolder
{
    param(
        [Parameter(Mandatory=$True)][string] $Source,
        [Parameter(Mandatory=$True)][string] $Dest,
        [Parameter(Mandatory=$false)][switch] $MakeExecutable
    )

    New-Item -ItemType File -Path $Dest -Force
    Copy-Item $Source $Dest -Force
    if ($MakeExecutable) {
        chmod u+x $Dest
    }
}

$binMac = Join-Path (pwd) bin
if (Test-Path $binMac){
    Remove-Item -LiteralPath "bin" -Force -Recurse
}
New-Item -Path "." -Name "bin" -ItemType "directory" | Out-Null
$binMac = New-Item -Path "bin" -Name "macOS" -ItemType "directory"

$macOsInstallerFilesPath = Join-Path (pwd) "app" "MacOSPackages"

$installerPath = Join-Path $binMac "Installers"
copyFileCreatingFolder -Source "$($macOsInstallerFilesPath)/Installer/postinstall" -Dest "$($installerPath)/Installer/scripts/postinstall" -MakeExecutable
copyFileCreatingFolder -Source "$($macOsInstallerFilesPath)/Uninstaller/postinstall" -Dest "$($installerPath)/Uninstaller/scripts/postinstall" -MakeExecutable

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

function buildInstaller
{
    param(
        [Parameter(Mandatory=$True)][string] $pkg,
        [Parameter(Mandatory=$True)][string] $resources,
        [Parameter(Mandatory=$True)][string] $title
    )

    productbuild --synthesize --package $pkg distribution.xml

    $distributionPath = Join-Path (pwd) distribution.xml
    [xml]$xmlDoc = Get-Content $distributionPath
    $gui = $xmlDoc['installer-gui-script']

    $titleElement = $xmlDoc.CreateElement("title")
    $titleValue = $xmlDoc.CreateTextNode($title)
    $titleElement.AppendChild($titleValue);
    $gui.AppendChild($titleElement)

    $domains = $xmlDoc.CreateElement("domains")
    $domains.SetAttribute("enable_anywhere", "false")
    $domains.SetAttribute("enable_currentUserHome", "false")
    $domains.SetAttribute("enable_localSystem", "true")
    $gui.AppendChild($domains);

    $welcome = $xmlDoc.CreateElement("welcome")
    $welcome.SetAttribute("file", "welcome.html")
    $welcome.SetAttribute("mime-type", "text/html")
    $gui.AppendChild($welcome)

    $conclusion = $xmlDoc.CreateElement("conclusion")
    $conclusion.SetAttribute("file", "conclusion.html")
    $conclusion.SetAttribute("mime-type", "text/html")
    $gui.AppendChild($conclusion)

    $xmlDoc.Save($distributionPath)

    productbuild --distribution $distributionPath --package-path $pkg --resources $resources temp.pkg
    Remove-Item $pkg
    Move-Item temp.pkg $pkg
}

pkgbuild --nopayload --scripts Uninstaller/scripts --identifier me.onchro.uninstall --version $Version OnChrome.Uninstall.unsigned.pkg
buildInstaller OnChrome.Uninstall.unsigned.pkg "$($macOsInstallerFilesPath)/Uninstaller/resources" "OnChrome Uninstaller"

if ($CodeSign) {
    productsign --sign $CodeSignPackageIdentity --timestamp OnChrome.Uninstall.unsigned.pkg OnChrome.Uninstall.pkg
} else {
    Rename-Item OnChrome.Uninstall.unsigned.pkg OnChrome.Uninstall.pkg
}

Copy-Item OnChrome.Uninstall.pkg Installer/payload/usr/local/share/OnChrome/

pkgbuild --root Installer/payload --scripts Installer/scripts --identifier me.onchro --version $Version OnChrome.unsigned.pkg
buildInstaller OnChrome.unsigned.pkg "$($macOsInstallerFilesPath)/Installer/resources" "OnChrome"

if ($CodeSign) {
    productsign --sign $CodeSignPackageIdentity --timestamp OnChrome.unsigned.pkg OnChrome.pkg
} else {
    Rename-Item OnChrome.unsigned.pkg OnChrome.pkg
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
    copyFileCreatingFolder -Source "$($installerPath)/OnChrome.pkg" -Dest "dist/$($Version)/OnChrome.pkg"
}
