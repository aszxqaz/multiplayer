using System.Collections.Generic;
using System.Threading.Tasks;
using GLTFast;
using Unity.XR.CoreUtils;
using UnityEngine;

public class Spawner
{
    private static List<GameObject> scenes = new();

    public static async Task<GameObject> SpawnModelFromGltf(GltfImport gltf, string checksum, bool isMultiplayer)
    {
        var success = await gltf.InstantiateSceneAsync(Locator.SpawnPoint().transform);
        if (!success)
        {
            Debug.LogError("Failed to instantiate scene");
            return null;
        }
        var scene = GameObjectHelpers.MustGetNamedChild(Locator.SpawnPoint(), "Scene");
        scene.name = checksum;
        scene.AddComponent<ModelScene>();
        if (isMultiplayer)
        {
            scene.AddComponent<ModelPublishable>();
            scene.AddComponent<ModelUpdatable>();

            var children = GameObjectHelpers.GetChildren(scene);
            foreach (var child in children)
            {
                child.AddComponent<ModelPublishable>();
                child.AddComponent<ModelUpdatable>();
            }
        }

        scenes.Add(scene);

        return scene;
    }

    public static async Task<GameObject> SpawnModelFromBytes(byte[] bytes, string checksum, bool isMultiplayer)
    {
        var gltf = new GltfImport();
        var success = await gltf.LoadGltfBinary(bytes);
        if (!success)
        {
            Debug.LogError("Failed to load binary GLTF");
            return null;
        }

        var scene = await SpawnModelFromGltf(gltf, checksum, isMultiplayer);
        return scene;
    }

    public static async Task<GameObject> SpawnScene(byte[] bytes, Scene scene)
    {
        var gltf = new GltfImport();
        var success = await gltf.LoadGltfBinary(bytes);
        if (!success)
        {
            Debug.LogError("Failed to load binary GLTF");
            return null;
        }

        var instantiated = await SpawnModelFromGltf(gltf, scene.Checksum, true);
        scene.ApplyToGameObject(instantiated);

        return instantiated;
    }

    public static void CreateSpawnPoint()
    {
        _ = new GameObject("SpawnPoint");
    }
}