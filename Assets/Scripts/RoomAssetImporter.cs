using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Windows;

[Serializable]
public class PositionPlusPotentialOthers
{
    public Vector2Int defaultPosition;
    public List<Vector2Int> potentialPositions;

    public PositionPlusPotentialOthers(Vector2Int defaultPosition)
    {
        this.defaultPosition = defaultPosition;
        potentialPositions = new List<Vector2Int>();
    }

    public override string ToString()
    {
        string toReturn = "Default Position: " + defaultPosition + " Potential Positions: ";
        foreach (Vector2Int potential in potentialPositions)
        {
            toReturn += potential + " ";
        }
        return toReturn;
    }
}

[Serializable]
public class RoomData
{
    public int width;
    public int height;
    public List<Pixel> pixels = new List<Pixel>();

    public List<Vector2Int> topOpenings = new List<Vector2Int>();
    public List<Vector2Int> bottomOpenings = new List<Vector2Int>();
    public List<Vector2Int> leftOpenings = new List<Vector2Int>();
    public List<Vector2Int> rightOpenings = new List<Vector2Int>();

    public List<PositionPlusPotentialOthers> topPotentials = new List<PositionPlusPotentialOthers>();
    public List<PositionPlusPotentialOthers> bottomPotentials = new List<PositionPlusPotentialOthers>();
    public List<PositionPlusPotentialOthers> leftPotentials = new List<PositionPlusPotentialOthers>();
    public List<PositionPlusPotentialOthers> rightPotentials = new List<PositionPlusPotentialOthers>();

    public SeedGenWrapper sgwUsedToMakeThis;

    public RoomData(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public RoomData(RoomData roomData)
    {
        width = roomData.width;
        height = roomData.height;

        List<Pixel> deepCopyPixels = new List<Pixel>();
        foreach (Pixel pixel in roomData.pixels)
        {
            deepCopyPixels.Add(new Pixel(pixel));
        }
        pixels = deepCopyPixels;
        
        topOpenings = new List<Vector2Int>(roomData.topOpenings);
        bottomOpenings = new List<Vector2Int>(roomData.bottomOpenings);
        leftOpenings = new List<Vector2Int>(roomData.leftOpenings);
        rightOpenings = new List<Vector2Int>(roomData.rightOpenings);

        topPotentials = new List<PositionPlusPotentialOthers>(roomData.topPotentials);
        bottomPotentials = new List<PositionPlusPotentialOthers>(roomData.bottomPotentials);
        leftPotentials = new List<PositionPlusPotentialOthers>(roomData.leftPotentials);
        rightPotentials = new List<PositionPlusPotentialOthers>(roomData.rightPotentials);
    }

    public int Width { get { return width; } }
    public int Height { get { return height; } }
    public List<Pixel> Pixels { get { return pixels; } }
    public List<Pixel> WallPixels
    {
        get
        {
            List<Pixel> toReturn = new List<Pixel>();
            foreach (Pixel pixel in pixels)
            {
                if (pixel.Color.Equals(Color.red))
                {
                    toReturn.Add(pixel);
                }
            }
            return toReturn;
        }
    }
    public List<Vector2Int> TopOpenings { get { return topOpenings; } }
    public List<Vector2Int> BottomOpenings { get { return bottomOpenings; } }
    public List<Vector2Int> LeftOpenings { get { return leftOpenings; } }
    public List<Vector2Int> RightOpenings { get { return rightOpenings; } }

    private void ModifyOpening(Vector2Int currentOpening, Vector2Int newOpening)
    {
        for (int i = 0; i < topOpenings.Count; i++)
        {
            if (topOpenings[i].Equals(currentOpening))
            {
                topOpenings[i] = newOpening;
                return;
            }
        }
        for (int i = 0; i < bottomOpenings.Count; i++)
        {
            if (bottomOpenings[i].Equals(currentOpening))
            {
                bottomOpenings[i] = newOpening;
                return;
            }
        }
        for (int i = 0; i < leftOpenings.Count; i++)
        {
            if (leftOpenings[i].Equals(currentOpening))
            {
                leftOpenings[i] = newOpening;
                return;
            }
        }
        for (int i = 0; i < rightOpenings.Count; i++)
        {
            if (rightOpenings[i].Equals(currentOpening))
            {
                rightOpenings[i] = newOpening;
                return;
            }
        }
    }

    public void AddPixel(Pixel pixel)
    {
        pixels.Add(pixel);
    }

    public void SetPotentialPositionsList(List<PositionPlusPotentialOthers> positions, Directions direction)
    {
        switch (direction)
        {
            case Directions.Top:
                topPotentials = positions;
                break;
            case Directions.Bottom:
                bottomPotentials = positions;
                break;
            case Directions.Left:
                leftPotentials = positions;
                break;
            case Directions.Right:
                rightPotentials = positions;
                break;
        }
    }

    public void AddPotentialPositions(PositionPlusPotentialOthers position, Directions direction)
    {
        switch (direction)
        {
            case Directions.Top:
                topPotentials.Add(position);
                break;
            case Directions.Bottom:
                bottomPotentials.Add(position);
                break;
            case Directions.Left:
                leftPotentials.Add(position);
                break;
            case Directions.Right:
                rightPotentials.Add(position);
                break;
        }
    }

    public List<RoomData> GetAllPotentialRooms()
    {
        List<RoomData> potentialRooms = new List<RoomData>();

        bool alreadyIncludesDefaultRoom = false;

        AddPotentialRooms(potentialRooms, topPotentials, Directions.Top, ref alreadyIncludesDefaultRoom);
        AddPotentialRooms(potentialRooms, bottomPotentials, Directions.Bottom, ref alreadyIncludesDefaultRoom);
        AddPotentialRooms(potentialRooms, leftPotentials, Directions.Left, ref alreadyIncludesDefaultRoom);
        AddPotentialRooms(potentialRooms, rightPotentials, Directions.Right, ref alreadyIncludesDefaultRoom);

        return potentialRooms;
    }

    private void AddPotentialRooms(List<RoomData> potentialRooms, List<PositionPlusPotentialOthers> potentials, Directions direction, ref bool alreadyIncludesDefaultRoom)
    {
        foreach (PositionPlusPotentialOthers positionPlusPotentialOthers in potentials)
        {
            Vector2Int originalOpeningPosition = positionPlusPotentialOthers.defaultPosition;
            Vector2Int originalOpeningDirectionIndicator = positionPlusPotentialOthers.defaultPosition + DirectionsUtil.GetDirectionVector(direction);

            foreach (Vector2Int potentialPosition in positionPlusPotentialOthers.potentialPositions)
            {
                RoomData potentialRoom = new RoomData(this);

                Vector2Int newOpeningPosition = potentialPosition;
                Vector2Int newOpeningDirectionIndicator = potentialPosition + DirectionsUtil.GetDirectionVector(direction);

                if (newOpeningPosition == originalOpeningPosition)
                {
                    if(alreadyIncludesDefaultRoom)
                    {
                        continue;
                    }
                    else
                    {
                        alreadyIncludesDefaultRoom = true;
                    }
                }
                else
                {
                    Pixel markedForDeletion = null;
                    foreach (Pixel pixel in potentialRoom.pixels)
                    {
                        if (pixel.GetPosition().Equals(originalOpeningPosition))
                        {
                            pixel.SetColor(Color.red);
                        }
                        else if (pixel.GetPosition().Equals(originalOpeningDirectionIndicator))
                        {
                            markedForDeletion = pixel;
                        }
                        else if (pixel.GetPosition().Equals(newOpeningPosition))
                        {
                            pixel.SetColor(Color.black);

                            potentialRoom.ModifyOpening(originalOpeningPosition, newOpeningPosition);
                        }
                        else if (pixel.GetPosition().Equals(newOpeningDirectionIndicator))
                        {
                            pixel.SetColor(Color.blue);
                        }
                    }

                    if(markedForDeletion != null)
                    {
                        potentialRoom.pixels.Remove(markedForDeletion);
                    }
                }

                potentialRooms.Add(potentialRoom);
            }
        }
    }

    public void ModifyAllPositions(Vector2Int modifier)
    {
        foreach (Pixel pixel in pixels)
        {
            pixel.SetPosition(pixel.GetPosition() + modifier);
        }
        for (int i = 0; i < topOpenings.Count; i++)
        {
            topOpenings[i] += modifier;
        }
        for (int i = 0; i < bottomOpenings.Count; i++)
        {
            bottomOpenings[i] += modifier;
        }
        for (int i = 0; i < leftOpenings.Count; i++)
        {
            leftOpenings[i] += modifier;
        }
        for (int i = 0; i < rightOpenings.Count; i++)
        {
            rightOpenings[i] += modifier;
        }

        for(int i = 0; i < topPotentials.Count; i++)
        {
            topPotentials[i].defaultPosition += modifier;
            for (int j = 0; j < topPotentials[i].potentialPositions.Count; j++)
            {
                topPotentials[i].potentialPositions[j] += modifier;
            }
        }
        for (int i = 0; i < bottomPotentials.Count; i++)
        {
            bottomPotentials[i].defaultPosition += modifier;
            for (int j = 0; j < bottomPotentials[i].potentialPositions.Count; j++)
            {
                bottomPotentials[i].potentialPositions[j] += modifier;
            }
        }
        for (int i = 0; i < leftPotentials.Count; i++)
        {
            leftPotentials[i].defaultPosition += modifier;
            for (int j = 0; j < leftPotentials[i].potentialPositions.Count; j++)
            {
                leftPotentials[i].potentialPositions[j] += modifier;
            }
        }
        for (int i = 0; i < rightPotentials.Count; i++)
        {
            rightPotentials[i].defaultPosition += modifier;
            for (int j = 0; j < rightPotentials[i].potentialPositions.Count; j++)
            {
                rightPotentials[i].potentialPositions[j] += modifier;
            }
        }
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

        toReturn += "Top Potentials\n";
        foreach (PositionPlusPotentialOthers potential in topPotentials)
        {
            toReturn += potential + " ";
        }
        toReturn += "\n";

        toReturn += "Bottom Potentials\n";
        foreach (PositionPlusPotentialOthers potential in bottomPotentials)
        {
            toReturn += potential + " ";
        }
        toReturn += "\n";

        toReturn += "Left Potentials\n";
        foreach (PositionPlusPotentialOthers potential in leftPotentials)
        {
            toReturn += potential + " ";
        }
        toReturn += "\n";

        toReturn += "Right Potentials\n";
        foreach (PositionPlusPotentialOthers potential in rightPotentials)
        {
            toReturn += potential + " ";
        }
        toReturn += "\n";

        return toReturn;
    }
}

public class RoomAssetImporterVisitor : PixelVisitor
{
    private RoomData roomData;
    private bool hasMoved = false;

    private int whatLoopAreWeOn = 1;
    private PotentialPlusDirection currentOpening = null;
    private Stack<PotentialPlusDirection> openingsToGo = null;
    private bool directionHasFlipped = false;
    private bool directionJustFlipped = false;

    private bool isDone = false;
    private Vector2Int startingPoint;

    public RoomAssetImporterVisitor(Texture2D toVisit) : base(new Vector2Int(-1,-1), Directions.Top, toVisit)
    {
        roomData = new RoomData(toVisit.width, toVisit.height);

        //Find the starting point
        bool done = false;
        for (int x = 0; x < toVisit.width; x++)
        {
            for (int y = 0; y < toVisit.height; y++)
            {
                if (toVisit.GetPixel(x, y).Equals(Color.red))
                {
                    startingPoint = new Vector2Int(x, y);
                    position = startingPoint;
                    done = true;
                }
            }

            if (done)
            {
                break;
            }
        }
    }

    public RoomData RoomData { get { return roomData; } }

    public bool IsDone() { return isDone; }

    public override VisitReport Visit()
    {
        VisitReport visitReport = null;

        if (whatLoopAreWeOn == 1)
        {
            visitReport = FirstLoopVisit();
        }
        else
        {
            visitReport = SecondLoopVisit();
        }

        return visitReport;
    }

    private VisitReport FirstLoopVisit()
    {
        if (startingPoint.x == -1 || startingPoint.y == -1)
        {
            return new VisitReport(false, null);
        }

        currentNeighbors = FindNeighbors(toVisit, position);
        if (currentNeighbors == null)
        {
            return new VisitReport(false, null);
        }

        if (currentNeighbors.Original.Color.Equals(Color.red))
        {
            roomData.AddPixel(currentNeighbors.Original);
        }
        else if (currentNeighbors.Original.Color.Equals(Color.black))
        {
            roomData.AddPixel(currentNeighbors.Original);

            if (currentNeighbors.Top.Color.Equals(Color.blue))
            {
                roomData.TopOpenings.Add(new Vector2Int(position.x, position.y));
                roomData.AddPixel(currentNeighbors.Top);
            }
            else if (currentNeighbors.Bottom.Color.Equals(Color.blue))
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
                Debug.LogError("RoomAssetImporterVisitor Visit found an opening with no direction indicator.");
                return new VisitReport(false, null);
            }
        }
        else
        {
            return new VisitReport(false, null);
        }

        return new VisitReport(true, currentNeighbors);
    }

    private VisitReport SecondLoopVisit()
    {
        currentNeighbors = FindNeighbors(toVisit, position);

        if (currentOpening == null)
        {
            //We're not ready yet to visit anything. Not a failure, just not ready.
            return new VisitReport(true, null);
        }

        //We're adding potential positions for the current opening. Is this pixel a potential position?
        Pixel neighborInDirection = currentNeighbors.GetNeighbor(direction);
        PixelNeighbors nextNeighbors = FindNeighbors(toVisit, neighborInDirection.GetPosition());
        Pixel nextNeighborInDirection = nextNeighbors.GetNeighbor(direction);

        bool isPotentialPosition = false;

        if(directionJustFlipped)
        {
            isPotentialPosition = false;
        }
        else if (neighborInDirection == null)
        {
            Debug.LogError("RoomAssetImporterVisitor SecondLoopVisit found an unexpected null neighbor.");
            return new VisitReport(false, null);
        }
        else if(currentOpening.positionAndOthers.potentialPositions.Contains(position))
        {
            isPotentialPosition = false;
        }
        else if (nextNeighborInDirection != null && nextNeighborInDirection.Color.Equals(Color.black) && !nextNeighborInDirection.GetPosition().Equals(currentOpening.positionAndOthers.defaultPosition))
        {
            //We don't want the potential openings to be within two spaces of another default opening (not including itself).
            //Walls are 1 unit thick, so if the openings went Opening-Wall-Opening, then that would cause guaranteed collisions I think,
            //as two rooms fight for that single middle wall space.
            isPotentialPosition = false;
        }
        else if(neighborInDirection.Color.Equals(Color.black) && neighborInDirection.GetPosition().Equals(currentOpening.positionAndOthers.defaultPosition))
        {
            isPotentialPosition = true;
        }
        else if (neighborInDirection.Color.Equals(Color.red))
        {
            isPotentialPosition = true;
        }

        if (isPotentialPosition)
        {

            Debug.LogWarning("Creating another potential position.");
            currentOpening.positionAndOthers.potentialPositions.Add(position);
        }

        return new VisitReport(true, null);
    }

    public override MoveReport MoveOn()
    {
        PreliminaryMovementData prelimData = MoveOnPrelimWork();
        if (!prelimData.prelimWorkSuccessful || isDone)
        {
            return new MoveReport(false, isDone);
        }

        MoveReport moveReport = null;
        if (whatLoopAreWeOn == 1)
        {
            moveReport = FirstLoopMove(prelimData);
        }
        else
        {
            moveReport = SecondLoopMove();
        }

        return moveReport;
    }

    private MoveReport FirstLoopMove(PreliminaryMovementData prelimData)
    {
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
                    whatLoopAreWeOn = 2;
                }

                hasMoved = true;
                didMove = true;
            }
        }

        return new MoveReport(didMove, isDone);
    }


    private class PotentialPlusDirection
    {
        public PositionPlusPotentialOthers positionAndOthers;
        public Directions direction;
        public PotentialPlusDirection(PositionPlusPotentialOthers positionAndOthers, Directions direction)
        {
            this.positionAndOthers = positionAndOthers;
            this.direction = direction;
        }

        public override string ToString()
        {
            return "Position: " + positionAndOthers.defaultPosition + " Direction: " + direction;
        }
    }

    private MoveReport SecondLoopMove()
    {
        if(openingsToGo == null)
        {
            //Debug.Log("Setting Up Second Loop");

            openingsToGo = new Stack<PotentialPlusDirection>();
            foreach (Vector2Int opening in roomData.TopOpenings)
            {
                openingsToGo.Push(new PotentialPlusDirection(new PositionPlusPotentialOthers(opening), Directions.Top));
            }
            foreach (Vector2Int opening in roomData.BottomOpenings)
            {
                openingsToGo.Push(new PotentialPlusDirection(new PositionPlusPotentialOthers(opening), Directions.Bottom));
            }
            foreach (Vector2Int opening in roomData.LeftOpenings)
            {
                openingsToGo.Push(new PotentialPlusDirection(new PositionPlusPotentialOthers(opening), Directions.Left));
            }
            foreach (Vector2Int opening in roomData.RightOpenings)
            {
                openingsToGo.Push(new PotentialPlusDirection(new PositionPlusPotentialOthers(opening), Directions.Right));
            }
        }

        if (openingsToGo.Count == 0 && currentOpening == null)
        {
            //Debug.Log("Second Loop End");

            isDone = true;
        }
        else if (currentOpening == null)
        {

            currentOpening = openingsToGo.Pop();
            //Debug.Log("Second Loop Switch Current Opening. New opening = " + currentOpening);
            position = currentOpening.positionAndOthers.defaultPosition;
            direction = DirectionsUtil.GetNextClockwiseDirection(currentOpening.direction);
            directionHasFlipped = false;
            directionJustFlipped = false;
        }
        else
        {
            if(directionJustFlipped)
            {
                directionJustFlipped = false;
            }

            Pixel neighbor = currentNeighbors.GetNeighbor(direction);
            PixelNeighbors nextNeighbors = FindNeighbors(toVisit, neighbor.GetPosition());
            Pixel nextNeighbor = nextNeighbors.GetNeighbor(direction);


            if (nextNeighbor != null && nextNeighbor.Color.Equals(Color.black) && !nextNeighbor.GetPosition().Equals(currentOpening.positionAndOthers.defaultPosition))
            {
                if (!directionHasFlipped)
                {
                    direction = DirectionsUtil.GetOppositeDirection(direction);
                    directionHasFlipped = true;
                    directionJustFlipped = true;
                }
                else
                {
                    //We're done with this opening
                    MoveOnWithSecondLoop();
                }
            }
            else if (neighbor != null && neighbor.Color.Equals(Color.black) && !neighbor.GetPosition().Equals(currentOpening.positionAndOthers.defaultPosition))
            {
                if(!directionHasFlipped)
                {
                    direction = DirectionsUtil.GetOppositeDirection(direction);
                    directionHasFlipped = true;
                    directionJustFlipped = true;
                }
                else
                {
                    //We're done with this opening
                    MoveOnWithSecondLoop();
                }
            }
            else if(neighbor != null && neighbor.IsTransparent)
            {
                //Debug.Log("Second Loop Actually Move: Neighbor is Transparent. Moving to the " + direction + " and current position = " + position);

                if (!directionHasFlipped)
                {
                    direction = DirectionsUtil.GetOppositeDirection(direction);
                    directionHasFlipped = true;
                    directionJustFlipped = true;
                }
                else
                {
                    MoveOnWithSecondLoop();
                }
            }
            else if(neighbor != null && (neighbor.Color.Equals(Color.red) || neighbor.Color.Equals(Color.black)))
            {
                //Debug.Log("Second Loop Actually Move: Neighbor is Valid. Moving to the " + direction + " and current position = " + position);

                position += DirectionsUtil.GetDirectionVector(direction);
            }
            else
            {
                Debug.LogError("RoomAssetImporterVisitor SecondLoopMove found an unexpected pixel.");
                return new MoveReport(false, isDone);
            }
        }

        return new MoveReport(true, isDone);
    }

    private void MoveOnWithSecondLoop()
    {
        roomData.AddPotentialPositions(currentOpening.positionAndOthers, currentOpening.direction);

        currentOpening = null;
    }
}

[ScriptedImporter(1, "room.png")]
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
                Debug.LogError("Room Importer Visit was not successful for " + ctx.assetPath);
            }

            MoveReport moveReport = visitor.MoveOn();
            if (!moveReport.WasSuccessful)
            {
                Debug.LogError("Room Importer MoveOn was not successful for " + ctx.assetPath);
            }
        }


        int topPotentialsCount = 0;
        int bottomPotentialsCount = 0;
        int leftPotentialsCount = 0;
        int rightPotentialsCount = 0;

        foreach (PositionPlusPotentialOthers positionPlusPotential in visitor.RoomData.topPotentials)
        {
            foreach(Vector2Int potential in positionPlusPotential.potentialPositions)
            {
                topPotentialsCount++;
            }
        }
        foreach (PositionPlusPotentialOthers positionPlusPotential in visitor.RoomData.bottomPotentials)
        {
            foreach (Vector2Int potential in positionPlusPotential.potentialPositions)
            {
                bottomPotentialsCount++;
            }
        }
        foreach (PositionPlusPotentialOthers positionPlusPotential in visitor.RoomData.leftPotentials)
        {
            foreach (Vector2Int potential in positionPlusPotential.potentialPositions)
            {
                leftPotentialsCount++;
            }
        }
        foreach (PositionPlusPotentialOthers positionPlusPotential in visitor.RoomData.rightPotentials)
        {
            foreach (Vector2Int potential in positionPlusPotential.potentialPositions)
            {
                rightPotentialsCount++;
            }
        }
        //Debug.Log("----------------------------------------"); 
        //Debug.Log("Room Data Potential Positions Counts"); 
        //Debug.Log("Top Potentials: " + topPotentialsCount);
        //Debug.Log("Bottom Potentials: " + bottomPotentialsCount);
        //Debug.Log("Left Potentials: " + leftPotentialsCount);
        //Debug.Log("Right Potentials: " + rightPotentialsCount);
        //Debug.Log("----------------------------------------");

        List<RoomData> allPotentialRooms = visitor.RoomData.GetAllPotentialRooms();
        //Debug.Log("----------------------------------------");
        //Debug.Log("All Potential Rooms: " + allPotentialRooms.Count); 
        //foreach (RoomData potentialRoom in allPotentialRooms)
        //{
        //    Debug.Log(potentialRoom);
        //}
        //Debug.Log("----------------------------------------");

        for(int i = 0; i < allPotentialRooms.Count; i++)
        {
            RoomData room = allPotentialRooms[i];

            string roomDataJSON = JsonUtility.ToJson(room);
            TextAsset roomDataAsset = new TextAsset(roomDataJSON);
            ctx.AddObjectToAsset("roomData" + i, roomDataAsset);
            
            if(i == 0)
            {
                ctx.SetMainObject(roomDataAsset);
            }
        }

        DestroyImmediate(image);
    }
}
