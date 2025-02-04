using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyGameObject : MonoBehaviour
{
    public Color prerequisite = Color.clear;
    public GameObject prereqIndicatorPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if(prerequisite != Color.clear)
        {
            GameObject indicator = Instantiate(prereqIndicatorPrefab, transform.position, Quaternion.identity, transform);
            indicator.GetComponent<Renderer>().material.color = prerequisite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
