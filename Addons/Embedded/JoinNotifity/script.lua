print(123)

local trayButton = script:getTrayMenuButton()
local window = script:createWindow("")

if not window or not trayButton then error("PERMISSIONS ERROR") end

local size = PixelsSize.fromScreenScale(0.5,0.5)
window.MinimumSize = size

print(1,tostring(os),1)

trayButton.Clicked:Connect(function()
    print("Tray button click")
    window:Show()
end)

-- Roblox.logReader.NewLogLineEvent:Connect(function(log)
--     print(("("..log.logLevel..")"..log.logType.." ").."["..log.Sender.."]",log.Data)
-- end)

local joined = Roblox.joinedGameData
Roblox.logReader.JoinedToPlace:Connect(function()
    print("PlaceId:",joined.PlaceId)
    print("JobId:",joined.JobId)
    print("ServerIP:",joined.ServerIP)
end)

Roblox.logReader.SuccessLeftEvent:Connect(function()
    print("Why you leave from this server?")
end)

Roblox.logReader.LeftEvent:Connect(function()
    print("Bye-bye")
end)

Roblox.logReader.Disconnected:Connect(function(reason)
    print("I know you has been disconnected with reason:",reason)
end)