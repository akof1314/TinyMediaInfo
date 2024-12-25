@ECHO OFF

dotnet publish /p:PublishProfile=Properties\PublishProfiles\FolderProfileWin64.pubxml

if exist "bin\Release\net8.0\publish\win-x64\TinyMediaInfo.pdb" (  
    del "bin\Release\net8.0\publish\win-x64\TinyMediaInfo.pdb"  
)
if exist "bin\Release\net8.0\publish\win-x64\avdevice-60.dll" (  
    del "bin\Release\net8.0\publish\win-x64\avdevice-60.dll"  
)
if exist "bin\Release\net8.0\publish\win-x64\avfilter-9.dll" (  
    del "bin\Release\net8.0\publish\win-x64\avfilter-9.dll"  
)
if exist "bin\Release\net8.0\publish\win-x64\postproc-57.dll" (  
    del "bin\Release\net8.0\publish\win-x64\postproc-57.dll"  
)
if exist "bin\Release\net8.0\publish\win-x64\swscale-7.dll" (  
    del "bin\Release\net8.0\publish\win-x64\swscale-7.dll"  
)
if exist "bin\Release\net8.0\publish\win-x64\avdevice-61.dll" (  
    del "bin\Release\net8.0\publish\win-x64\avdevice-61.dll"  
)
if exist "bin\Release\net8.0\publish\win-x64\avfilter-10.dll" (  
    del "bin\Release\net8.0\publish\win-x64\avfilter-10.dll"  
)
if exist "bin\Release\net8.0\publish\win-x64\postproc-58.dll" (  
    del "bin\Release\net8.0\publish\win-x64\postproc-58.dll"  
)
if exist "bin\Release\net8.0\publish\win-x64\swscale-8.dll" (  
    del "bin\Release\net8.0\publish\win-x64\swscale-8.dll"  
)

echo publish bin\Release\net8.0\publish\win-x64 finish

pause