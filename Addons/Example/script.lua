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
script.addonSettingsChanged:Connect(function()
    print("HEHE")
end)