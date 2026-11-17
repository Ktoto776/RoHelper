---@class JoinedGameData
---@field isConnected boolean#Подключён ли клиент роблокс к серверу
---@field PlaceId string#PlaceId последнего подключённого сервер, если подключения не выполнялись =''
---@field ServerIP string#IP последнего подключённого сервер, если подключения не выполнялись =''
---@field JobId T?#JobId последнего подключённого сервер, если подключения не выполнялись =''
local JoinedGameData={}