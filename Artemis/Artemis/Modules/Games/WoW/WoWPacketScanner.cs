﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Services;
using Ninject.Extensions.Logging;
using PcapDotNet.Core;
using PcapDotNet.Packets;

namespace Artemis.Modules.Games.WoW
{
    public class WowPacketScanner
    {
        private const string MsgStart = "\u0001";
        private const string MsgNext = "\u0002";
        private const string MsgLast = "\u0003";
        private readonly MetroDialogService _dialogService;
        private PacketCommunicator _communicator;
        private string _dataParts;

        public WowPacketScanner(ILogger logger, MetroDialogService dialogService)
        {
            _dialogService = dialogService;
            Logger = logger;
        }

        public ILogger Logger { get; }

        public event EventHandler<WowDataReceivedEventArgs> RaiseDataReceived;

        public void Start(string networkAdapter)
        {
            // Start scanning WoW packets
            // Retrieve the device list from the local machine
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

            if (allDevices.Count == 0)
            {
                Logger.Warn("No interfaces found! Can't scan WoW packets.");
                return;
            }

            // Take the configured network adapter
            // TODO: Create UI with available network adapters dropdown
            PacketDevice selectedDevice = null;
            if (!string.IsNullOrEmpty(networkAdapter))
                selectedDevice = allDevices.FirstOrDefault(d => d.Name.Contains(networkAdapter));
            // If not found take the first active network adapter
            if (selectedDevice == null)
                selectedDevice = allDevices.First(d => d.Addresses.Any());

            // Open the device
            try
            {
                _communicator = selectedDevice.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 40);
                Logger.Debug("Listening on " + selectedDevice.Description + " for WoW packets");

                // Compile the filter
                using (var filter = _communicator.CreateFilter("tcp"))
                {
                    // Set the filter
                    _communicator.SetFilter(filter);
                }
            }
            catch (Exception e)
            {
                _dialogService.ShowErrorMessageBox("Cannot start the WoW module, maybe an antivirus is interfering.\n\n" +
                                                   $"Try whitelisting '{AppDomain.CurrentDomain.BaseDirectory}' in your virus " +
                                                   "scanner and reinstalling Artemis.");
                Logger.Warn(e, "Cannot start the WoW module, maybe an antivirus is interfering.");
            }


            Task.Run(() => ReceivePackets());
        }

        public void Stop()
        {
            try
            {
                _communicator?.Break();
                _communicator?.Dispose();
                _communicator = null;
            }
            catch (Exception e)
            {
                _dialogService.ShowErrorMessageBox("Cannot start the WoW module, maybe an antivirus is interfering.\n\n" +
                                                   $"Try whitelisting '{AppDomain.CurrentDomain.BaseDirectory}' in your virus scanner " +
                                                   "and reinstalling Artemis.");
                Logger.Warn(e, "Cannot start the WoW module, maybe an antivirus is interfering.");
            }
        }

        private void ReceivePackets()
        {
            // start the capture
            try
            {
                _communicator.ReceivePackets(0, PacketHandler);
            }
            catch (InvalidOperationException)
            {
                // ignored, happens on shutdown
            }
        }

        // Callback function invoked by Pcap.Net for every incoming packet
        private void PacketHandler(Packet packet)
        {
            var str = Encoding.Default.GetString(packet.Buffer);
            if (!str.ToLower().Contains("artemis"))
                return;

            // Split the string at the prefix
            var parts = str.Split(new[] {"(artemis)"}, StringSplitOptions.None);
            if (parts.Length < 2)
                return;
            var msg = parts[1];
            // Start escape char
            if (msg.StartsWith(MsgStart))
                _dataParts = msg.Substring(1);
            else if (msg.StartsWith(MsgNext))
                _dataParts = _dataParts + msg.Substring(1);
            else if (msg.StartsWith(MsgLast))
            {
                _dataParts = _dataParts + msg.Substring(1);
                var dataParts = _dataParts.Split('|');
                // Data is wrapped in artemis(), take this off
                OnRaiseDataReceived(dataParts[0].Substring(8), dataParts[1].Substring(0, dataParts[1].Length - 1));
            }
            else
            {
                var dataParts = msg.Split('|');
                // Data is wrapped in artemis(), take this off
                OnRaiseDataReceived(dataParts[0].Substring(8), dataParts[1].Substring(0, dataParts[1].Length - 1));
            }
        }

        private void OnRaiseDataReceived(string command, string data)
        {
            RaiseDataReceived?.Invoke(this, new WowDataReceivedEventArgs(command, data));
        }

        public class WowDataReceivedEventArgs : EventArgs
        {
            public WowDataReceivedEventArgs(string command, string data)
            {
                Command = command;
                Data = data;
            }

            public string Command { get; }
            public string Data { get; set; }
        }
    }
}
