---@diagnostic disable: missing-return

--[[Выполнить функцию параллельно]]
---@overload fun(func:fun(...),...)
---@param func fun(...) #Выполнить функцию параллельно
function spawn(func) end

--[[Класс создания нового PixelsSize]]
---@type PixelsSizeCreator
PixelsSize = nil

--[[Класс создания нового UDim]]
---@type UDimCreator
UDim = nil

--[[Получает тип обьекта или класса]]
---@param object any? #Получает тип обьекта или класса
---@return string type #Получает тип обьекта или класса
function typeof(object) end

--[[Класс скрипта]]
---@type Script
script = nil

--[[Класс для роблокса]]
---@type Roblox
Roblox = nil

--[[Класс создания нового UDim2]]
---@type UDim2Creator
UDim2 = nil

--[[Выполнить функцию параллельно через заданное время]]
---@overload fun(durationInSecs:number,func:fun(...),...)
---@param durationInSecs number #Выполнить функцию параллельно через заданное время
---@param func fun(...) #Выполнить функцию параллельно через заданное время
function delay(durationInSecs,func) end

--[[Заморживает выполнение на заданное время в секундах]]
---@param durationInSecs number #Заморживает выполнение на заданное время в секундах
function wait(durationInSecs) end

--[[Сборник функций связанных с параллельными задачами]]
---@type task
task = nil