using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if WINDOWS_UWP
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using System;
#endif

public class Config
{
    public float domScale = 300f;
    public float globalScale = 0.01f;
    
    public float timeScale = 800f; //speed up factor (more = faster)
    public float startDelay = 5f; //in s
    
    public float eventStart = 0f;
    public float eventEnd = 1e9f;
};

public class Pulse
{
    public float width;
    public float signal;
    public int domID;
    public int stringID;
    public Pulse(float width, float signal, int domID, int stringID)
    {
        this.width = width;
        this.signal = signal;
        this.domID = domID;
        this.stringID = stringID;
    }
}

//DOM is simply the name used in unity and does not refer to IceCube DOM
public class DOM : MonoBehaviour
{
    private static Config cfg = new Config();
    public Mesh mesh;
    public Mesh cylMesh;
	public Mesh ballMesh;
    public Material material;
    public float firstEventTime;
    public Dictionary<int, I3String> strings = new Dictionary<int, I3String>();
    public List<float> pulseTimes = new List<float>();
    public List<Pulse> pulses = new List<Pulse>();

#if WINDOWS_UWP
    public async Task<IList<string>> GetLinesAsync(string path)
        {
            StorageFile textFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///" + path + "")); //, UriKind.Relative));
            var readFile = await FileIO.ReadLinesAsync(textFile);
            return readFile;
        }
#endif

    private void Start() {
        Debug.Log("IceCube start called.");

        int counter = 0;
        this.firstEventTime = -1f;

        ////////////// Read the detector config file
#if WINDOWS_UWP
        {
            var result1 = GetLinesAsync("Assets/detector_config.txt").Result;
            foreach (var line in result1)
#else

        var fileStream = new FileStream(@"Assets/detector_config.txt", FileMode.Open, FileAccess.Read);
        using (var streamReader = new System.IO.StreamReader(fileStream, System.Text.Encoding.UTF8))
        {
            string line;
            while ((line = streamReader.ReadLine()) != null)
#endif
            {
                string[] fields = line.Split(' ');
                int stringID = int.Parse(fields[0]);
                int domID = int.Parse(fields[1]);
                float x = float.Parse(fields[2]);
                float y = float.Parse(fields[3]);
                float z = float.Parse(fields[4]) + 550f; //move icecube coord system by 550 m
                
                Vector3 pos = Vector3.right * x * cfg.globalScale
                    + Vector3.forward * y * cfg.globalScale
                    + Vector3.up * z * cfg.globalScale;
                
                //create new string if it doesn't exist yet
                if(!strings.ContainsKey(stringID))
                {
                    I3String theString = new GameObject("String " + stringID).AddComponent<I3String>().
                            Initialize(this, stringID, pos);
                    strings.Add(stringID, theString);
                }
                    
                strings[stringID].AddDOM(this, domID, pos);
	                        
                counter++;
            }
        }
        //        fileStream.Close();
        
        
        foreach (KeyValuePair<int, I3String> iString in strings)
        {
            iString.Value.Finalise();
        }
        
        Debug.Log("Read " + counter.ToString() + " DOMs from file.");
        
        
                
        /////////// Read event file
#if WINDOWS_UWP
        {
            var resultP = GetLinesAsync("Assets/event.txt").Result;
            foreach (var line in resultP)
#else

        var fileStreamP = new FileStream(@"Assets/event.txt", FileMode.Open, FileAccess.Read);
        using (var streamReaderP = new System.IO.StreamReader(fileStreamP, System.Text.Encoding.UTF8))
        {
            string line;
            while ((line = streamReaderP.ReadLine()) != null)
#endif
            {
                //check string dom FIXME
                string[] fields = line.Split(',');
                if(this.firstEventTime == -1f)
                    this.firstEventTime = float.Parse(fields[0]);
                float time = cfg.startDelay + (float.Parse(fields[0]) - this.firstEventTime) / cfg.timeScale;
                //Debug.Log(line);
                float width = float.Parse(fields[1]) / cfg.timeScale;
                int domID = int.Parse(fields[2]);
                int stringID = int.Parse(fields[3]);
                float signal = float.Parse(fields[4]);
                //Debug.Log("Pulse time " + time + " width" + width);
                
                        
                try
                {
                    pulses.Add(new Pulse(width, signal, domID, stringID));
                    pulseTimes.Add(time);
                }
                catch (System.ArgumentException)
                {
                    Debug.Log("Ignoring pulse at event time = " + time.ToString());
                }
                
            }
        }
        cfg.eventStart = pulseTimes[0];
        cfg.eventEnd = pulseTimes[pulseTimes.Count - 1];
        foreach(var str in strings)
        {
	 str.Value.reinitDomsWithTimes(  cfg.eventStart ,cfg.eventEnd);
	}
        Debug.Log("Set event start/end time to " + cfg.eventStart.ToString() + "/" + cfg.eventEnd.ToString());
    }
    
    void Update () {
        float t = Time.time;
        float tp = pulses.Count != 0 ? pulseTimes[0] : 1e9f;
        //Debug.Log(pulses.Count.ToString() + " Curr time " + t.ToString() + " next pulseTime" + tp.ToString());
        while(t >= tp)
        {
            if(pulses.Count == 0)
                break;
                
            var p = pulses[0];
            tp = pulseTimes[0];
            pulses.RemoveAt(0);
            pulseTimes.RemoveAt(0);
            strings[p.stringID].RegisterPulse(p.domID, tp, p.width, p.signal);
        }
        
    }
}

public class Dom : MonoBehaviour {
    private static readonly Config cfg = new Config();
    public int id;
    private Mesh mesh;
    private Material material;
    private float radius = 0.01651f;
    private int depth;

    // Use this for initialization
    private void Start() {
        //Debug.Log("PMT start called.");
        //now in ball?
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        //gameObject.AddComponent<MeshRenderer>().material = material;
    }

    public Dom Initialize (DOM parent, int id, Vector3 pos) {
        this.id = id;
        this.mesh = parent.ballMesh;
        //this.material = parent.material;
        this.radius *= cfg.domScale * cfg.globalScale;
        transform.parent = parent.transform;
        transform.localScale = Vector3.one * radius; //relative
        transform.localPosition = pos;
        return this;
    }


    //private IEnumerator CreateChildren () {
    //    yield return new WaitForSeconds(0.5f);
    //}

    // Update is called once per frame
    void Update () {

    }
}

public class I3String : MonoBehaviour
{
    private static readonly Config cfg = new Config();
    public int id;
    private Mesh mesh;
    private Material material;
    private float maxZ = -1e9f;
    private float minZ = 1e9f;
    private float radius = 0.005f; //5cm??
    public Dictionary<int, Dom> doms = new Dictionary<int, Dom>();
    private BallExpander prefab;

    // Use this for initialization
    private void Start() {
        Debug.Log("String start called.");
        gameObject.AddComponent<MeshFilter>().mesh = mesh;//PrimitiveType.Cylinder;
        gameObject.AddComponent<MeshRenderer>().material = material;
    }
    
    public I3String Initialize (DOM parent, int id, Vector3 pos) {
        this.id = id;
        this.mesh = parent.cylMesh;
        this.material = parent.material;
        this.radius *= cfg.domScale * cfg.globalScale;
        transform.parent = parent.transform;
        pos.y = 0; //y is up, seems so  
        transform.localPosition = pos;
        return this;
    }
    
    public void Finalise() {
        float length = this.maxZ - this.minZ;
        transform.localScale = Vector3.one * this.radius + Vector3.up * length/2f; //relative
        transform.localPosition = transform.localPosition + Vector3.up * (length/2f + this.minZ);
    }

    
    public void AddDOM(DOM parent, int domID, Vector3 pos)
    {
		if(prefab==null)
		{
			prefab=Resources.Load<BallExpander>("Prefabs/Ball");
			//Debug.Log(prefab);
		}
		BallExpander exp=Instantiate<BallExpander>(prefab).Initialize(0,1);		
		exp.gameObject.name="String " + this.id + " DOM "+ domID;
		Dom dom = exp.gameObject.AddComponent<Dom>().
                Initialize(parent, domID, pos);

        this.doms.Add(domID, dom);
        this.maxZ = System.Math.Max(this.maxZ, pos.y);
        this.minZ = System.Math.Min(this.minZ, pos.y);
    }
    public void reinitDomsWithTimes(float startTime,float endTime)
    {
    foreach(var dom in doms)
     {
			BallExpander cm=dom.Value.gameObject.GetComponent<BallExpander>();
    cm.Initialize(startTime,endTime);
          }
    }

	public void RegisterPulse(int domID, float startTime, float width, float signal)
    {
        var iDom = doms[domID];
	    BallExpander cm=iDom.gameObject.GetComponent<BallExpander>();
	    if(cm!=null)
	    {
		    cm.RegisterPulse(startTime, width, signal);
	    }
    }
}
