@echo off
echo Остановка всех сервисов...

REM Остановка процессов приложений
taskkill /f /im dotnet.exe /t >nul 2>&1
taskkill /f /im valuator.exe /t >nul 2>&1
taskkill /f /im rankcalculator.exe /t >nul 2>&1
taskkill /f /im eventslogger.exe /t >nul 2>&1

REM Остановка nginx
taskkill /f /im nginx.exe /t >nul 2>&1
if exist "..\nginx\nginx.exe" (
    cd ..\nginx\
    nginx -s stop >nul 2>&1
    cd ..\
)

REM Остановка Docker-контейнеров
rem docker-compose down >nul 2>&1
rem docker stop redis-main redis-ru redis-eu redis-asia >nul 2>&1
rem docker rm redis-main redis-ru redis-eu redis-asia >nul 2>&1

echo Все сервисы остановлены
pause