using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Insect", menuName = "Insect")]
public class Insect : ScriptableObject
{
	public GameObject insectPrefab;
	public string insectSpecies;
	public string insectFamily;
	public string insectDescription;
}
