using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Insect : MonoBehaviour
{
    new public string insectFamily = "Default Family";
    public string insectDescription = "Default Description";

    public virtual string GetInsectDescription()
    {
        return insectDescription;
    }
}
