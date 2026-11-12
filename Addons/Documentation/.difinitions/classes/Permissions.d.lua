---@diagnostic disable: missing-return

---@class Permissions
local Permissions={}

--[[Узнаёт выдано ли разрешение в info.json]]
---@param permissionName string #Название разрешения
---@return boolean isGranted #Выдано ли разрешение
function Permissions:isPermissionGranted(permissionName)end