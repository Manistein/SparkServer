--[["
    desc: battle client 
    author: manistein
    since: 2019-03-28
 "]]

local skynet 	= require "skynet"
local cluster 	= require "cluster"

skynet.start(function()
	cluster.open("battlecli")

	for i = 1, 10 do 
		skynet.newservice("battleagent", i)
	end 

	skynet.exit();
end)