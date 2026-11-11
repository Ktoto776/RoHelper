--wait(1)

local ii = 0
spawn(function()
    while true do
        print(ii)
        wait(0.1)
        -- script.addonSettingsChanged:Wait()
    end
end)

spawn(function()
    while true do
        ii = ii+1
        wait(0.1) -- fix - stops all threads
        script.addonSettingsChanged:Wait()
    end
end)

script.addonSettingsChanged:Connect(function(groupName,settingName,value)
    print("GName:",groupName)
    print("settingName:",settingName)
    print("value:",value)
end)

-- script.addonSettingsChanged:Once(function()
--     print("ONCE")
-- end)


print(_ENV.type(script.settings),type(script))
for k,v in pairs(script.settings) do
    print("Group:",k)
    for k,v in pairs(v) do
        print(k,v)
        print(type(v))
        v.onValueChanged:Connect(function(val)
            print(k,v,val)
            print("WAITed: ",v.onValueChanged:Wait())
        end)
        -- v.onValueChanged:Connect(function(val)
        --     print(k,v,val)
        -- end)
    end
end