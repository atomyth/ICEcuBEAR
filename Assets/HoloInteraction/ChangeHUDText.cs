using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeHUDText : MonoBehaviour {
    Text txt;
    string tmp = "";
	// Use this for initialization
	void Start () {
		txt = GetComponent<Text>();
		if(txt == null)
		    Debug.Log("Cannot get txt from ChangeHUDText class");
		else
		    txt.text = tmp;
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	public void changeHUD(string str)
	{
	    if(txt)
		    txt.text = str;
		else
		    tmp = str;
	}
	
}
