REM Перед запуском приложений
taskkill /f /im dotnet.exe /t >nul 2>&1

REM В start.bat добавьте
set DB_MAIN=localhost:6000
set DB_RU=localhost:6001
set DB_EU=localhost:6002
set DB_ASIA=localhost:6003

cd ..\RankCalculator\
docker-compose up -d

cd ..\Valuator\
start dotnet build
cd ..\RankCalculator\RankCalculator\
start dotnet build
cd ..\..\EventsLogger\
start dotnet build

start "Redis MAIN" docker run -d -p 6000:6379 --name redis-main redis
start "Redis RU" docker run -d -p 6001:6379 --name redis-ru redis
start "Redis EU" docker run -d -p 6002:6379 --name redis-eu redis
start "Redis ASIA" docker run -d -p 6003:6379 --name redis-asia redis

pause

cd ..\Valuator\
start dotnet run --no-build --urls "http://localhost:5001"
start dotnet run --no-build --urls "http://localhost:5002"

cd ..\RankCalculator\RankCalculator\
start "rank1" dotnet run --no-build
start "rank2" dotnet run --no-build

cd ..\..\EventsLogger\
start "logger 1" dotnet run --no-build
start "logger 2" dotnet run --no-build

cd ..\nginx\
start nginx.exe