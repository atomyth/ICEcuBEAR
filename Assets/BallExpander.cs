using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallExpander : MonoBehaviour
{
    private static Config cfg = new Config();
    private float sizeCf = 10f;
	public Gradient colorLine;
	
	private float startTime;
	private float signal = 0;
	private float lastSignal = 0;
	private float sumSignal = 0; //sum up if more than one pulse
	private float width = 1;
    private bool hasPulse = false;
	
	private float eventStart;
	
	private float _ballSz;
	MeshRenderer rnd;
	MaterialPropertyBlock props;
    Mesh msh;
     List<Vector2> uvs;
	// Use this for initialization
	void Start () {
		rnd=gameObject.GetComponent<MeshRenderer>();
		props=new MaterialPropertyBlock();
	}
	
    public BallExpander Initialize (float eventStart, float eventEnd) {
		Debug.Log("Initialising prefab ball with event times " + eventStart.ToString() + " " + eventEnd.ToString());
		this.eventStart = eventStart;
        msh = gameObject.GetComponent<MeshFilter>().mesh;
        uvs = new List<Vector2>();
        for (int i = 0; i < msh.vertexCount; i++)
            uvs.Add(new Vector2());
	    if(props==null) 
	        props=new MaterialPropertyBlock();
	    
	    rnd=gameObject.GetComponent<MeshRenderer>();
		if(rnd!=null)
		{
		    //props.SetColor("_color",colorLine.Evaluate(0));
			//rnd.material.SetColor("_color",colorLine.Evaluate(0));
			//rnd.SetPropertyBlock(props);
            msh.SetUVs(0, uvs);
            this._ballSz = this.sizeCf * cfg.globalScale;
            Vector3 sz = new Vector3(1, 1, 1);
            gameObject.transform.localScale = sz * this._ballSz;
        }
	    return this;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!this.hasPulse)
            return;

        float t = Time.time;

        if(t < this.startTime)
            return;
                  
		
		if(rnd!=null)
		{
        	Vector3 sz=new Vector3(1,1,1);
		    float inPulseTime = t - this.startTime;
		    float inPulseRatio = this.width>0f ? Mathf.Clamp(inPulseTime / this.width,0f,1f) : 0f;
		    float pulseEvo = 1f / (1f+Mathf.Exp(-2f*(inPulseRatio-0.5f))); //error function and shift from -1 to 1
		    //-2f * Mathf.Abs(inPulseRatio - 0.5f) + 1f; // from 0 to 1 to 0, linearly
		    //Debug.Log(pulseEvo.ToString() + " " + this.signal.ToString());
		    float radius = Mathf.Pow(0.2f * (this.sumSignal + this.signal * pulseEvo),0.8f); //everything above pow1 is dominated by 3 pmts
		    this._ballSz = (this.sizeCf + radius) * cfg.globalScale;
		    //Debug.Log("The signal " + this.signal + " the pulse " + pulseEvo + " the size" + this._ballSz.ToString());
		    gameObject.transform.localScale = sz * this._ballSz;

		}

        if (t > this.startTime + this.width)
        {
            this.hasPulse = false;
        }
        return;
    }
	public void RegisterPulse(float startTime, float width, float signal)
	{
	    if(width<=0) return;
	    if(startTime<this.eventStart) return;
	    // Debug.Assert(width > 0, "Width cannot be " + width.ToString());
	    // Debug.Assert(startTime > this.eventStart, "Start pulse before event " + startTime.ToString());
	    // debug prints/ string to float conversions are slow at such scale. 
	    this.sumSignal += lastSignal;
		this.lastSignal = this.signal;
		this.signal = signal;
		
		this.width = width*50f;//FIXME cheating
		this.startTime = startTime;
        this.hasPulse = true;
		
		
		MeshRenderer rnd=gameObject.GetComponent<MeshRenderer>();
		if(this.sumSignal == 0 && rnd!=null) //colour determined by first hit
		{
		    float tim=(startTime - this.eventStart) / (6000f / cfg.timeScale);
            Debug.Log(tim);
            uvs.Clear();
            for (int i = 0; i < msh.vertexCount; i++)
                uvs.Add(new Vector2(Mathf.Clamp(tim,0,1.0f), 0.5f));

            msh.SetUVs(0, uvs);
            //tim = Mathf.Floor(tim*25.0f)/25.0f;
	        //props.SetColor("_color",colorLine.Evaluate(tim));
            
            //)
            //      rnd.SetPropertyBlock(props);
			//nd.material.SetColor("_color",colorLine.Evaluate((startTime - this.eventStart) / (6000f / cfg.timeScale))); //FIXME, can event length be determined from file?
		}
	}
}
