
for /r %cd% %%i in (artifacts\*.nupkg) do del %%i
dotnet clean


setlocal enabledelayedexpansion
for /f "tokens=2 delims==" %%a in ('wmic path win32_operatingsystem get LocalDateTime /value') do set t=%%a

set now=CI-%t:~,8%-%t:~8,6%

dotnet pack -c Debug -o .\..\..\artifacts --version-suffix=!now!

for /r %cd% %%i in (artifacts\*.nupkg) do dotnet nuget push %%i -k 2F3E3FAF-4A81-4B24-91BF-E73EEEFA496B -s http://developer.brlite.com:8112/nuget
