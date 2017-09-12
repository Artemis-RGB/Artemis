----------------------------------------------------------------------------------
-------------------------------- Artemis LUA file --------------------------------
----------------------------------------------------------------------------------

-- This is a default script to be executed by Artemis.
-- You do not need to use this if you don't want to script. The default profiles
-- should provide you with a lot of functionality out of the box.
-- However, if you want to change the way profiles work, this is the ideal way 
-- go about it.

-- For docs and examples, see wiki: https://github.com/SpoinkyNL/Artemis/wiki/LUA

-- Note: You are editing a temporary file. Whenever you save this file the 
-- changes are applied to the profile and the script is restarted.

-- This event is raised after every profile update, before drawing.
function updateHandler(profile, eventArgs)
    -- Don't do anything when previewing (this means the editor is open)
    if eventArgs.Preview then 
        return 
    end

    -- In this example we only want to update once per frame when the keyboard is 
    -- updated. If you don't do this the updateHandler will trigger on every
    -- device's update.
    if not (eventArgs.DeviceType == "keyboard") then
        return
    end
    
    -- Custom update code here
end

-- This event is raised after every profile draw, after updating.
function drawHandler(profile, eventArgs)
    -- Don't do anything when previewing (this means the editor is open)
    if eventArgs.Preview then 
        return 
    end

    -- In this example we only want to draw to the keyboard. Each device has it's
    -- own drawing event
    if not (eventArgs.DeviceType == "keyboard") then
        return
    end
    
    -- Custom draw code here
end


-- Register the default events, you can rename/remove these if you so desire.
-- These events are raised every 40 ms (25 times a second).
Events.DeviceUpdating.add(updateHandler)
Events.DeviceDrawing.add(drawHandler)