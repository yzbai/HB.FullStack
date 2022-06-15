$version_start = '2'
$nuget_directory = 'D:\nuget_localfeed\'

Set-Location C:\Project\First\HB.FullStack
Remove-Item .\packages\*.*
& 'C:\Program Files\Microsoft Visual Studio\2022\Preview\MSBuild\Current\Bin\amd64\MSBuild.exe' .\HB.FullStack-no-test.sln /t:pack /p:OutputPath=C:\Project\First\HB.FullStack\packages\