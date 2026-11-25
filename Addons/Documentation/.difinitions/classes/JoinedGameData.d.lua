---@class JoinedGameData
---@field ServerAddress string#Адрес(IP:Порт) последнего подключённого сервер, если подключения не выполнялись =''
---@field JobId string#JobId последнего подключённого сервер, если подключения не выполнялись =''
---@field RobloxServerIP string#IP последнего подключённого сервер, если подключения не выполнялись =''
---@field isTeleported boolean#Был ли игрок телепортирован из другого плейса
---@field PlaceId string#PlaceId последнего подключённого сервер, если подключения не выполнялись =''
---@field isConnected boolean#Подключён ли клиент роблокс к серверу
local JoinedGameData={}