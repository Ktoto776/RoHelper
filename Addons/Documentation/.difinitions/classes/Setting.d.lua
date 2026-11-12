---@diagnostic disable: missing-return
---@diagnostic disable: inject-field

---@class Setting
---@field displayName string #Отображаемое имя этой настройки
---@field value any #Значение этой настройки
local Setting={}


---@diagnostic disable-next-line: undefined-doc-name
---@type Event
Setting.onValueChanged = {}
---Параллельно подключиться к событию
---@param callback fun(value: any)
function Setting.onValueChanged:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun(value: any)
function Setting.onValueChanged:Once(callback) end
---Подождать пока запустится событие
---@return any value #Новое значение этой настройки
function Setting.onValueChanged:Wait() end