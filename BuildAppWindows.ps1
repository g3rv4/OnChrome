[CmdletBinding(DefaultParametersetName='None')]
param(
    [Parameter()][switch] $Sign
)

if (Test-Path "bin"){
    Remove-Item -LiteralPath "bin" -Force -Recurse
}
New-Item -Path "." -Name "bin" -ItemType "directory" | Out-Null
$bin = New-Item -Path "bin" -Name "win64" -ItemType "directory"

[xml]$xmlDoc = Get-Content app/Directory.Build.props
$version = $xmlDoc['Project']['PropertyGroup']['AssemblyVersion'].InnerText

Copy-Item -Path app/OnChrome.wxs $bin

Push-Location "app/OnChrome"
dotnet publish -c Release -r win-x64 /property:Version=$version -o "$($bin)/publish"
Pop-Location

Push-Location $bin

# build the wix file
$wxsPath = Join-Path $bin OnChrome.wxs
[xml]$xmlDoc = Get-Content $wxsPath
$wix = $xmlDoc['Wix']
$product = $wix['Product']
$product.SetAttribute("Version", $version)
$dirRef = $product['DirectoryRef']
$feature = $product['Feature']

$filesToSign = @()

$files = Get-ChildItem -Path publish
foreach ($file in $files){
    $isDllOrExe = $file.Name.EndsWith(".exe") -or $file.Name.EndsWith(".dll")
    if ($isDllOrExe) {
        $signature = Get-AuthenticodeSignature $file.FullName
        if ($signature.Status -eq "NotSigned") {
            $filesToSign += $file.FullName
        } elseif ($signature.Status -eq "Valid") {
        } else {
            Write-Error "Signature of file $($File.Name) is $($signature.Status)"
            exit 1
        }
    }

    $id = $file.Name -replace '[^a-zA-Z0-9_\.]', '_'

    $component = $xmlDoc.CreateElement("Component", $wix.NamespaceURI)
    $component.SetAttribute("Id", $id)
    $dirRef.AppendChild($component) | Out-Null
    
    $fileObj = $xmlDoc.CreateElement("File", $wix.NamespaceURI)
    $fileObj.SetAttribute("Id", $id)
    $fileObj.SetAttribute("Source", "publish\$($file.Name)")
    $fileObj.SetAttribute("KeyPath", "yes")
    if ($isDllOrExe) {
        $fileObj.SetAttribute("Checksum", "yes")
    }
    $component.AppendChild($fileObj) | Out-Null

    $componentRef = $xmlDoc.CreateElement("ComponentRef", $wix.NamespaceURI)
    $componentRef.SetAttribute("Id", $id)
    $feature.AppendChild($componentRef) | Out-Null
}

if ($Sign) {
    & "C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe" sign /fd SHA256 /t http://timestamp.comodoca.com $filesToSign
}

$xmlDoc.Save($wxsPath)

& "C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe" OnChrome.wxs
& "C:\Program Files (x86)\WiX Toolset v3.11\bin\light.exe" OnChrome.wixobj

if ($Sign) {
    & "C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe" sign /fd SHA256 /t http://timestamp.comodoca.com /d OnChrome.msi OnChrome.msi
}

Move-Item -Path OnChrome.msi -Destination $bin.FullName
Pop-Location
