# win-port-proxy

添加windows端口转发的小工具：

```
> win-port-proxy.exe
Error: required flag(s) "lport", "rhost", "rport" not set
Usage:
  win-port-proxy [flags]
  win-port-proxy [command]

Available Commands:
  help        Help about any command
  version

Flags:
  -h, --help           help for win-port-proxy
      --lport int      listen port (required)
      --nocheck        don't check connection
      --rhost string   remote host or "wsl" for wsl host (required)
      --rport int      remote port (required)
  -u, --update         update mode, override existed rule

Use "win-port-proxy [command] --help" for more information about a command.
```

例如将本机的22端口转发到wsl2的22端口：

```
> win-port-proxy.exe -u --rhost wsl --rport 22 --lport 22
[*] resolving wsl ip ...
[*] get wsl ip: 172.19.129.225
[*] check tcp connection to 172.19.129.225:22 ...
[*] tcp connection ok
[*] deleting exist port proxy rule: "0.0.0.0:22 -> 172.19.129.225:22"
[*] check if firewall rule exist ...
[*] firewall rule exist, check if rule is enabled ...
[*] firewall rule already enabled
[*] add port proxy success: "0.0.0.0:22 -> 172.19.129.225:22"
```

