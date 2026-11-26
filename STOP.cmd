@echo off
rem Peatab koik, mille START.cmd kaivitas: API, taustateenuse ja veebilehe ning sulgeb nende aknad.
echo Peatan Argent API, taustateenuse ja veebilehe...
taskkill /f /im Restoran.Api.exe >nul 2>&1
taskkill /f /im Restoran.Worker.exe >nul 2>&1
rem Koik, mis kuulab porte 5000 (API) ja 5173 (veebileht)
powershell -NoProfile -Command "Get-NetTCPConnection -LocalPort 5000,5173 -State Listen -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }"
rem START.cmd-i aknad ("Argent API", "Argent Worker", "Argent web")
taskkill /f /t /fi "WINDOWTITLE eq Argent API*" >nul 2>&1
taskkill /f /t /fi "WINDOWTITLE eq Argent Worker*" >nul 2>&1
taskkill /f /t /fi "WINDOWTITLE eq Argent web*" >nul 2>&1
echo Valmis: koik on peatatud.
timeout /t 3 >nul
