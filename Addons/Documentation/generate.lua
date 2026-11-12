local function makefolder(name)
    if package.config:sub(1,1) == "\\" then
        os.execute('md "' .. name .. '"')
    else
        os.execute('mkdir -p "' .. name .. '"')
    end
end

local function writefile(path,value)
    local file,err = io.open(path, "w")
    if file then
        file:write(value)
        file:close()
    else error("Error to open file: "..err)
    end
end

local function listfiles(path)
    local data
    if package.config:sub(1,1) == "\\" then
        data = io.popen('dir "'..path..'" /B')
    else data = io.popen('ls "'..path..'"')
    end local files = {}
    if not data then return {} end
    for file in data:lines() do
        table.insert(files,file)
    end return files
end

local function readfile(path)
    local file,err = io.open(path, "r")
    if file then
        local content = file:read("*all")
        file:close() return content
    else error("Error to open file: err")
    end
end

local json = require("json")
local function JSONDecode(str)
    return json.decode(str)
end

local descriptionLang = "ru"
local defaultDescriptionLang = "en"

---@type {[string]:boolean}
local ignorePermissions = {["create.tray_button"]=true}

local isTest = false

local DocumentationGlobals = "./globals.json"
local DocumentationClassesPath = "./classes/"

local outFolder = "./.difinitions/"
local outClassesFolder = outFolder.."classes/"
makefolder(outFolder)
makefolder(outClassesFolder)

local function getType(typeName,generics,typeData)
    if typeName=="function" then
        if not typeData then return "fun(...): (...)" end
        local data = "fun("
        for k,v in pairs(typeData.args or {}) do
            if k>1 then data = data.."," end
            data = data..v.name..": "..getType(v.type,v.generics,v.typeData)
        end data = data..")"
        if typeData.returns and #typeData.returns>0 then
            data = data..": ("
            for k,v in pairs(typeData.returns) do
                if k>1 then data = data.."," end
                data = data..v.name..": "..getType(v.type,v.generics,v.typeData)
            end data = data..")"
        end return data
    else local data = typeName
        local canBeNil = false
        if string.sub(data,-1)=="?" then
            canBeNil = true
            data = string.sub(data,1,-2)
        end if generics and #generics>0 then
            data = data.."<"..table.concat(generics,",")..">"
        end if canBeNil then data = data.."?" end
        return data
    end
end

local permissionsTextLangs = {
    ru="Требуются разрешения: ",
    en="Need permission(s): "
}
local permissionsText = permissionsTextLangs[descriptionLang] or 
                        permissionsTextLangs[defaultDescriptionLang]

local function getPermissionsText(permissions)
    if not permissions then return end
    local perms = {}
    for _,v in pairs(permissions) do
        if not ignorePermissions[v] then
            table.insert(perms,v)
        end
    end if perms and #perms>0 then
        return "--* **["..permissionsText..table.concat(perms,"; ").."]**\n"
    end
end

local globalD = "---@diagnostic disable: missing-return"
for name,v in pairs(JSONDecode(readfile("./globals.json"))) do
    if globalD~="" then globalD = globalD.."\n\n" end
    local permissions = getPermissionsText(v.Permissions)
    if v.type=="function" then
        local data = ""
        local str = "function "..name.."("
        if v.Args then
            for k,arg in pairs(v.Args) do
                if k>1 then str=str.."," end
                str = str..arg.name
                local type = arg.dType or getType(arg.type,arg.generics,arg.typeData)
                data = data.."---@param "..arg.name.." "..type
                if v.Description then
                    if v.Description[descriptionLang] then
                        data = data.." #"..v.Description[descriptionLang]
                    elseif v.Description[defaultDescriptionLang] then
                        data = data.." #"..v.Description[defaultDescriptionLang]
                    end
                end data = data.."\n"
            end str = str..") end"
        end if v.Returns then
            for _,ret in pairs(v.Returns) do
                local type = ret.dType or getType(ret.type,ret.generics,ret.typeData)
                data = data.."---@return "..type.." "..ret.name
                if v.Description then
                    if v.Description[descriptionLang] then
                        data = data.." #"..v.Description[descriptionLang]
                    elseif v.Description[defaultDescriptionLang] then
                        data = data.." #"..v.Description[defaultDescriptionLang]
                    end
                end data = data.."\n"
            end
        end if v.Overload then
            for _,overload in pairs(v.Overload) do
                data = "---@overload "..(overload.dType or getType("function",nil,overload)).."\n"..data
            end
        end data = (permissions or "")..data
        if v.Description then
            if v.Description[descriptionLang] then
                data = "--[["..v.Description[descriptionLang].."]]\n"..data
            elseif v.Description[defaultDescriptionLang] then
                data = "--[["..v.Description[defaultDescriptionLang].."]]\n"..data
            end
        end globalD = globalD..data..str
        print("Global method detected: "..name)
    else local type = v.ClassName or v.type
        if v.Generics and #v.Generics>0 then
            type = type.."<"..table.concat(v.Generics,",")..">"
        end local str = "---@type "..type.."\n"..name.." = nil"
        if v.isDeprecated then
            str = "---@deprecated\n"..str
        end str = (permissions or "")..str
        if v.Description then
            if v.Description[descriptionLang] then
                str = "--[["..v.Description[descriptionLang].."]]\n"..str
            elseif v.Description[defaultDescriptionLang] then
                str = "--[["..v.Description[defaultDescriptionLang].."]]\n"..str
            end
        end
        print("Global detected: "..name.." (type: "..type..")")
        globalD = globalD..str
    end
end

writefile(outFolder.."globals.d.lua",globalD)

local defaultEventData = string.sub(readfile("./event.d.lua"),
    string.len("---@diagnostic disable: inject-field, undefined-doc-name")+1)
local eventLangs = {
    en={
        connectComment="Connect to an event in parallel",
        onceComment="Connect to an event in parallel, but for a single execution",
        waitComment="Wait for an event to fire"
    },ru={
        connectComment="Параллельно подключиться к событию",
        onceComment="Параллельно подключиться к событию, но на 1 запуск",
        waitComment="Подождать пока запустится событие"
    }
} local selected = eventLangs[descriptionLang] or eventLangs[defaultDescriptionLang] or eventLangs.en
defaultEventData = string.gsub(defaultEventData,"connectComment**",selected.connectComment)
defaultEventData = string.gsub(defaultEventData,"onceComment**",selected.onceComment)
defaultEventData = string.gsub(defaultEventData,"waitComment**",selected.waitComment)

for _,fname in pairs(listfiles(DocumentationClassesPath)) do
    if string.sub(fname,1,1)=="." and not isTest then goto continue end
    local headers = {}
    local fpath = DocumentationClassesPath..fname
    local v = JSONDecode(readfile(fpath))
    local className = v.ClassName
    if v.Generics and #v.Generics>0 then
        className = className.."<"..table.concat(v.Generics,",")..">"
    end local data = "---@class "..className
    if v.Inherited and #v.Inherited>0 then
        data = data.." :"..table.concat(v.Inherited,",")
    end local classVarName = v.ClassName
    local data2 = "local "..classVarName.."={}"
    if v.Members then
        for name,member in pairs(v.Members) do
            local permissions = getPermissionsText(member.Permissions)
            if member.memberType=="Event" then
                local eventData = string.gsub(defaultEventData,"_event_name",classVarName.."."..name)
                local inFunData = ""
                local returns = ""
                if member.Args and #member.Args>0 then
                    headers["---@diagnostic disable: missing-return"] = true
                    for k,v in pairs(member.Args) do
                        if k>1 then inFunData = inFunData.."," returns=returns.."\n" end
                        local type = v.dType or getType(v.type,v.generics,v.typeData)
                        inFunData = inFunData..v.name..": "..type
                        returns = returns.."---@return "..type.." "..v.name
                        if v.description then
                            if v.description[descriptionLang] then
                                returns = returns.." #"..v.description[descriptionLang]
                            elseif v.description[defaultDescriptionLang] then
                                returns = returns.." #"..v.description[defaultDescriptionLang]
                            end
                        end
                    end
                end eventData = string.gsub(eventData,"inFunData**",inFunData)
                eventData = string.gsub(eventData,"--returns**",returns)
                headers["---@diagnostic disable: inject-field"] = true
                eventData = (permissions or"")..eventData
                if member.Description then
                    if member.Description[descriptionLang] then
                        eventData = "--[["..member.Description[descriptionLang].."]]"..eventData
                    elseif member.Description[defaultDescriptionLang] then
                        eventData = "--[["..member.Description[defaultDescriptionLang].."]]"..eventData
                    end
                end data2 = data2.."\n\n"..eventData
            elseif member.memberType=="Method" then
                local methodData = "function "..classVarName
                if member.isWithSelf~=false then
                    methodData = methodData..":"
                else methodData = methodData.."."
                end local argNames = {}
                local argsData = ""
                if member.Overload then
                    for _,overload in pairs(member.Overload) do
                        if member.isWithSelf~=false and (not overload.args[1]
                                                        or overload.args[1].name~="self") then
                            table.insert(overload.args,1,{
                                type="any",
                                name="self"
                            })
                        end
                        argsData = argsData.."---@overload "..(overload.dType or getType("function",nil,overload)).."\n"
                    end
                end  for _,arg in pairs(member.Args or {}) do
                    table.insert(argNames,arg.name)
                    local argData = "---@param "..arg.name.." "..(arg.dType or getType(arg.type,arg.generics,arg.typeData))
                    if arg.description then
                        if arg.description[descriptionLang] then
                            argData = argData.." #"..arg.description[descriptionLang]
                        elseif arg.description[defaultDescriptionLang] then
                            argData = argData.." #"..arg.description[defaultDescriptionLang]
                        end
                    end argsData = argsData..argData.."\n"
                end if member.Returns then
                    for _,ret in pairs(member.Returns) do
                        local argData = "---@return "..(ret.dType or getType(ret.type,ret.generics,ret.typeData))
                            .." "..ret.name
                        if ret.description then
                            if ret.description[descriptionLang] then
                                argData = argData.." #"..ret.description[descriptionLang]
                            elseif ret.description[defaultDescriptionLang] then
                                argData = argData.." #"..ret.description[defaultDescriptionLang]
                            end
                        end argsData = argsData..argData.."\n"
                    end headers["---@diagnostic disable: missing-return"] = true
                end methodData = methodData..name.."("..table.concat(argNames,",")..")end"
                argsData = (permissions or"")..argsData
                if member.Description then
                    if member.Description[descriptionLang] then
                        argsData = "--[["..member.Description[descriptionLang].."]]\n"..argsData
                    elseif member.Description[defaultDescriptionLang] then
                        argsData = "--[["..member.Description[defaultDescriptionLang].."]]\n"..argsData
                    end
                end
                data2 = data2.."\n\n"..argsData..methodData
            else local type = member.valueType
                if not type or member.memberType~="Property" then
                    type = "unknown"
                    warn("Class "..className.." has unknown member '"..name.."'")
                else type = member.dType or getType(type,member.valueGenerics,member.typeData)
                end local memberData = (permissions or"").."---@field "..name.." "..type
                if member.Description then
                    if member.Description[descriptionLang] then
                        memberData = memberData.." #"..member.Description[descriptionLang]
                    elseif member.Description[defaultDescriptionLang] then
                        memberData = memberData.." #"..member.Description[defaultDescriptionLang]
                    end
                end data = data.."\n"..memberData
            end
        end
    end local headersData = ""
    for str,_ in pairs(headers) do
        headersData = headersData..str.."\n"
    end if headersData~="" then headersData = headersData.."\n" end
    writefile(string.gsub(outClassesFolder..fname,".json",".d.lua"),headersData..data.."\n"..data2)
    ::continue::
end