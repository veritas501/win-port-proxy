using NetFwTypeLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace WinPortProxy
{
    internal class ProxyHelper
    {
        private static string[] directions = new string[] { "v4tov4", "v6tov4", "v4tov6", "v6tov6" };
        private static Regex regex = new Regex(@"^(\S+)\s+(\d+)\s+(\S+)\s+(\d+)\s*$", RegexOptions.ECMAScript | RegexOptions.Multiline);

        // 检查连接是否可达
        public static bool CheckConnection(string host, int port)
        {
            Socket socket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp
                    );
            try
            {
                socket.Connect(host, port);
                if (socket.Connected)
                {
                    socket.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        // 自动获取wsl的ip
        public static string GetWSLIP()
        {
            try
            {
                ExecResult result = ExecCommand("wsl", "hostname -I");
                return result.output.Trim(new char[] { '\r', '\n', ' ' });
            }
            catch
            {
                return "";
            }
        }

        // 执行命令
        public static ExecResult ExecCommand(string cmd, string args)
        {
            Process p = new Process();
            p.StartInfo.FileName = cmd;
            p.StartInfo.Arguments = args;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.Start();
            string outstr = p.StandardOutput.ReadToEnd();
            string errstr = p.StandardError.ReadToEnd();
            p.WaitForExit();
            int exitCode = p.ExitCode;
            ExecResult result = new ExecResult(exitCode, outstr, errstr);
            return result;
        }

        public static bool AddRule(ProxyRule rule)
        {
            ExecResult result = ExecCommand("netsh", "interface portproxy add " + rule.ToString());
            return result.code == 0;
        }

        public static bool DeleteRule(ProxyRule rule)
        {
            ExecResult result = ExecCommand("netsh", "interface portproxy delete " + rule.ToShortString());
            return result.code == 0;
        }

        public static bool SetRule(ProxyRule rule)
        {
            ExecResult result = ExecCommand("netsh", "interface portproxy set " + rule.ToString());
            return result.code == 0;
        }

        public static bool ResetRule()
        {
            ExecResult result = ExecCommand("netsh", "interface portproxy reset");
            return result.code == 0;
        }

        public static List<ProxyRule> GetRules()
        {
            List<ProxyRule> rules = new List<ProxyRule>();
            foreach (string direction in directions)
            {
                ExecResult result = ExecCommand("netsh", "interface portproxy show " + direction);
                Match m = regex.Match(result.output);
                while (m.Success)
                {
                    ProxyRule rule = new ProxyRule(
                        direction,
                        m.Groups[1].Captures[0].Value,
                        int.Parse(m.Groups[2].Captures[0].Value),
                        m.Groups[3].Captures[0].Value,
                        int.Parse(m.Groups[4].Captures[0].Value)
                        );
                    rules.Add(rule);
                    m = m.NextMatch();
                }
            }
            return rules;
        }

        public static void GetFirewallRules()
        {
            Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            dynamic fwPolicy2 = Activator.CreateInstance(tNetFwPolicy2);
            IEnumerable Rules = fwPolicy2.Rules as IEnumerable;
            foreach (dynamic rule in Rules)
            {
                if (!string.IsNullOrEmpty(rule.Name) && !string.IsNullOrEmpty(rule.LocalPorts))
                {
                    Console.WriteLine(string.Format("{0} => {1}", rule.Name, rule.LocalPorts));
                }
            }
        }

        public static bool isFirewallRuleExist(int port)
        {
            Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            dynamic fwPolicy2 = Activator.CreateInstance(tNetFwPolicy2);
            IEnumerable Rules = fwPolicy2.Rules as IEnumerable;
            foreach (dynamic rule in Rules)
            {
                if (!string.IsNullOrEmpty(rule.Name) && !string.IsNullOrEmpty(rule.LocalPorts))
                {
                    if (rule.Name == string.Format("Port {0} opened by WinPortProxy", port) && rule.LocalPorts == port.ToString())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool AddFirewallRule(int port)
        {
            Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            dynamic fwPolicy2 = Activator.CreateInstance(tNetFwPolicy2);
            dynamic newRule = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwRule"));
            newRule.Enabled = true;
            newRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            newRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
            // NOTE: Must do this after setting the Protocol!
            newRule.LocalPorts = port.ToString();
            newRule.Name = string.Format("Port {0} opened by WinPortProxy", port);
            //newRule.Description = "WinPortProxy rule";
            newRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            try
            {
                fwPolicy2.Rules.Add(newRule);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
