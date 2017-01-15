using Defib.Entity;
using System.Collections.Generic;
using NLua;
using Defib.Security;

namespace Defib
{
    public static class Context
    {
        public static Dictionary<int, Heartbeat> Heartbeats;
        public static Dictionary<int, User> Users;
        public static Dictionary<string, Token> Tokens;

        public static Lua LuaEngine;

        public static void Initialize()
        {
            Heartbeats = new Dictionary<int, Heartbeat>();
            Users = new Dictionary<int, User>();
            Tokens = new Dictionary<string, Token>();

            LuaEngine = new Lua();
        }
    }
}
