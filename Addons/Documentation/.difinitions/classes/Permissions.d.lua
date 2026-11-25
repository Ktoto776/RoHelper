---@diagnostic disable: missing-return

---@class Permissions
local Permissions={}

--[[Спрашивает разрешение у пользователя, и возвращает было ли оно выдано]]
---@param userPermissionName string #Название разрешения пользователя
---@return boolean isGranted #Выдал ли пользователь разрешение
function Permissions:PermissionRequest(userPermissionName)end

--[[Узнаёт выдано ли разрешение в info.json]]
---@param permissionName string #Название разрешения
---@return boolean isGranted #Выдано ли разрешение
function Permissions:isPermissionGranted(permissionName)end

--[[Узнаёт выдано ли разрешение пользователем]]
---@param userPermissionName string #Название разрешения пользователя
---@return boolean isGranted #Выдано ли разрешение пользователем
function Permissions:isUserPermissionGranted(userPermissionName)end