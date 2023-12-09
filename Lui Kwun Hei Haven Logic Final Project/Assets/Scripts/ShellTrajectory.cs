using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellTrajectory : MonoBehaviour
{
    public GameObject explosion;
    Rigidbody rb;

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Player" || col.gameObject.tag == "ground")
        {
            GameObject exp = Instantiate(explosion, this.transform.position, Quaternion.identity);
            Destroy(exp, 0.5f);
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.forward = rb.velocity;
    }
}
