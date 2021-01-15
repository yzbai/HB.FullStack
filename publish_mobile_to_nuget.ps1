dotnet restore HB.FullStack.Mobile.sln
MSBuild HB.FullStack.Mobile.sln -t:Rebuild -p:Configuration=Release -p:ServerAddress=192.168.0.100 -p:ServerUser=hulijuan -p:ServerPassword=hlJLovebyz1314!
msbuild HB.FullStack.Mobile.sln -t:pack -p:Configuration=Release -p:ServerAddress=192.168.0.100 -p:ServerUser=hulijuan -p:ServerPassword=hlJLovebyz1314! -p:OutputPath=c:\Project\HB.FullStack\packages