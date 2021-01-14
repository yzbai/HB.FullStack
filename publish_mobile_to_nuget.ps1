msbuild HB.FullStack.Mobile.sln -t:Rebuild -p:Configuration=Release
msbuild HB.FullStack.Mobile.sln -t:pack -p:Configuration=Release -p:OutputPath=c:\Project\HB.FullStack\packages