FROM dbogatov/docker-sources:microsoft-dotnet-2.2-sdk-alpine

LABEL maintainer="dmytro@dbogatov.org"

ARG CI_BUILD_REF
ENV CI_BUILD_REF=$CI_BUILD_REF

WORKDIR /benchmark

# the context MUST be the entire repo
COPY . .

RUN \
	apk --update add bash && \
	./tools/reproducibility/build.sh

WORKDIR /benchmark/scripts
