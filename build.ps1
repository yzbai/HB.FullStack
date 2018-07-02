
$urlCurrent = "https://dotnetcli.azureedge.net/dotnet/Sdk/2.1.3/dotnet-sdk-2.1.3-win-x64.zip"
$env:DOTNET_INSTALL_DIR = "$pwd\.dotnetsdk"
mkdir $env:DOTNET_INSTALL_DIR -Force | Out-Null
$tempFileCurrent = [System.IO.Path]::GetTempFileName()
(New-Object System.Net.WebClient).DownloadFile($urlCurrent, $tempFileCurrent)
Add-Type -AssemblyName System.IO.Compression.FileSystem; [System.IO.Compression.ZipFile]::ExtractToDirectory($tempFileCurrent, $env:DOTNET_INSTALL_DIR)
$env:Path = "$env:DOTNET_INSTALL_DIR;$env:Path"

dotnet --info
dotnet restore
dotnet build

$revision = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = 1 }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$revision = [convert]::ToInt32($revision, 10) + 4000
#$revision = [string]::Format("{0:D4}", $revision)

dotnet pack -c Release -o .\..\..\artifacts --version-suffix=alpha-$revision