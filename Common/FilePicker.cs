using SFB;

public class FilePicker
{
    public static byte[] Pick()
    {
        var path = StandaloneFileBrowser.OpenFilePanel("Model", "", "", false);
        if (path != null && path.Length > 0)
        {
            var bytes = System.IO.File.ReadAllBytes(path[0]);
            return bytes;
        }
        return null;
    }
}