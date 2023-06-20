package main

import (
	"github.com/veritas501/win-port-proxy/cmd"
	"log"
)

func init() {
	log.SetFlags(0)
	log.SetPrefix("[*] ")
}

func main() {
	cmd.Execute()
}
