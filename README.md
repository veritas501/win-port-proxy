# WinPortProxy

添加windows端口转发的小工具：

```
> WinPortProxy.exe --help
WinPortProxy 1.0.2.0
Copyright 2022

  -u           Update mode

  --rhost      Required. remote host

  --rport      Required. remote port

  --lport      Required. listen host

  --help       Display this help screen.

  --version    Display version information.
```



例如将本机的22端口转发到wsl2的22端口：

```
WinPortProxy.exe -u --rhost wsl --rport 22 --lport 22
```

