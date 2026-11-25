local rawsetmetatable,rawgetmetatable,os,debug = setmetatable,getmetatable,os,debug
local changeEnvironment = _changeEnvironment
local newENV = {}
local oldENV = {
    wait=wait,
    spawn=spawn,
    print=print,
    type=type,
    classGetMembersWriteTypes=classGetMembersWriteTypes,
    getClassTypeName=getClassTypeName,
    os=os,
} local scriptClass = _ENV.script
function newENV.wait(sec)
    oldENV.wait(sec or 0)
    coroutine.yield(scriptClass)
end
function newENV.spawn(f,...)
    local tab = {...}
    oldENV.spawn(function() f(table.unpack(tab)) end)
end
function newENV.delay(s,f,...)
    local tab = {...} 
    oldENV.spawn(function() newENV.wait(s) f(table.unpack(tab)) end)
end
function newENV.print(...)
    local str = ""
    for _,v in ipairs(table.pack(...)) do
        if str~="" then
            str = str.."   "
        end str = str..tostring(v)
    end return oldENV.print(str)
end
newENV.task = {wait=newENV.wait,spawn=newENV.spawn,delay=newENV.delay,pass=function()
    coroutine.yield(scriptClass)
end,exit=function()
    coroutine.yield()
end,}
local parsedClassesCS = {}
local parsedToCS = {}
local typeForClasses = {[newENV.task]="task"}
function newENV.type(obj)
    if typeForClasses[obj] then
        return "userdata"
    end return oldENV.type(obj)
end function newENV.typeof(obj)
    return typeForClasses[obj] or oldENV.type(obj)
end
for k,v in pairs(newENV) do
    changeEnvironment(k,v)
end
local classIndex = {
    ScriptEvent={
        Wait=function(self)
            parsedToCS[self].Wait(self)
            coroutine.yield(scriptClass)
            local args = self._lastArgs
            return args and table.unpack(args)
        end,
    }
}
local metatablesBlocked = {}
local parseCSharpClass
local function parseReturnInputs(level,...)
    local tab = table.pack(...)
    local res = {}
    for _,v in ipairs(tab) do
        local className = oldENV.getClassTypeName(v)
        if className=="PromiseReturn" then
            local tab = v.Connect(scriptClass)
            for _,vv in ipairs(parseReturnInputs(level+1,tab and table.unpack(tab)
                or
                coroutine.yield(scriptClass)
            )) do
                table.insert(res,vv)
            end
        elseif className=="error" then
            error(v.description,level+1)
        else table.insert(res,parseCSharpClass(v))
        end
    end return res
end function parseCSharpClass(class,self)
    if parsedClassesCS[class] then return parsedClassesCS[class] end
    if parsedToCS[class] then return class end
    local typ = oldENV.type(class)
    if typ=="userdata" then
        local className = oldENV.getClassTypeName(class)
        if className=="error" then error(class.description,2) end
        local propertiesType = oldENV.classGetMembersWriteTypes(class)
        local obj = rawsetmetatable({},{
            __index=function(_,name)
                if name=="new" and not propertiesType[name] then name = "_"..name end
                if classIndex[className] and classIndex[className][name] then 
                    return classIndex[className][name]
                elseif propertiesType[name] then return parseCSharpClass(class[name],class)
                else error("field '"..name.."' is not exists in class "..className,2)
                end
            end, __newindex=function(_,name,val)
                local t = propertiesType[name]
                if t==true then
                    error("field '"..name.."' is readonly in class "..className,2)
                elseif not t then
                    error("field '"..name.."' is not exists in class "..className,2)
                end
                if t~=newENV.typeof(val) then
                    error("type error for field '"..name.."' in class "..className..
                    ", expected: "..t.." (got: "..newENV.typeof(val)..")",2)
                end
                class[name] = parsedToCS[val] or val
            end,
            __pairs=function(_)
                error("Pairs is not supported")
            end,
            __tostring=function(_)
                return tostring("<"..className..">")
            end,
        }) parsedClassesCS[class] = obj
        parsedToCS[obj] = class
        typeForClasses[obj] = className
        metatablesBlocked[obj] = true
        return obj
    elseif typ=="table" then
        local mt = rawgetmetatable(class) or {}
        mt.__pairs = function(t)
            return function(t,i)
                local k,v = next(t,i)
                return k,parseCSharpClass(v)
            end,t,nil
        end 
        local oldNI = mt.__newindex
        mt.__newindex=function(t,k,v)
            v = parseCSharpClass(v)
            if oldNI then oldNI(t,k,v) else rawset(t,k,v) end
        end for k,v in pairs(class) do class[k] = rawset(class,k,parseCSharpClass(v)) end
        rawsetmetatable(class,mt)
        parsedClassesCS[class] = class
        metatablesBlocked[class] = true
        return class
    elseif typ=="function" then
        local parsed = parsedClassesCS[self]
        return function(mbSelf,...)
            if mbSelf==self or mbSelf==parsed then
                return table.unpack(parseReturnInputs(2,class(...)))
            else return table.unpack(parseReturnInputs(2,class(mbSelf,...)))
            end
        end
    else return class
    end
end
local change = {"script","Roblox","UDim","UDim2","PixelsSize"}
for _,v in pairs(change) do changeEnvironment(v,parseCSharpClass(_ENV[v])) end
if not script.permissions:isPermissionGranted("os.time") then
    changeEnvironment("os",nil)
end
local function checkBlockedMT(tab)
    if metatablesBlocked[tab] then
        local class = newENV.typeof(tab)
        if class=="table" then
            error("Metatable blocked",3)
        else error("Metatable blocked for '"..class.."' class",3)
        end
    end
end changeEnvironment("setmetatable",function(tab,...)
    checkBlockedMT(tab) return rawsetmetatable(tab,...)
end) changeEnvironment("getmetatable",function(tab)
    checkBlockedMT(tab) return rawgetmetatable(tab)
end)