local trayButton = script:getTrayMenuButton()
local window = script:createWindow("")

if not window or not trayButton then error("PERMISSIONS ERROR") end

local size = PixelsSize.fromScreenScale(0.5,0.5)
window.MinimumSize = size

trayButton.Clicked:Connect(function()
    print("Tray button click")
    window:Show()
end)

task.delay(15,function()
    print("Destroyed")
    window:Destroy()
end)