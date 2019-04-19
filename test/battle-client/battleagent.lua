--[["
    desc: battle client 
    author: manistein
    since: 2019-03-28
 "]]

local skynet 	= require "skynet"
local cluster 	= require "cluster"
local md5       = require "md5"

local index = ...
local proto_id = tonumber(index)
local char_table = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' }

local command = {}

function command.init()
	skynet.timeout(1, command.update)	
	math.randomseed(skynet.time())
end 

function command.update()
	local request = ""

	for i = 1, 65536 do 
		local random_idx = math.random(#char_table)
		request = request .. char_table[random_idx]
	end 

	local request_value = md5.sumhexa(request)
	skynet.error(string.format("index:%d time:%s request:%s", proto_id, skynet.time(), request_value))

	local proxy = cluster.proxy("battlesvr", "battletask")
	local result_index, response = skynet.call(proxy, "lua", proto_id, request)	
	skynet.error(string.format("---result_index:%s", result_index))
	local response_value = md5.sumhexa(response)

	skynet.error(string.format("index:%d response time:%s %d %s", proto_id, skynet.time(), result_index, response_value))
	assert(request_value == response_value, "fetal error, response is not equal to request")

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