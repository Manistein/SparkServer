--[["
desc: client
author: zhangzhijie
since: 2019-05-09
"]]

local socket		= require "socket"
local skynet		= require "skynet"
local sprotoloader	= require "sprotoloader"
local sprotocore	= require "sproto.core"
local const         = require "const"

local sproto_s2ss = nil

local function encode_data(proto_name, lua_table)
    local sproto_stream = sproto_s2ss:request_encode(proto_name, lua_table)
	local proto_id = 100
    local sproto_length = #sproto_stream
    local sproto_data = string.pack("<I2c" .. tostring(sproto_length), proto_id, sproto_stream)

    local package = string.pack(">s2", sproto_data)
    return package
end

local function decode_data(package)
    local size = #package - 2
    local proto_id, body = string.unpack("<I2c" .. tostring(size), package)
	local proto_name = sprotocore.protocol(sproto_s2ss.__cobj, proto_id)
	return proto_name, sproto_s2ss:response_decode(proto_id, body)
end

function new_client() 
    local client_info = {}
    local fd = nil

    client_info.connect = function(ip, port)
        fd = socket.open(ip, port)
        assert(fd, "can not open connect")
    end

    client_info.close = function()
        assert(fd)
        socket.close(fd)
        fd = nil
    end

    client_info.read_package = function()
        local size_buf = socket.read(fd, 2)
        local size = string.unpack("<I2", size_buf)
        local data_buf = socket.read(fd, size)
        local proto_name, data = decode_data(data_buf)
        return proto_name, data
    end

    client_info.send_package = function(proto_name, send_data)
        local package = encode_data(proto_name, send_data)
        socket.write(fd, package)
    end

    return client_info
end

function request_one(client_info, proto_name, ...)
    local send_data = request.get_request_info(proto_name, ...)
    client_info.send_package(proto_name, send_data)
end

function read_one_data(client_info)
    local proto_name, data = client_info.read_package()
    pbc.extract(data)

    skynet.error("request_one result proto_name:%s, data:%s ", proto_name, cjson.encode(data))
    return proto_name, data
end

function read_all_data(client_info)
    framework.sleep(100)
    read_one_data(client_info)
    read_all_data(client_info)
end

local end_role_id	= 2
local current_id	= 1

function begin_connection()
    if current_id >= end_role_id then
        return
    end

    current_id = current_id + 1
    local client_info = new_client()
    client_info.connect("192.168.210.124", 8888)
    skynet.error("connect spark server success")

    local send_data = {
        method = "test",
        param = "hello spark server"
    }

    client_info.send_package("RPC", send_data)
    read_one_data(client_info)
    client_info.close()
end

skynet.start(function ()
    skynet.newservice("sprotoboot")
    sproto_s2ss = sprotoloader.load(const.SPROTO_LOADER_S2SS)

    begin_connection()
end)
