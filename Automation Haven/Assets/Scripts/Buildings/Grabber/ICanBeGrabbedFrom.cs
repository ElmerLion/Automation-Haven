using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICanBeGrabbedFrom {
    
    void GrabObject(ItemObject itemObject);
    ItemObject GetPotentialObject();

}
