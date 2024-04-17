@ECHO OFF

dotnet publish /p:PublishProfile=Properties\PublishProfiles\FolderProfileMac64.pubxml

set "appName=TinyMediaInfo"  
set "appDir=bin\Release\net8.0\publish\%appName%.app"  

REM 检查文件夹是否存在  
if exist "%appDir%\" (  
    REM 删除文件夹及其内容  
    rmdir /s /q "%appDir%"  
)
  
REM 创建.app目录
mkdir "%appDir%"
  
REM 创建Contents目录  
mkdir "%appDir%\Contents"
  
REM 创建MacOS目录（存放可执行文件）  
mkdir "%appDir%\Contents\MacOS"
  
REM 创建Resources目录（存放资源文件）  
mkdir "%appDir%\Contents\Resources"
  
REM 创建Info.plist文件（描述应用程序信息的XML文件）  
copy "Info.plist" "%appDir%\Contents"

copy "Assets\TinyMedia.icns" "%appDir%\Contents\Resources"

xcopy /E /I "bin\Release\net8.0\publish\osx-x64" "%appDir%\Contents\MacOS"

if exist "%appDir%\Contents\MacOS\TinyMediaInfo.pdb" (  
    del "%appDir%\Contents\MacOS\TinyMediaInfo.pdb"
)

echo publish bin\Release\net8.0\publish\osx-x64 finish

pause