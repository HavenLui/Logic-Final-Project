using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossFSM : FSM
{
	public enum FSMState
    {
        None,
        Patrol,
        Chase,
        Attack,
		Shoot,
		Takesupply,
		TakeCover,
		CoverShoot,
		Dead,
    }
	
	private int health;
	private int Maxhealth = 200;
	
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
	public GameObject GMshell;
	public GameObject GMshellSpawnPos;
	public GameObject shellSpawnPos2;
	public GameObject GMshellSpawnPos2;
	public GameObject target;
	public GameObject parent;
	public GameObject GMparent;
	public GameObject parent2;
	public GameObject GMparent2;
	bool upgrade = false;
	float speed = 50;
    bool canShoot = true;
	public float Y ;
	public GameObject fort;
	public GameObject GMfort;
	public GameObject fort2;
	public GameObject GMfort2;
	float shoottimer = 0f;
	float GMshoottimer = 0f;
	public GameObject win;
	
	public NavMeshAgent agent;
	
	public FSMState curState;
	
	public GameObject supply;
	
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
			case FSMState.Takesupply: UpdateTakesupplyState(); break;
			case FSMState.TakeCover: UpdateTakeCoverState(); break;
			case FSMState.CoverShoot: UpdateCoverShootState(); break;
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
			curState = FSMState.TakeCover;
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
			curState = FSMState.TakeCover;
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
            curState = FSMState.Shoot;
        }
		if (health < (Maxhealth/2))
		{
			curState = FSMState.TakeCover;
		}
    }
	
	protected void UpdateShootState()
	{
		anim.Stop();
		Vector3 direction = (target.transform.position - parent.transform.position).normalized;
		dist = Vector3.Distance(transform.position, playerTransform.position);
		GMshoottimer += Time.deltaTime;
		//this.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
		
		GMparent.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
		GMparent2.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
	
		if (GMshoottimer >= 0.5f){
			//this.transform.localEulerAngles = new Vector3(-(float)angle, 0f, 0f);
		    GMshoottimer = 0f;
			GMFire();
		}
		if (dist >= 30.0f)
        {
            curState = FSMState.Chase;
        }
		if (health < (Maxhealth/2))
		{
			curState = FSMState.TakeCover;
		}
	}
	
	protected void UpdateTakeCoverState()
	{
		if(findingcover==0)
		{
			FindNextCoverPoint();
			findingcover++;
		}
		coverhit = false;
		if (Vector3.Distance(transform.position, destPos) <= 5.0f)
		{
			curState = FSMState.CoverShoot;
		}
		anim.Play("Run");
		Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);
		agent.SetDestination(destPos);
	}
	
	protected void UpdateCoverShootState()
	{
		anim.Stop();
		findingcover = 0;
		if (coverhit == true)
		{
			if(health<=50 && upgrade==false)
			{
				coverhit = false;
				curState = FSMState.Takesupply;
			}
			else
			{
			    curState = FSMState.TakeCover;
			}
		}
		
		Vector3 direction = (target.transform.position - parent.transform.position).normalized;
		dist = Vector3.Distance(transform.position, playerTransform.position);
		shoottimer += Time.deltaTime;
		GMshoottimer += Time.deltaTime;
		
		GMparent.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
		GMparent2.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
	
		if (GMshoottimer >= 0.5f){
			//this.transform.localEulerAngles = new Vector3(-(float)angle, 0f, 0f);
		    GMshoottimer = 0f;
			GMFire();
		}
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
			if(upgrade==true)
			{
				parent2.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
			    fort2.transform.localEulerAngles = new Vector3(-(float)angle, 0f, 0f);
			}
			
	    }else{
			//this.transform.localEulerAngles = new Vector3(0f, RL, 0f);
			parent.transform.localEulerAngles = new Vector3(0f,0f,0f);
			fort.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			//parent.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
			if(upgrade==true)
			{
				parent2.transform.localEulerAngles = new Vector3(0f,0f,0f);
			    fort2.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			}
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
	
	protected void UpdateTakesupplyState()
	{
		destPos = supply.transform.position;
		dist = Vector3.Distance(transform.position, supply.transform.position);
		if (dist <= 10.0f)
		{
			health = Maxhealth;
			upgrade = true;
			parent2.SetActive(true);
			GMparent2.SetActive(true);
			curState = FSMState.Patrol;
		}
		anim.Play("Run");
		Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);
		agent.SetDestination(destPos);
	}
	
	protected void UpdateDeadState()
	{
		if (!bDead)
        {
			win.SetActive(true);
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
	
	public void FindNextCoverPoint()
    {
        Debug.Log("Finding next cover point");
        int rndIndex = Random.Range(0, coverwaypoints.Length);
        destPos = coverwaypoints[rndIndex].transform.position;
    }
	
	void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            Debug.Log("Robot is hit by bullet");
            health -= 10;
			if(curState == FSMState.CoverShoot)
			{
				coverhit = true;
			}
        } 
		else if (collision.gameObject.tag == "Missile")
        {
            Debug.Log("Robot is hit by missile");
            health -= 40;
			if(curState == FSMState.CoverShoot)
			{
				coverhit = true;
			}
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
		if(upgrade==true)
		{
			GameObject shell2 = Instantiate(shellPrefab, shellSpawnPos2.transform.position, shellSpawnPos2.transform.rotation);
            shell2.GetComponent<Rigidbody>().velocity = speed * fort2.transform.forward;
		}
	}
	
	void GMFire(){
		GameObject gmshell = Instantiate(GMshell, GMshellSpawnPos.transform.position, GMshellSpawnPos.transform.rotation);
        gmshell.GetComponent<Rigidbody>().velocity = speed * GMfort.transform.forward;
		if(upgrade==true)
		{
			GameObject gmshell2 = Instantiate(GMshell, GMshellSpawnPos2.transform.position, GMshellSpawnPos2.transform.rotation);
            gmshell2.GetComponent<Rigidbody>().velocity = speed * GMfort2.transform.forward;
		}
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
