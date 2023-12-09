using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Behave.Runtime;
using UnityEngine.AI;

public class RobotController : AgentAI, IAgent
{
    //My Behavior tree
    Behave.Runtime.Tree mTree;
    protected GameObject[] waypoints;
	protected GameObject[] coverwaypoints;
    protected Vector3 destPos;
    protected ArrayList path;
    protected int curPathIndex;
    protected bool reached = false;
    private Animation anim;
    private float curSpeed;
    private float curRotSpeed;
	protected Vector3 rayDirection;
	public int HalfAngle = 45;
    public int ViewDistance;
	protected Vector3 PdestPos;
	public float dist;
	public bool coverhit = false;
	int findingcover = 0;
	
	public GameObject shellPrefab;
	public GameObject shellSpawnPos;
	public GameObject target;
	public GameObject parent;
	float speed = 50;
    bool canShoot = true;
	public float Y ;
	public GameObject fort;
	float shoottimer = 0f;
	
	public int health = 100;
	public int Maxhealth = 100;
	
	public NavMeshAgent agent;
	
	private Transform playerTrans;


    // Use this for initialization
    protected override IEnumerator Initialise()
    {
        health = Maxhealth;
        waypoints = GameObject.FindGameObjectsWithTag("WandarPoints");
		coverwaypoints = GameObject.FindGameObjectsWithTag("CoverWandarPoints");
		playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
		rayDirection = Player.position - transform.position;
        anim = GetComponent<Animation>();
		curSpeed = 10.0f;
        curRotSpeed = 2.0f;
		PdestPos = Player.position;
		dist = Vector3.Distance(transform.position, Player.position);
		RaycastHit hit;
		
		agent = GetComponent<NavMeshAgent>();

        if (destPos == Vector3.zero)
        {
            FindNextPoint();	//Find next new points on the scene
            reached = false;
        }

        //Initialise the Robots_and_Aliens_AgentAI behavior tree
        mTree = BLAgentBehaveLibrary.InstantiateTree(BLAgentBehaveLibrary.TreeType.Robots_and_Aliens_AgentAI, this);

        //Update the behavior tree tick according to the frequency specified
        while (Application.isPlaying && mTree != null)
        {
            AgentUpdate();
            yield return new WaitForSeconds(1.0f / mTree.Frequency);
        }
    }
	
	void Fire(){
		GameObject shell = Instantiate(shellPrefab, shellSpawnPos.transform.position, shellSpawnPos.transform.rotation);
        shell.GetComponent<Rigidbody>().velocity = speed * fort.transform.forward;
	}

    public void FindNextPoint()
    {
        Debug.Log("Finding next point");
        int rndIndex = Random.Range(0, waypoints.Length);
        destPos = waypoints[rndIndex].transform.position;
    }
	
	public void FindNextCoverPoint()
    {
        Debug.Log("Finding next cover point");
        int rndIndex = Random.Range(0, coverwaypoints.Length);
        destPos = coverwaypoints[rndIndex].transform.position;
    }
    
    // Update per frame
    void Update() {
        if (lastActionType == BLAgentBehaveLibrary.ActionType.Idle)
        {
            // Do nothing
        }
        else if (lastActionType == BLAgentBehaveLibrary.ActionType.Die)
        {
            Destroy(this.gameObject);
        }
		else if (lastActionType == BLAgentBehaveLibrary.ActionType.Patrol)
		{
			RaycastHit hit;
			if (Vector3.Distance(transform.position, destPos) <= 5.0f)
            {
                print("Reached to the destination point\ncalculating the next point");
                FindNextPoint(); // find next random wanderpoint
            }
			anim.Play("Run");
			Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);  

            //Go Forward by one step depends on Time.deltaTime
            //transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
			agent.SetDestination(destPos);
		}
		else if (lastActionType == BLAgentBehaveLibrary.ActionType.Attack)
		{
			anim.Play("Attack");
			destPos = Player.position;
			agent.SetDestination(destPos);
		}
		else if (lastActionType == BLAgentBehaveLibrary.ActionType.TakeCover)
		{
			if(findingcover==0)
			{
				FindNextCoverPoint();
				findingcover++;
			}
			coverhit = false;
			anim.Play("Run");
			Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);  

            //Go Forward by one step depends on Time.deltaTime
            //transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
			agent.SetDestination(destPos);
		}
		else if (lastActionType == BLAgentBehaveLibrary.ActionType.CoverShoot)
		{
			findingcover = 0;
			anim.Stop();
			Vector3 direction = (target.transform.position - parent.transform.position).normalized;
			dist = Vector3.Distance(transform.position, Player.position);
			shoottimer += Time.deltaTime;
		    //this.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
		    float? angle;
			if(dist>=40)
			{
		        angle = CalculateAngle(false);
			}
			else
			{
				angle = CalculateAngle(true);
			}
		    Y = parent.transform.rotation.y;
		
		    if(angle != null){
		        //this.transform.localEulerAngles = new Vector3(-(float)angle, T, 0f);
			    parent.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
			    fort.transform.localEulerAngles = new Vector3(-(float)angle, 0f, 0f);
			
	        }else{
			    //this.transform.localEulerAngles = new Vector3(0f, RL, 0f);
			    parent.transform.localEulerAngles = new Vector3(0f,0f,0f);
			    fort.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			    //parent.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
		    }
	
		    if (angle != null && shoottimer >= 2f){
			    //this.transform.localEulerAngles = new Vector3(-(float)angle, 0f, 0f);
				shoottimer = 0f;
			    Fire();
		    }
		    float? CalculateAngle(bool low)
            {
            Vector3 targetDir = target.transform.position - parent.transform.position;
            float y = targetDir.y;
            float x = targetDir.magnitude;
            float gravity = 9.81f;
            float sSqr = speed * speed;
            float underTheSqrRoot = (sSqr * sSqr) - gravity * (gravity * x * x + 2 * y * sSqr);

            if (underTheSqrRoot >= 0f)
            {
                float root = Mathf.Sqrt(underTheSqrRoot);
                float highAngle = sSqr + root;
                float lowAngle = sSqr - root;

                if (low){
                    return (Mathf.Atan2(lowAngle, gravity * x) * Mathf.Rad2Deg);
                }else{
                    return (Mathf.Atan2(highAngle, gravity * x) * Mathf.Rad2Deg);
                }
            }
            else
                return null;
            }
		}

        Debug.DrawLine(transform.position, destPos, Color.magenta);
    }
    //Update Behaviors Tree
    void AgentUpdate()
    {
        mTree.Tick();
    }

    //Behavior Tick Handler for Unhandling actions and decorators
    //This example handles all the action so this function won't get called
    public BehaveResult Tick(Behave.Runtime.Tree sender, bool init)
    {
        Debug.Log("Ticked Received by unhandled " +
            (BLAgentBehaveLibrary.IsAction(sender.ActiveID) ? "Action " : "Decorator ") + " ... " +
            (BLAgentBehaveLibrary.IsAction(sender.ActiveID) ? ((BLAgentBehaveLibrary.ActionType)sender.ActiveID).ToString() :
            ((BLAgentBehaveLibrary.DecoratorType)sender.ActiveID).ToString())
        );

        return BehaveResult.Success;
    }

    //Reset the tree every time an action is done
    public void Reset(Behave.Runtime.Tree sender)
    {
        //Reset the tree every time the action or decorators is done
        //Debug.Log("Resetting behavior trees");
    }

    //Behaviors of the very first priority selector
    public int SelectTopPriority(Behave.Runtime.Tree sender, params int[] IDs) {
        if (health <= 0) {
            Debug.Log("Robot is Dead.");
            return IDs[2];
        }
		else if(health >= (Maxhealth/2))
		{
			return IDs[1];
		}
		else if(health < (Maxhealth/2) && health > 0)
		{
			return IDs[3];
		}

        // Default to idle
        return IDs[0];
    }

    //Handle Decorators
    public BehaveResult TickUnknownDecorator(Behave.Runtime.Tree sender)
    {
        //Decorator doesn't exist in this example
        return BehaveResult.Failure;
    }



    public BehaveResult TickIdleAction(Behave.Runtime.Tree sender)
    {
        Debug.Log("Idle");
        lastActionType = BLAgentBehaveLibrary.ActionType.Idle;
        // Run animation
        //GetComponent<Animation>().CrossFade("Run"); // start to play run
        // Animation stop
        //anim.Stop();
        return BehaveResult.Success;
    }
	
	public BehaveResult TickPatrolAction(Behave.Runtime.Tree sender)
	{
		Debug.Log("Patrol");
		lastActionType = BLAgentBehaveLibrary.ActionType.Patrol;
		rayDirection = playerTrans.position - transform.position;
		RaycastHit hit;
		dist = Vector3.Distance(transform.position, Player.position);
		//if((Vector3.Angle(rayDirection, transform.forward)) < HalfAngle)
		//{
			//if (Physics.Raycast(transform.position, rayDirection, out hit, ViewDistance))
            //{
                //if (hit.collider.gameObject.tag == "Player")
                //{
                    //print("Player seen");
					//return BehaveResult.Success;
                //}
            //}
		//}
		if(dist <= 15)
		{
			return BehaveResult.Success;
		}
		else
		{
			return BehaveResult.Running;	
		}
		//return BehaveResult.Running;
	}
	
	public BehaveResult TickAttackAction(Behave.Runtime.Tree sender)
	{
		Debug.Log("Attack");
		lastActionType = BLAgentBehaveLibrary.ActionType.Attack;
		dist = Vector3.Distance(transform.position, Player.position);
		if(dist >= 20)
		{
			return BehaveResult.Success;
		}
		else
		{
			return BehaveResult.Running;	
		}
	}
	
	public BehaveResult TickHealthDecorator(Behave.Runtime.Tree sender)
	{
		if(health>(Maxhealth/2))
		{
			return BehaveResult.Running;
		}
		else
		{
			return BehaveResult.Failure;
		}
	}
	
	public BehaveResult TickLowHealthDecorator(Behave.Runtime.Tree sender)
	{
		if(health>0)
		{
			return BehaveResult.Running;
		}
		else
		{
			return BehaveResult.Failure;
		}
	}
	
	public BehaveResult TickTakeCoverAction(Behave.Runtime.Tree sender)
	{
		lastActionType = BLAgentBehaveLibrary.ActionType.TakeCover;
		coverhit = false;
		if (Vector3.Distance(transform.position, destPos) <= 5.0f)
		{
			return BehaveResult.Success;
		}
		else
		{
			return BehaveResult.Running;	
		}
	}
	public BehaveResult TickCoverShootAction(Behave.Runtime.Tree sender)
	{
		lastActionType = BLAgentBehaveLibrary.ActionType.CoverShoot;
		Debug.Log("CoverShooting");
		if (coverhit == true)
		{
			FindNextCoverPoint();
			return BehaveResult.Success;
		}
		else
		{
			return BehaveResult.Running;	
		}
	}

    public BehaveResult TickDieAction(Behave.Runtime.Tree sender)
    {
        Debug.Log("Dead");
        lastActionType = BLAgentBehaveLibrary.ActionType.Die;
        // Animation stop
        anim.Stop();
        return BehaveResult.Success;
    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            Debug.Log("Robot is hit by bullet");
            health -= 10;
			if(lastActionType == BLAgentBehaveLibrary.ActionType.CoverShoot)
			{
				coverhit = true;
			}
        } 
		else if (collision.gameObject.tag == "Missile")
        {
            Debug.Log("Robot is hit by missile");
            health -= 40;
			if(lastActionType == BLAgentBehaveLibrary.ActionType.CoverShoot)
			{
				coverhit = true;
			}
        }
        Debug.Log("Health : " + health);
        if (health <= 0)
        {
            AgentUpdate(); // Force tick the tree. Immediate death
        }
    }

    public void OnDrawGizmos(){
		if (Player == null)
            return;
        Gizmos.DrawWireSphere(transform.position, 50);
		Debug.DrawLine(transform.position, Player.position, Color.red);

        Vector3 frontRayPoint = transform.position + (transform.forward * ViewDistance);

        //Approximate perspective visualization
        Quaternion leftQ = Quaternion.AngleAxis(-HalfAngle, Vector3.up);
        Vector3 leftRayPoint = leftQ * (transform.forward * ViewDistance) + transform.position;

        Quaternion rightQ = Quaternion.AngleAxis(HalfAngle, Vector3.up);
        Vector3 rightRayPoint = rightQ * (transform.forward * ViewDistance) + transform.position;

        Debug.DrawLine(transform.position, frontRayPoint, Color.green); // forward vector
        Debug.DrawLine(transform.position, leftRayPoint, Color.green); // left vector
        Debug.DrawLine(transform.position, rightRayPoint, Color.green); // right vector
    }
}