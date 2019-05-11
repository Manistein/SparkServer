--[["
    desc: battle client 
    author: manistein
    since: 2019-03-28
 "]]

local skynet    = require "skynet"
local cluster   = require "cluster"
local md5       = require "md5"
local crypt     = require "skynet.crypt"
local sprotoloader = require "sprotoloader"
local sprotocore = require "sproto.core"

local sprotoreq = nil

local function print_tbl(root)
    if root == nil then
        return skynet.error("PRINT_T root is nil")
    end
    if type(root) ~= type({}) then
        return skynet.error("PRINT_T root not table type")
    end
    if not next(root) then
        return skynet.error("PRINT_T root is space table")
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
    skynet.error(_dump(root, "",""))
end

local command = {}

function command.init()
    skynet.timeout(1, command.update)   
    math.randomseed(skynet.time())

    sprotoreq = sprotoloader.load(1)
end 

function command.update()
    local rpcparam = sprotoreq:encode("SkynetMessageReceiver_OnProcessRequest", { request_count = math.floor(skynet.time()), request_text = "hahahaha hohohoho xixixixi" })

    local content, proto_id = sprotoreq:request_encode("RPC", { method = "OnProcessRequest", param = crypt.base64encode(rpcparam) })

    local proxy = cluster.proxy("testserver", "RecvSkynetSend")
    skynet.error(string.format(">>>>>>>>>>>>>>>>>content len:%d", string.len(content)))
    local result_index, response = skynet.call(proxy, "lua", proto_id, content) 
    skynet.error(string.format("<<<<<<<<<<<<<<<<<result_index:%d response:%d", result_index, string.len(response))) 

    local tbl, name = sprotoreq:response_decode(result_index, response)
    print_tbl(tbl)

    local response_rpcparam = sprotoreq:decode("SkynetMessageReceiver_OnProcessRequestResponse", crypt.base64decode(tbl.param))
    skynet.error(string.format("response method:%s request_count:%d param:%s", tbl.method, response_rpcparam.request_count, response_rpcparam.request_text))

    skynet.timeout(500, command.update) 
end

skynet.start(function()
    skynet.dispatch("lua", function(session, source, cmd, ...)
        local func = command[cmd]
        if func then 
            func(...)
        end 
    end)    

    command.init()
end)