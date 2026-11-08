@echo off
cd /d "%~dp0.."
where pythonw >nul 2>nul
if %errorlevel%==0 (
    start "" pythonw "DevTools\CardPresetEditor.py"
) else (
    python "DevTools\CardPresetEditor.py"
)
