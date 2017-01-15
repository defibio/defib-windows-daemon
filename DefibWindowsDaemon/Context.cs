using Defib.Entity;
using System.Collections.Generic;
using NLua;

namespace Defib
{
    public static class Context
    {
        public static Dictionary<int, Heartbeat> Heartbeats;
        public static Dictionary<int, User> Users;

        public static Lua LuaEngine;

        public static void Initialize()
        {
            Heartbeats = new Dictionary<int, Heartbeat>();
            Users = new Dictionary<int, User>();

            LuaEngine = new Lua();
        }
    }
}
