FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Can I do this in not such a shitty way?
# Build args from docker hub:

ARG ADMINDISCORDIDS__0
ARG ADMINDISCORDIDS__1
ARG ASPNETCORE_ENVIRONMENT
ARG CONNECTIONSTRING
ARG DISCORD__TOKEN
ARG STRAVA__CLIENTID
ARG STRAVA__CLIENTSECRET
ARG HUMIO__TOKEN

ENV ASPNETCORE_ADMINDISCORDIDS__0=$ADMINDISCORDIDS__0
ENV ASPNETCORE_ADMINDISCORDIDS__1=$ADMINDISCORDIDS__1
ENV ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT
ENV ASPNETCORE_CONNECTIONSTRING=$CONNECTIONSTRING
ENV ASPNETCORE_DISCORD__TOKEN=$DISCORD__TOKEN
ENV ASPNETCORE_STRAVA__CLIENTID=$STRAVA__CLIENTID
ENV ASPNETCORE_STRAVA__CLIENTSECRET=$STRAVA__CLIENTSECRET
ENV ASPNETCORE_HUMIO__TOKEN=$HUMIO__TOKEN

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["StravaDiscordBot.csproj", ""]
RUN dotnet restore "./StravaDiscordBot.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "StravaDiscordBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StravaDiscordBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StravaDiscordBot.dll"]

RUN echo "Europe/Copenhagen" > /etc/timezone
RUN dpkg-reconfigure -f noninteractive tzdata