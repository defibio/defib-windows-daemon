using Defib.Entity;
using System.Collections.Generic;

namespace Defib.Response
{
    public class HeartbeatResponse : ApiResponse
    {
        public List<Heartbeat> Message;

        public HeartbeatResponse()
        {
            this.Type = "HeartbeatList";
            this.Message = new List<Heartbeat>();
        }
    }
}
