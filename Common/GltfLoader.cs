using System.Threading.Tasks;
using GLTFast;

public class GltfLoader
{
    public static async Task<GltfImport> Load(byte[] bytes)
    {
        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary(bytes);
        if (!success) return null;
        return gltf;
    }
}