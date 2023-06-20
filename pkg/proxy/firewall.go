package proxy

import (
	"fmt"
	"os/exec"
)

func IsFirewallRuleExist(port int) (exist bool) {
	cmdline := fmt.Sprintf(`if (!((New-Object -ComObject HNetCfg.FwPolicy2).rules | `+
		`Where-Object { $_.Name -EQ "Port %d opened by WinPortProxy" -and $_.LocalPorts -EQ %d} | `+
		`Select-Object -first 1)) {Exit 1} else {Exit 0}`, port, port)

	cmd := exec.Command("powershell", cmdline)
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

func IsFirewallRuleEnabled(port int) (enabled bool) {
	cmdline := fmt.Sprintf(`if (!((New-Object -ComObject HNetCfg.FwPolicy2).rules | `+
		`Where-Object { $_.Name -EQ "Port %d opened by WinPortProxy" -and $_.LocalPorts -EQ %d -and $_.Enabled} | `+
		`Select-Object -first 1)) {Exit 1} else {Exit 0}`, port, port)

	cmd := exec.Command("powershell", cmdline)
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

func SetFirewallRuleEnabled(port int) (err error) {
	cmdline := fmt.Sprintf(`(New-Object -ComObject HNetCfg.FwPolicy2).rules | `+
		`Where-Object { $_.Name -EQ "Port %d opened by WinPortProxy" -and $_.LocalPorts -EQ %d} | `+
		`Select-Object -first 1 | ForEach-Object { if ( !$_.Enabled ) {  $_.Enabled = $true }}`, port, port)

	cmd := exec.Command("powershell", cmdline)
	if err = cmd.Run(); err != nil {
		return
	}

	return nil
}

func AddFirewallRule(port int) (err error) {
	cmdline := fmt.Sprintf(
		"set-variable -name NET_FW_IP_PROTOCOL_TCP -value 6 -option constant;"+
			"set-variable -name NET_FW_ACTION_ALLOW -value 1 -option constant;"+
			"set-variable -name NET_FW_RULE_DIR_IN -value 1 -option constant;"+
			"$fwPolicy2 = new-object -comobject HNetCfg.FwPolicy2;"+
			"$RulesObject = $fwPolicy2.Rules;"+
			"$NewRule = new-object -comobject HNetCfg.FWRule;"+
			"$NewRule.Enabled = $True;"+
			"$NewRule.Action = $NET_FW_ACTION_ALLOW;"+
			"$NewRule.Protocol = $NET_FW_IP_PROTOCOL_TCP;"+
			"$NewRule.LocalPorts = %d;"+
			"$NewRule.Name = \"Port %d opened by WinPortProxy\";"+
			"$NewRule.Direction = $NET_FW_RULE_DIR_IN;"+
			"$RulesObject.Add($NewRule)",
		port, port,
	)

	cmd := exec.Command("powershell", cmdline)
	if err = cmd.Run(); err != nil {
		return
	}

	return nil
}
