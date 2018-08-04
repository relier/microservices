FROM microsoft/aspnetcore:2.0

# Copia os binários do projeto para a o container.
COPY /bin/Release/netcoreapp2.0/publish /app

WORKDIR /app
ENTRYPOINT [ "dotnet", "TestService.dll" ]