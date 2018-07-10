using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class ModelBaseAccess : MonoBehaviour, IInputClickHandler{
    private bool isLarge = false;
    private bool isBanana = false;
    private float bananaSize = 1f;
    private float bananaStep;

    // Use this for initialization
    void Start ()
	{		
	}
	
	// Update is called once per frame
	void Update ()
	{	
	    if(isBanana)
	    {
	        float currSize = transform.localScale.x;
	        if(currSize < bananaSize)
                transform.localScale = Vector3.one * (currSize + bananaStep * Time.deltaTime);
        }   
	}
	
	
    public void ActionChangeSize()
    {   
        isLarge = !isLarge;
        isBanana = false;
        transform.localScale = Vector3.one * (isLarge?0.01f:0.0005f);
    }

    public void TokyoBanana()
    {
        isBanana = true;
        bananaStep = Mathf.Abs(bananaSize - transform.localScale.x) / 5f;
    }

    public void OnInputClicked(InputEventData eventData)
    {
        ActionChangeSize();
    }
}
