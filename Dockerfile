FROM dbogatov/docker-sources:microsoft-dotnet-2.2-runtime-alpine

LABEL maintainer="dmytro@dbogatov.org"

WORKDIR /benchmark

COPY ./src/cli/dist/ .

COPY ./data/ ./data

RUN apk --update add tree && rm -rf /var/cache/apk/*
