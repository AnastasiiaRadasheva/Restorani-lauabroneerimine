@echo off
rem Запускает API и сайт в двух окнах, потом открывает браузер.
cd /d "%~dp0"
start "Argent API" cmd /k "cd src\Restoran.Api && dotnet run --urls http://localhost:5000"
start "Argent Worker" cmd /k "timeout /t 15 >nul && cd src\Restoran.Worker && dotnet run"
start "Argent web" cmd /k "cd web && npm run dev"
timeout /t 12 >nul
start http://localhost:5173
