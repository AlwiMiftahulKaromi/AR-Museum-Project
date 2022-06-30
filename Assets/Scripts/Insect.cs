using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Insect", menuName = "MuseumProject/Add Insect", order = 0)]
public class Insect : ScriptableObject
{
    public GameObject insectPrefab;

    public string insectFamily;
    public string insectDescription;
}
// {
//     new public string insectFamily = "Default Family";
//     public string insectDescription = "Default Description";

//     public virtual string GetInsectDescription()
//     {
//         return insectDescription;
//     }
// }
