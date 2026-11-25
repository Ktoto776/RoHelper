---@diagnostic disable: missing-return

---@class Roblox
---@field UserId string#UserId пользователя(если не был установлен =''), nil если нету разрешения * **[Требуются разрешения: roblox.userId]**
---@field logReader LogReader#Класс для чтения логов
---@field joinedGameData JoinedGameData#Данные сервера на котором находится игрок, если нету разрешения будет =nil * **[Требуются разрешения: roblox.joined_game]**
local Roblox={}

--[[Заходит в игру через deeplink]]
--* **[Требуются разрешения: roblox.join_game]**
---@overload fun(self:Roblox,placeId:string,jobId:string)
---@param joinData JoinServerData #Данные для присоедининения к серверу
function Roblox:joinToServer(joinData)end