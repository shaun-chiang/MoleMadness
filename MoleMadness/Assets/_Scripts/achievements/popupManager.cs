using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class popupManager : MonoBehaviour {

    public void close()
    {
        GameObject.Destroy(this.gameObject.transform.parent.gameObject);
    }
}
