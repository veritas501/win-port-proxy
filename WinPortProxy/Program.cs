using CommandLine;
using System;
using System.Collections.Generic;

namespace WinPortProxy
{
    internal class Program
    {
        public class Options
        {
            [Option('u', Required = false, HelpText = "Update mode")]
            public bool updateMode { get; set; }

            [Option("rhost", Required = true, HelpText = "remote host")]
            public string rhost { get; set; }

            [Option("rport", Required = true, HelpText = "remote port")]
            public int rport { get; set; }

            [Option("lport", Required = true, HelpText = "listen host")]
            public int lport { get; set; }
        }

        private static void Main(string[] args)
        {
            args = ElevateHelper.AutoElevate(args);

            string remoteHost = "";
            int remotePort = 0;
            int listenPort = 0;
            bool updateMode = false;
            bool parseFailed = false;

            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(o =>
                   {
                       remoteHost = o.rhost;
                       remotePort = o.rport;
                       listenPort = o.lport;
                       updateMode = o.updateMode;
                   }).WithNotParsed(o =>
                   {
                       parseFailed = true;

                   });
            if (parseFailed)
            {
                return;
            }

            // if ip == "wsl", try to resolve the true ip
            if (remoteHost.ToLower() == "wsl")
            {
                remoteHost = ProxyHelper.GetWSLIP();
                if (string.IsNullOrEmpty(remoteHost))
                {
                    Console.WriteLine(string.Format("[-] resolve wsl ip failed"));
                    return;
                }
            }

            // try to connect,
            if (!ProxyHelper.CheckConnection(remoteHost, remotePort))
            {
                Console.WriteLine(string.Format("[-] Can't connect to {0}:{1}", remoteHost, remotePort));
                return;
            }

            List<ProxyRule> rules = ProxyHelper.GetRules();
            foreach (ProxyRule rule in rules)
            {
                // 发现与已有端口重复
                if (rule.ListenPort == listenPort)
                {
                    // 如果是update 模式，尝试删除后创建
                    if (updateMode)
                    {
                        Console.WriteLine(string.Format("[!] Deleting exist portproxy: {0}", rule.ToShortString()));
                        if (!ProxyHelper.DeleteRule(rule))
                        {
                            Console.WriteLine(string.Format("[-] Delete rule failed, check permission"));
                            return;
                        }
                    }
                    // 否者报错
                    else
                    {
                        Console.WriteLine(string.Format("[-] Find exist portproxy: {0}", rule.ToShortString()));
                        return;
                    }
                }
            }

            // 添加新规则
            ProxyRule newRule = new ProxyRule(
                "v4tov4",
                "0.0.0.0",
                listenPort,
                remoteHost,
                remotePort
            );

            if (!ProxyHelper.AddRule(newRule))
            {
                Console.WriteLine(string.Format("[-] Add rule failed, check permission"));
                return;
            }

            // 检查防火墙规则
            if (!ProxyHelper.isFirewallRuleExist(listenPort))
            {
                // 添加防火墙规则
                if (!ProxyHelper.AddFirewallRule(listenPort))
                {
                    Console.WriteLine(string.Format("[-] Add firewall rule failed, check permission"));
                    return;
                }
            }
            Console.WriteLine(string.Format("[+] Add port proxy success."));
            return;
        }
    }
}
