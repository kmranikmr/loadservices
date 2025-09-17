@echo off
echo ====================================================
echo     REPOSITORY CLEANUP SCRIPT - SAFE VERSION
echo ====================================================
echo.
echo This script will clean up your repository without
echo trying to remove locked Visual Studio files.
echo.
echo Please close Visual Studio before proceeding.
echo.
pause

echo.
echo === Removing temporary files ===
if exist "New Text Document.txt" (
    echo Removing New Text Document.txt
    del "New Text Document.txt"
)

echo.
echo === Removing upgrade logs ===
if exist "UpgradeLog.htm" (
    echo Removing UpgradeLog.htm
    del "UpgradeLog.htm"
)

if exist "UpgradeLog2.htm" (
    echo Removing UpgradeLog2.htm
    del "UpgradeLog2.htm"
)

echo.
echo === Removing backup files ===
if exist "docker-compose.yml.save" (
    echo Removing docker-compose.yml.save
    del "docker-compose.yml.save"
)

echo.
echo === Removing redundant gitignore ===
if exist "gitignore" (
    echo Removing redundant gitignore file
    del "gitignore"
)

echo.
echo === Removing miscellaneous files ===
if exist "Presentation1.png" (
    echo Removing Presentation1.png
    del "Presentation1.png"
)

if exist "patch" (
    echo Removing patch file
    del "patch"
)

echo.
echo === Removing old project files ===
if exist "DataAnalyticsPlatform.Common\DataAnalyticsPlatform.Common.csproj.old" (
    echo Removing DataAnalyticsPlatform.Common.csproj.old
    del "DataAnalyticsPlatform.Common\DataAnalyticsPlatform.Common.csproj.old"
)

echo.
echo === Cleaning bin and obj folders (safe approach) ===
for /d /r . %%d in (bin obj) do (
    if exist "%%d" (
        echo Removing: %%d
        rd /s /q "%%d" 2>nul
    )
)

echo.
echo === Removing packages folder ===
if exist "packages" (
    echo Removing packages folder
    rd /s /q "packages" 2>nul
)

echo.
echo ====================================================
echo                CLEANUP COMPLETE
echo ====================================================
echo.
echo IMPORTANT: If you encountered any errors about locked files,
echo           please close Visual Studio and any other applications
echo           that might be using those files, then run this script again.
echo.
echo Note: The .vs folder was left untouched as it often contains
echo      locked files. If you want to clean it as well, manually
echo      delete it after closing Visual Studio completely.
echo.
pause
