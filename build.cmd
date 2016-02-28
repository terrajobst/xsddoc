@ECHO OFF

:: Note: We're not using parallel build (/m) because it doesn't seem to
::       work with Sandcastle Help File Builder.

"%ProgramFiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe" %~dp0\build.proj /nologo /v:m /flp:verbosity=detailed  %*
