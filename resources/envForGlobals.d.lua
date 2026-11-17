---@diagnostic disable: missing-return

---@param name string
---@param value any?
function _changeEnvironment(name,value) end

---@param class any
---@return {[string]:boolean|string} writeTypes
function classGetMembersWriteTypes(class) end

---@param err string
function _setLastError(err) end