using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasProgress {

    event Action OnProgressChanged;

    float GetProgressNormalized();
    float GetMaxProgress();
 
}
