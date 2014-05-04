@ECHO OFF

:: Note: We're not using parallel build (/m) because it doesn't seem to
::       work with Sandcastle Help File Builder.

"%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" %~dp0\build.proj /nologo /v:m /flp:verbosity=normal %1 %2 %3 %4 %5 %6 %7 %8 %9