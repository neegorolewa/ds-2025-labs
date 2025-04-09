cd ..\RankCalculator\
docker-compose up -d

cd ..\Valuator\
start dotnet run --urls "http://localhost:5001"
start dotnet run --urls "http://localhost:5002"

cd ..\RankCalculator\RankCalculator\
start "rank1" dotnet run
start "rank2" dotnet run

cd ..\..\nginx\
start nginx.exe