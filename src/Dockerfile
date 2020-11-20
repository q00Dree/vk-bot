FROM mcr.microsoft.com/dotnet/core/sdk:3.1
WORKDIR /app

COPY ./chatbotvk.Bot.EntryPoint/chatbotvk.Bot.EntryPoint.csproj ./chatbotvk.Bot.EntryPoint/
COPY ./chatbotvk.Bot.Core/chatbotvk.Bot.Core.csproj ./chatbotvk.Bot.Core/

COPY ./chatbotvk.Core/chatbotvk.Core.csproj ./chatbotvk.Core/
COPY ./chatbotvk.Database.Models/chatbotvk.Database.Models.csproj ./chatbotvk.Database.Models/
COPY ./chatbotvk.Database.NpgSqlContext/chatbotvk.Database.NpgSqlContext.csproj ./chatbotvk.Database.NpgSqlContext/
COPY ./chatbotvk.Database.Repositories/chatbotvk.Database.Repositories.csproj ./chatbotvk.Database.Repositories/
COPY ./chatbotvk.Services/chatbotvk.Services.csproj ./chatbotvk.Services/

COPY ./chatbot-vk.sln .
RUN dotnet restore

COPY . .
RUN dotnet publish ./chatbotvk.Bot.EntryPoint/chatbotvk.Bot.EntryPoint.csproj -o /publish/
WORKDIR /publish

ENTRYPOINT ["dotnet", "chatbotvk.Bot.EntryPoint.dll"]
