@echo off
rem Kaivitab API, taustateenuse ja veebilehe eraldi akendes ning avab veebilehe ja Swaggeri.
cd /d "%~dp0"
rem Kui API, taustateenus voi veebileht juba tootavad (nt eelmisest kaivitamisest), suletakse need enne uut kaivitamist,
rem muidu ei saa uus API porti 5000 ega uut versiooni ehitada
taskkill /f /im Restoran.Api.exe >nul 2>&1
taskkill /f /im Restoran.Worker.exe >nul 2>&1
powershell -NoProfile -Command "Get-NetTCPConnection -LocalPort 5000,5173 -State Listen -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }"
rem Esimesel kaivitamisel (nt parast git clone) paigaldatakse veebilehe paketid
if not exist "web\node_modules" (
  echo Paigaldan veebilehe pakette...
  pushd web
  call npm ci --no-audit --no-fund
  popd
)
start "Argent API" cmd /k "cd src\Restoran.Api && dotnet run --urls http://localhost:5000"
start "Argent Worker" cmd /k "timeout /t 15 >nul && cd src\Restoran.Worker && dotnet run"
start "Argent web" cmd /k "cd web && npm run dev"
timeout /t 15 >nul
start http://localhost:5173
start http://localhost:5000/swagger
