local stopped = false

task.spawn(function()
    print(123)
    while not stopped do
        wait(0.1)
        print("CLICK TRAY BUTTON")
    end
end)
print(321)

local trayButton = script:getTrayMenuButton()
if not trayButton then return end
trayButton.Clicked:Wait()

script:createWindow()

stopped = true
trayButton.Clicked:Connect(function()
    print("LOL")
end)