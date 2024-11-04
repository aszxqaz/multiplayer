using UnityEngine;

public class Locator
{
    public static GameObject SpawnPoint() => GameObjectHelpers.MustFind("SpawnPoint");
}