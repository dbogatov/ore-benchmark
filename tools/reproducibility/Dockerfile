FROM dbogatov/docker-sources:microsoft-dotnet-2.2-sdk-alpine

LABEL maintainer="dmytro@dbogatov.org"

WORKDIR /benchmark

# the context MUST be the entire repo
COPY . .

RUN \
	apk --update add bash && \
	./tools/reproducibility/build.sh

WORKDIR /benchmark/scripts
