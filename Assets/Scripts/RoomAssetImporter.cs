using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Windows;

[Serializable]
public class RoomData
{
    public int width;
    public int height;
    public List<Pixel> pixels;
    public List<Vector2Int> topOpenings;
    public List<Vector2Int> bottomOpenings;
    public List<Vector2Int> leftOpenings;
    public List<Vector2Int> rightOpenings;

    public RoomData(int width, int height)
    {
        this.width = width;
        this.height = height;
        pixels = new List<Pixel>();

        topOpenings = new List<Vector2Int>();
        bottomOpenings = new List<Vector2Int>();
        leftOpenings = new List<Vector2Int>();
        rightOpenings = new List<Vector2Int>();
    }

    public int Width { get { return width; } }
    public int Height { get { return height; } }
    public List<Pixel> Pixels { get { return pixels; } }
    public List<Vector2Int> TopOpenings { get { return topOpenings; } }
    public List<Vector2Int> BottomOpenings { get { return bottomOpenings; } }
    public List<Vector2Int> LeftOpenings { get { return leftOpenings; } }
    public List<Vector2Int> RightOpenings { get { return rightOpenings; } }

    public void AddPixel(Pixel pixel)
    {
        pixels.Add(pixel);
    }

    public override string ToString()
    {
        string toReturn = "Room Data\n";
        toReturn += "Width: " + width + "\n";
        toReturn += "Height: " + height + "\n";
        toReturn += "Pixels\n";

        foreach(Pixel pixel in pixels)
        {
            toReturn += pixel + " \n";
        }

        toReturn += "\n";

        toReturn += "Top Openings\n";
        foreach (Vector2Int opening in topOpenings)
        {
            toReturn += opening + " ";
        }
        toReturn += "\n";
        toReturn += "Bottom Openings\n";
        foreach (Vector2Int opening in bottomOpenings)
        {
            toReturn += opening + " ";
        }
        toReturn += "\n";
        toReturn += "Left Openings\n";
        foreach (Vector2Int opening in leftOpenings)
        {
            toReturn += opening + " ";
        }
        toReturn += "\n";
        toReturn += "Right Openings\n";
        foreach (Vector2Int opening in rightOpenings)
        {
            toReturn += opening + " ";
        }
        toReturn += "\n";
        return toReturn;
    }
}

public class RoomAssetImporterVisitor : PixelVisitor
{
    private RoomData roomData;
    private bool hasMoved = false;
    private bool isDone = false;
    private Vector2Int startingPoint;

    public RoomAssetImporterVisitor(Texture2D toVisit) : base(new Vector2Int(-1,-1), Directions.Top, toVisit)
    {
        roomData = new RoomData(toVisit.width, toVisit.height);

        //Find the starting point
        for (int x = 0; x < toVisit.width; x++)
        {
            for (int y = 0; y < toVisit.height; y++)
            {
                if(toVisit.GetPixel(x, y).Equals(Color.red))
                {
                    startingPoint = new Vector2Int(x, y);
                    position = startingPoint;
                }
            }
        }
    }

    public RoomData RoomData { get { return roomData; } }

    public bool IsDone() { return isDone; }

    public override VisitReport Visit()
    {
        if(startingPoint.x == -1 || startingPoint.y == -1)
        {
            return new VisitReport(false, null);
        }

        currentNeighbors = FindNeighbors(toVisit, position);
        if (currentNeighbors == null)
        {
            return new VisitReport(false, null);
        }

        if(currentNeighbors.Original.Color.Equals(Color.red))
        {
            roomData.AddPixel(currentNeighbors.Original);
        }
        else if (currentNeighbors.Original.Color.Equals(Color.black))
        {
            roomData.AddPixel(currentNeighbors.Original);

            if(currentNeighbors.Top.Color.Equals(Color.blue))
            {
                roomData.TopOpenings.Add(new Vector2Int(position.x, position.y));
                roomData.AddPixel(currentNeighbors.Top);
            }
            else if(currentNeighbors.Bottom.Color.Equals(Color.blue))
            {
                roomData.BottomOpenings.Add(new Vector2Int(position.x, position.y));
                roomData.AddPixel(currentNeighbors.Bottom);
            }
            else if (currentNeighbors.Left.Color.Equals(Color.blue))
            {
                roomData.LeftOpenings.Add(new Vector2Int(position.x, position.y));
                roomData.AddPixel(currentNeighbors.Left);
            }
            else if (currentNeighbors.Right.Color.Equals(Color.blue))
            {
                roomData.RightOpenings.Add(new Vector2Int(position.x, position.y));
                roomData.AddPixel(currentNeighbors.Right);
            }
            else
            {
                return new VisitReport(false, null);
            }
        }
        else
        {
            return new VisitReport(false, null);
        }

        return new VisitReport(true, currentNeighbors);
    }

    public override MoveReport MoveOn()
    {
        PreliminaryMovementData prelimData = MoveOnPrelimWork();
        if (!prelimData.prelimWorkSuccessful || isDone)
        {
            return new MoveReport(false, isDone);
        }

        bool didMove = false;
        foreach (Directions potentialDirection in prelimData.toCheck)
        {
            Pixel neighbor = currentNeighbors.GetNeighbor(potentialDirection);
            if (neighbor != null && (neighbor.Color.Equals(Color.red) || neighbor.Color.Equals(Color.black)))
            {
                position += DirectionsUtil.GetDirectionVector(potentialDirection);
                direction = potentialDirection;

                if (hasMoved && position.Equals(startingPoint))
                {
                    isDone = true;
                }

                hasMoved = true;
                didMove = true;
            }
        }

        return new MoveReport(didMove, isDone);
    }
}

[ScriptedImporter(1, "room")]
public class RoomAssetImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        Debug.Log("Importing Room Asset");

        Texture2D image = new Texture2D(1, 1);
        ImageConversion.LoadImage(image, File.ReadAllBytes(ctx.assetPath));

        RoomAssetImporterVisitor visitor = new RoomAssetImporterVisitor(image);

        while (!visitor.IsDone())
        {
            VisitReport visitReport = visitor.Visit();
            if(!visitReport.WasSuccessful)
            {
                Debug.LogError("Room Importer Visit was not successful");
            }

            MoveReport moveReport = visitor.MoveOn();
            if (!moveReport.WasSuccessful)
            {
                Debug.LogError("Room Importer MoveOn was not successful");
            }
        }

        string roomDataJSON = JsonUtility.ToJson(visitor.RoomData);
        TextAsset roomDataAsset = new TextAsset(roomDataJSON);

        ctx.AddObjectToAsset("roomData", roomDataAsset);
        ctx.SetMainObject(roomDataAsset);

        //RoomData readBack = JsonUtility.FromJson<RoomData>(roomDataJSON);
        //Debug.Log("Read Back Room Data");
        //Debug.Log(readBack);

        //Debug the image
        //Debug.Log("Width: " + image.width);
        //Debug.Log("Height: " + image.height);
        //for (int i = 0; i < image.width; i++)
        //{
        //    for (int j = 0; j < image.height; j++)
        //    {
        //        Debug.Log("Color at (" + i + ", " + j + "): " + image.GetPixel(i, j));
        //    }
        //}
    }
}
