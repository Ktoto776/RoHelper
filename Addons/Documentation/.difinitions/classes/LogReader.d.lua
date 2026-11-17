---@diagnostic disable: inject-field
---@diagnostic disable: missing-return

---@class LogReader
local LogReader={}

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

--[[Запускается когда клиент вышел из игры в основное меню, без разрешение вместо эвента nil]]--* **[Требуются разрешения: read_logs.left_event]**

---@diagnostic disable-next-line: undefined-doc-name
---@type Event
LogReader.LeftEvent = {}
---Параллельно подключиться к событию
---@param callback fun()
function LogReader.LeftEvent:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun()
function LogReader.LeftEvent:Once(callback) end
---Подождать пока запустится событие

function LogReader.LeftEvent:Wait() end

--[[Запускается когда клиент заходит на сервер, без разрешение вместо эвента nil]]--* **[Требуются разрешения: read_logs.joined_event]**

---@diagnostic disable-next-line: undefined-doc-name
---@type Event
LogReader.JoinedToPlace = {}
---Параллельно подключиться к событию
---@param callback fun()
function LogReader.JoinedToPlace:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun()
function LogReader.JoinedToPlace:Once(callback) end
---Подождать пока запустится событие

function LogReader.JoinedToPlace:Wait() end

--[[Запускается когда клиент вышел с сервера без ошибок, без разрешение вместо эвента nil]]--* **[Требуются разрешения: read_logs.left_event]**

---@diagnostic disable-next-line: undefined-doc-name
---@type Event
LogReader.SuccessLeftEvent = {}
---Параллельно подключиться к событию
---@param callback fun()
function LogReader.SuccessLeftEvent:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun()
function LogReader.SuccessLeftEvent:Once(callback) end
---Подождать пока запустится событие

function LogReader.SuccessLeftEvent:Wait() end

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