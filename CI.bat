
for /r %cd% %%i in (artifacts\*.nupkg) do del %%i
dotnet clean
dotnet restore
dotnet build

setlocal enabledelayedexpansion
for /f "tokens=2 delims==" %%a in ('wmic path win32_operatingsystem get LocalDateTime /value') do set t=%%a

set now=CI-%t:~,8%-%t:~8,6%

dotnet pack -c Release -o .\..\..\artifacts --version-suffix=!now!

for /r %cd% %%i in (artifacts\*.nupkg) do dotnet nuget push %%i -k 2F3E3FAF-4A81-4B24-91BF-E73EEEFA496B -s http://developer.brlite.com:8112/nuget
for /r %cd% %%i in (artifacts\*.nupkg) do dotnet nuget push %%i -k 69864ec9-ba75-41a4-ab8f-5c874cb0f5f5 -s https://api.nuget.org/v3/index.json