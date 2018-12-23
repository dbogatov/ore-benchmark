FROM dbogatov/docker-sources:microsoft-dotnet-2.2-aspnetcore-runtime

# Create directory for the app source code
WORKDIR /srv

# Copy the binary
COPY dist/ /srv

ENV ASPNETCORE_ENVIRONMENT=Production

RUN rm -f /srv/appsettings.development.json

ENTRYPOINT /bin/sh -c "dotnet web.dll"
