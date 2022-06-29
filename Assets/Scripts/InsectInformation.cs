using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InsectInformation : MonoBehaviour
{
	public Text insectName;
	public Text insectDescription;

	public void SetUp(string name, string description)
	{
		insectName.text = name;
		insectDescription.text = description;
	}
}
