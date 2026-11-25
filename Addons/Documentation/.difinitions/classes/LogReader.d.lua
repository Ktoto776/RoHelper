---@diagnostic disable: inject-field
---@diagnostic disable: missing-return

---@class LogReader
local LogReader={}

--[[Запускается при новой строке из роблокс лога, с сырой строкой, без разрешения вместо эвента nil]]--* **[Требуются разрешения: read_logs.all]**

---@diagnostic disable-next-line: undefined-doc-name
---@type Event
LogReader.NewRawLineEvent = {}
---Параллельно подключиться к событию
---@param callback fun(rawLine: string)
function LogReader.NewRawLineEvent:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun(rawLine: string)
function LogReader.NewRawLineEvent:Once(callback) end
---Подождать пока запустится событие
---@return string rawLine #Новая необработанная строка из роблокс лога
function LogReader.NewRawLineEvent:Wait() end

--[[Запускается когда клиент отключился(кикнут или ошибка), без разрешение вместо эвента nil]]--* **[Требуются разрешения: read_logs.left_event]**

---@diagnostic disable-next-line: undefined-doc-name
---@type Event
LogReader.Disconnected = {}
---Параллельно подключиться к событию
---@param callback fun(reason: string)
function LogReader.Disconnected:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun(reason: string)
function LogReader.Disconnected:Once(callback) end
---Подождать пока запустится событие
---@return string reason #Причина отключения
function LogReader.Disconnected:Wait() end

--[[Запускается когда плейс телепортировал игрока на другой плейс]]--* **[Требуются разрешения: read_logs.teleport_events]**

---@diagnostic disable-next-line: undefined-doc-name
---@type Event
LogReader.Teleported = {}
---Параллельно подключиться к событию
---@param callback fun()
function LogReader.Teleported:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun()
function LogReader.Teleported:Once(callback) end
---Подождать пока запустится событие

function LogReader.Teleported:Wait() end

--[[Запускается при новой строке из роблокс лога, без разрешения вместо эвента nil]]--* **[Требуются разрешения: read_logs.all]**

---@diagnostic disable-next-line: undefined-doc-name
---@type Event
LogReader.NewLogLineEvent = {}
---Параллельно подключиться к событию
---@param callback fun(logLine: LogLine)
function LogReader.NewLogLineEvent:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun(logLine: LogLine)
function LogReader.NewLogLineEvent:Once(callback) end
---Подождать пока запустится событие
---@return LogLine logLine #Новая строка из роблокс лога
function LogReader.NewLogLineEvent:Wait() end

--[[Запускается когда клиент вышел из сервера, без разрешение вместо эвента nil]]--* **[Требуются разрешения: read_logs.left_event]**

---@diagnostic disable-next-line: undefined-doc-name
---@type Event
LogReader.Leaving = {}
---Параллельно подключиться к событию
---@param callback fun()
function LogReader.Leaving:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun()
function LogReader.Leaving:Once(callback) end
---Подождать пока запустится событие

function LogReader.Leaving:Wait() end

--[[Запускается когда плейс телепортирует игрока на другой плейс]]--* **[Требуются разрешения: read_logs.teleport_events]**

---@diagnostic disable-next-line: undefined-doc-name
---@type Event
LogReader.Telepoting = {}
---Параллельно подключиться к событию
---@param callback fun()
function LogReader.Telepoting:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun()
function LogReader.Telepoting:Once(callback) end
---Подождать пока запустится событие

function LogReader.Telepoting:Wait() end

--[[Запускается когда клиент захошёл на сервер, без разрешение вместо эвента nil]]--* **[Требуются разрешения: read_logs.joined_event]**

---@diagnostic disable-next-line: undefined-doc-name
---@type Event
LogReader.Joined = {}
---Параллельно подключиться к событию
---@param callback fun()
function LogReader.Joined:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun()
function LogReader.Joined:Once(callback) end
---Подождать пока запустится событие

function LogReader.Joined:Wait() end

--[[Запускается когда клиент заходит на сервер, без разрешение вместо эвента nil]]--* **[Требуются разрешения: read_logs.joined_event]**

---@diagnostic disable-next-line: undefined-doc-name
---@type Event
LogReader.Joining = {}
---Параллельно подключиться к событию
---@param callback fun()
function LogReader.Joining:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun()
function LogReader.Joining:Once(callback) end
---Подождать пока запустится событие

function LogReader.Joining:Wait() end