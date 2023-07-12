# Use the official Microsoft .NET image, it has Windows Server Core and .NET Core runtime
FROM mcr.microsoft.com/dotnet/runtime:7.0-nanoserver-ltsc2022 AS base
WORKDIR /app

# Set up the environment for the dlls
ENV PATH="C:\\windows\\system32;C:\\windows"

# Copy over the dll files
COPY "lib/Msacm32.dll" "C:/windows/system32/"

# Other dll files...
# COPY lib/YourOtherDll.dll C:/windows/system32/
# ...

# Expose the port your app runs on
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0-nanoserver-ltsc2022 AS build
WORKDIR /src
COPY ["src/VoicerStudio.Api/VoicerStudio.Api.csproj", "VoicerStudio.Api/"]
RUN dotnet restore "VoicerStudio.Api/VoicerStudio.Api.csproj"
COPY . .
WORKDIR "/src/VoicerStudio.Api"
RUN dotnet build "VoicerStudio.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VoicerStudio.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Entry point
ENTRYPOINT ["dotnet", "VoicerStudio.Api.dll"]
