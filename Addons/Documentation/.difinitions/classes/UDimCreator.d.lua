---@diagnostic disable: missing-return

---@class UDimCreator
local UDimCreator={}

--[[Создаёт новый UDim]]
---@param Scale number? #Размер в отношении к максимальному этого UDim [0-1]
---@param Offset number? #Смещение в пикселях этого UDim
---@return UDim UDim
function UDimCreator.new(Scale,Offset)end

---@param Offset number #Смещение в пикселях этого UDim
---@return UDim UDim
function UDimCreator.fromOffset(Offset)end

---@param Scale number #Размер в отношении к максимальному этого UDim [0-1]
---@return UDim UDim
function UDimCreator.fromScale(Scale)end