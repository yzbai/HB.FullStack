cd C:\Project\First\HB.FullStack
Remove-Item .\packages\*.*
& 'C:\Program Files\Microsoft Visual Studio\2022\Preview\MSBuild\Current\Bin\amd64\MSBuild.exe' .\HB.FullStack-no-test.sln /p:OutputPath=C:\Project\First\HB.FullStack\packages\
Get-ChildItem .\packages\ -Filter *.nupkg | 
	ForEach-Object -Process {
		nuget add .\packages\$_ -Source D:\nuget_localfeed -Expand
	}