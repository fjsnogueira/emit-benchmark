@echo off
powershell -NoLogo -NoProfile -ExecutionPolicy ByPass "%~dp0scripts\run.ps1" %*
exit /b %ErrorLevel%
