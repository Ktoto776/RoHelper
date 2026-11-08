local task,print,delay,spawn,wait
do
    local function doNothing() end
    function wait(...) _ENV.wait(...) doNothing()
    end function spawn(f,...)
        local tab = {...} 
        _ENV.spawn(function() f(table.unpack(tab)) end)
    end function delay(s,f,...)
        local tab = {...} 
        _ENV.spawn(function() wait(s) f(table.unpack(tab)) end)
    end function print(...)
        local str = ""
        for _,v in ipairs(table.pack(...)) do
            if str~="" then
                str = str.."   "
            end str = str..tostring(v)
        end return _ENV.print(str)
    end task = {wait=wait,spawn=spawn,delay=delay}
end