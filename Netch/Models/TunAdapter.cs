﻿using Netch.Utils;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace Netch.Models
{
    public class TunAdapter : IAdapter
    {
        private const string ComponentIdWintun = "wintun";

        public TunAdapter()
        {
            AdapterId = AdapterUtils.GetAdapterId(ComponentIdWintun) ?? throw new Exception("wintun adapter not found");

            NetworkInterface = NetworkInterface.GetAllNetworkInterfaces().First(i => i.Id == AdapterId);
            InterfaceIndex = NetworkInterface.GetIPProperties().GetIPv4Properties().Index;
            Logging.Info($"TAP 适配器：{NetworkInterface.Name} {NetworkInterface.Id} {NetworkInterface.Description}, index: {InterfaceIndex}");
        }

        public string AdapterId { get; set; }

        public int InterfaceIndex { get; }

        public IPAddress Gateway { get; } = IPAddress.Parse("100.64.0.1");

        public NetworkInterface NetworkInterface { get; }
    }
}