package cmd

import (
	"fmt"
	"github.com/spf13/cobra"
	"github.com/veritas501/win-port-proxy/internal/version"
)

func init() {
	rootCmd.AddCommand(versionCmd)
}

var versionCmd = &cobra.Command{
	Use: "version",
	Run: func(cmd *cobra.Command, args []string) {
		fmt.Println(version.DefaultVer().String())
	},
}
