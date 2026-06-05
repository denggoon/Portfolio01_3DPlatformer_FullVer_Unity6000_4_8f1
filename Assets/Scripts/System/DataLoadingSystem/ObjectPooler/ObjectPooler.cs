using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour { 
	private static ObjectPooler instance_; 

	public static ObjectPooler instance { 
		get { 
			return instance_; 
		} 
	} 

	void OnDestroy()
	{
		instance_ = null;
	}
	
	[System.Serializable]
	public class ObjPoolInfo
	{
		public string objName;
		public int poolAmount;

		public ObjPoolInfo(string name, int amount)
		{
			objName = name;
			poolAmount = amount;
		}
	}

	[System.Serializable]
	public class ObjStkInfo
	{
		public string stackName;
		public GameObject stackParent;
		public Stack<GameObject> stack = new Stack<GameObject>();

		public ObjStkInfo(GameObject stackParent, Stack<GameObject> stack)
		{
			this.stackParent = stackParent;
			this.stack = stack;
		}

		public GameObject GetStackParent()
		{
			return this.stackParent;
		}

		public Stack<GameObject> GetStack()
		{
			return this.stack;
		}

	}

	public new Transform transform;
	public List<ObjPoolInfo> lstObjInfo = new List<ObjPoolInfo>();
	public int presetCount = 10;
	public string[] poolKeywords; //풀링 되어야할 번들 키워드 

	public bool IsPoolerInitialized = false;

	void Awake()
	{
		instance_ = this;
		transform = GetComponent<Transform> ();
	}
	
	// Addressables를 통해 lstObjInfo에 등록된 에셋을 비동기로 로드하여 풀을 초기화
	public IEnumerator Initialze()
	{
		for (int i = 0; i < lstObjInfo.Count; i++)
		{
			string address = lstObjInfo[i].objName;
			int count = lstObjInfo[i].poolAmount;

			GameObject prefab = null;
			yield return ResourcesManager.instance.LoadGameObjectAsync(address, obj => prefab = obj);

			if (prefab != null)
				PrepareObjStk(address, count, prefab);
			else
				Debug.LogError("ObjectPooler: 에셋 로드 실패 — " + address);
		}

		IsPoolerInitialized = true;
	}

	private Dictionary<string, ObjStkInfo> dicStkInfo = new Dictionary<string, ObjStkInfo>();
	public void PrepareObjStk(string name, int count, GameObject prefab)
	{
		GameObject poolParentObj = new GameObject("Pool_" + name);
		Stack<GameObject> stkPool = new Stack<GameObject>();
		Transform poolParentTrans = poolParentObj.GetComponent<Transform>();
		poolParentTrans.SetParent(this.transform);

		for (int i = 0; i < count; i++)
		{
			GameObject poolObj = GameObject.Instantiate(prefab, Vector3.zero, prefab.transform.rotation);
			poolObj.name = name;
			poolObj.SetActive(false);
			poolObj.GetComponent<Transform>().SetParent(poolParentTrans);
			PushObjInStk(poolObj, stkPool);
		}

		ObjStkInfo stkInfo = new ObjStkInfo(poolParentObj, stkPool);
		AddStkInfoDic(name, stkInfo);
	} 

	public bool AddStkInfoDic(string name , ObjStkInfo stkInfo) 
	{ 
		if(!dicStkInfo.ContainsKey(name)) 
		{ 
			dicStkInfo.Add(name, stkInfo); 
			return true; 
		} 
		return false;
	}

	public GameObject SearchStkObjInDic(string name)
	{
		GameObject stkObj = null;

		if (dicStkInfo.ContainsKey (name)) 
		{
			stkObj = dicStkInfo[name].GetStackParent();
		}

		return stkObj;
	}
	
	public GameObject ObjPop(string name, Vector3 popPos, bool autoActive = true) 
	{ 
		if (IsPoolerInitialized == false)
			return null;

		GameObject objToPop  = null; 
		if(dicStkInfo.ContainsKey(name)) 
		{ 
			if(dicStkInfo[name].GetStack().Count <= 0) 
			{ 
				GameObject stkObj = SearchStkObjInDic(name);
				Transform stkObjTrans = stkObj.GetComponent<Transform>();

				if(stkObj != null )
				{
					GameObject loadedObj = ResourcesManager.instance.LoadGameObject(name);

					if(loadedObj != null)
					{
						objToPop = GameObject.Instantiate(loadedObj, popPos, loadedObj.transform.rotation) as GameObject;
						if(objToPop != null) 
						{ 
							objToPop.name = name; 
							Transform objToPopTrans = objToPop.GetComponent<Transform>();

							objToPopTrans.SetParent(stkObjTrans);

							if(autoActive)
								objToPop.SetActive(true);

							return objToPop; 
						}
					}
				}
				
				return null;

			} else {

				objToPop = dicStkInfo[name].GetStack().Pop() as GameObject; 

				Transform objToPopTrans = objToPop.GetComponent<Transform>();
				objToPopTrans.position = popPos;

				if(autoActive)
					objToPop.SetActive(true);

				return objToPop; 
			}
			
		} else {

//			Debug.Log("ObjPop: Obj [" + name + "] does not exist in dictionary, try with lower case: " + name.ToLower());
			if(ObjPop(name.ToLower(), popPos) == null) //try with lower case 
			{
				Debug.LogError("ObjPop: Obj [" + name + "] does not exist in dictionary, returning null.");
			}
		}

		return null;
	}
	
	public bool ObjPush(string name, GameObject go) 
	{ 
		if (IsPoolerInitialized == false)
			return false;

		if (dicStkInfo.ContainsKey (name)) {
			go.SetActive (false);

			PushObjInStk (go, dicStkInfo [name].GetStack ());
			return true; 

		} else {
//			Debug.LogError("ObjPush: Cannot find stack for [" + name + "]. Destroying object instead.");

			Destroy (go);
		}

		return false;
	}
	
	public void PushObjInStk(GameObject go, Stack<GameObject> stack) 
	{
		if( stack != null )
		{
			stack.Push(go);
		}
		else 
			Debug.Log("PushObjInStk: Stack to push obj is null!!"); 
	}
}
