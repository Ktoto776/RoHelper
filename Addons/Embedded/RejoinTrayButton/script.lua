local tButton = script:getTrayMenuButton()
if tButton==nil then error("Permission isn't granted") end

tButton.Clicked:Connect(function()
    local joinedData = Roblox.joinedGameData
    if joinedData==nil then error("Permission isnt granted") end
    if joinedData.JobId=="" then return end
    Roblox:joinToServer(joinedData.PlaceId,joinedData.JobId)
end)