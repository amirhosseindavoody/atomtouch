using UnityEngine;
using System.Collections;
public class GameControl : MonoBehaviour{
	public static GameControl self;
	public enum GameState{
		Running,
		AddingAtom
	};
	public static int gameState;
	Ray ray;
	private GameObject atomToBeAdded;
	public GameObject AtomToBeAdded{
		get{return atomToBeAdded;}
	}
	void Awake(){
		self = this;

	}
	void Start(){
		gameState = (int)GameState.Running;
		atomToBeAdded = null;
	}
	void Update(){
			if(gameState == (int)GameState.Running && atomToBeAdded != null){
				//do nothing
				Vector3 dir = atomToBeAdded.transform.position - Camera.main.gameObject.transform.position;
				ray = new Ray(Camera.main.gameObject.transform.position, dir);
				Debug.DrawRay(ray.origin, ray.direction * dir.magnitude, Color.yellow);
			}else if(gameState == (int)GameState.AddingAtom){

				Vector3 mouseposWithDistance 
					= new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.1f);
				//mouseposWithDistance.z = 10f ;
				Vector3 atomPos = Camera.main.ScreenToWorldPoint(mouseposWithDistance);
				ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
				Vector3 dir = ray.direction;
				//Debug.Log("ray dir: " + dir);
				atomPos = ray.origin + ray.direction * 10f;
				//check collision
				float r = atomToBeAdded.GetComponent<SphereCollider>().radius ;
				Vector3 sphereCastDir = atomPos-atomToBeAdded.transform.position;
				dir.Normalize();
				RaycastHit[] hits
					= Physics.SphereCastAll(atomPos, r, sphereCastDir, sphereCastDir.magnitude);
				bool hitsBox = false;
				foreach(RaycastHit hit in hits){
					if(hit.collider.gameObject.name == "Cube"){
						hitsBox = true;
						break;
					}
				}

				if(!hitsBox ){
					atomToBeAdded.transform.position = atomPos;
				}
				
				atomToBeAdded.GetComponent<Rigidbody>().velocity = Vector3.zero;
				Color solidColor = 
					atomToBeAdded.GetComponent<MeshRenderer>().material.color;
				atomToBeAdded.GetComponent<MeshRenderer>().material.color
					= new Color(solidColor.r, solidColor.g, solidColor.b, 60/255);
				Debug.Log(atomToBeAdded.GetComponent<MeshRenderer>().material.color);
				//if on non mobile: mouse left click
				if(Input.GetMouseButtonDown(0)){
					FinishAddingAtom();
					//yield return new WaitForSeconds(1f);
					AtomPhysics.self.Ions.Add(atomToBeAdded);
					SetGameStateRunning();
				}
				//if on mobile: mouse up

			}
		
		
		
	}
	//register atom and stuff
	void FinishAddingAtom(){
		atomToBeAdded.name = "Atom" + atomToBeAdded.GetInstanceID().ToString();
		//add to NaCl list
		atomToBeAdded.GetComponent<AtomGooey>().Kick();
		//yield return new WaitForSeconds(3f);
		//AtomPhysics.self.Ions.Add(atomToBeAdded);
		//SetGameStateRunning();
	}
	public void OnAddAtom(GameObject atomPrefab){
		StaticVariables.pauseTime = true;
		CreateAtom(atomPrefab);
	}
	public void SetGameStateAddingAtom(GameObject atom){
		atomToBeAdded = atom;
		gameState = (int)GameState.AddingAtom;
	}
	public void SetGameStateRunning(){
		//atomToBeAdded = null;
		StaticVariables.pauseTime = false;
		gameState = (int)GameState.Running;
	}
	public void CreateAtom(GameObject prefab){
		int preFabID = prefab.GetInstanceID();
		Debug.Log("creating atom");
		Quaternion curRotation = Quaternion.Euler(0, 0, 0);
		Vector3 spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Debug.Log("spawning atom at: " + spawnPos);
		GameObject atom = Instantiate(prefab, spawnPos, curRotation) as GameObject;
		atom.GetComponent<Rigidbody>().velocity = Vector3.zero;
		atom.GetComponent<Rigidbody>().isKinematic = false;
		SetGameStateAddingAtom(atom);
		//kick it
		//env.AtomKick(i);
		//Potential.myPotential.calculateVerletRadius (currAtom);

	}
}