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
    private bool reachedBossRoom = false;

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
        set { previousSeed = value; } 
    }

    public CriticalPathVisitor BuildingOffOf { get { return successorOf; } set { successorOf = value; } }

    public bool ReachedBossRoom { get { return reachedBossRoom; } }

    public override MoveReport MoveOn()
    {
        //Debug.Log("Critical Path Visitor at " + position + " moving in direction " + direction);

        PreliminaryMovementData prelimData = MoveOnPrelimWork();

        if (!prelimData.prelimWorkSuccessful || isDone)
        {
            return new MoveReport(false, isDone);
        }

        bool successfullyMoved = isDone;
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
                else if(neighbor.Color.Equals(Color.black))
                {
                    reachedBossRoom = true;
                    break;
                }
            }
        }

        if (!successfullyMoved)
        {
            //We've reached the end of this branch, so we're done.
            isDone = true;
        }

        return new MoveReport(successfullyMoved, isDone);
    }
}

public class TravelData
{
    private Directions travelDirection;
    private int sectionNumber;
    private Vector2Int position;

    private RoomSeed leadsTo = null;

    public TravelData(Directions travelDirection, int sectionNumber, Vector2Int position, RoomSeed leadsTo)
    {
        this.travelDirection = travelDirection;
        this.sectionNumber = sectionNumber;
        this.position = position;
        this.leadsTo = leadsTo;
    }
    
    public Directions TravelDirection { get { return travelDirection; } }

    public int SectionNumber { get { return sectionNumber; } }

    public RoomSeed LeadsTo { get { return leadsTo; } }
    public void SetLeadsTo(RoomSeed leadsTo)
    {
        this.leadsTo = leadsTo;
    }

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
    private RoomSeedComposite partOf = null;

    public RoomSeed(Vector2Int position, int sectionNumber)
    {
        this.position = position;
        this.sectionNumber = sectionNumber;
    }
    public Vector2Int Position { get { return position; } }
    public int SectionNumber { get { return sectionNumber; } }

    public List<TravelData> Entrances { get { return entrances; } }

    public List<TravelData> Exits { get { return exits; } }

    public bool PartOfComposite { get { return partOf != null; } }

    public RoomSeedComposite PartOf { get { return partOf; } set { partOf = value; } }

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

public class RoomSeedComposite: RoomSeed
{
    private List<RoomSeed> internalSeeds = new List<RoomSeed>();
    private int width;
    private int height;

    public RoomSeedComposite(Vector2Int position, int sectionNumber, int width, int height) : base(position, sectionNumber)
    {
        this.width = width;
        this.height = height;
    }

    public List<RoomSeed> InternalSeeds { get { return internalSeeds; } }

    public int Width { get { return width; } }

    public int Height { get { return height; } }

    public void AddInternalSeed(RoomSeed seed)
    {
        internalSeeds.Add(seed);
        seed.PartOf = this;
    }

    private void DetermineOpenings(List<RoomSeed> seeds, Directions direction)
    {
        foreach (RoomSeed seed in seeds)
        {
            foreach (TravelData exit in seed.Exits)
            {
                if (exit.TravelDirection == direction)
                {
                    AddExit(exit);
                }
            }

            foreach (TravelData entrance in seed.Entrances)
            {
                if (entrance.TravelDirection == direction)
                {
                    AddEntrance(entrance);
                }
            }
        }
    }

    public void GenerateEntrancesAndExits()
    {
        RoomSeed rightMostSeed = null;
        RoomSeed leftMostSeed = null;
        RoomSeed topMostSeed = null;
        RoomSeed bottomMostSeed = null;

        foreach (RoomSeed seed in internalSeeds)
        {
            if (rightMostSeed == null || seed.Position.x > rightMostSeed.Position.x)
            {
                rightMostSeed = seed;
            }
            if (leftMostSeed == null || seed.Position.x < leftMostSeed.Position.x)
            {
                leftMostSeed = seed;
            }
            if (topMostSeed == null || seed.Position.y > topMostSeed.Position.y)
            {
                topMostSeed = seed;
            }
            if (bottomMostSeed == null || seed.Position.y < bottomMostSeed.Position.y)
            {
                bottomMostSeed = seed;
            }
        }

        List<RoomSeed> allRightMostSeeds = new List<RoomSeed>();
        List<RoomSeed> allLeftMostSeeds = new List<RoomSeed>();
        List<RoomSeed> allTopMostSeeds = new List<RoomSeed>();
        List<RoomSeed> allBottomMostSeeds = new List<RoomSeed>();

        foreach (RoomSeed seed in internalSeeds)
        {
            if (seed.Position.x == rightMostSeed.Position.x)
            {
                allRightMostSeeds.Add(seed);
            }
            if (seed.Position.x == leftMostSeed.Position.x)
            {
                allLeftMostSeeds.Add(seed);
            }
            if (seed.Position.y == topMostSeed.Position.y)
            {
                allTopMostSeeds.Add(seed);
            }
            if (seed.Position.y == bottomMostSeed.Position.y)
            {
                allBottomMostSeeds.Add(seed);
            }
        }

        DetermineOpenings(allRightMostSeeds, Directions.Right);
        DetermineOpenings(allLeftMostSeeds, Directions.Left);
        DetermineOpenings(allTopMostSeeds, Directions.Top);
        DetermineOpenings(allBottomMostSeeds, Directions.Bottom);
    }
}

public class RoomSeedExpander
{
    public RoomSeedExpander()
    {
    }

    public RoomSeedComposite ExpandRoom(RoomSeed[,] roomSeeds, int width, int height, Vector2Int startingPosition)
    {
        RoomSeedComposite composite = new RoomSeedComposite(startingPosition, roomSeeds[startingPosition.y, startingPosition.x].SectionNumber, width, height);

        int yGoal = Mathf.Clamp(startingPosition.y + height, 0, roomSeeds.GetLength(1));
        int xGoal = Mathf.Clamp(startingPosition.x + width, 0, roomSeeds.GetLength(0));

        for (int y = startingPosition.y; y < yGoal; y++)
        {
            for (int x = startingPosition.x; x < xGoal; x++)
            {
                if (roomSeeds[y, x] != null && !roomSeeds[y, x].PartOfComposite)
                {
                    composite.AddInternalSeed(roomSeeds[y, x]);
                }
            }
        }

        composite.GenerateEntrancesAndExits();

        return composite;
    }
}

public class MapGenerator : MonoBehaviour
{
    public bool generateNewMap = false;
    public int usePathsFromLevel = 0;
    public int useRoomsFromLevel = 0;

    [Range(0, 100)]
    public int chanceToExpandRoom = 50;
    public int roomMinExpandedWidth = 2;
    public int roomMaxExpandedWidth = 4;
    public int roomMinExpandedHeight = 2;
    public int roomMaxExpandedHeight = 4;

    public RoomWithOpeningMarks roomVisualPrefab;

    private Texture2D[] paths;
    private RoomVisualizer[] roomVisualizers;

    private void Start()
    {
        paths = Resources.LoadAll<Texture2D>("Paths/Level " + usePathsFromLevel + " Paths");
    
        roomVisualizers = Resources.LoadAll<RoomVisualizer>("Rooms/Level " + useRoomsFromLevel + " Rooms");
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
                if (startingPoint.x == -1 && path.GetPixel(i, j).Equals(Color.white))
                {
                    //With the way that Texture2D handles coordinates, (0,0) being at the bottom left,
                    //and this algorithm searching from bottom to top, this starting point should be
                    //the bottom left corner of the starting room.
                    startingPoint = new Vector2Int(i, j);
                }
                else if (endPoint.x == -1 && path.GetPixel(i, j).Equals(Color.black))
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
        StartRoomBorderVisitor startBorderVisitor = new StartRoomBorderVisitor(startingPoint, Directions.Top, path);
        MoveReport moveReport = null;
        do
        {
            VisitReport visitReport = startBorderVisitor.Visit();

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

            moveReport = startBorderVisitor.MoveOn();
        } while (moveReport == null || !moveReport.VisitorIsDone);

        if (branchStarters.Count == 0)
        {
            Debug.LogError("No branches off of the start square found");
            return;
        }

        //Now we need to visit all of the branches and generate the "seeds" that we will use later to 
        //make some rooms. Additionally, we need to generate the initial and boss rooms.
        RoomSeed[,] roomSeeds = new RoomSeed[48, 48];
        List<Vector2Int> criticalPathSeedPositions = new List<Vector2Int>();//Makes the next step after generating the seeds easier.
        CriticalPathVisitor currentVisitor = branchStarters.Dequeue();

        Vector2Int startRoomCenter = new Vector2Int(-1, -1);//TODO: Turn this and the next bit dealing with the boss room into a method.
        Vector2Int tempPosition = startingPoint; //StartingPoint is the bottom left corner of the start room.
        List<Vector2Int> potentialStartCenters = new List<Vector2Int>();
        for (int i = 0; i < 9; i++) //The maximum size of the start room square in the path representation is 9x9.
        {
            potentialStartCenters.Add(tempPosition);
            tempPosition += new Vector2Int(1, 1);

            if(!path.GetPixel(tempPosition.x, tempPosition.y).Equals(Color.white))
            {
                break;
            }
        }

        if(potentialStartCenters.Count == 0)
        {
            Debug.LogError("No potential centers found for start room");
            return;
        }
        else if(potentialStartCenters.Count % 2 == 0)
        {
            Debug.LogError("The start room has no real center becuase its dimensions are even."); //TODO: There might be a better/more accurate way to do this by using the borderVisitor from earlier to measure the dimensions of the starting room.
            return;
        }
        else
        {
            startRoomCenter = potentialStartCenters[potentialStartCenters.Count / 2];
            Debug.Log(startingPoint);
        }

        Vector2Int bossRoomCenter = new Vector2Int(-1, -1);
        tempPosition = endPoint;
        List<Vector2Int> potentialEndCenters = new List<Vector2Int>();
        for (int i = 0; i < 9; i++) //The maximum size of the boss room square in the path representation is 9x9.
        {
            potentialEndCenters.Add(tempPosition);
            tempPosition += new Vector2Int(1, 1);

            if (!path.GetPixel(tempPosition.x, tempPosition.y).Equals(Color.black))
            {
                break;
            }
        }

        if (potentialEndCenters.Count == 0)
        {
            Debug.LogError("No potential centers found for boss room");
            return;
        }
        else if (potentialEndCenters.Count % 2 == 0)
        {
            Debug.LogError("The boss room has no real center because its dimensions are even."); //TODO: There might be a better/more accurate way to do this by using the borderVisitor from earlier to measure the dimensions of the starting room.
            return;
        }
        else
        {
            bossRoomCenter = potentialEndCenters[potentialEndCenters.Count / 2];
        }

        RoomSeed startRoomSeed = new RoomSeed(startRoomCenter, 0);
        RoomSeed bossRoomSeed = new RoomSeed(bossRoomCenter, 0);
        roomSeeds[startRoomCenter.y, startRoomCenter.x] = startRoomSeed;
        roomSeeds[bossRoomCenter.y, bossRoomCenter.x] = bossRoomSeed;

        currentVisitor.PreviousSeed = startRoomSeed;//We dequeued earlier and it's not in the list anymore, so we need to set the current visitor's previous seed manually

        foreach (CriticalPathVisitor v in branchStarters)
        {
            v.PreviousSeed = startRoomSeed;
        }

        while (currentVisitor != null)//Now we deal with the critical path.
        {
            MoveReport branchMoveReport = null;
            do
            {
                VisitReport visitReport = currentVisitor.Visit();

                if (!visitReport.WasSuccessful)
                {
                    Debug.LogError("Failed to follow critical path.");
                    return;
                }

                RoomSeed currentSeed = new RoomSeed(visitReport.Neighbors.Original.GetPositionAsVector2Int(), currentVisitor.SectionNumber);
                if (roomSeeds[currentSeed.Position.y, currentSeed.Position.x] != null)
                {
                    currentSeed = roomSeeds[currentSeed.Position.y, currentSeed.Position.x];
                }

                if (currentVisitor.PreviousSeed != null && roomSeeds[currentVisitor.PreviousSeed.Position.y, currentVisitor.PreviousSeed.Position.x] != null)
                {
                    //We reverse the direction for the entrance because, for example, the visitor moves to the right, entering the new room from the left.
                    currentSeed.AddEntrance(new TravelData(DirectionsUtil.GetOppositeDirection(currentVisitor.Direction), currentVisitor.SectionNumber, currentSeed.Position, currentVisitor.PreviousSeed));

                    roomSeeds[currentVisitor.PreviousSeed.Position.y, currentVisitor.PreviousSeed.Position.x].AddExit(new TravelData(currentVisitor.Direction, currentVisitor.SectionNumber, currentSeed.Position, currentSeed));
                }
                else
                {
                    Debug.LogError("No previous seed found for critical path visitor.");
                    return;
                }

                roomSeeds[currentSeed.Position.y, currentSeed.Position.x] = currentSeed;
                if(!criticalPathSeedPositions.Contains(currentSeed.Position))
                {
                    criticalPathSeedPositions.Add(currentSeed.Position);
                }

                currentVisitor.PreviousSeed = currentSeed;
                branchMoveReport = currentVisitor.MoveOn();

                if (currentVisitor.ReachedBossRoom)
                {
                    currentSeed.AddExit(new TravelData(currentVisitor.Direction, currentVisitor.SectionNumber, bossRoomCenter, bossRoomSeed));
                    roomSeeds[bossRoomCenter.y, bossRoomCenter.x].AddEntrance(new TravelData(DirectionsUtil.GetOppositeDirection(currentVisitor.Direction), currentVisitor.SectionNumber, bossRoomCenter, currentSeed));
                }
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

        //InstantiateRoomVisuals(roomSeeds);

        //Grow some of the room seeds and make them encompass some of their fellow seeds.
        RoomSeedExpander expander = new RoomSeedExpander();
        List<RoomSeedComposite> roomSeedComposites = new List<RoomSeedComposite>();
        RoomSeedComposite startRoomComposite = new RoomSeedComposite(startRoomSeed.Position, startRoomSeed.SectionNumber, potentialStartCenters.Count, potentialStartCenters.Count);
        RoomSeedComposite bossRoomComposite = new RoomSeedComposite(bossRoomSeed.Position, bossRoomSeed.SectionNumber, potentialEndCenters.Count, potentialEndCenters.Count);

        startRoomComposite.AddInternalSeed(startRoomSeed);
        bossRoomComposite.AddInternalSeed(bossRoomSeed);

        startRoomComposite.GenerateEntrancesAndExits();
        bossRoomComposite.GenerateEntrancesAndExits();

        roomSeedComposites.Add(startRoomComposite);

        foreach (Vector2Int position in criticalPathSeedPositions)
        {
            if (roomSeeds[position.y, position.x] != null && !roomSeeds[position.y, position.x].PartOfComposite)
            {
                int width = 1;
                int height = 1;

                if(Random.Range(0, 100) < chanceToExpandRoom)
                {
                    width = Random.Range(roomMinExpandedWidth, roomMaxExpandedWidth);
                    height = Random.Range(roomMinExpandedHeight, roomMaxExpandedHeight);
                }
                
                RoomSeedComposite composite = expander.ExpandRoom(roomSeeds, width, height, position);
                roomSeedComposites.Add(composite);
            }
        }

        roomSeedComposites.Add(bossRoomComposite);

        //InstantiateCompositeRoomVisuals(roomSeedComposites);
        ////Display the composite room seeds in the console.
        //foreach (RoomSeedComposite composite in roomSeedComposites)
        //{
        //    Debug.Log(composite);
        //}

        //Now we need to generate the actual rooms.
        Stack<RoomSeedComposite> roomSeedStack = new Stack<RoomSeedComposite>();
        roomSeedStack.Push(startRoomComposite);
        while(roomSeedStack.Count > 0)
        {
            RoomSeedComposite currentComposite = roomSeedStack.Pop();
            RoomVisualizer room = PickRoom(currentComposite);

            if(room == null)
            {
                Debug.LogError("No room found for composite " + currentComposite);
                return;
            }

            //Instantiate<RoomVisualizer>()
        }
    }

    private RoomVisualizer PickRoom(RoomSeedComposite composite)
    {
        List<RoomVisualizer> potentialRooms = new List<RoomVisualizer>();

        foreach (RoomVisualizer room in roomVisualizers)
        {
            if (room.width == composite.Width && room.height == composite.Height)
            {
                return room;
            }
        }

        RoomVisualizer roomToReturn = null;

        roomToReturn = potentialRooms[Random.Range(0, potentialRooms.Count)];

        return roomToReturn;
    }

    private void InstantiateCompositeRoomVisuals(List<RoomSeedComposite> composites)
    {
        foreach (RoomSeedComposite composite in composites)
        {
            GameObject roomVisual = Instantiate(roomVisualPrefab.GameObject(), new Vector3(0, 0, 0), Quaternion.identity);
            RoomWithOpeningMarks room = roomVisual.GetComponent<RoomWithOpeningMarks>();
            room.position = composite.Position;
            room.width = composite.Width;
            room.height = composite.Height;

            foreach (TravelData entrance in composite.Entrances)
            {
                switch (entrance.TravelDirection)
                {
                    case Directions.Top:
                        room.topOpenings++;
                        break;
                    case Directions.Bottom:
                        room.bottomOpenings++;
                        break;
                    case Directions.Left:
                        room.leftOpenings++;
                        break;
                    case Directions.Right:
                        room.rightOpenings++;
                        break;
                }
            }

            foreach (TravelData exit in composite.Exits)
            {
                switch (exit.TravelDirection)
                {
                    case Directions.Top:
                        room.topOpenings++;
                        break;
                    case Directions.Bottom:
                        room.bottomOpenings++;
                        break;
                    case Directions.Left:
                        room.leftOpenings++;
                        break;
                    case Directions.Right:
                        room.rightOpenings++;
                        break;
                }
            }
        }
    }

    private void InstantiateRoomVisuals(RoomSeed[,] roomSeeds)
    {
        for(int y = 0; y < roomSeeds.GetLength(0); y++)
        {
            for(int x = 0; x < roomSeeds.GetLength(1); x++)
            {
                if(roomSeeds[y, x] == null)
                {
                    continue;
                }

                GameObject roomVisual = Instantiate(roomVisualPrefab.GameObject(), new Vector3(x, 0, y), Quaternion.identity);
                RoomWithOpeningMarks room = roomVisual.GetComponent<RoomWithOpeningMarks>();
                foreach(TravelData entrance in roomSeeds[y, x].Entrances)
                {
                    switch (entrance.TravelDirection)
                    {
                        case Directions.Top:
                            room.topOpenings = 1;
                            break;
                        case Directions.Bottom:
                            room.bottomOpenings = 1;
                            break;
                        case Directions.Left:
                            room.leftOpenings = 1;
                            break;
                        case Directions.Right:
                            room.rightOpenings = 1;
                            break;
                    }
                }

                foreach (TravelData exit in roomSeeds[y, x].Exits)
                {
                    switch (exit.TravelDirection)
                    {
                        case Directions.Top:
                            room.topOpenings = 1;
                            break;
                        case Directions.Bottom:
                            room.bottomOpenings = 1;
                            break;
                        case Directions.Left:
                            room.leftOpenings = 1;
                            break;
                        case Directions.Right:
                            room.rightOpenings = 1;
                            break;
                    }
                }
            }
        }
    }
}