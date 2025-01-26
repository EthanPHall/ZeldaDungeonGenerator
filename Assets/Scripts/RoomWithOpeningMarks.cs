using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomWithOpeningMarks : MonoBehaviour
{
    public int topOpenings = 0;
    public int bottomOpenings = 0;
    public int leftOpenings = 0;
    public int rightOpenings = 0;

    public GameObject openingMark;

    public int width = 1;
    public int height = 1;
    public Vector2Int position = Vector2Int.zero;

    void Start()
    {
        transform.localScale = new Vector3(width - .5f, .4f, height - .5f);
        transform.position = new Vector3(position.x + width/2f, 0, position.y + height/2f);

        for (int i = 0; i < topOpenings; i++)
        {
            Instantiate(openingMark, new Vector3(transform.position.x, transform.position.y, transform.position.z + transform.localScale.z / 2), Quaternion.identity, transform);
        }
        for(int i = 0; i < bottomOpenings; i++)
        {
            Instantiate(openingMark, new Vector3(transform.position.x, transform.position.y, transform.position.z - transform.localScale.z / 2), Quaternion.identity, transform);
        }
        for (int i = 0; i < leftOpenings; i++)
        {
            Instantiate(openingMark, new Vector3(transform.position.x - transform.localScale.x / 2, transform.position.y, transform.position.z), Quaternion.identity, transform);
        }
        for (int i = 0; i < rightOpenings; i++)
        {
            Instantiate(openingMark, new Vector3(transform.position.x + transform.localScale.x / 2, transform.position.y, transform.position.z), Quaternion.identity, transform);
        }
    }

    void Update()
    {
        
    }
}
