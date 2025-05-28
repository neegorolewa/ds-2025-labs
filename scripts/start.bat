
taskkill /f /im dotnet.exe /t >nul 2>&1

set DB_MAIN=localhost:6000
set DB_RU=localhost:6001
set DB_EU=localhost:6002
set DB_ASIA=localhost:6003
set DB_USERS=localhost:6004

set DB_MAIN_PASS=main
set DB_RU_PASS=ru
set DB_EU_PASS=eu
set DB_ASIA_PASS=asia
set DB_USERS_PASS=users

set RABBITMQ_DEFAULT_USER=rabbituser
set RABBITMQ_DEFAULT_PASS=rabbitpass

cd ..\RankCalculator\
docker-compose up -d

cd ..\Valuator\
start dotnet build
cd ..\RankCalculator\RankCalculator\
start dotnet build
cd ..\..\EventsLogger\
start dotnet build

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