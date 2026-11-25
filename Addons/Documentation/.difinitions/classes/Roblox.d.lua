---@class Roblox
---@field joinedGameData JoinedGameData#Данные сервера на котором находится игрок, если нету разрешения будет =nil * **[Требуются разрешения: roblox.joined_game]**
---@field UserId string#UserId пользователя(если не был установлен =''), nil если нету разрешения * **[Требуются разрешения: roblox.userId]**
---@field logReader LogReader#Класс для чтения логов
local Roblox={}