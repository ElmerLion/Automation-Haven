using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CompanyListSO", menuName = "ScriptableObjects/CompanyListSO")]
public class CompanyListSO : ScriptableObject {
    
    public List<CompanySO> companies;

}
