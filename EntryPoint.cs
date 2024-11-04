using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    void Awake()
    {
        Screen.SetResolution(600, 400, false);
        gameObject.AddComponent<GameManager>();
    }
}
