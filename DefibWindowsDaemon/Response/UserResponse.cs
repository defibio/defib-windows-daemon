using Defib.Entity;
using System.Collections.Generic;

namespace Defib.Response
{
    public class UserResponse : ApiResponse
    {
        public List<User> Message;

        public UserResponse()
        {
            this.Type = "UserList";
            this.Message = new List<User>();
        }
    }
}
