---@diagnostic disable: missing-return

---@class PixelsSizeCreator
local PixelsSizeCreator={}

--[[Создаёт новый PixelsSize из размера экрана]]
---@param WidthScale number?
---@param HeightScale number?
---@return PixelsSize PixelsSize
function PixelsSizeCreator.fromScreenScale(WidthScale,HeightScale)end

--[[Создаёт новый PixelsSize]]
---@param Width number? #Ширина в пикселях
---@param Height number? #Высота в пикселях
---@return PixelsSize PixelsSize
function PixelsSizeCreator.new(Width,Height)end