using Defib.Entity;
using Defib.Response;
using Defib.Security;
using System;
using System.Web.Http;

namespace Defib.Controllers
{
    public class ScriptController : ApiController
    {
        [HttpGet]
        public ApiResponse Run(string token, string script)
        {
            if (!Utils.IsTokenValid(token))
            {
                return new GenericResponse("InvalidToken", "[API ERROR] Invalid token.");
            }

            Context.LuaEngine["result"] = "Please set the `result` variable to the requested result, just as in regular scripting.";

            try
            {
                Context.LuaEngine.DoString(script);
            }
            catch (Exception e)
            {
                return new GenericResponse("ScriptError", e.Message);
            }

            return new GenericResponse("ScriptResult", Context.LuaEngine["result"].ToString());
        }
    }
}
