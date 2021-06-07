1，显示log
adb logcat *:I -v color  | where {$_ -match "HB.FullStack"}