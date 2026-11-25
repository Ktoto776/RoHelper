---@diagnostic disable: missing-return

---@class UDim2Creator
local UDim2Creator={}

---@param XScale number? #Размер x [0-1]
---@param YScale number? #Размер y [0-1]
---@return UDim2 UDim2
function UDim2Creator.fromScale(XScale,YScale)end

---@param XOffset number? #X в пикселях
---@param YOffset number? #Y в пикселях
---@return UDim2 UDim2
function UDim2Creator.fromOffset(XOffset,YOffset)end

--[[Создаёт новый UDim2]]
---@param X UDim
---@param Y UDim
---@return UDim2 UDim2
function UDim2Creator.new(X,Y)end