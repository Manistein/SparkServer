--[["
    desc: battle client 
    author: manistein
    since: 2019-03-28
 "]]

local skynet 	= require "skynet"
local cluster 	= require "cluster"
local sprotoloader  = require "sprotoloader"
local sprotocore    = require "sproto.core"

local sproto_c2s = nil
local sproto_s2c = nil

skynet.start(function()
	cluster.open("battlecli")
    skynet.error("hello battle server ===================")
    skynet.newservice("client")

	skynet.exit();
end)
