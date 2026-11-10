--wait(1)

local ii = 0
spawn(function()
    while true do
        print(ii)
        wait(0.1)
        script.addonSettingsChanged:Wait()
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

print(_ENV.type(script.settings),type(script))
for k,v in pairs(script.settings) do
    print("Group:",k)
    for k,v in pairs(v) do
        print(k,v)
        print(type(v))
        v.onValueChanged:Connect(function()
            print(k,v,v.value)
        end)
    end
end