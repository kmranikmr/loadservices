@echo off
REM Cleanup script to remove unnecessary files before uploading to another repository

echo Cleaning up unnecessary files...

REM Remove temporary files
if exist "New Text Document.txt" (
    echo Removing New Text Document.txt
    del "New Text Document.txt"
)

REM Remove upgrade logs
if exist "UpgradeLog.htm" (
    echo Removing UpgradeLog.htm
    del "UpgradeLog.htm"
)

if exist "UpgradeLog2.htm" (
    echo Removing UpgradeLog2.htm
    del "UpgradeLog2.htm"
)

REM Remove backup files
if exist "docker-compose.yml.save" (
    echo Removing docker-compose.yml.save
    del "docker-compose.yml.save"
)

REM Remove redundant gitignore
if exist "gitignore" (
    echo Removing redundant gitignore file
    del "gitignore"
)

REM Remove misc files
if exist "Presentation1.png" (
    echo Removing Presentation1.png
    del "Presentation1.png"
)

if exist "patch" (
    echo Removing patch file
    del "patch"
)

REM Remove old project files
if exist "DataAnalyticsPlatform.Common\DataAnalyticsPlatform.Common.csproj.old" (
    echo Removing DataAnalyticsPlatform.Common.csproj.old
    del "DataAnalyticsPlatform.Common\DataAnalyticsPlatform.Common.csproj.old"
)

echo Cleanup complete!
echo.
echo Please make sure to run "git clean -fdx" to remove all untracked files and directories
echo before pushing to a new repository.
echo.
echo This will remove all files and directories not under source control, including:
echo - bin and obj directories
echo - build artifacts
echo - temporary files
echo - NuGet packages
echo.
echo To preview what will be deleted, run: git clean -ndx
echo To proceed with deletion, run: git clean -fdx

pause
