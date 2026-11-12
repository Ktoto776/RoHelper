---@diagnostic disable: inject-field

---@class TrayButton
local TrayButton={}

--[[Запускается при клике на кнопку в трее]]
---@diagnostic disable-next-line: undefined-doc-name
---@type Event
TrayButton.Clicked = {}
---Параллельно подключиться к событию
---@param callback fun()
function TrayButton.Clicked:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun()
function TrayButton.Clicked:Once(callback) end
---Подождать пока запустится событие

function TrayButton.Clicked:Wait() end