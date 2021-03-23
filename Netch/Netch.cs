﻿using Netch.Controllers;
using Netch.Forms;
using Netch.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vanara.PInvoke;
using static Vanara.PInvoke.User32;

namespace Netch
{
    public static class Netch
    {
        private static readonly Stopwatch Stopwatch = new();

        public static void StartStopwatch(string name)
        {
            if (Stopwatch.IsRunning)
                throw new Exception();

            Stopwatch.Start();
            Console.WriteLine($"Start {name} Stopwatch");
        }

        public static void TimePoint(string name, bool restart = true)
        {
            if (!Stopwatch.IsRunning)
                throw new Exception();

            Stopwatch.Stop();
            Console.WriteLine($"{name} Stopwatch: {Stopwatch.ElapsedMilliseconds}");
            if (restart)
                Stopwatch.Restart();
        }
        /// <summary>
        ///     应用程序的主入口点
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
#if DEBUG
            AttachConsole();
#else
            if (args.Contains("-console"))
                AttachConsole();
#endif

            StartStopwatch("Netch");

            // 设置当前目录
            Directory.SetCurrentDirectory(Global.NetchDir);
            Environment.SetEnvironmentVariable("PATH",
                Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process) + ";" + Path.Combine(Global.NetchDir, "bin"),
                EnvironmentVariableTarget.Process);

            Updater.Updater.CleanOld(Global.NetchDir);

            // 预创建目录
            var directories = new[] { "mode\\Custom", "data", "i18n", "logging" };
            foreach (var item in directories)
                if (!Directory.Exists(item))
                    Directory.CreateDirectory(item);

            TimePoint("Clean Old, Create Directory");

            // 加载配置
            Configuration.Load();

            TimePoint("Load Configuration");

            // 检查是否已经运行
            if (!Global.Mutex.WaitOne(0, false))
            {
                ShowOpened();

                // 退出进程
                Environment.Exit(1);
            }

            // 清理上一次的日志文件，防止淤积占用磁盘空间
            if (Directory.Exists("logging"))
            {
                var directory = new DirectoryInfo("logging");

                foreach (var file in directory.GetFiles())
                    file.Delete();

                foreach (var dir in directory.GetDirectories())
                    dir.Delete(true);
            }

            // 加载语言
            i18N.Load(Global.Settings.Language);

            if (!Directory.Exists("bin") || !Directory.EnumerateFileSystemEntries("bin").Any())
            {
                MessageBoxX.Show(i18N.Translate("Please extract all files then run the program!"));
                Environment.Exit(2);
            }

            Logging.Info($"版本: {UpdateChecker.Owner}/{UpdateChecker.Repo}@{UpdateChecker.Version}");
            Task.Run(() => { Logging.Info($"主程序 SHA256: {Utils.Utils.SHA256CheckSum(Global.NetchExecutable)}"); });

            TimePoint("Get Info, Pre-Form");

            // 绑定错误捕获
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_OnException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(Global.MainForm);
        }

        private static void AttachConsole()
        {
            if (!NativeMethods.AttachConsole(-1))
                NativeMethods.AllocConsole();
        }

        public static void Application_OnException(object sender, ThreadExceptionEventArgs e)
        {
            Logging.Error(e.Exception.ToString());
            Utils.Utils.Open(Logging.LogFile);
        }

        private static void ShowOpened()
        {
            static HWND GetWindowHandleByPidAndTitle(int process, string title)
            {
                var sb = new StringBuilder(256);
                HWND pLast = IntPtr.Zero;
                do
                {
                    pLast = FindWindowEx(HWND.NULL, pLast, null, null);
                    GetWindowThreadProcessId(pLast, out var id);
                    if (id != process)
                        continue;

                    if (GetWindowText(pLast, sb, sb.Capacity) <= 0)
                        continue;

                    if (sb.ToString().Equals(title))
                        return pLast;
                } while (pLast != IntPtr.Zero);

                return HWND.NULL;
            }

            var self = Process.GetCurrentProcess();
            var activeProcess = Process.GetProcessesByName("Netch").Single(p => p.Id != self.Id);
            HWND handle = activeProcess.MainWindowHandle;
            if (handle.IsNull)
                handle = GetWindowHandleByPidAndTitle(activeProcess.Id, "Netch");

            if (handle.IsNull)
                return;

            ShowWindow(handle, ShowWindowCommand.SW_NORMAL);
            SwitchToThisWindow(handle, true);
        }
    }
}