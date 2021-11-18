/**
 * Code copied from https://github.com/winsw/winsw
 */

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;

namespace WinPortProxy
{
    class ElevateHelper
    {
        public static string[] AutoElevate(string[] args)
        {
            bool elevated;
            if (args.Length > 0 && args[0] == "--elevated")
            {
                elevated = true;

                ConsoleApis.FreeConsole();
                ConsoleApis.AttachConsole(ConsoleApis.ATTACH_PARENT_PROCESS);

                string stdinName = args[1];
                string stdoutName = args[2];
                string stderrName = args[3];
                var stdin = new NamedPipeClientStream(".", stdinName, PipeDirection.In, PipeOptions.Asynchronous);
                stdin.Connect();
                Console.SetIn(new StreamReader(stdin));
                var stdout = new NamedPipeClientStream(".", stdoutName, PipeDirection.Out, PipeOptions.Asynchronous);
                stdout.Connect();
                Console.SetOut(new StreamWriter(stdout) { AutoFlush = true });
                var stderr = new NamedPipeClientStream(".", stderrName, PipeDirection.Out, PipeOptions.Asynchronous);
                stderr.Connect();
                Console.SetError(new StreamWriter(stderr) { AutoFlush = true });

                string[] oldArgs = args;
                int newLength = oldArgs.Length - 4;
                args = new string[newLength];
                Array.Copy(oldArgs, 4, args, 0, newLength);
            }
            else if (Environment.OSVersion.Version.Major == 5)
            {
                // Windows XP
                elevated = true;
            }
            else
            {
                elevated = IsProcessElevated();
            }

            if (elevated)
            {

                return args;
            }
            else
            {
                Elevate();
            }

            // should not be here
            return new string[0];
        }

        private static string ExecutablePath
        {
            get
            {
                var current = Process.GetCurrentProcess();
                return current.MainModule.FileName;
            }
        }

        static bool IsProcessElevated()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void Elevate()
        {
            string stdinName = Guid.NewGuid().ToString();
            string stdoutName = Guid.NewGuid().ToString();
            string stderrName = Guid.NewGuid().ToString();

            string exe = Environment.GetCommandLineArgs()[0];
            string commandLine = Environment.CommandLine;
            string arguments = "--elevated" +
                " " + stdinName +
                " " + stdoutName +
                " " + stderrName +
                commandLine.Remove(commandLine.IndexOf(exe), exe.Length).TrimStart('"');

            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                Verb = "runas",
                FileName = ExecutablePath,
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            try
            {
                Process elevated_process = Process.Start(startInfo);
                var stdin = new NamedPipeServerStream(stdinName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                stdin.WaitForConnectionAsync().ContinueWith(_ => Console.OpenStandardInput().CopyToAsync(stdin));
                var stdout = new NamedPipeServerStream(stdoutName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                stdout.WaitForConnectionAsync().ContinueWith(_ => stdout.CopyToAsync(Console.OpenStandardOutput()));
                var stderr = new NamedPipeServerStream(stderrName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                stderr.WaitForConnectionAsync().ContinueWith(_ => stderr.CopyToAsync(Console.OpenStandardError()));
                elevated_process.WaitForExit();
                Environment.Exit(elevated_process.ExitCode);
            }
            catch (Win32Exception e) when (e.NativeErrorCode == 1223) // ERROR_CANCELLED
            {
                Environment.Exit(e.ErrorCode);
            }
        }
    }
}
