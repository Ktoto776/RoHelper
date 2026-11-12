---@diagnostic disable: missing-return

---@class task
local task={}

--[[Выполнить функцию параллельно через заданное время]]
---@param durationInSecs number #Время для задержки в секундах
---@param func fun(...) #Функция для параллельного выполнения
function task.delay(durationInSecs,func)end

--[[Выполнить функцию параллельно]]
---@param func fun(...) #Функция для параллельного выполнения
function task.spawn(func)end

--[[Заморживает выполнение на заданное время в секундах]]
---@param durationInSecs number #Время для задержки в секундах
function task.wait(durationInSecs)end