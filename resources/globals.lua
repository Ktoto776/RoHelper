local task,print,delay,spawn,wait,type,typeof
local script
local UDim,PixelsSize,UDim2
do
    function wait(sec) _ENV.wait(sec or 0) coroutine.yield(_ENV.script)
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
    end task = {wait=wait,spawn=spawn,delay=delay,pass=function()
        coroutine.yield(_ENV.script)
    end,exit=function()
        coroutine.yield()
    end,} local parsedClassesCS = {}
    local parsedToCS = {}
    local typeForClasses = {[task]="task"}
    function type(obj) if typeForClasses[obj] then return "userdata" end return _ENV.type(obj) end 
    function typeof(obj) return typeForClasses[obj] or _ENV.type(obj) end
    local classIndex = {
        ScriptEvent={
            Wait=function(self)
                parsedToCS[self].Wait(self)
                coroutine.yield(_ENV.script)
                local args = self._lastArgs
                return args and table.unpack(args)
            end,
        }
    }
    local function parseCSharpClass(class,self)
        if parsedClassesCS[class] then return parsedClassesCS[class] end
        if parsedToCS[class] then return class end
        local typ = _ENV.type(class)
        if typ=="userdata" then
            local className = getClassTypeName(class)
            if className=="error" then error(class.description,1) end
            local propertiesType = _ENV.classGetMembersWriteTypes(class)
            local obj = setmetatable({},{
                __index=function(_,name)
                    if name=="new" and not propertiesType[name] then name = "_"..name end
                    if classIndex[className] and classIndex[className][name] then return classIndex[className][name]
                    elseif propertiesType[name] then return parseCSharpClass(class[name],class)
                    else error("value '"..name.."' is not exists in class "..className)
                    end
                end, __newindex=function(_,name,val)
                    local t = propertiesType[name]
                    --[[if t=="userdata" then error("Is not supported!") end]]
                    if t==true then error("value '"..name.."' is readonly in class "..className,1) end
                    if t~=typeof(val) then error("type error for value '"..name.."' in class "..className..
                        ", expected: "..t.." (got: "..typeof(val)..")",1) end
                    class[name] = parsedToCS[val] or val
                end,
                __pairs=function(_) error("Pairs is not supported") end,
                __tostring=function(_) return tostring(class) end,
            }) parsedClassesCS[class] = obj
            parsedToCS[obj] = class
            typeForClasses[obj] = className
            return obj
        elseif typ=="table" then
            local mt = getmetatable(class) or {}
            --[[mt.__pairs = function(t)
                return function(t,i)
                    local k,v = next(t,i)
                    return k,parseCSharpClass(v)
                end,t,nil
            end]] local oldNI = mt.__newindex
            mt.__newindex=function(t,k,v)
                v = parseCSharpClass(v)
                if oldNI then oldNI(t,k,v) else rawset(t,k,v) end
            end for k,v in pairs(class) do class[k] = rawset(class,k,parseCSharpClass(v)) end
            setmetatable(class,mt)
            parsedClassesCS[class] = class
            return class
        elseif typ=="function" then
            local parsed = parsedClassesCS[self]
            return function(mbSelf,...)
                local tab if mbSelf==self or mbSelf==parsed then
                    tab = table.pack(class(...))
                else tab = table.pack(class(mbSelf,...))
                end for k,v in ipairs(tab) do tab[k] = parseCSharpClass(v) end
                return table.unpack(tab)
            end
        else return class
        end
    end script = parseCSharpClass(_ENV.script)
    UDim = parseCSharpClass(_ENV.UDim)
    PixelsSize = parseCSharpClass(_ENV.PixelsSize)
    UDim2 = parseCSharpClass(_ENV.UDim2)
end