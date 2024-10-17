#FROM mcr.microsoft.com/dotnet/aspnet:2.1-focal
#
#RUN apt-get update \
    #&& apt-get install -y --allow-unauthenticated \
        #libc6-dev \
        #libgdiplus \
        #libx11-dev \
     #&& rm -rf /var/lib/apt/lists/*
#
#COPY Keystone App/
##COPY Keystone App/
#WORKDIR /App
#ENTRYPOINT [ "dotnet", "Keystone.dll" 

# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:2.1-focal AS build

RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libc6-dev \
        libgdiplus \
        libx11-dev \
     && rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Copy the project files
COPY KeystoneLibrary/KeystoneLibrary.csproj ./
RUN dotnet restore ./KeystoneLibrary.csproj

# Copy the project files
COPY Keystone/Keystone.csproj ./
RUN dotnet restore ./Keystone.csproj

# Copy the entire source code for both projects
COPY KeystoneLibrary/ KeystoneLibrary/
COPY Keystone/ Keystone/

# Build the main project, which will also build KeystoneLib
WORKDIR /src/Keystone
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:2.1-focal AS runtime

WORKDIR /app

# Copy the build artifacts from the previous stage
COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "Keystone.dll"]]