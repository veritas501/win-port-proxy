.PHONY: binary

GIT_COMMIT := $(shell git rev-parse --short HEAD || echo unsupported)
VERSION := $(shell cat ./internal/version/VERSION)
BUILD_TIME := $(shell date -u +"%Y-%m-%dT%H:%M:%SZ")
LDFLAGS := "${LDFALGS} \
	-w -s \
	-X github.com/veritas501/win-port-proxy/internal/version.Version=${VERSION} \
	-X github.com/veritas501/win-port-proxy/internal/version.GitCommit=${GIT_COMMIT} \
	-X github.com/veritas501/win-port-proxy/internal/version.BuildTime=${BUILD_TIME}"

binary:
	mkdir -p bin/release
	GOOS=windows GOARCH=amd64 go build -o bin/release -ldflags ${LDFLAGS} -gcflags=all=-l
