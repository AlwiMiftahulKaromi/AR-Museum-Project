using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Insect", menuName = "Insect")]
public class Insect : ScriptableObject
{
	public GameObject insectPrefab;
	public int insectKey;
	public string insectDescription;
}
