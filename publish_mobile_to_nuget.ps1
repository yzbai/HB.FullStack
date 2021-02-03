//dotnet restore HB.FullStack.Mobile.sln
//MSBuild HB.FullStack.Mobile.sln -t:Rebuild -p:Configuration=Release -p:ServerAddress=192.168.0.100 -p:ServerUser=hulijuan -p:ServerPassword=hlJLovebyz1314!
//msbuild HB.FullStack.Mobile.sln -t:pack -p:Configuration=Release -p:ServerAddress=192.168.0.100 -p:ServerUser=hulijuan -p:ServerPassword=hlJLovebyz1314! -p:OutputPath=c:\Project\HB.FullStack\packages

dotnet build HB.FullStack.Mobile.sln
dotnet pack HB.FullStack.Mobile.sln -c Release -o ./packages
//Get-ChildItem .\packages\ -Filter HB* | 
//	ForEach-Object -Process {
//		dotnet nuget push .\packages\$_ --api-key oy2g2vhtbpk6vumrytpalvdcmhvaz3cezm4mngmexxhfta --source https://api.nuget.org/v3/index.json --skip-duplicate
//	}