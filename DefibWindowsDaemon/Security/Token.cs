using Defib.Entity;

namespace Defib.Security
{
    class Token
    {
        public int Id;
        public string Key;
        public User Authorized;
        public int Expires;
    }
}
