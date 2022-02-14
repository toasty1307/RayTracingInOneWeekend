FROM mcr.microsoft.com/dotnet/aspnet:6.0
COPY SomeoneStopMe/bin/Release/net6.0/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "SomeoneStopMe.dll"]