using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;


public class CreateMeshFromCsv : ScriptableWizard {
	public TextAsset csvFile;
    public Material meshMat;

	int [] jet=new int [] {12, 22, 24, 29, 44, 51, 55, 64, 84, 105, 106, 136, 156, 160, 165, 175, 182, 190, 196, 200, 261, 263, 269, 287, 293, 299, 307, 314, 315, 319, 360, 368, 373, 374, 387 };
    [MenuItem("Seva Tools/Csv To Mesh Wizard")]
    static void CreateWizard() {
	ScriptableWizard.DisplayWizard<CreateMeshFromCsv>("Mesh Combine Wizard");
    }
  PosEntry entryFromLine(string line)
	{
	   Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
	   string[] X = CSVParser.Split(line);
	   PosEntry ret=new PosEntry();
	   ret.time=float.Parse(X[1]);
	   ret.pos=new Vector3(float.Parse(X[2]),float.Parse(X[3]),float.Parse(X[4]));
	   ret.id=int.Parse(X[5]);
	   ret.meshnum=int.Parse(X[6]);
	   return ret;
	}
	void MakeGo(Mesh curM, GameObject go, int nm)
	{
		GameObject msh=new GameObject();
		msh.name="mesh_"+nm.ToString();
            msh.transform.parent=go.transform;
            msh.transform.localPosition=Vector3.zero;
            MeshFilter mf= msh.AddComponent<MeshFilter>();
	    MeshRenderer mr= msh.AddComponent<MeshRenderer>();
	    mf.mesh=curM;
	    mr.sharedMaterial=meshMat;
	}
    void OnWizardCreate() {
        if(csvFile == null) return;
        if (meshMat==null) return;
	List <PosEntry> lst=new List<PosEntry>();
        GameObject go=new GameObject(csvFile.name);
        GameObject sec=new GameObject("offset");
		sec.transform.parent=go.transform;
		sec.transform.localPosition=Vector3.zero;
        go.transform.position = Vector3.zero;
	 string meshPath="Assets/prefabsAndMeshes/mesh_"+csvFile.name;
	 string fs = csvFile.text;
         string[] fLines = Regex.Split ( fs, "\n|\r|\r\n" );	
         int cnt=0;
         int curMesh=-1;
         int curId=-1;
	 List<Vector3> verts = new List<Vector3>();
	 List<Vector2> uvs = new List<Vector2>();
	 List<Color> clrs = new List<Color>();
		List<int> indc = new List<int>();
	 Mesh curM=null;
	 Vector3 avg=Vector3.zero;
	 float mintime=-1;
	 float maxtime=-1;
	 float minInitTime=-1;
	 float maxInitTime=-1;
         foreach(string line in fLines)
         {
         if(line.Length<2) continue;
         if(line[0]==',') continue;
         PosEntry en=entryFromLine(line);
         if(en.id!=curId)
         {
	  if(maxInitTime<0||en.time>maxInitTime) maxInitTime=en.time;
	  if(minInitTime<0||en.time<minInitTime) minInitTime=en.time;
         curId=en.id;
         }
         lst.Add(en);
         cnt++;
         avg+=en.pos;
         if(maxtime<0||en.time>maxtime) maxtime=en.time;
         if(mintime<0||en.time<mintime) mintime=en.time;
         }
         Debug.Log(mintime);
	 Debug.Log(maxtime);
	 Debug.Log(avg/cnt);
	 avg/=(float)cnt;
         sec.transform.localPosition=avg;
	 cnt=0;
	 float td=maxtime-mintime;
	 float tdInit=129.0f;//maxInitTime-minInitTime; - set it fixed... 
	
	 Color ccolor=new Color();
	 curId=-1;
	 foreach(PosEntry en in lst)
	 {
	 if(curMesh!=en.meshnum)
          {
           if(curM!=null)
            {
            curM.vertices=verts.ToArray();
            curM.uv=uvs.ToArray();
            curM.colors=clrs.ToArray();
            curM.SetIndices(indc.ToArray(),MeshTopology.Lines,0);
	    MakeGo(curM,sec,curMesh);
	    AssetDatabase.CreateAsset(curM, meshPath + curMesh.ToString() + ".asset");

            }
            verts.Clear();
            uvs.Clear();
            clrs.Clear();
            indc.Clear();
            curMesh=en.meshnum;
            curM=new Mesh();
            cnt=0;
          }
	float shft=(en.time-mintime)/td;
        if(curId==en.id)
        {
        indc.Add(cnt-1);
	indc.Add(cnt);
        }
        else
        {
        curId=en.id;
	ccolor=Color.Lerp(Color.red, Color.green, (en.time-minInitTime)/tdInit);
     if(System.Array.IndexOf (jet,en.id)>-1)
{
     ccolor.b=0.8f;
}
		else
{
ccolor*=0.8f;
}
        }
	verts.Add(en.pos-avg);

	uvs.Add(new Vector2(shft,0));
       clrs.Add(ccolor );//Color.Lerp(Color.red, Color.green, shft));

	   cnt++;
	 }
	if(curM!=null)
            {
            curM.vertices=verts.ToArray();
            curM.uv=uvs.ToArray();
            curM.colors=clrs.ToArray();
            curM.SetIndices(indc.ToArray(),MeshTopology.Lines,0);
	 MakeGo(curM,sec,curMesh);
	     AssetDatabase.CreateAsset(curM, meshPath + curMesh.ToString() + ".asset");
            }
		



	PrefabUtility.CreatePrefab("Assets/prefabsAndMeshes/"+csvFile.name+".prefab",go,ReplacePrefabOptions.ConnectToPrefab);
     
    }
}