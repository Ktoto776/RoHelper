local task,print,delay,spawn,wait,type,typeof
local script
do
    local function doNothing() end
    function wait(sec,...) _ENV.wait(sec or 0,...) doNothing()
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
    local parsedClassesCS = {}
    local typeForClasses = {}
    function type(obj) if typeForClasses[obj] then return "userdata" end return _ENV.type(obj) end 
    function typeof(obj) return typeForClasses[obj] or _ENV.type(obj) end
    local function parseCSharpClass(class)
        if parsedClassesCS[class] then return parsedClassesCS[class] end
        local typ = _ENV.type(class)
        if typ=="userdata" then
            local className = "unknown"
            local propertiesType = _ENV.classGetMembersWriteTypes(class)
            local obj = setmetatable({},{
                __index=function(_,name)
                    if propertiesType[name] then return parseCSharpClass(class[name])
                    else error("value '"..name.."' is not exists in class "..className)
                    end
                end, __newindex=function(_,name,val)
                    local t = propertiesType[name]
                    if t=="userdata" then error("Is not supported!") end
                    if t~=type(val) then error("type error for value '"..name.."' in class "..className) end
                    class[name] = val
                end,
                __pairs=function(_) error("Pairs is not supported") end,
                __tostring=function(_) return tostring(class) end,
            }) parsedClassesCS[class] = obj
            typeForClasses[obj] = className
            return obj
        elseif typ=="table" then
            local mt = getmetatable(class) or {}
            mt.__pairs = function(t)
                return function(t,i)
                    local k,v = next(t,i)
                    return k,parseCSharpClass(v)
                end,t,nil
            end setmetatable(class,mt)
            parsedClassesCS[class] = class
            return class
        else return class
        end
    end script = parseCSharpClass(_ENV.script)
end