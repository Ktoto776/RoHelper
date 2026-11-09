--wait(1)

local ii = 0
spawn(function()
    while true do
        print(ii)
        wait(0.1)
    end
end)

spawn(function()
    while true do
        ii = ii+1
        wait(0.1) -- fix - stops all threads
        script.addonSettingsChanged:Wait()
    end
end)

script.addonSettingsChanged:Connect(function()
    print("TEST") -- fix - stops nothing
    script.addonSettingsChanged:Wait()
    print("TEST2")
end)

script.addonSettingsChanged:Once(function()
    print("ONCE")
end)