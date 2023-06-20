package cmd

import (
	"github.com/spf13/cobra"
	"github.com/veritas501/go-elevate-demo/pkg/elevate"
	"github.com/veritas501/win-port-proxy/pkg/proxy"
	"log"
)

var (
	cfgUpdateMode          bool
	cfgRemoteHost          string
	cfgRemotePort          int
	cfgListenPort          int
	cfgDontCheckConnection bool
)

func init() {
	rootCmd.CompletionOptions.DisableDefaultCmd = true

	rootCmd.Flags().BoolVarP(
		&cfgUpdateMode, "update", "u", false,
		"update mode, override existed rule",
	)

	rootCmd.Flags().BoolVarP(
		&cfgDontCheckConnection, "nocheck", "", false,
		"don't check connection",
	)

	rootCmd.Flags().StringVarP(
		&cfgRemoteHost, "rhost", "", "",
		`remote host or "wsl" for wsl host (required)`,
	)
	_ = rootCmd.MarkFlagRequired("rhost")

	rootCmd.Flags().IntVarP(
		&cfgRemotePort, "rport", "", 0,
		"remote port (required)",
	)
	_ = rootCmd.MarkFlagRequired("rport")

	rootCmd.Flags().IntVarP(
		&cfgListenPort, "lport", "", 0,
		"listen port (required)",
	)
	_ = rootCmd.MarkFlagRequired("lport")

	// add elevate cmdline to cobra
	elevate.AddCmdlineToCobra(rootCmd)
}

func entryPoint(cmd *cobra.Command, args []string) {
	var err error
	// resolve wsl ip
	if cfgRemoteHost == "wsl" || cfgRemoteHost == "wsl2" {
		log.Printf("resolving wsl ip ...\n")
		cfgRemoteHost, err = proxy.GetWslIP()
		if err != nil {
			log.Println("get wsl ip failed: ", err)
			return
		}
		log.Printf("get wsl ip: %s\n", cfgRemoteHost)
	}

	if !cfgDontCheckConnection {
		log.Printf("check tcp connection to %s:%d ...", cfgRemoteHost, cfgRemotePort)
		// try to check tcp connection
		if !proxy.TcpConnectionTest(cfgRemoteHost, cfgRemotePort) {
			log.Println("tcp connection test failed: ", err)
			return
		} else {
			log.Println("tcp connection ok")
		}
	}

	rules, _ := proxy.GetProxyRules()

	// check if rule already exist
	for _, rule := range rules {
		if rule.ListenPort == cfgListenPort {
			if !cfgUpdateMode {
				log.Printf("found exist port proxy rule: %q\n", rule.String())
				return
			} else {
				log.Printf("deleting exist port proxy rule: %q\n", rule.String())
				// update mode and rule exist, delete old one
				if !proxy.DeleteProxyRule(rule) {
					log.Printf("delete rule failed, check permission\n")
					return
				}
			}
		}
	}

	// add new rule
	newRule := proxy.Rule{
		Direction:      "v4tov4",
		ListenAddress:  "0.0.0.0",
		ListenPort:     cfgListenPort,
		ConnectAddress: cfgRemoteHost,
		ConnectPort:    cfgRemotePort,
	}
	if !proxy.AddProxyRule(newRule) {
		log.Printf("add rule failed, check permission\n")
		return
	}

	// check firewall rule
	log.Println("check if firewall rule exist ...")
	if proxy.IsFirewallRuleExist(cfgListenPort) {
		log.Println("firewall rule exist, check if rule is enabled ...")
		if !proxy.IsFirewallRuleEnabled(cfgListenPort) {
			log.Println("firewall rule disabled, try to enable it ...")
			err = proxy.SetFirewallRuleEnabled(cfgListenPort)
			if err != nil {
				log.Println("set firewall rule enable failed: ", err)
				return
			} else {
				log.Println("enable firewall rule success")
			}
		} else {
			log.Println("firewall rule already enabled")
		}
	} else {
		log.Println("firewall rule not exist, add new rule ...")
		err = proxy.AddFirewallRule(cfgListenPort)
		if err != nil {
			log.Println("add firewall rule failed: ", err)
			return
		} else {
			log.Println("add firewall rule success")
		}
	}

	log.Printf("add port proxy success: %q", newRule.String())
}

var rootCmd = &cobra.Command{
	Use:   "win-port-proxy",
	Short: "A small utility to add Windows port proxy rule.",
	Run: func(cmd *cobra.Command, args []string) {
		elevate.Run(cmd, args, entryPoint)
	},
}

func Execute() {
	_ = rootCmd.Execute()
}
