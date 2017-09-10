Artemis = LibStub("AceAddon-3.0"):NewAddon("Artemis", "AceConsole-3.0", "AceEvent-3.0", "AceTimer-3.0", "AceComm-3.0")
json = LibStub("json")
local debugging = false
local lastLine = {}
local channeling = {}
local unitUpdates = {}
local lastTransmitMessage
local lastTransmitTime
local lastBuffs = "";
local lastDebuffs = "";
local prefixCounts = {}

channeling["player"] = false
channeling["target"] = false

function Artemis:OnEnable()
    Artemis:RegisterEvent("PLAYER_ENTERING_WORLD")
    Artemis:RegisterEvent("PLAYER_LEVEL_UP")
    Artemis:RegisterEvent("ACHIEVEMENT_EARNED")
	Artemis:RegisterEvent("ACTIVE_TALENT_GROUP_CHANGED")
    Artemis:RegisterEvent("UNIT_TARGET")
    Artemis:RegisterEvent("UNIT_HEALTH")
    Artemis:RegisterEvent("UNIT_POWER")
    Artemis:RegisterEvent("UNIT_AURA")
    Artemis:RegisterEvent("UNIT_SPELLCAST_START")    
    Artemis:RegisterEvent("UNIT_SPELLCAST_SUCCEEDED")
    Artemis:RegisterEvent("UNIT_SPELLCAST_FAILED")
    Artemis:RegisterEvent("UNIT_SPELLCAST_DELAYED")
    Artemis:RegisterEvent("UNIT_SPELLCAST_CHANNEL_START")
    Artemis:RegisterEvent("UNIT_SPELLCAST_CHANNEL_STOP")
    Artemis:RegisterEvent("UNIT_SPELLCAST_CHANNEL_UPDATE")
    Artemis:RegisterEvent("UNIT_SPELLCAST_INTERRUPTED")
    Artemis:RegisterEvent("ZONE_CHANGED")
    Artemis:RegisterEvent("ZONE_CHANGED_NEW_AREA")

    Artemis:RegisterChatCommand("artemis", "HandleChatCommand")
end

function Artemis:HandleChatCommand(input)
    if input == "debug" then
        debugging = not (debugging)
        if debugging then
            Artemis:Print("Debugging enabled.")
        else
            Artemis:Print("Debugging disabled.")
        end
    end
    if input == "rc" then
        prefixCounts = {}
        Artemis:Print("Reset the send counters.")
    end
    if input == nill or input == "" or input == "help" then
        Artemis:Print("Available chat commands:")
        Artemis:Printf("|cffb7b7b7/artemis debug|r: Toggle debugging")
        Artemis:Printf("|cffb7b7b7/artemis rc|r: Reset the debug counters")
    end
end

function Artemis:Transmit(prefix, data, prio)    
    local msg = "artemis(".. prefix .. "|" .. json.encode(data) ..")"
    -- If the message is the same as the previous, make sure it wasn't sent less than 250ms ago
    if msg == lastTransmitMessage then 
        if not (lastTransmitTime == nil) then           
            local diff = GetTime() - lastTransmitTime;            
            if (diff < 0.25) then
                return
            end
        end
    end
    
    lastTransmitTime = GetTime()

    if debugging == true then
		if prefixCounts[prefix] == nill then
			prefixCounts[prefix] = 0
		end
		prefixCounts[prefix] = prefixCounts[prefix] + 1
    end
    
    if debugging == true then
        Artemis:Printf("Transmitting with prefix |cfffdff71" .. prefix .. "|r (" .. prefixCounts[prefix] .. ").")
        Artemis:Print(msg)
    end
    Artemis:SendCommMessage("(artemis)", msg, "WHISPER", UnitName("player"), prio)
end

function Artemis:TransmitUnitState(unit, ignoreThrottle)
    if not ignoreThrottle then
        if not (unitUpdates[unit] == nil) then           
            local diff = GetTime() - unitUpdates[unit]            
            if (diff < 0.5) then
                return
            end
        end
    end

    local table = {
        h = UnitHealth(unit), 
        mh = UnitHealthMax(unit), 
        p = UnitPower(unit),
        mp = UnitPowerMax(unit), 
        t = UnitPowerType(unit)        
    };

    unitUpdates[unit] = GetTime()
    Artemis:Transmit(unit .. "State", table)
end

function Artemis:GetUnitDetails(unit)
   return {
       n = UnitName(unit), 
       c = UnitClass(unit), 
       l = UnitLevel(unit),
       r = UnitRace(unit),
       g = UnitSex(unit),
       f = UnitFactionGroup(unit)
   };
end

function Artemis:GetPlayerDetails()
	local details = Artemis:GetUnitDetails("player")
    local id, name, _, _, role = GetSpecializationInfo(GetSpecialization())
	
	details.realm = GetRealmName()
    details.achievementPoints = GetTotalAchievementPoints(false)
	details.s = {id = id, n = name, r = role}	
	
	return details
end

function Artemis:GetUnitAuras(unit, filter)
    local auras = {};
    for index = 1, 40 do
         local name, _, _, count, _, duration, expires, caster, _, _, spellID = UnitAura(unit, index, filter);
         if not (name == nil) then
            local buffTable = {n = name, id = spellID}
            -- Leave these values out if they are 0 to save some space
            if count > 0 then
                buffTable["c"] = count
            end
            if duration > 0 then
                buffTable["d"] = duration
            end
            if expires > 0 then
                buffTable["e"] = expires
            end
            table.insert(auras, buffTable)
         end
    end
    return auras
end

function Artemis:PLAYER_ENTERING_WORLD(...)	
    Artemis:Transmit("player", Artemis:GetPlayerDetails())
    Artemis:TransmitUnitState("player", true);
end

function Artemis:PLAYER_LEVEL_UP(...)
    Artemis:Transmit("player", Artemis:GetPlayerDetails())
end

function Artemis:ACHIEVEMENT_EARNED(...)
    Artemis:Transmit("player", Artemis:GetPlayerDetails())
end

function Artemis:ACTIVE_TALENT_GROUP_CHANGED(...)
    Artemis:Transmit("player", Artemis:GetPlayerDetails())
end

function Artemis:UNIT_TARGET(...)
    local _, source = ...
    if not (source == "player") then
        return
    end

    local details = Artemis:GetUnitDetails("target")
    channeling["target"] = false

    Artemis:Transmit("target", details)
    Artemis:TransmitUnitState("target", true);
end

function Artemis:UNIT_HEALTH(...)
    local _, source = ...
    if not (source == "player") and not (source == "target") then
        return
    end
        
    Artemis:TransmitUnitState(source, false);
end

function Artemis:UNIT_POWER(...)
    local _, source = ...
    if not (source == "player") and not (source == "target") then
        return
    end
        
    Artemis:TransmitUnitState(source, false);
end

function Artemis:UNIT_AURA(...)
    local _, source = ...
    if not (source == "player") then
        return
    end
    
    local buffs = Artemis:GetUnitAuras(source, "PLAYER|HELPFUL")
    local debuffs = Artemis:GetUnitAuras(source, "PLAYER|HARMFUL")    
	
	local newBuffs = json.encode(buffs)
	local newDebuffs = json.encode(debuffs)
	
    if not (lastBuffs == newBuffs) then
		Artemis:Transmit("buffs", buffs)    
	end
	if not (lastDebuffs == newDebuffs) then
		Artemis:Transmit("debuffs", debuffs)    
	end

	lastBuffs = newBuffs
    lastDebuffs = newDebuffs
end

-- Detect non-instant spell casts
function Artemis:UNIT_SPELLCAST_START(...)
    local _, unitID, spell, rank, lineID, spellID = ...
    if not (unitID == "player") and not (unitID == "target") then
        return
    end
    
    local name, _, _, _, startTime, endTime, _, _, notInterruptible = UnitCastingInfo(unitID)
    local table = {uid = unitID, n = name, sid = spellID, s = startTime, e = endTime, ni = notInterruptible}
    lastLine[unitID] = lineID

    Artemis:Transmit("spellCast", table, "ALERT")
end

-- Detect instant spell casts
function Artemis:UNIT_SPELLCAST_SUCCEEDED (...)       
    local _, unitID, spell, rank, lineID, spellID = ...
    if not (unitID == "player") and not (unitID == "target") then
        return
    end    
    if channeling[unitID] == true then
        return
    end
    -- Many spells are irrelevant system spells, don't transmit these
    if unitID == "player" and not (IsPlayerSpell(spellID)) then
        return
    end

    local name, subText, text, texture, startTime, endTime, isTradeSkill, castID, notInterruptible = UnitCastingInfo(unitID)    
    -- Don't trigger on the success of a non instant cast
    if not (lastLine[unitID] == nil) and lastLine[unitID] == lineID then        
        return
    end
    
    -- Set back the last line to what is currently being cast (Fireblast during Fireball per example)
    if not (name == nil) then        
        lastLine[unitID] = castID
    else
        lastLine[unitID] = nil
    end
    
    local table = {uid = unitID, n = spell, sid = spellID}

    Artemis:Transmit("instantSpellCast", table, "ALERT")    
end

-- Detect falure of non instant casts
function Artemis:UNIT_SPELLCAST_FAILED (...)
    local source, unitID, _, _, lineID = ...
    if not (unitID == "player") and not (unitID == "target") then
        return
    end        
    if lastLine[unitID] == nil or not (lastLine[unitID] == lineID) then
        return
    end

    lastLine[unitID] = nil

    Artemis:Transmit("spellCastFailed", unitID, "ALERT")     
end

-- Detect falure of non instant casts
function Artemis:UNIT_SPELLCAST_DELAYED (...)
    local _, unitID, spell, rank, lineID, spellID = ...
    if not (unitID == "player") and not (unitID == "target") then
        return
    end
    local name, _, _, _, startTime, endTime, _, _, notInterruptible = UnitCastingInfo(unitID)
    local table = {uid = unitID, n = name, sid = spellID, s = startTime, e = endTime, ni = notInterruptible}

     Artemis:Transmit("spellCast", table, "ALERT")
end

-- Detect cancellation of non instant casts
function Artemis:UNIT_SPELLCAST_INTERRUPTED (...)
    local source, unitID, _, _, lineID = ...
    if not (unitID == "player") and not (unitID == "target") then
        return
    end
    if lastLine[unitID] == nil or not (lastLine[unitID] == lineID) then
        return
    end

    lastLine[unitID] = nil

    Artemis:Transmit("spellCastInterrupted", unitID, "ALERT")             
end

-- Detect spell channels
function Artemis:UNIT_SPELLCAST_CHANNEL_START(...)
    local _, unitID, spell, rank, lineID, spellID = ...
    if not (unitID == "player") and not (unitID == "target") then
        return
    end
    channeling[unitID] = true
    
    local name, _, _, _, startTime, endTime, _, notInterruptible = UnitChannelInfo(unitID)
    local table = {uid = unitID, n = name, sid = spellID, s = startTime, e = endTime, ni = notInterruptible}

    Artemis:Transmit("spellChannel", table, "ALERT")
end

function Artemis:UNIT_SPELLCAST_CHANNEL_UPDATE (...)
    local _, unitID, spell, rank, lineID, spellID = ...
    if not (unitID == "player") and not (unitID == "target") then
        return
    end
    local name, _, _, _, startTime, endTime, _, notInterruptible = UnitChannelInfo(unitID)
    local table = {uid = unitID, n = name, sid = spellID, s = startTime, e = endTime, ni = notInterruptible}

     Artemis:Transmit("spellChannel", table, "ALERT")
end

-- Detect cancellation of channels
function Artemis:UNIT_SPELLCAST_CHANNEL_STOP (...)
    local source, unitID, _, _, lineID = ...
    if not (unitID == "player") and not (unitID == "target") then
        return
    end

    channeling[unitID] = false
    
    Artemis:Transmit("spellChannelInterrupted", unitID, "ALERT")             
end

function Artemis:ZONE_CHANGED_NEW_AREA (...)
    local pvpType, isSubZonePVP, factionName = GetZonePVPInfo()

    Artemis:Transmit("zone", {z = GetRealZoneText(), s = GetSubZoneText(), t = pvpType, p = isSubZonePVP, f = factionName})
end
function Artemis:ZONE_CHANGED (...)
    local pvpType, isSubZonePVP, factionName = GetZonePVPInfo()
    
    Artemis:Transmit("zone", {z = GetRealZoneText(), s = GetSubZoneText(), t = pvpType, p = isSubZonePVP, f = factionName})
end