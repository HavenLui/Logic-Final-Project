using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RobotFSM : FSM
{
	public enum FSMState
    {
        None,
        Patrol,
        Chase,
        Attack,
		Shoot,
		Dead,
    }
	
	private int health;
	private int Maxhealth = 100;
	
	private Animation anim;
	
	private bool bDead;
	
	private float curSpeed;
    private float curRotSpeed;
	
	private Vector3 rayDirection;
	public int HalfAngle = 45;
    public int ViewDistance = 100;
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
	
	public NavMeshAgent agent;
	
	public FSMState curState;
	
	private Transform playerTrans;
	
	protected override void Initialize ()
	{
		curState = FSMState.Patrol;
        curSpeed = 15.0f;
        curRotSpeed = 2.0f;
        bDead = false;
        health = Maxhealth;
		
		//elapsedTime = 0.0f;
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;

        //Get the list of points
        waypoints = GameObject.FindGameObjectsWithTag("WandarPoints");
		coverwaypoints = GameObject.FindGameObjectsWithTag("CoverWandarPoints");
		
		anim = GetComponent<Animation>();
		
		agent = GetComponent<NavMeshAgent>();

        //Set Random destination point first
        FindNextPoint();

        //Get the target enemy(Player)
        GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
        playerTransform = objPlayer.transform;

        if(!playerTransform)
            print("Player doesn't exist.. Please add one with Tag named 'Player'");
	}
	
	protected override void FSMUpdate()
	{
		switch (curState)
		{
			case FSMState.Patrol: UpdatePatrolState(); break;
            case FSMState.Chase: UpdateChaseState(); break;
            case FSMState.Attack: UpdateAttackState(); break;
			case FSMState.Shoot: UpdateShootState(); break;
			case FSMState.Dead: UpdateDeadState(); break;
		}
		if (health <= 0)
            curState = FSMState.Dead;
	}
	
	protected void UpdatePatrolState()
    {
        RaycastHit hit;
		anim.Play("Run");
        rayDirection = playerTrans.position - transform.position;
		if (Vector3.Distance(transform.position, destPos) <= 5.0f)
        {
            print("Find next point");
            FindNextPoint(); // find next random wanderpoint
        }
		if ((Vector3.Angle(rayDirection, transform.forward)) < HalfAngle)
        {
            if (Physics.Raycast(transform.position, rayDirection, out hit, ViewDistance))
            {
                if (hit.collider.gameObject.tag == "Player")
                {
                    curState = FSMState.Chase;
                }
            }
        }
		if (health < (Maxhealth/2))
		{
			curState = FSMState.Shoot;
		}
		Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);  
		agent.SetDestination(destPos);
		//transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }
	
	protected void UpdateChaseState()
    {
        destPos = playerTransform.position;
		anim.Play("Run");
		float dist = Vector3.Distance(transform.position, playerTransform.position);
		if (dist <= 10.0f)
        {
            curState = FSMState.Attack;
        }
		if (dist >= 40.0f)
        {
            curState = FSMState.Patrol;
        }
		if (health < (Maxhealth/2))
		{
			curState = FSMState.Shoot;
		}
		Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);  
		agent.SetDestination(destPos);
    }
	
	protected void UpdateAttackState()
    {
        destPos = playerTransform.position;
		float dist = Vector3.Distance(transform.position, playerTransform.position);
		anim.Play("Attack");
		if (dist >= 15.0f)
        {
            curState = FSMState.Chase;
        }
		if (health < (Maxhealth/2))
		{
			curState = FSMState.Shoot;
		}
    }
	
	protected void UpdateShootState()
	{
		anim.Stop();
		Vector3 direction = (target.transform.position - parent.transform.position).normalized;
		dist = Vector3.Distance(transform.position, playerTransform.position);
		shoottimer += Time.deltaTime;
		//this.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
		
		parent.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
	
		if (shoottimer >= 1f){
			//this.transform.localEulerAngles = new Vector3(-(float)angle, 0f, 0f);
		    shoottimer = 0f;
			Fire();
		}
	}
	
	protected void UpdateDeadState()
	{
		if (!bDead)
        {
            bDead = true;
            Explode();
        }
	}
	
	public void FindNextPoint()
    {
        Debug.Log("Finding next point");
        int rndIndex = Random.Range(0, waypoints.Length);
        destPos = waypoints[rndIndex].transform.position;
    }
	
	void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            Debug.Log("Robot is hit by bullet");
            health -= 10;
        } 
		else if (collision.gameObject.tag == "Missile")
        {
            Debug.Log("Robot is hit by missile");
            health -= 40;
        }
        Debug.Log("Health : " + health);
        if (health <= 0)
        {
            curState = FSMState.Dead;
        }
    }
	
	protected void Explode()
    {
        Destroy (gameObject);
    }
	
	void Fire(){
		GameObject shell = Instantiate(shellPrefab, shellSpawnPos.transform.position, shellSpawnPos.transform.rotation);
        shell.GetComponent<Rigidbody>().velocity = speed * fort.transform.forward;
	}
	
	public void OnDrawGizmos()
	{
		if (playerTrans == null)
            return;

		Debug.DrawLine(transform.position, destPos, Color.magenta);

        Debug.DrawLine(transform.position, playerTrans.position, Color.red);

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
