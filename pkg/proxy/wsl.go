package proxy

import (
	"errors"
	"os/exec"
	"strings"
)

func GetWslIP() (ip string, err error) {
	ans, err := exec.Command("wsl.exe", "hostname", "-I").Output()
	if err != nil {
		return
	}
	trimAns := strings.Trim(string(ans), "\n\r\t ")
	for _, ip = range strings.Split(trimAns, " ") {
		if !strings.HasSuffix(ip, ".1") {
			return
		}
	}
	return "", errors.New("wsl ip not found")
}
