using Netch.Controllers;
using Netch.Models;
using Netch.Servers.Shadowsocks;
using Netch.Servers.Socks5;
using System;
using System.IO;
using System.Linq;

namespace Netch.Utils
{
    public static class ModeHelper
    {
        private const string MODE_DIR = "mode";
        public const string DISABLE_MODE_DIRECTORY_FILENAME = "disabled";

        public static readonly string ModeDirectory = Path.Combine(Global.NetchDir, $"{MODE_DIR}\\");

        public static string GetRelativePath(string fullName)
        {
            return fullName.Substring(ModeDirectory.Length);
        }

        public static string GetFullPath(string relativeName)
        {
            return Path.Combine(ModeDirectory, relativeName);
        }

        /// <summary>
        ///     从模式文件夹读取模式
        /// </summary>
        public static void Load()
        {
            Global.Modes.Clear();
            LoadModeDirectory(ModeDirectory);

            Sort();
        }

        private static void LoadModeDirectory(string modeDirectory)
        {
            try
            {
                foreach (var directory in Directory.GetDirectories(modeDirectory))
                    LoadModeDirectory(directory);

                // skip Directory with a disabled file in
                if (File.Exists(Path.Combine(modeDirectory, DISABLE_MODE_DIRECTORY_FILENAME)))
                    return;

                foreach (var file in Directory.GetFiles(modeDirectory).Where(f => f.EndsWith(".txt")))
                    LoadModeFile(file);
            }
            catch
            {
                // ignored
            }
        }

        private static void LoadModeFile(string fullName)
        {
            var mode = new Mode(fullName);

            var content = File.ReadAllLines(fullName);
            if (content.Length == 0)
                return;

            for (var i = 0; i < content.Length; i++)
            {
                var text = content[i].Trim();

                if (i == 0)
                {
                    if (text.First() != '#')
                        return;

                    try
                    {
                        var splited = text.Substring(1).SplitTrimEntries(',');

                        mode.Remark = splited[0];

                        var typeResult = int.TryParse(splited.ElementAtOrDefault(1), out var type);
                        mode.Type = typeResult ? type : 0;

                        var bypassChinaResult = int.TryParse(splited.ElementAtOrDefault(2), out var bypassChina);
                        mode.BypassChina = mode.ClientRouting() && bypassChinaResult && bypassChina == 1;
                    }
                    catch
                    {
                        return;
                    }
                }
                else
                {
                    mode.Rule.Add(text);
                }
            }

            Global.Modes.Add(mode);
        }

        private static void Sort()
        {
            Global.Modes.Sort((a, b) => string.Compare(a.Remark, b.Remark, StringComparison.Ordinal));
        }

        public static void Add(Mode mode)
        {
            Global.Modes.Add(mode);
            Sort();
            Global.MainForm.LoadModes();
        }

        public static void Delete(Mode mode)
        {
            if (mode.FullName == null)
                throw new ArgumentException("FullName");

            if (File.Exists(mode.FullName))
                File.Delete(mode.FullName);

            Global.Modes.Remove(mode);
            Global.MainForm.LoadModes();
        }

        public static bool SkipServerController(Server server, Mode mode)
        {
            return mode.Type switch
            {
                0 => server switch
                {
                    Socks5 => true,
                    Shadowsocks shadowsocks when !shadowsocks.HasPlugin() && Global.Settings.RedirectorSS => true,
                    _ => false
                },
                _ => false
            };
        }

        public static IModeController? GetModeControllerByType(int type, out ushort? port, out string portName, out PortType portType)
        {
            port = null;
            portName = string.Empty;
            portType = PortType.Both;
            switch (type)
            {
                case 0:
                    port = Global.Settings.RedirectorTCPPort;
                    portName = "Redirector TCP";
                    portType = PortType.TCP;
                    return new NFController();
                case 1:
                case 2:
                    return new TUNTAPController();
                case 3:
                case 5:
                    port = Global.Settings.HTTPLocalPort;
                    portName = "HTTP";
                    portType = PortType.TCP;
                    StatusPortInfoText.HttpPort = (ushort)port;
                    return new HTTPController();
                case 4:
                    return null;
                case 6:
                    return new PcapController();
                default:
                    Logging.Error("未知模式类型");
                    throw new MessageException("未知模式类型");
            }
        }
    }
}