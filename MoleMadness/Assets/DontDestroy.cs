using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour {

    private static DontDestroy instance = null;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
