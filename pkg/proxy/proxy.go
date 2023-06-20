package proxy

import (
	"fmt"
	"os/exec"
	"regexp"
	"strconv"
	"strings"
)

var Directions = []string{"v4tov4", "v4tov6", "v6tov4", "v6tov6"}

type Rule struct {
	Direction      string
	ListenAddress  string
	ListenPort     int
	ConnectAddress string
	ConnectPort    int
}

func GetProxyRules() (rules []Rule, err error) {
	r := regexp.MustCompile(`(\S+)\s+(\d+)\s+(\S+)\s+(\d+)\s*$`)

	for _, direction := range Directions {
		var output []byte
		output, err = exec.Command("cmd", "/c",
			fmt.Sprintf("chcp 65001 && netsh interface portproxy show %s", direction)).Output()
		if err != nil {
			return
		}
		for _, line := range strings.Split(string(output), "\n") {
			line = strings.Trim(line, "\r\n\t ")
			m := r.FindStringSubmatch(line)
			if m != nil {
				listenPort, _ := strconv.Atoi(m[2])
				connectPort, _ := strconv.Atoi(m[4])
				rules = append(rules, Rule{
					Direction:      direction,
					ListenAddress:  m[1],
					ListenPort:     listenPort,
					ConnectAddress: m[3],
					ConnectPort:    connectPort,
				})
			}
		}
	}

	return
}

func DeleteProxyRule(rule Rule) (success bool) {
	cmdline := fmt.Sprintf("netsh interface portproxy delete %s listenaddress=%s listenport=%d",
		rule.Direction, rule.ListenAddress, rule.ListenPort)
	cmd := exec.Command("cmd", "/c", cmdline)
	if err := cmd.Run(); err != nil {
		if exitError, ok := err.(*exec.ExitError); ok {
			if exitError.ExitCode() == 0 {
				return true
			} else {
				return false
			}
		}
	}

	return true
}

func AddProxyRule(rule Rule) (success bool) {
	cmdline := fmt.Sprintf("netsh interface portproxy add %s listenaddress=%s listenport=%d connectaddress=%s connectport=%d",
		rule.Direction, rule.ListenAddress, rule.ListenPort, rule.ConnectAddress, rule.ConnectPort)
	cmd := exec.Command("cmd", "/c", cmdline)
	if err := cmd.Run(); err != nil {
		if exitError, ok := err.(*exec.ExitError); ok {
			if exitError.ExitCode() == 0 {
				return true
			} else {
				return false
			}
		}
	}

	return true
}

func (r Rule) String() string {
	return fmt.Sprintf("%s:%d -> %s:%d",
		r.ListenAddress,
		r.ListenPort,
		r.ConnectAddress,
		r.ConnectPort,
	)
}
