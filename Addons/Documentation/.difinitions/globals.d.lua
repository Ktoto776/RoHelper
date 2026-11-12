---@diagnostic disable: missing-return

---@param func fun(...)
function spawn(func) end

---@param durationInSecs number
function wait(durationInSecs) end

---@param durationInSecs number
---@param func fun(...)
function delay(durationInSecs,func) end

--[[Класс скрипта]]
---@type Script
script = nil

--[[Сборник функций связанных с параллельными задачами]]
---@type task
task = nil