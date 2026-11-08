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
        wait(0.1)
    end
end)