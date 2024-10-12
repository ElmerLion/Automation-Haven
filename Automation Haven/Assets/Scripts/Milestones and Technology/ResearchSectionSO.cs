using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResearchSection", menuName = "ScriptableObjects/ResearchSectionSO")]
public class ResearchSectionSO : ScriptableObject {

    public string sectionIndex;
    public ResearchManager.ResearchCategory researchCategory;
    public List<ResearchNodeSO> researchNodes;

}
