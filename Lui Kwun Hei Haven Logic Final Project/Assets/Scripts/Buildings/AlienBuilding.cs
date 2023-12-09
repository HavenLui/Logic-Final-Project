using UnityEngine;
using System.Collections;

public class AlienBuilding : MonoBehaviour 
{
    public GameObject AlienPrefab;
    public Transform SpawnPoint;

	// Use this for initialization
	void Start () 
    {
        Invoke("CreateAlien", 8.0f);
        Invoke("CreateAlien", 13.0f);
	}

    void CreateAlien()
    {
        Instantiate(AlienPrefab, SpawnPoint.position, SpawnPoint.rotation);
    }
}
