﻿using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Modules.Games.WoW.Models;
using Newtonsoft.Json.Linq;

namespace Artemis.Modules.Games.WoW
{
    public class WoWModel : ModuleModel
    {
        private readonly WowPacketScanner _packetScanner;

        public WoWModel(DeviceManager deviceManager, LuaManager luaManager, WowPacketScanner packetScanner) : base(deviceManager, luaManager)
        {
            Settings = SettingsProvider.Load<WoWSettings>();
            DataModel = new WoWDataModel();
            ProcessNames.Add("Wow-64");

            _packetScanner = packetScanner;
            _packetScanner.RaiseDataReceived += (sender, args) => HandleGameData(args.Command, args.Data);
        }

        public override string Name => "WoW";
        public override bool IsOverlay => false;
        public override bool IsBoundToProcess => true;

        public override void Enable()
        {
            _packetScanner.Start();
            base.Enable();
        }

        public override void Dispose()
        {
            _packetScanner.Stop();
            base.Dispose();
        }

        public override void Update()
        {
            var dataModel = (WoWDataModel) DataModel;

            dataModel.Player.Update();
            dataModel.Target.Update();
        }

        private void HandleGameData(string command, string data)
        {
            JToken json = null;
            if (!data.StartsWith("\"") && !data.EndsWith("\""))
                json = JToken.Parse(data);

            lock (DataModel)
            {
                var dataModel = (WoWDataModel) DataModel;
                switch (command)
                {
                    case "gameState":
                        ParseGameState(data, dataModel);
                        break;
                    case "player":
                        ParsePlayer(json, dataModel);
                        break;
                    case "target":
                        ParseTarget(json, dataModel);
                        break;
                    case "playerState":
                        ParsePlayerState(json, dataModel);
                        break;
                    case "targetState":
                        ParseTargetState(json, dataModel);
                        break;
                    case "buffs":
                        ParseAuras(json, dataModel, true);
                        break;
                    case "debuffs":
                        ParseAuras(json, dataModel, false);
                        break;
                    case "spellCast":
                        ParseSpellCast(json, dataModel, false);
                        break;
                    case "instantSpellCast":
                        ParseInstantSpellCast(json, dataModel);
                        break;
                    case "spellCastFailed":
                        ParseSpellCastFailed(data, dataModel);
                        break;
                    case "spellCastInterrupted":
                        ParseSpellCastInterrupted(data, dataModel);
                        break;
                    case "spellChannel":
                        ParseSpellCast(json, dataModel, true);
                        break;
                    case "spellChannelInterrupted":
                        ParseSpellCastInterrupted(data, dataModel);
                        break;
                    default:
                        Logger.Warn("The WoW addon sent an unknown command: {0}", command);
                        break;
                }
            }
        }

        private void ParseGameState(string data, WoWDataModel dataModel)
        {
            if (data == "\"ingame\"")
                dataModel.State = WoWState.Ingame;
            else if (data == "\"afk\"")
                dataModel.State = WoWState.Afk;
            else if (data == "\"dnd\"")
                dataModel.State = WoWState.Dnd;
            else if (data == "\"loggedOut\"")
                dataModel.State = WoWState.LoggedOut;
        }

        private void ParsePlayer(JToken json, WoWDataModel dataModel)
        {
            dataModel.Player.ApplyJson(json);

            // At this point class/race data is available so no point pretending to be logged out
            if (dataModel.State == WoWState.LoggedOut)
                dataModel.State = WoWState.Ingame;
        }

        private void ParseTarget(JToken json, WoWDataModel dataModel)
        {
            dataModel.Target.ApplyJson(json);
        }

        private void ParsePlayerState(JToken json, WoWDataModel dataModel)
        {
            dataModel.Player.ApplyStateJson(json);
        }

        private void ParseTargetState(JToken json, WoWDataModel dataModel)
        {
            dataModel.Target.ApplyStateJson(json);
        }

        private void ParseAuras(JToken json, WoWDataModel dataModel, bool buffs)
        {
            dataModel.Player.ApplyAuraJson(json, buffs);
        }

        private void ParseSpellCast(JToken json, WoWDataModel dataModel, bool isChannel)
        {
            if (json["uid"].Value<string>() == "player")
                dataModel.Player.CastBar.ApplyJson(json, isChannel);
            else if (json["uid"].Value<string>() == "target")
                dataModel.Target.CastBar.ApplyJson(json, isChannel);
        }

        private void ParseInstantSpellCast(JToken json, WoWDataModel dataModel)
        {
            var spell = new WoWSpell
            {
                Name = json["n"].Value<string>(),
                Id = json["sid"].Value<int>()
            };

            if (json["uid"].Value<string>() == "player")
                dataModel.Player.AddInstantCast(spell);
            else if (json["uid"].Value<string>() == "target")
                dataModel.Target.AddInstantCast(spell);
        }

        private void ParseSpellCastFailed(string data, WoWDataModel dataModel)
        {
            if (data == "\"player\"")
                dataModel.Player.CastBar.Clear();
            else if (data == "\"target\"")
                dataModel.Target.CastBar.Clear();
        }

        private void ParseSpellCastInterrupted(string data, WoWDataModel dataModel)
        {
            if (data == "\"player\"")
                dataModel.Player.CastBar.Clear();
            else if (data == "\"target\"")
                dataModel.Target.CastBar.Clear();
        }
    }
}
