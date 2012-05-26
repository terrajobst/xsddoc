@echo off

:: Workaround: MSBuild apparently snapshots the variables. Setting the environment
::             variable from within the build process doesn't work.
set SHFBCOMPONENTROOT=%~dp0..\Output\Bin

"%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" %~dp0\Build.proj /t:Build

PAUSE