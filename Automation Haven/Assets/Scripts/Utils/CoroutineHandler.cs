using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHandler : MonoBehaviour {
    private static CoroutineHandler _instance;

    public static CoroutineHandler Instance {
        get {
            if (_instance == null) {
                GameObject obj = new GameObject("CoroutineHandler");
                _instance = obj.AddComponent<CoroutineHandler>();
            }
            return _instance;
        }
    }

    public void StartHandledCoroutine(IEnumerator coroutine) {
        StartCoroutine(coroutine);
    }
}
