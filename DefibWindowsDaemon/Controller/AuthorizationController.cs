using Defib.Entity;
using Defib.Response;
using Defib.Security;
using System.Web.Http;

namespace Defib.Controllers
{
    public class AuthorizationController : ApiController
    {
        [HttpGet]
        public ApiResponse Login(string username, string password)
        {
            User localUser = Database.ValidateUser(username, password);

            if (localUser.Id == -1)
            {
                return new GenericResponse("InvalidCredentials", "Invalid username or password.");
            }

            Token localToken = new Token();
            localToken.Expires = Utils.GetCurrentTimestamp() + (3 * 3600);
            localToken.Authorized = localUser;
            localToken.Key = Utils.GenerateToken(localUser.Username);
            localToken.Id = localUser.Id;

            Context.Tokens.Add(localToken.Key, localToken);

            string responseType = localUser.Administrator ? "ValidatedAdminCredentials" : "ValidatedCredentials";

            return new GenericResponse(responseType, localToken.Key);
        }
    }
}
