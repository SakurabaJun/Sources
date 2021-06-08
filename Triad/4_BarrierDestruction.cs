using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierDestruction : MonoBehaviour
{
    public float speed = 10;

    List<Rigidbody> child = new List<Rigidbody>();

    private void Start()
    {
        for(int i = 0; i < transform.GetChildCount(); i++)
        {
            child.Add(transform.GetChild(i).gameObject.GetComponent<Rigidbody>());
        }

        Destruction();
    }

    void Destruction()
    {
        foreach(Rigidbody r in child)
        {
            var velocity = (r.transform.position - transform.position).normalized * speed;

            r.AddForce(velocity);
        }

        Destroy(gameObject, 2);
    }
}
