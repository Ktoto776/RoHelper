-- Ждать 1 секунду
wait(1)
task.wait(1)


-- Запустить в другом "потоке"
spawn(function(one,two)
    wait(1)
    print("1 sec")
    print(one) --> 1
    print(two) --> arg
end,1,"arg")
task.spawn(function(one,two)
    wait(2)
    print("2 sec")
    print(one) --> 1
    print(two) --> arg
end,1,"arg")


-- Запустить в другом "потоке", но через время
delay(1,function(one,two)
    print("1 sec")
    print(one) --> 1
    print(two) --> arg
end,1,"arg")
task.delay(2,function(one,two)
    print("2 sec")
    print(one) --> 1
    print(two) --> arg
end,1,"arg")



-- Данные в script

-- luaEvent когда настройки аддона сменились
script.addonSettingsChanged:Connect(function(groupName,settingName,value)
    print("Group Name:",groupName)
    print("Setting Name:",settingName)
    print("Value:",value)
    print("Value type:",type(value))
end)
script.addonSettingsChanged:Once(function()
    print("Когда изменились, но запустится только 1 раз")
    script.addonSettingsChanged:Wait()
    print("Дождались изменения настроек, после 1 раза")
end)
for groupName,group in pairs(script.settings) do
    print("Сканирую группу настроек:",groupName)
    for settingName,setting in pairs(group) do
        print("Нашёл настройку '"..settingName.."' в группе",groupName,"её значение: ",setting.value,
            "её имя: ",setting.displayName)
        setting.onValueChanged:Connect(function(value)
            print("Изменилась настройка",settingName,"теперь её значение:",value)
        end)
        if setting.value=="ChangeMe" then
            -- Можно менять настройку таким образом:
            setting.value="Setting changed"
        end
    end
end

-- Получить типы
print(type(script)) --> "userdata", вернёт название lua типа
print(typeof(script)) --> "Script", вернёт название класса(или lua типа)