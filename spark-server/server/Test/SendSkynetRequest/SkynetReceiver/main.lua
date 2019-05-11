--[["
    desc: battle client 
    author: manistein
    since: 2019-03-28
 "]]

local skynet 	    = require "skynet"
local cluster 	    = require "cluster"
local sprotoloader  = require "sprotoloader"
local sprotoparser  = require "sprotoparser"
local sprotocore    = require "sproto.core"

local sproto_c2s = nil
local sproto_s2c = nil

local file_name = "../../SendSkynetRequest/Resource/RPCProtoSchema/SkynetMessageSender.sproto"

local function read_file(file_name)
    local file_handle = assert(io.open(file_name))
    local content = file_handle:read("*a")
    file_handle:close()
    return content
end

skynet.start(function()
    sprotoloader.save(sprotoparser.parse(read_file(file_name)), 1)

	cluster.open("testclient")
    skynet.error("hello battle server ===================")
    skynet.newservice("client")
end)
