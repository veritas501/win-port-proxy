package proxy

import (
	"fmt"
	"net"
)

func TcpConnectionTest(host string, port int) (success bool) {
	address := fmt.Sprintf("%s:%d", host, port)
	conn, err := net.Dial("tcp", address)
	if err != nil {
		return false
	}

	defer conn.Close()
	return true
}
