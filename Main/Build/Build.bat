@echo off

"%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" Build.proj /t:Build

PAUSE