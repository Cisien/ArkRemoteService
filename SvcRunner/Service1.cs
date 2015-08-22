using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;

namespace SvcRunner
{
    public enum LogSource
    {
        Error,
        Output,
        Log
    }

    public partial class Service1 : ServiceBase
    {
        private Process _runningProcess;

        private readonly StreamWriter _outWriter = new StreamWriter($"d:\\logs\\out-{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-tt")}.log", true);
        private const string ArkPath = @"D:\ArkServerLauncher\ArkRemote.exe";
        private readonly int minuteMs = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var command = ArkPath;
            var workDir = Path.GetDirectoryName(ArkPath);
            var psi = new ProcessStartInfo
            {
                FileName = command,
                WorkingDirectory = workDir,
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            };

            _runningProcess = Process.Start(psi);
            _runningProcess.OutputDataReceived += OutputDataReceived;
            _runningProcess.ErrorDataReceived += ErrorDataReceived;

            _runningProcess.BeginErrorReadLine();
            _runningProcess.BeginOutputReadLine();
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            WriteLogEntry(LogSource.Output, e.Data);
        }

        private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            WriteLogEntry(LogSource.Error, e.Data);
        }

        private void WriteLogEntry(LogSource source, string entry)
        {
            _outWriter.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-tt")}: {source}: {entry}");
            _outWriter.Flush();
        }

        protected override void OnStop()
        {
            WriteLogEntry(LogSource.Log, "Received stop request.");
            
            TerminateProcess(_runningProcess);
            
            WriteLogEntry(LogSource.Log, "Searching for running server instances");
            var processes = Process.GetProcessesByName("ShooterGameServer");
            WriteLogEntry(LogSource.Log, $"Found {processes.Length} server processes");

            foreach (var process in processes)
            {
                TerminateProcess(process);
            }

            _outWriter.Flush();
            _outWriter.Close();
            _outWriter.Dispose();

        }

        private void TerminateProcess(Process process)
        {
            WriteLogEntry(LogSource.Log, $"Asking process with name {process.ProcessName} to exit.");

            if (AttachConsole((uint)process.Id))
            {
                SetConsoleCtrlHandler(null, true);
                try
                {
                    if (GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0))
                    {
                        if (process.WaitForExit(minuteMs))
                        {
                            WriteLogEntry(LogSource.Log, $"Process with name {process.ProcessName} exited.");
                            return;
                        }
                        else
                        {
                            WriteLogEntry(LogSource.Log, $"Process with name {process.ProcessName} didn't respond to Ctrl+C, killing instead.");
                            process.Kill();
                        }
                    } 
                    else 
                    {
                        WriteLogEntry(LogSource.Log, $"Process with name {process.ProcessName} failed to generate a Ctrl+C event, killing instead.");
                        process.Kill();
                    }
                }
                finally
                {
                    FreeConsole();
                    SetConsoleCtrlHandler(null, false);
                }
            }

            process.WaitForExit();

            WriteLogEntry(LogSource.Log, $"Process with name {process.ProcessName} killed.");
        }

        internal const int CTRL_C_EVENT = 0;
        [DllImport("kernel32.dll")]
        internal static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AttachConsole(uint dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool FreeConsole();
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
        // Delegate type to be used as the Handler Routine for SCCH
        delegate bool ConsoleCtrlDelegate(uint CtrlType);
    }
}
