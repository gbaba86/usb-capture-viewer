param([string]$Dotnet = 'dotnet')
$ErrorActionPreference = 'Stop'
$buildRoot = $PSScriptRoot
$nativeRoot = Join-Path $buildRoot 'native'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
if (-not (Test-Path -LiteralPath (Join-Path $nativeRoot 'plugins\access\libdshow_plugin.dll'))) {
    $archive = Join-Path $buildRoot 'vlc-3.0.24-win64.zip'
    Invoke-WebRequest 'https://download.videolan.org/pub/videolan/vlc/3.0.24/win64/vlc-3.0.24-win64.zip' -OutFile $archive
    $expected = 'FCF30850371AD10C9373CC4F0F4501E7DEE49E3E9AE9F20C72FB2661A1CA6323'
    if ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $expected) { throw 'VLC runtime checksum mismatch' }
    $unpack = Join-Path $buildRoot 'runtime-download'
    Expand-Archive -LiteralPath $archive -DestinationPath $unpack -Force
    $runtime = Join-Path $unpack 'vlc-3.0.24'
    New-Item -ItemType Directory -Path $nativeRoot -Force | Out-Null
    foreach ($name in @('libvlc.dll','libvlccore.dll','plugins','hrtfs','lua','AUTHORS.txt','COPYING.txt','THANKS.txt')) {
        Copy-Item -LiteralPath (Join-Path $runtime $name) -Destination $nativeRoot -Recurse -Force
    }
}
& $Dotnet publish (Join-Path $buildRoot 'src\SwitchCaptureViewer.csproj') -c Release -r win-x64 --self-contained true -p:RestoreLockedMode=true -o (Join-Path $buildRoot 'publish')
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
$publish = Join-Path $buildRoot 'publish'
Copy-Item -LiteralPath (Join-Path $buildRoot 'licenses') -Destination $publish -Recurse -Force
Copy-Item -LiteralPath (Join-Path $buildRoot 'LICENSE'),(Join-Path $buildRoot 'THIRD_PARTY_NOTICES.md'),(Join-Path $buildRoot 'README.md'),(Join-Path $buildRoot 'README.th.md') -Destination $publish -Force
Copy-Item -LiteralPath (Join-Path $buildRoot 'docs') -Destination $publish -Recurse -Force
Write-Host 'Build complete. Open publish\USBCaptureViewer.exe; keep the whole folder together.'
