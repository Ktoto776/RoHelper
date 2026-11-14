---@diagnostic disable: missing-return
---@diagnostic disable: inject-field

---@class Window
---@field MinimumSize PixelsSize #Минимальный размер окна в пикселях
---@field ClientSize PixelsSize #Размер внутренней области окна
---@field isBorderFixed boolean #Зафиксирована ли граница окна
---@field isVisible boolean #Видно ли окно
---@field Size PixelsSize #Размер окна в пикселях
---@field MaximazeBox boolean #Может ли пользователь увеличить окно на весь экран, игнорируется если установлен MaximumSize
---@field MaximumSize PixelsSize #Максимальный размер окна в пикселях
local Window={}

--[[Скрывает окно]]
function Window:Hide()end

--[[Запускается когда окно удалено]]
---@diagnostic disable-next-line: undefined-doc-name
---@type Event
Window.Destroying = {}
---Параллельно подключиться к событию
---@param callback fun()
function Window.Destroying:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun()
function Window.Destroying:Once(callback) end
---Подождать пока запустится событие

function Window.Destroying:Wait() end

--[[Запускается когда изменился размер внутренней области окна]]
---@diagnostic disable-next-line: undefined-doc-name
---@type Event
Window.ClientSizeChanged = {}
---Параллельно подключиться к событию
---@param callback fun()
function Window.ClientSizeChanged:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun()
function Window.ClientSizeChanged:Once(callback) end
---Подождать пока запустится событие

function Window.ClientSizeChanged:Wait() end

--[[Запускается когда видимость окна изменилась]]
---@diagnostic disable-next-line: undefined-doc-name
---@type Event
Window.VisibleChanged = {}
---Параллельно подключиться к событию
---@param callback fun(isVisible: boolean)
function Window.VisibleChanged:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun(isVisible: boolean)
function Window.VisibleChanged:Once(callback) end
---Подождать пока запустится событие
---@return boolean isVisible #Видно ли окно
function Window.VisibleChanged:Wait() end

--[[Показывает окно]]
function Window:Show()end

--[[Запускается когда изменился минимальный размер окна]]
---@diagnostic disable-next-line: undefined-doc-name
---@type Event
Window.MinimumSizeChanged = {}
---Параллельно подключиться к событию
---@param callback fun(MinimumSize: PixelsSize)
function Window.MinimumSizeChanged:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun(MinimumSize: PixelsSize)
function Window.MinimumSizeChanged:Once(callback) end
---Подождать пока запустится событие
---@return PixelsSize MinimumSize
function Window.MinimumSizeChanged:Wait() end

--[[Удаляет окно]]
function Window:Destroy()end

--[[Запускается когда изменился максимальный размер окна]]
---@diagnostic disable-next-line: undefined-doc-name
---@type Event
Window.MaximumSizeChanged = {}
---Параллельно подключиться к событию
---@param callback fun(MaximumSize: PixelsSize)
function Window.MaximumSizeChanged:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun(MaximumSize: PixelsSize)
function Window.MaximumSizeChanged:Once(callback) end
---Подождать пока запустится событие
---@return PixelsSize MaximumSize
function Window.MaximumSizeChanged:Wait() end

--[[Запускается когда изменился размер окна]]
---@diagnostic disable-next-line: undefined-doc-name
---@type Event
Window.SizeChanged = {}
---Параллельно подключиться к событию
---@param callback fun(Size: PixelsSize)
function Window.SizeChanged:Connect(callback) end
---Параллельно подключиться к событию, но на 1 запуск
---@param callback fun(Size: PixelsSize)
function Window.SizeChanged:Once(callback) end
---Подождать пока запустится событие
---@return PixelsSize Size
function Window.SizeChanged:Wait() end