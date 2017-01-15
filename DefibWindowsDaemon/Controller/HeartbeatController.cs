using Defib.Entity;
using Defib.Response;
using Defib.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;

namespace Defib.Controllers
{
    public class HeartbeatController : ApiController
    {
        [HttpGet]
        public ApiResponse List(string token)
        {
            if (!Utils.IsTokenValid(token))
            {
                return new GenericResponse("InvalidToken", "[API ERROR] Invalid token.");
            }

            HeartbeatResponse response = new HeartbeatResponse();
            foreach (KeyValuePair<int, Heartbeat> pairs in Context.Heartbeats)
            {
                Heartbeat tempBeat = (Heartbeat) pairs.Value.Clone();
                Console.WriteLine(tempBeat.Script);
                tempBeat.Script = File.ReadAllText("scripts/" + tempBeat.Script);

                response.Message.Add(tempBeat);
            }

            return response;
        }

        [HttpGet]
        public ApiResponse Create(string token, string name, string key, int interval, string script)
        {
            if (!Utils.IsTokenValid(token))
            {
                return new GenericResponse("InvalidToken", "[API ERROR] Invalid token.");
            }

            Heartbeat tempBeat = new Heartbeat();
            tempBeat.Name = name;
            tempBeat.Key = key;
            tempBeat.Interval = interval;
            tempBeat.LastBeat = -1;
            tempBeat.NextBeat = -1;
            tempBeat.Script = "inst_" + Utils.GetCurrentTimestamp() + ".lua";

            if (!Directory.Exists("scripts"))
            {
                Directory.CreateDirectory("scripts");
            }

            Stream fStream = File.Create("scripts/" + tempBeat.Script);
            fStream.Close();

            File.WriteAllText("scripts/" + tempBeat.Script, script);

            Database.SaveHeartbeat(tempBeat);
            Context.Heartbeats.Clear();
            Database.LoadHeartbeats();

            return new GenericResponse("CreatedHeartbeat", "Success.");
        }

        [HttpGet]
        public ApiResponse Update(string token, int id, string name, string key, int interval, string script)
        {
            if (!Utils.IsTokenValid(token))
            {
                return new GenericResponse("InvalidToken", "[API ERROR] Invalid token.");
            }

            Heartbeat tempBeat = Context.Heartbeats[id];
            tempBeat.Name = name;
            tempBeat.Key = key;
            tempBeat.Interval = interval;

            File.Delete("scripts/" + tempBeat.Script);

            Stream fStream = File.Create("scripts/" + tempBeat.Script);
            fStream.Close();

            File.WriteAllText("scripts/" + tempBeat.Script, script);

            Database.SaveHeartbeat(tempBeat);
            Context.Heartbeats.Clear();
            Database.LoadHeartbeats();

            return new GenericResponse("UpdatedHeartbeat", "Success.");
        }

        [HttpGet]
        public ApiResponse Delete(string token, int id)
        {
            if (!Utils.IsTokenValid(token))
            {
                return new GenericResponse("InvalidToken", "[API ERROR] Invalid token.");
            }

            Database.DeleteHeartbeat(id);

            Context.Heartbeats.Clear();
            Database.LoadHeartbeats();

            return new GenericResponse("DeletedHeartbeat", "Success.");
        }
    }
}
