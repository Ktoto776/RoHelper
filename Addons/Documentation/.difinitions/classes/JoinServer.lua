---@class JoinedGameData
---@field isConnected boolean#Подключён ли клиент роблокс к серверу
---@field RobloxServerIP string#IP последнего подключённого сервер, если подключения не выполнялись =''
---@field JobId string#JobId последнего подключённого сервер, если подключения не выполнялись =''
---@field ServerAddress string#Адрес(IP:Порт) последнего подключённого сервер, если подключения не выполнялись =''
---@field PlaceId string#PlaceId последнего подключённого сервер, если подключения не выполнялись =''
local JoinedGameData={}