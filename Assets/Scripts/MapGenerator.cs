using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public static class Texture2DUtil
{
    public static bool IsPositionInBounds(Texture2D texture, int x, int y)
    {
        if (x < 0 || x >= texture.width || y < 0 || y >= texture.height)
        {
            return false;
        }
        return true;
    }
}

public class Pixel
{
    private bool transparent;
    private int x;
    private int y;
    private Color color;
    public Pixel(int x, int y, Color color)
    {
        this.x = x;
        this.y = y;
        this.color = color;

        if (color.a != 1)
        {
            transparent = true;
        }
        else
        {
            transparent = false;
        }
    }

    public int X { get { return x; } }
    public int Y { get { return y; } }
    public Color Color { get { return color; } }
    public bool IsTransparent { get { return transparent; } }

    public bool AreValuesEquivalent(Pixel other)
    {
        return x == other.x && y == other.y && color.Equals(other.color);
    }

    public Vector2Int GetPositionAsVector2Int()
    {
        return new Vector2Int(x, y);
    }
}

public class PixelNeighbors
{
    private Pixel top, bottom, left, right, original;

    public PixelNeighbors(Pixel top, Pixel bottom, Pixel left, Pixel right, Pixel original)
    {
        this.top = top;
        this.bottom = bottom;
        this.left = left;
        this.right = right;
        this.original = original;
    }

    public Pixel Top { get { return top; } }
    public Pixel Bottom { get { return bottom; } }
    public Pixel Left { get { return left; } }
    public Pixel Right { get { return right; } }
    public Pixel Original { get { return original; } }

    public Pixel GetNeighbor(Directions direction)
    {
        switch (direction)
        {
            case Directions.Top:
                return top;
            case Directions.Bottom:
                return bottom;
            case Directions.Left:
                return left;
            case Directions.Right:
                return right;
            default:
                return null;
        }
    }

    public Pixel[] GetNeighbors()
    {
        return new Pixel[] { top, bottom, left, right };
    }

    public Directions GetDirectionOfNeighbor(Pixel neighbor)
    {
        if (neighbor.AreValuesEquivalent(top))
        {
            return Directions.Top;
        }
        else if (neighbor.AreValuesEquivalent(bottom))
        {
            return Directions.Bottom;
        }
        else if (neighbor.AreValuesEquivalent(left))
        {
            return Directions.Left;
        }
        else if (neighbor.AreValuesEquivalent(right))
        {
            return Directions.Right;
        }
        else
        {
            return Directions.None;
        }
    }
}

public enum Directions
{
    Top,
    Bottom,
    Left,
    Right,
    None
}

public static class DirectionsUtil
{
    public static Vector2Int GetDirectionVector(Directions direction)
    {
        switch (direction)
        {
            case Directions.Top:
                return new Vector2Int(0, 1);
            case Directions.Bottom:
                return new Vector2Int(0, -1);
            case Directions.Left:
                return new Vector2Int(-1, 0);
            case Directions.Right:
                return new Vector2Int(1, 0);
            default:
                return new Vector2Int(0, 0);
        }
    }

    public static Directions GetOppositeDirection(Directions direction)
    {
        switch (direction)
        {
            case Directions.Top:
                return Directions.Bottom;
            case Directions.Bottom:
                return Directions.Top;
            case Directions.Left:
                return Directions.Right;
            case Directions.Right:
                return Directions.Left;
            default:
                return Directions.None;
        }
    }

    public static Directions GetDirectionFromVector(Vector2Int vector)
    {
        if (vector.x == 0 && vector.y > 0)
        {
            return Directions.Top;
        }
        else if (vector.x == 0 && vector.y < 0)
        {
            return Directions.Bottom;
        }
        else if (vector.x < 0 && vector.y == 0)
        {
            return Directions.Left;
        }
        else if (vector.x > 0 && vector.y == 0)
        {
            return Directions.Right;
        }
        else
        {
            return Directions.None;
        }
    }

    public static Directions GetNextClockwiseDirection(Directions direction)
    {
        switch (direction)
        {
            case Directions.Top:
                return Directions.Right;
            case Directions.Right:
                return Directions.Bottom;
            case Directions.Bottom:
                return Directions.Left;
            case Directions.Left:
                return Directions.Top;
            default:
                return Directions.None;
        }
    }

    public static Directions GetNextCounterClockwiseDirection(Directions direction)
    {
        switch (direction)
        {
            case Directions.Top:
                return Directions.Left;
            case Directions.Left:
                return Directions.Bottom;
            case Directions.Bottom:
                return Directions.Right;
            case Directions.Right:
                return Directions.Top;
            default:
                return Directions.None;
        }
    }
}

public class VisitReport 
{
    private bool wasSuccessful;
    private PixelNeighbors neighbors;

    public VisitReport(bool wasSuccessful, PixelNeighbors neighbors)
    {
        this.wasSuccessful = wasSuccessful;
        this.neighbors = neighbors;
    }

    public bool WasSuccessful { get { return wasSuccessful; } }
    public PixelNeighbors Neighbors { get { return neighbors; } }
}

public class MoveReport 
{
    bool wasSuccessful;
    bool visitorIsDone;

    public MoveReport(bool wasSuccessful, bool visitorIsDone = false)
    {
        this.wasSuccessful = wasSuccessful;
        this.visitorIsDone = visitorIsDone;
    }

    public bool WasSuccessful { get { return wasSuccessful; } }

    public bool VisitorIsDone { get { return visitorIsDone; } }

    public override string ToString()
    {
        return "MoveReport: " + wasSuccessful + ". It is done? " + visitorIsDone;
    }
}


public abstract class PixelVisitor
{
    protected Vector2Int position;
    protected Directions direction;
    protected Texture2D toVisit;

    protected PixelNeighbors currentNeighbors = null;
    protected Color[] validMoveToColors;

    public PixelVisitor(Vector2Int position, Directions direction, Texture2D toVisit)
    {
        this.position = position;
        this.direction = direction;
        this.toVisit = toVisit;
    }

    public Vector2Int Position { get { return position; } }
    public Directions Direction { get { return direction; } }
    private PixelNeighbors FindNeighbors(Texture2D path, Vector2Int centralPixel)
    {
        Pixel original = Texture2DUtil.IsPositionInBounds(path, centralPixel.x, centralPixel.y) ? new Pixel(centralPixel.x, centralPixel.y, path.GetPixel(centralPixel.x, centralPixel.y)) : null;
        if (original == null)
        {
            return null;
        }

        Vector2Int topPosition = new Vector2Int(centralPixel.x, centralPixel.y + 1);
        Vector2Int bottomPosition = new Vector2Int(centralPixel.x, centralPixel.y - 1);
        Vector2Int leftPosition = new Vector2Int(centralPixel.x - 1, centralPixel.y);
        Vector2Int rightPosition = new Vector2Int(centralPixel.x + 1, centralPixel.y);

        //Are the neighboring positions in bounds? If not, we just set the respective pixel to null.
        Pixel top = Texture2DUtil.IsPositionInBounds(path, topPosition.x, topPosition.y) ? new Pixel(topPosition.x, topPosition.y, path.GetPixel(topPosition.x, topPosition.y)) : null;
        Pixel bottom = Texture2DUtil.IsPositionInBounds(path, bottomPosition.x, bottomPosition.y) ? new Pixel(bottomPosition.x, bottomPosition.y, path.GetPixel(bottomPosition.x, bottomPosition.y)) : null;
        Pixel left = Texture2DUtil.IsPositionInBounds(path, leftPosition.x, leftPosition.y) ? new Pixel(leftPosition.x, leftPosition.y, path.GetPixel(leftPosition.x, leftPosition.y)) : null;
        Pixel right = Texture2DUtil.IsPositionInBounds(path, rightPosition.x, rightPosition.y) ? new Pixel(rightPosition.x, rightPosition.y, path.GetPixel(rightPosition.x, rightPosition.y)) : null;


        PixelNeighbors newNeighbors = new PixelNeighbors(top, bottom, left, right, original);

        return newNeighbors;
    }

    public VisitReport Visit()
    {
        currentNeighbors = FindNeighbors(toVisit, position);
        if (currentNeighbors == null) 
        {
            return new VisitReport(false, null);
        }

        return new VisitReport(true, currentNeighbors);
    }

    protected struct PreliminaryMovementData
    {
        public bool prelimWorkSuccessful;
        public Directions[] toCheck;

        public PreliminaryMovementData(bool prelimWorkSuccessful, Directions[] toCheck)
        {
            this.prelimWorkSuccessful = prelimWorkSuccessful;
            this.toCheck = toCheck;
        }
    }

    protected PreliminaryMovementData MoveOnPrelimWork()
    {
        if(currentNeighbors == null)
        {
            currentNeighbors = FindNeighbors(toVisit, position);
            if(currentNeighbors == null)
            {
                return new PreliminaryMovementData(false, null);
            }
        }

        //First, we check if we can move in the direction we're currently facing. Then clockwise, then counterclockwise. We don't check the position opposite the current direction because that's where we came from and we don't want to double back.
        Directions[] toCheck = new Directions[] { direction, DirectionsUtil.GetNextClockwiseDirection(direction), DirectionsUtil.GetNextCounterClockwiseDirection(direction) };
        return new PreliminaryMovementData(true, toCheck);
    }

    public abstract MoveReport MoveOn();
}

public class StartRoomBorderVisitor : PixelVisitor
{
    private bool hasMoved = false;
    private Vector2Int startingPoint;
    private bool isDone = false;

    public StartRoomBorderVisitor(Vector2Int position, Directions direction, Texture2D toVisit) : base(position, direction, toVisit)
    {
        startingPoint = position;
    }

    public override MoveReport MoveOn()
    {
        PreliminaryMovementData prelimData = MoveOnPrelimWork();
        if (!prelimData.prelimWorkSuccessful || isDone)
        {
            return new MoveReport(false, isDone);
        }

        foreach (Directions potentialDirection in prelimData.toCheck)
        {
            Pixel neighbor = currentNeighbors.GetNeighbor(potentialDirection);
            if (neighbor != null && neighbor.Color.Equals(Color.white))
            {
                position += DirectionsUtil.GetDirectionVector(potentialDirection);
                direction = potentialDirection;

                if (hasMoved && position.Equals(startingPoint))
                {
                    isDone = true;
                }

                hasMoved = true;
                return new MoveReport(true, isDone);
            }
        }

        return new MoveReport(false, isDone);
    }
}

public class CriticalPathVisitor: PixelVisitor
{
    private bool isDone = false;
    private int sectionNumber;
    private RoomSeed previousSeed = null;
    private CriticalPathVisitor successorOf = null;

    public CriticalPathVisitor(Vector2Int position, Directions direction, Texture2D toVisit, int sectionNumber) : base(position, direction, toVisit)
    {
        this.sectionNumber = sectionNumber;
    }

    public int SectionNumber { get { return sectionNumber; } }

    public RoomSeed PreviousSeed 
    {
        get 
        { 
            if(previousSeed == null && successorOf != null)
            {
                return successorOf.PreviousSeed;
            }

            return previousSeed;
        } 
        set { previousSeed = value; } }

    public CriticalPathVisitor BuildingOffOf { get { return successorOf; } set { successorOf = value; } }

    public override MoveReport MoveOn()
    {
        //Debug.Log("Critical Path Visitor at " + position + " moving in direction " + direction);

        PreliminaryMovementData prelimData = MoveOnPrelimWork();

        if (!prelimData.prelimWorkSuccessful || isDone)
        {
            return new MoveReport(false, isDone);
        }

        bool successfullyMoved = false;
        foreach (Directions potentialDirection in prelimData.toCheck)
        {
            Pixel neighbor = currentNeighbors.GetNeighbor(potentialDirection);
            if (neighbor != null)
            {
                if (neighbor.Color.Equals(Color.red))
                {
                    position += DirectionsUtil.GetDirectionVector(potentialDirection);
                    direction = potentialDirection;
                    successfullyMoved = true;
                    break;
                }
                else if (neighbor.Color.Equals(Color.black))
                {
                    //Debug.Log("Critical Path Visitor is Done");
                    position += DirectionsUtil.GetDirectionVector(potentialDirection);
                    direction = potentialDirection;
                    isDone = true;
                    successfullyMoved = false;
                    break;
                }
            }
        }

        return new MoveReport(successfullyMoved, isDone);
    }
}

public class TravelData
{
    private Directions travelDirection;
    private int sectionNumber;

    public TravelData(Directions travelDirection, int sectionNumber)
    {
        this.travelDirection = travelDirection;
        this.sectionNumber = sectionNumber;
    }
    
    public Directions TravelDirection { get { return travelDirection; } }

    public int SectionNumber { get { return sectionNumber; } }

    public override string ToString()
    {
        return "TravelData: " + travelDirection + " to section " + sectionNumber;
    }
}

public class RoomSeed
{
    private Vector2Int position;
    private List<TravelData> entrances = new List<TravelData>();
    private List<TravelData> exits = new List<TravelData>();
    private int sectionNumber;
    public RoomSeed(Vector2Int position, int sectionNumber)
    {
        this.position = position;
        this.sectionNumber = sectionNumber;
    }
    public Vector2Int Position { get { return position; } }
    public int SectionNumber { get { return sectionNumber; } }

    public List<TravelData> Entrances { get { return entrances; } }

    public List<TravelData> Exits { get { return exits; } }

    public void AddEntrance(TravelData entrance)
    {
        entrances.Add(entrance);
    }

    public void AddExit(TravelData exit)
    {
        exits.Add(exit);
    }

    public override string ToString()
    {
        string entranceString = "Entrances: ";
        foreach (TravelData entrance in entrances)
        {
            entranceString += entrance.ToString() + ", ";
        }

        string exitString = "Exits: ";
        foreach (TravelData exit in exits)
        {
            exitString += exit.ToString() + ", ";
        }

        return "RoomSeed at position " + position + " in section " + sectionNumber + " with \n" + entranceString + " and \n" + exitString;
    }
}

public class MapGenerator : MonoBehaviour
{
    public bool generateNewMap = false;
    public int usePathsFromLevel = 0;

    private Texture2D[] paths;

    private void Start()
    {
        paths = Resources.LoadAll<Texture2D>("Paths/Level " + usePathsFromLevel + " Paths");
    }

    void Update()
    {
        if (generateNewMap)
        {
            generateNewMap = false;

            if(paths.Length == 0)
            {
                Debug.LogError("No paths found in level " + usePathsFromLevel);
                return;
            }

            Texture2D path = paths[Random.Range(0, paths.Length)];

            //log what path was found
            Debug.Log("Path: " + path.name);

            BasicGenerateMap(path);
        }
    }

    private void BasicGenerateMap(Texture2D path)
    {
        //First, we look at the map and try to find the starting point. We also make sure that there's an end point.
        Vector2Int startingPoint = new Vector2Int(-1, -1);
        Vector2Int endPoint = new Vector2Int(-1, -1);

        for (int i = 0; i < path.width; i++)
        {
            for (int j = 0; j < path.height; j++)
            {
                if (path.GetPixel(i, j).Equals(Color.white))
                {
                    //With the way that Texture2D handles coordinates, (0,0) being at the bottom left,
                    //and this algorithm searching from bottom to top, this starting point should be
                    //the bottom left corner of the starting room.
                    startingPoint = new Vector2Int(i, j);
                }
                else if (path.GetPixel(i, j).Equals(Color.black))
                {
                    //bottomLeft of the end room
                    endPoint = new Vector2Int(i, j);
                }
            }
        }

        //Make sure everything has progressed as needed.
        bool abort = false;
        if (startingPoint.x == -1)
        {
            abort = true;
            Debug.LogError("Starting point not found");
        }
        if (endPoint.x == -1)
        {
            abort = true;
            Debug.LogError("End point not found");
        }
        if (abort)
        {
            return;
        }

        //Now we need to look at the border of the start room and check all of the neighbors for red pixels.
        //If we find one, we mark it down as a branch.
        Queue<CriticalPathVisitor> branchStarters = new Queue<CriticalPathVisitor>();
        StartRoomBorderVisitor visitor = new StartRoomBorderVisitor(startingPoint, Directions.Top, path);
        MoveReport moveReport = null;
        do
        {
            VisitReport visitReport = visitor.Visit();

            if (!visitReport.WasSuccessful)
            {
                Debug.LogError("Start Room initial visit failed");
                return;
            }

            foreach (Pixel neighbor in visitReport.Neighbors.GetNeighbors())
            {
                if (neighbor.Color == Color.red)
                {
                    Directions branchSeedDirection = visitReport.Neighbors.GetDirectionOfNeighbor(neighbor);
                    branchStarters.Enqueue(new CriticalPathVisitor(new Vector2Int(neighbor.X, neighbor.Y), branchSeedDirection, path, 0));
                }
            }

            moveReport = visitor.MoveOn();
        } while (moveReport == null || !moveReport.VisitorIsDone);

        if (branchStarters.Count == 0)
        {
            Debug.LogError("No branches off of the start square found");
            return;
        }

        //Now we need to visit all of the branches and generate the "seeds" that we will use later to 
        //make some rooms.
        RoomSeed[,] roomSeeds = new RoomSeed[48, 48];
        CriticalPathVisitor currentVisitor = branchStarters.Dequeue();
        int counter = 0;
        while (currentVisitor != null)
        {
            MoveReport branchMoveReport = null;

            do
            {
                counter++;
                VisitReport visitReport = currentVisitor.Visit();

                if (!visitReport.WasSuccessful)
                {
                    Debug.LogError("Failed to follow critical path.");
                    return;
                }

                RoomSeed newSeed = new RoomSeed(visitReport.Neighbors.Original.GetPositionAsVector2Int(), currentVisitor.SectionNumber);
                if (roomSeeds[newSeed.Position.y, newSeed.Position.x] != null)
                {
                    newSeed = roomSeeds[newSeed.Position.y, newSeed.Position.x];
                }

                newSeed.AddEntrance(new TravelData(currentVisitor.Direction, currentVisitor.SectionNumber));

                //Before we add the new seed, we want to add an exit to the previous seed, if there is one.
                if (currentVisitor.PreviousSeed != null && roomSeeds[currentVisitor.PreviousSeed.Position.y, currentVisitor.PreviousSeed.Position.x] != null)
                {
                    Debug.Log("Setting exit for " + currentVisitor.PreviousSeed.Position + " to " + currentVisitor.Direction + ". Counter is: " + counter);
                    roomSeeds[currentVisitor.PreviousSeed.Position.y, currentVisitor.PreviousSeed.Position.x].AddExit(new TravelData(currentVisitor.Direction, currentVisitor.SectionNumber));
                }

                roomSeeds[newSeed.Position.y, newSeed.Position.x] = newSeed;

                currentVisitor.PreviousSeed = newSeed;
                branchMoveReport = currentVisitor.MoveOn();
            } while (!branchMoveReport.VisitorIsDone);

            if (branchStarters.Count > 0)
            {
                currentVisitor = branchStarters.Dequeue();
            }
            else
            {
                currentVisitor = null;
            }
        }

        Debug.Log("Counter = " + counter);
        for (int y = 0; y < roomSeeds.GetLength(0); y++)
        {
            for (int x = 0; x < roomSeeds.GetLength(1); x++)
            {
                if (roomSeeds[y, x] != null)
                {
                    Debug.Log(roomSeeds[y, x]);
                }
            }
        }
    }
}

//RoomSeed[,] roomSeeds = new RoomSeed[48, 48];
//CriticalPathVisitor currentVisitor = branchStarters.Dequeue();
//while (currentVisitor != null)
//{
//    MoveReport branchMoveReport = null;

//    do
//    {
//        VisitReport visitReport = visitor.Visit();

//        if (!visitReport.WasSuccessful)
//        {
//            Debug.LogError("Failed to follow critical path.");
//            return;
//        }


//        RoomSeed currentSeed = new RoomSeed(visitReport.Neighbors.Original.GetPositionAsVector2Int(), currentVisitor.SectionNumber);
//        if (roomSeeds[currentSeed.Position.y, currentSeed.Position.x] != null)
//        {
//            currentSeed = roomSeeds[currentSeed.Position.y, currentSeed.Position.x];
//        }

//        currentSeed.AddEntrance(new TravelData(currentVisitor.Direction, currentVisitor.SectionNumber));

//        //Before we add the new seed, we want to add an exit to the previous seed, if there is one.
//        RoomSeed previousSeed = currentVisitor.PreviousSeed;
//        if (previousSeed != null)
//        {
//            RoomSeed previousSeedInList = roomSeeds[previousSeed.Position.y, previousSeed.Position.x];
//            if (previousSeedInList != null)
//            {
//                previousSeedInList.AddExit(new TravelData(DirectionsUtil.GetOppositeDirection(currentVisitor.Direction), currentVisitor.SectionNumber));
//            }
//        }

//        roomSeeds[currentSeed.Position.y, currentSeed.Position.x] = currentSeed;

//        currentVisitor.PreviousSeed = currentSeed;

//        branchMoveReport = visitor.Move();
//    } while (!branchMoveReport.VisitorIsDone);


//List<RoomSeed> roomSeeds = new List<RoomSeed>();
//CriticalPathVisitor currentVisitor = branchStarters.Dequeue();
//int counter = 0;
//while (currentVisitor != null)
//{
//    MoveReport branchMoveReport = null;
//    List<RoomSeed> currentBranchSeeds = new List<RoomSeed>();

//    do
//    {
//        counter++;
//        VisitReport visitReport = currentVisitor.Visit();

//        if (!visitReport.WasSuccessful)
//        {
//            Debug.LogError("Failed to follow critical path.");
//            return;
//        }

//        RoomSeed newSeed = new RoomSeed(visitReport.Neighbors.Original.GetPositionAsVector2Int(), currentVisitor.SectionNumber);
//        newSeed.AddEntrance(new TravelData(currentVisitor.Direction, currentVisitor.SectionNumber));

//        //Before we add the new seed, we want to add an exit to the previous seed, if there is one.
//        if (currentVisitor.PreviousSeed != null)
//        {
//            Debug.Log("Setting exit for " + currentVisitor.PreviousSeed.Position + " to " + currentVisitor.Direction);
//            currentVisitor.PreviousSeed.AddExit(new TravelData(currentVisitor.Direction, currentVisitor.SectionNumber));
//        }

//        currentBranchSeeds.Add(newSeed);

//        currentVisitor.PreviousSeed = newSeed;
//        branchMoveReport = currentVisitor.MoveOn();
//    } while (!branchMoveReport.VisitorIsDone);
