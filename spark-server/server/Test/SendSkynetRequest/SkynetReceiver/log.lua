--[["
    desc: 日志辅助类
    author: wuyinjie
    since: 2017-01-08
 "]]

local skynet   = require "skynet" -- 不能引用framework,会有循环引用问题
local machine  = require "adapter"
local const    = require "common.sharedefine.const"

local pcall    = pcall
local format   = string.format
local error    = skynet.error
local tostring = tostring

local log = {}

function log.format(...)
    local ok, str = pcall(format, ...)
    if not ok then
        return tostring(...) .. ":" .. str
    else
        return str
    end
end

function log.debug(...)
    error("[debug] "..log.format(...))
end

function log.info(...)
    error("[info] "..log.format(...))
end

function log.warning(...)
    error("[warning] "..log.format(...))
end

function log.error(...)
    error("[error] "..log.format(...))
end

function log.fatal(...)
    error("[fatal] "..log.format(...))
end

function log.print(...)
    error(...)
end

function log.print_tbl(root)
    if root == nil then
        return log.info("PRINT_T root is nil")
    end
    if type(root) ~= type({}) then
        return log.info("PRINT_T root not table type")
    end
    if not next(root) then
        return log.info("PRINT_T root is space table")
    end

    local cache = { [root] = "." }
    local function _dump(t,space,name)
        local temp = {}
        for k,v in pairs(t) do
            local key = tostring(k)
            if cache[v] then
                table.insert(temp,"+" .. key .. " {" .. cache[v].."}")
            elseif type(v) == "table" then
                local new_key = name .. "." .. key
                cache[v] = new_key
                table.insert(temp,"+" .. key .. _dump(v,space .. (next(t,k) and "|" or " " ).. string.rep(" ",#key),new_key))
            else
                table.insert(temp,"+" .. key .. " [" .. tostring(v).."]")
            end
        end
        return table.concat(temp,"\n"..space)
    end
    log.print(_dump(root, "",""))
end

return log