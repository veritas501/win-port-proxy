package version

import "fmt"

// Reference: https://github.com/moby/moby/blob/v24.0.1/dockerversion/version_lib.go
// Default build-time variable for library-import.
// These variables are overridden on build with build-time information.
var (
	ProductName = "win-port-proxy"
	Version     = "library-import"
	GitCommit   = "library-import"
	BuildTime   = "library-import"
)

type Ver struct {
	ProductName string
	Version     string
	GitCommit   string
	License     string
	BuildTime   string
}

func DefaultVer() *Ver {
	return &Ver{
		ProductName: ProductName,
		Version:     Version,
		GitCommit:   GitCommit,
		BuildTime:   BuildTime,
	}
}

func (v Ver) String() string {
	return fmt.Sprintf("%s version %s, build %s at %s",
		v.ProductName,
		v.Version,
		v.GitCommit,
		v.BuildTime,
	)
}
