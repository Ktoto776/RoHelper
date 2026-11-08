local wait,print,task,spawn,delay
do
    local function exit()
        _ENV._refreshExitWhile()
        while true do wait() end
    end local runId = _getRunId()
    function wait(sec)
        if not _ENV.wait(runId,sec or 0) then
            exit()
        end
    end function spawn(f,...) local tab = {...} 
        _ENV.spawn(function() f(table.unpack(tab)) end) 
    end function delay(sec,f,...) 
        local tab = {...} spawn(function() wait(sec) f(table.unpack(tab)) end)
    end task = {wait=wait,spawn=spawn,delay=delay}
end