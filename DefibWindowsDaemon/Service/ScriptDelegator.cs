using Defib.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Defib.Service
{
    public class ScriptDelegator
    {
        public static void Initialize()
        {
            Network networkEngine = new Network();

            Context.LuaEngine.RegisterFunction("is_port_listening", networkEngine, networkEngine.GetType().GetMethod("IsPortListening"));
            Context.LuaEngine.RegisterFunction("is_server_online", networkEngine, networkEngine.GetType().GetMethod("IsServerOnline"));
        }
    }
}
