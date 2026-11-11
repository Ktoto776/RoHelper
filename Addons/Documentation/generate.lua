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
    local data = ""
    if package.config:sub(1,1) == "\\" then
        data = io.popen('dir "'..path..'" /B')
    else data = io.popen('dir "'..path..'" /B')
    end local files = {}
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

local DocumentationGlobals = "./globals.json"
local DocumentationClassesPath = "./classes/"

local outFolder = "./.difinitions/"
local outClassesFolder = outFolder.."classes/"
makefolder(outFolder)
makefolder(outClassesFolder)

local globalD = ""
for name,v in pairs(JSONDecode(readfile("./globals.json"))) do
    local type = v.ClassName or v.type
    if v.Generics and #v.Generics>0 then
        type = type.."<"..table.concat(v.Generics,",")..">"
    end
    local str = "---@type "..type.."\n"..name.." = nil"
    if v.isDeprecated then
        str = "---@deprecated\n"..str
    end if v.Description then
        if v.Description[descriptionLang] then
            str = "--[["..v.Description[descriptionLang].."]]\n"..str
        elseif v.Description[defaultDescriptionLang] then
            str = "--[["..v.Description[defaultDescriptionLang].."]]\n"..str
        end
    end
    
    print(name,type)
    if globalD~="" then globalD = globalD.."\n\n" end
    globalD = globalD..str
end

writefile(outFolder.."globals.d.lua",globalD)