---@diagnostic disable: missing-return

---@class task
local task={}

--[[Пропускает эту задачу скрипта, чтобы быстрее выполнить другие задачи]]
function task.pass()end

--[[Закрывает этот поток]]
function task.exit()end

--[[Выполнить функцию параллельно через заданное время]]
---@overload fun(durationInSecs:number,func:fun(...),...)
---@param durationInSecs number #Время для задержки в секундах
---@param func fun(...) #Функция для параллельного выполнения
function task.delay(durationInSecs,func)end

--[[Заморживает выполнение на заданное время в секундах]]
---@param durationInSecs number #Время для задержки в секундах
function task.wait(durationInSecs)end

--[[Выполнить функцию параллельно]]
---@overload fun(func:fun(...),...)
---@param func fun(...) #Функция для параллельного выполнения
function task.spawn(func)end