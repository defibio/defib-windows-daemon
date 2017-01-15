using Defib.Entity;
using Defib.Response;
using Defib.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;

namespace Defib.Controllers
{
    public class UserController : ApiController
    {
        [HttpGet]
        public ApiResponse List(string token)
        {
            if (!Utils.IsTokenValid(token) || Context.Tokens[token].Authorized.Administrator == false)
            {
                return new GenericResponse("InvalidToken", "[API ERROR] Invalid token.");
            }

            UserResponse response = new UserResponse();
            foreach (KeyValuePair<int, User> pairs in Context.Users)
            {
                response.Message.Add(pairs.Value);
            }

            return response;
        }

        [HttpGet]
        public ApiResponse Create(string token, string username, string password, int admin)
        {
            if (!Utils.IsTokenValid(token) || Context.Tokens[token].Authorized.Administrator == false)
            {
                return new GenericResponse("InvalidToken", "[API ERROR] Invalid token.");
            }

            User tempUser = new User();
            tempUser.Username = username;
            tempUser.Password = Utils.HashPassword(password, tempUser.Salt, tempUser.Username);
            tempUser.Administrator = admin == 1 ? true : false;

            Database.SaveUser(tempUser);
            Context.Users.Clear();
            Database.LoadUsers();

            return new GenericResponse("CreateUser", "Success.");
        }

        [HttpGet]
        public ApiResponse Update(string token, int id, string password, int admin)
        {
            if (!Utils.IsTokenValid(token) || Context.Tokens[token].Authorized.Administrator == false)
            {
                return new GenericResponse("InvalidToken", "[API ERROR] Invalid token.");
            }

            User tempUser = Context.Users[id];
            tempUser.Password = Utils.HashPassword(password, tempUser.Salt, tempUser.Username);
            tempUser.Administrator = admin == 1 ? true : false;

            Database.SaveUser(tempUser);
            Context.Users.Clear();
            Database.LoadUsers();

            return new GenericResponse("UpdatedUser", "Success.");
        }

        [HttpGet]
        public ApiResponse Password(string token, string password)
        {
            if (!Utils.IsTokenValid(token))
            {
                return new GenericResponse("InvalidToken", "[API ERROR] Invalid token.");
            }

            User tempUser = Context.Tokens[token].Authorized;
            tempUser.Password = Utils.HashPassword(password, tempUser.Salt, tempUser.Username);

            Database.SaveUser(tempUser);
            Context.Users.Clear();
            Database.LoadUsers();

            return new GenericResponse("UpdatedPasswordUser", "Success.");
        }

        [HttpGet]
        public ApiResponse Delete(string token, int id)
        {
            if (!Utils.IsTokenValid(token) || Context.Tokens[token].Authorized.Administrator == false)
            {
                return new GenericResponse("InvalidToken", "[API ERROR] Invalid token.");
            }

            Database.DeleteUser(id);

            Context.Users.Clear();
            Database.LoadUsers();

            return new GenericResponse("DeletedUser", "Success.");
        }
    }
}
