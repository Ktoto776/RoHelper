---@diagnostic disable: missing-return

--[[Получает тип обьекта или класса]]
---@param object any? #Получает тип обьекта или класса
---@return string type #Получает тип обьекта или класса
function typeof(object) end

--[[Класс создания нового PixelsSize]]
---@type PixelsSizeCreator
PixelsSize = nil

--[[Класс для роблокса]]
---@type Roblox
Roblox = nil

--[[Сборник функций связанных с параллельными задачами]]
---@type task
task = nil

--[[Класс создания нового UDim2]]
---@type UDim2Creator
UDim2 = nil

--[[Заморживает выполнение на заданное время в секундах]]
---@param durationInSecs number #Заморживает выполнение на заданное время в секундах
function wait(durationInSecs) end

--[[Выполнить функцию параллельно]]
---@overload fun(func:fun(...),...)
---@param func fun(...) #Выполнить функцию параллельно
function spawn(func) end

--[[Класс скрипта]]
---@type Script
script = nil

--[[Класс создания нового UDim]]
---@type UDimCreator
UDim = nil

--[[Выполнить функцию параллельно через заданное время]]
---@overload fun(durationInSecs:number,func:fun(...),...)
---@param durationInSecs number #Выполнить функцию параллельно через заданное время
---@param func fun(...) #Выполнить функцию параллельно через заданное время
function delay(durationInSecs,func) end