public class Error
{
    public string Message { get; set; }
    public int Code { get; set; }
    public Error(int code, string message)
    {
        Message = message;
        Code = code;
    }
}