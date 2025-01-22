using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Drawing.Bitmap;

public class GettingFamiliarWithBitmaps : MonoBehaviour
{
    public Texture2D path;
    public GameObject redCube;
    public GameObject greyCube;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Hello World");
        Debug.Log("Width: " + path.width);
        Debug.Log("Height: " + path.height);

        //Display the color of the pixel at position (0, 0)
        Debug.Log("Color: " + path.GetPixel(0, 0));

        //Display the color of the pixel at position (24, 36)
        Debug.Log("Color: " + path.GetPixel(24, 36));

        //Is pixel at 24, 36 transparent?
        Debug.Log("Is pixel at 24, 36 transparent? " + path.GetPixel(24, 36).a);

        //Is pixel at 24, 36 white?
        Debug.Log("Is pixel at 24, 36 white? " + path.GetPixel(24, 36).Equals(Color.white));

        //Is pixel at 21, 21 red?
        Debug.Log("Is pixel at 21, 21 red? " + path.GetPixel(21, 21).Equals(Color.red));

        //Is pixel at 21, 21 transparent?
        Debug.Log("Is pixel at 21, 21 transparent? " + path.GetPixel(21, 21).a);

        //Display all the pixels in the image and their positions
        Debug.Log("Display all the pixels in the image");
        for (int i = 0; i < path.width; i++)
        {
            for (int j = 0; j < path.height; j++)
            {
                if(path.GetPixel(i, j).a > 0)
                {
                    Debug.Log("Pixel at position (" + i + ", " + j + ") is " + path.GetPixel(i, j));
                }
            }
        }

        // Instantiate a cube at every pixel in the image using redCube for non-transparent pixels and greyCube for transparent pixels
        for (int i = 0; i < path.width; i++)
        {
            for (int j = 0; j < path.height; j++)
            {
                if (path.GetPixel(i, j).a > 0)
                {
                    Instantiate(redCube, new Vector3(i, 0, j), Quaternion.identity);
                }
                else
                {
                    Instantiate(greyCube, new Vector3(i, 0, j), Quaternion.identity);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
