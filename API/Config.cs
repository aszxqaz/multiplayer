public class ApiConfig
{
    public static readonly string HttpBaseUrlDev = "http://localhost:5000";
    public static readonly string HttpBaseUrlProd = "https://your-production-api-url.com";
    public static readonly string WsBaseUrlDev = "ws://localhost:5000/ws";
    public static readonly string WsBaseUrlProd = "wss://your-production-api-url.com/ws";


    public static string GetHttpBaseUrl()
    {
        return Config.IsDevelopment ? HttpBaseUrlDev : HttpBaseUrlProd;
    }

    public static string GetWsBaseUrl()
    {
        return Config.IsDevelopment ? WsBaseUrlDev : WsBaseUrlProd;
    }
}
