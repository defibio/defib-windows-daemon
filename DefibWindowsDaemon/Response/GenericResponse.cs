namespace Defib.Response
{
    public class GenericResponse : ApiResponse
    {
        public string Message;

        public GenericResponse(string type, string message)
        {
            this.Type = type;
            this.Message = message;
        }
    }
}
