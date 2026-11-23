print(123)

local trayButton = script:getTrayMenuButton()
local window = script:createWindow("")

if not window or not trayButton then error("PERMISSIONS ERROR") end

local size = PixelsSize.fromScreenScale(0.5,0.5)
window.MinimumSize = size

print(1,tostring(os),1)

trayButton.Clicked:Connect(function()
    print("Tray button click")
    -- window:Show()
    print(tostring(script.permissions.PermissionRequest("roblox.join_game")))
end)

task.exit()

while true do
    -- task.wait(1)
    print("LOL")
end

-- Roblox.logReader.NewLogLineEvent:Connect(function(log)
--     -- if log.Data~="leaveUGCGameInternal"and not string.find(log.Data,"NoReload") then return end
--     print(("("..log.logLevel..")"..log.logType.." ").."["..log.Sender.."]",log.Data)
-- end)

Roblox.logReader.Joining:Connect(function()
    print("Joining")
end)

local joined = Roblox.joinedGameData
Roblox.logReader.Joined:Connect(function()
    print("PlaceId:",joined.PlaceId)
    print("JobId:",joined.JobId)
    print("ServerIP:",joined.ServerIP)
end)

-- Roblox.logReader.SuccessLeftEvent:Connect(function()
--     print("Why you leave from this server?")
--     task.wait(5)
--     print("Go back!")
--     -- Roblox:joinToServer(joined.PlaceId)
-- end)

Roblox.logReader.Leaving:Connect(function()
    print("Bye-bye")
end)

Roblox.logReader.Disconnected:Connect(function(reason)
    print("I know you has been disconnected with reason:",reason)
end)