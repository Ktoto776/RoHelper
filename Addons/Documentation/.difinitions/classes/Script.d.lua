---@diagnostic disable: inject-field
---@diagnostic disable: missing-return

---@class Script
---@field settings {[string]:{[string]:Setting}}#Таблица настроек, где 1 слой - группы, а второй уже сами настройки
---@field permissions Permissions#Разрешения скрипта
local Script={}

--[[Возвращает кнопку в трее]]
---@return TrayButton? trayButton #Кнопка в меню трея, если разрешение не выдано => вернёт nil
function Script:getTrayMenuButton()end


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
---@param windowName string #Имя окна, Основное имя окна обычно ''
---@return Window? window #Новое окно, если разрешение не выдано => вернёт nil
function Script:createWindow(windowName)end