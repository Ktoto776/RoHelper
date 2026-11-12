---@diagnostic disable: missing-return
---@diagnostic disable: inject-field

---@class Script
---@field permissions Permissions #Разрешения скрипта
---@field settings {[string]:{[string]:Setting}} #Таблица настроек, где 1 слой - группы, а второй уже сами настройки
local Script={}


---@diagnostic disable-next-line: undefined-doc-name
---@type Event
Script.addonSettingsChanged = {}
---Параллельно подключиться к событию
---@param callback fun(settingsGroupName: string,settingName: string,value: any)
function Script.addonSettingsChanged:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun(settingsGroupName: string,settingName: string,value: any)
function Script.addonSettingsChanged:Once(callback) end
---Подождать пока запустится событие
---@return string settingsGroupName #Название группы настроек
---@return string settingName #Название настройки
---@return any value #Новое значение этой настройки
function Script.addonSettingsChanged:Wait() end

--[[Возвращает окно]]
--* **[Требуются разрешения: create.window]**
---@return Window? window #Новое окно, если разрешение не выдано => вернёт nil
function Script:createWindow()end

--[[Возвращает кнопку в трее]]
---@return TrayButton? trayButton #Кнопка в меню трея, если разрешение не выдано => вернёт nil
function Script:getTrayMenuButton()end