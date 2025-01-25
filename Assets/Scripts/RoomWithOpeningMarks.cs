using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomWithOpeningMarks : MonoBehaviour
{
    public bool topOpening;
    public bool bottomOpening;
    public bool leftOpening;
    public bool rightOpening;

    public GameObject openingMark;

    void Start()
    {
        if(topOpening)
        {
            Instantiate(openingMark, new Vector3(transform.position.x, transform.position.y, transform.position.z + transform.localScale.z / 2), Quaternion.identity, transform);
        }
        if (bottomOpening)
        {
            Instantiate(openingMark, new Vector3(transform.position.x, transform.position.y, transform.position.z - transform.localScale.z / 2), Quaternion.identity, transform);
        }
        if (leftOpening)
        {
            Instantiate(openingMark, new Vector3(transform.position.x - transform.localScale.x / 2, transform.position.y, transform.position.z), Quaternion.Euler(0, 90, 0), transform);
        }
        if (rightOpening)
        {
            Instantiate(openingMark, new Vector3(transform.position.x + transform.localScale.x / 2, transform.position.y, transform.position.z), Quaternion.Euler(0, 90, 0), transform);
        }
    }

    void Update()
    {
        
    }
}
