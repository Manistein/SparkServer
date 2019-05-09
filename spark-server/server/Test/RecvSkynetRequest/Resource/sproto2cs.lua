local lfs = require "lfs"

local sproto_path = ".\\RPCProtoSchema\\"

local dump_cs_path = ".\\RPCProtoCS\\"

local lua_exe_path = "..\\..\\..\\Framework\\Tools\\sproto2cs\\lib\\lua.exe"

local function is_sproto_file(file_name)
    return string.match(file_name, ".sproto") == ".sproto"
end

local function main()
    local sproto_file_list = ""
    for file_name in lfs.dir(sproto_path) do
        if is_sproto_file(file_name) then
            sproto_file_list = sproto_file_list .. " " .. sproto_path ..file_name
        end
    end
    os.execute(lua_exe_path .. " ..\\..\\..\\Framework\\Tools\\sproto2cs\\sprotodump\\sprotodump.lua -cs " .. sproto_file_list .. " -d " .. dump_cs_path .. " -p Net")
    os.remove(".\\RPCProtoCS\\NetProtocol.cs")
end

main()