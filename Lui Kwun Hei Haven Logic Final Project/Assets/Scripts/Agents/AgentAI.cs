using UnityEngine;
using System.Collections;

public enum UnitType
{
    Robot,
}

public class AgentAI : MonoBehaviour
{
    public UnitType type;
	protected Transform Player;
    protected BLAgentBehaveLibrary.ActionType lastActionType;

    protected GameObject curTargetObject;

    protected virtual IEnumerator Initialise() { yield return new WaitForSeconds(1.0f); }
    
    void Start()
    {

		Player =  GameObject.FindWithTag("Player").transform;
        StartCoroutine(Initialise());
    }

    //Check Enemies in Range
		
    protected bool IsPatrolPointInRange(Vector3 curPos, Vector3 destPos)
    {
        if (Vector3.Distance(curPos, destPos) <= 10.0f)
        {
            return true;
        }

        return false;
    }

    // check distance between player and robot
    protected bool IsPlayerInRange(Vector3 robotPos, Vector3 playerPos, float range)
    {
        if (Vector3.Distance(robotPos, playerPos) <= range)   
        {
            Debug.Log("Player in range.");
            return true;
        }
        return false;
    }

}