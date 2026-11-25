local tButton = script:getTrayMenuButton()
if tButton==nil then error("Permission isn't granted") end

tButton.Clicked:Connect(function()
    print("Rejoin click")
    if not script.permissions:PermissionRequest("roblox.join_game") then
        return print("user permission isnt granted")
    end local joinedData = Roblox.joinedGameData
    if joinedData==nil then error("Permission isnt granted") end
    if joinedData.JobId=="" then return end
    print("Rejoining")
    Roblox:joinToServer({
        PlaceId=joinedData.PlaceId,
        JobId=joinedData.JobId,
    })
end)