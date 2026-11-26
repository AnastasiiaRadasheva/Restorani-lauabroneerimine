@echo off
rem Kaivitab API, taustateenuse ja veebilehe eraldi akendes ning avab veebilehe ja Swaggeri.
cd /d "%~dp0"
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
