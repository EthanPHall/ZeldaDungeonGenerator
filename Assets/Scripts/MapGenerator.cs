using OpenCover.Framework.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

[Serializable]
public class Pixel
{
    [SerializeField] private bool transparent;
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private Color color;
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

    public Pixel(Pixel pixel)
    {
        x = pixel.x;
        y = pixel.y;
        color = pixel.color;
        transparent = pixel.transparent;
    }

    public int X { get { return x; } }
    public int Y { get { return y; } }
    public Color Color { get { return color; } }
    public bool IsTransparent { get { return transparent; } }

    public void SetColor(Color color)
    {
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

    public bool AreValuesEquivalent(Pixel other)
    {
        return x == other.x && y == other.y && color.Equals(other.color);
    }

    public Vector2Int GetPosition()
    {
        return new Vector2Int(x, y);
    }

    public void SetPosition(Vector2Int position)
    {
        x = position.x;
        y = position.y;
    }

    public override string ToString()
    {
        return "Pixel at " + x + ", " + y + " with color " + color;
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
    protected PixelNeighbors FindNeighbors(Texture2D path, Vector2Int centralPixel)
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

    public virtual VisitReport Visit()
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

[Serializable]
public class RoomOpening
{
    [SerializeField] private Directions travelDirection;
    [SerializeField] private int sectionNumber;
    [SerializeField] private Vector2Int position;

    [SerializeField] private RoomOpening leadsTo = null;
    [SerializeField] private Vector2Int leadsToPosition = new Vector2Int(-1,-1);
    [SerializeField] private RoomSeed belongsTo = null;

    public RoomOpening(Directions travelDirection, int sectionNumber, Vector2Int position)
    {
        this.travelDirection = travelDirection;
        this.sectionNumber = sectionNumber;
        this.position = position;
    }

    public Vector2Int Position { get { return position; } }

    public Directions TravelDirection { get { return travelDirection; } }

    public int SectionNumber { get { return sectionNumber; } }

    public RoomOpening LeadsTo { get { return leadsTo; } }
    public void SetLeadsTo(RoomOpening leadsTo)
    {
        this.leadsTo = leadsTo;
        this.leadsToPosition = leadsTo.Position;
    }

    public RoomSeed BelongsTo { get { return belongsTo; } }
    public void SetBelongsTo(RoomSeed belongsTo)
    {
        this.belongsTo = belongsTo;
    }

    public override string ToString()
    {
        return "TravelData: " + travelDirection + " to section " + sectionNumber;
    }
}

[Serializable]
public class RoomSeed
{
    [SerializeField] private Vector2Int position;
    [SerializeField] private List<RoomOpening> entrances = new List<RoomOpening>();
    [SerializeField] private List<RoomOpening> exits = new List<RoomOpening>();
    [SerializeField] private int sectionNumber;
    [SerializeField] private RoomSeedComposite partOf = null;

    public RoomSeed(Vector2Int position, int sectionNumber)
    {
        this.position = position;
        this.sectionNumber = sectionNumber;
    }
    public Vector2Int Position { get { return position; } }
    public int SectionNumber { get { return sectionNumber; } }

    public List<RoomOpening> Entrances { get { return entrances; } }

    public List<RoomOpening> Exits { get { return exits; } }

    public bool PartOfComposite { get { return partOf != null; } }

    public virtual RoomSeedComposite PartOf { get { return partOf; } set { partOf = value; } }

    public void AddEntrance(RoomOpening entrance)
    {
        entrances.Add(entrance);
        entrance.SetBelongsTo(this);
    }

    public void AddExit(RoomOpening exit)
    {
        exits.Add(exit);
        exit.SetBelongsTo(this);
    }

    public List<RoomOpening> GetOpeningsInDirection(Directions direction)
    {
        List<RoomOpening> openings = new List<RoomOpening>();
        foreach (RoomOpening entrance in Entrances)
        {
            if (entrance.TravelDirection == direction)
            {
                openings.Add(entrance);
            }
        }
        foreach (RoomOpening exit in Exits)
        {
            if (exit.TravelDirection == direction)
            {
                openings.Add(exit);
            }
        }
        return openings;
    }


    public override string ToString()
    {
        string entranceString = "Entrances: ";
        foreach (RoomOpening entrance in entrances)
        {
            entranceString += entrance.ToString() + ", ";
        }

        string exitString = "Exits: ";
        foreach (RoomOpening exit in exits)
        {
            exitString += exit.ToString() + ", ";
        }

        return "RoomSeed at position " + position + " in section " + sectionNumber + " with \n" + entranceString + " and \n" + exitString;
    }
}

[Serializable]
public class RoomSeedComposite: RoomSeed
{
    [SerializeField] private List<RoomSeed> internalSeeds = new List<RoomSeed>();
    [SerializeField] private int width;
    [SerializeField] private int height;

    [SerializeField] private RoomData roomData;
    [SerializeField] private bool baseRoom = false;

    public RoomSeedComposite(Vector2Int position, int sectionNumber, int width, int height) : base(position, sectionNumber)
    {
        this.width = width;
        this.height = height;
    }

    public List<RoomSeed> InternalSeeds { get { return internalSeeds; } }

    public int Width { get { return width; } }

    public int Height { get { return height; } }

    public RoomData RoomData { get { return roomData; } set { roomData = value; } }

    public bool HasGeneratedRoomData { get { return roomData != null; } }

    public bool IsBaseRoom { get { return baseRoom; } }
    public void SetIsBaseRoom(bool isBaseRoom)
    {
        baseRoom = isBaseRoom;
    }

    public override RoomSeedComposite PartOf { get { return this; } set { return; } }


    public void AddInternalSeed(RoomSeed seed)
    {
        internalSeeds.Add(seed);
        seed.PartOf = this;
    }

    private void DetermineOpenings(List<RoomSeed> seeds, Directions direction)
    {
        foreach (RoomSeed seed in seeds)
        {
            foreach (RoomOpening exit in seed.Exits)
            {
                if (exit.TravelDirection == direction)
                {
                    AddExit(exit);

                    //Reevaluate where the exit leads to.
                    if (exit.LeadsTo == null)
                    {
                        continue;
                    }
                    
                    RoomSeed leadsToRoom = exit.LeadsTo.BelongsTo;
                    if (leadsToRoom.PartOfComposite)//We need to lead into the composite, not the individual seed.
                    {
                        RoomSeedComposite leadsToComposite = leadsToRoom.PartOf;
                        List<RoomOpening> compositeEntrances = leadsToComposite.GetEntrancesInDirection(direction);
                        RoomOpening closestCompositeEntrance = null;
                        foreach(RoomOpening compositeEntrance in compositeEntrances)
                        {
                            if (closestCompositeEntrance == null || Vector2Int.Distance(exit.Position, compositeEntrance.Position) < Vector2Int.Distance(seed.Position, closestCompositeEntrance.Position))
                            {
                                closestCompositeEntrance = compositeEntrance;
                            }
                        }

                        if(closestCompositeEntrance != null)
                        {
                            exit.SetLeadsTo(closestCompositeEntrance);
                            closestCompositeEntrance.SetLeadsTo(exit);
                        }
                    }
                }
            }

            foreach (RoomOpening entrance in seed.Entrances)
            {
                if (entrance.TravelDirection == direction)
                {
                    AddEntrance(entrance);

                    //Reevaluate where the entrance leads to.
                    if (entrance.LeadsTo == null)
                    {
                        continue;
                    }

                    RoomSeed leadsToRoom = entrance.LeadsTo.BelongsTo;
                    if (leadsToRoom.PartOfComposite)//We need to lead into the composite, not the individual seed.
                    {
                        RoomSeedComposite leadsToComposite = leadsToRoom.PartOf;
                        List<RoomOpening> compositeExits = leadsToComposite.GetExitsInDirection(direction);
                        RoomOpening closestCompositeExit = null;
                        foreach (RoomOpening compositeExit in compositeExits)
                        {
                            if (closestCompositeExit == null || Vector2Int.Distance(entrance.Position, compositeExit.Position) < Vector2Int.Distance(seed.Position, closestCompositeExit.Position))
                            {
                                closestCompositeExit = compositeExit;
                            }
                        }
                        if (closestCompositeExit != null)
                        {
                            entrance.SetLeadsTo(closestCompositeExit);
                            closestCompositeExit.SetLeadsTo(entrance);
                        }
                    }
                }
            }
        }
    }

    public void GenerateEntrancesAndExits()
    {
        List<RoomSeed> allRightMostSeeds = new List<RoomSeed>();//Seeds that are the rightmost in their row.
        List<RoomSeed> allLeftMostSeeds = new List<RoomSeed>();
        List<RoomSeed> allTopMostSeeds = new List<RoomSeed>();//Seeds that are the topmost in their column.
        List<RoomSeed> allBottomMostSeeds = new List<RoomSeed>();

        List<int> allRowCoordinates = new List<int>();
        List<int> allColumnCoordinates = new List<int>();

        foreach (RoomSeed seed in internalSeeds)
        {
            if (!allColumnCoordinates.Contains(seed.Position.x))
            {
                allColumnCoordinates.Add(seed.Position.x);
            }
            if (!allRowCoordinates.Contains(seed.Position.y))
            {
                allRowCoordinates.Add(seed.Position.y);
            }
        }

        foreach(int column in allColumnCoordinates)
        {
            RoomSeed topMostSeed = null;
            RoomSeed bottomMostSeed = null;
            foreach (RoomSeed seed in internalSeeds)
            {
                if (seed.Position.x == column)
                {
                    if (topMostSeed == null || seed.Position.y > topMostSeed.Position.y)
                    {
                        topMostSeed = seed;
                    }
                    if (bottomMostSeed == null || seed.Position.y < bottomMostSeed.Position.y)
                    {
                        bottomMostSeed = seed;
                    }
                }
            }

            if (topMostSeed != null)
            {
                allTopMostSeeds.Add(topMostSeed);
            }
            if (bottomMostSeed != null)
            {
                allBottomMostSeeds.Add(bottomMostSeed);
            }
        }

        foreach (int row in allRowCoordinates)
        {
            RoomSeed rightMostSeed = null;
            RoomSeed leftMostSeed = null;
            foreach (RoomSeed seed in internalSeeds)
            {
                if (seed.Position.y == row)
                {
                    if (rightMostSeed == null || seed.Position.x > rightMostSeed.Position.x)
                    {
                        rightMostSeed = seed;
                    }
                    if (leftMostSeed == null || seed.Position.x < leftMostSeed.Position.x)
                    {
                        leftMostSeed = seed;
                    }
                }
            }
            if (rightMostSeed != null)
            {
                allRightMostSeeds.Add(rightMostSeed);
            }
            if (leftMostSeed != null)
            {
                allLeftMostSeeds.Add(leftMostSeed);
            }
        }

        DetermineOpenings(allRightMostSeeds, Directions.Right);
        DetermineOpenings(allLeftMostSeeds, Directions.Left);
        DetermineOpenings(allTopMostSeeds, Directions.Top);
        DetermineOpenings(allBottomMostSeeds, Directions.Bottom);
    }

    //TODO: This could probably be done in the function(s) that actually generates the exit and entrance data.
    //Just have some local variable lists that get populated in those other functions.
    public List<RoomOpening> GetExitsInDirection(Directions direction)
    {
        List<RoomOpening> exits = new List<RoomOpening>();
        foreach (RoomOpening exit in Exits)
        {
            if (exit.TravelDirection == direction)
            {
                exits.Add(exit);
            }
        }
        return exits;
    }

    public List<RoomOpening> GetEntrancesInDirection(Directions direction)
    {
        List<RoomOpening> entrances = new List<RoomOpening>();
        foreach (RoomOpening entrance in Entrances)
        {
            if (entrance.TravelDirection == direction)
            {
                entrances.Add(entrance);
            }
        }

        return entrances;
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

        for (int y = startingPosition.y; y <= yGoal; y++)
        {
            for (int x = startingPosition.x; x <= xGoal; x++)
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

    public bool detectRoomCollisions = true;

    [Range(0, 100)]
    public int chanceToExpandRoom = 50;
    public int roomMinExpandedWidth = 2;
    public int roomMaxExpandedWidth = 4;
    public int roomMinExpandedHeight = 2;
    public int roomMaxExpandedHeight = 4;

    public int connectionAttemptsMax = 1000;

    public int averagePixelsPerSmallRoom = 100;

    public RoomWithOpeningMarks roomVisualPrefab;

    public GameObject wallPrefab;

    private Texture2D[] paths;
    private List<RoomData> baseRooms = new List<RoomData>();

    private void Start()
    {
        paths = Resources.LoadAll<Texture2D>("Paths/Level " + usePathsFromLevel + " Paths");

        TextAsset[] baseRoomTexts = Resources.LoadAll<TextAsset>("Rooms/Level " + useRoomsFromLevel + " Rooms");

        foreach (TextAsset roomText in baseRoomTexts)
        {
            RoomData roomData = JsonUtility.FromJson<RoomData>(roomText.text);

            if (roomData != null)
            {
                baseRooms.Add(roomData);
            }
        }

        Debug.Log("Base rooms found: " + baseRooms.Count);

        if (baseRooms.Count == 0)
        {
            Debug.LogError("No base rooms found in level " + useRoomsFromLevel);
        }
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

            Texture2D path = paths[UnityEngine.Random.Range(0, paths.Length)];

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

                RoomSeed currentSeed = new RoomSeed(visitReport.Neighbors.Original.GetPosition(), currentVisitor.SectionNumber);
                if (roomSeeds[currentSeed.Position.y, currentSeed.Position.x] != null)
                {
                    currentSeed = roomSeeds[currentSeed.Position.y, currentSeed.Position.x];
                }

                if (currentVisitor.PreviousSeed != null && roomSeeds[currentVisitor.PreviousSeed.Position.y, currentVisitor.PreviousSeed.Position.x] != null)
                {
                    //We reverse the direction for the entrance because, for example, the visitor moves to the right, entering the new room from the left.
                    RoomOpening entrance = new RoomOpening(DirectionsUtil.GetOppositeDirection(currentVisitor.Direction), currentVisitor.SectionNumber, currentSeed.Position);
                    RoomOpening exit = new RoomOpening(currentVisitor.Direction, currentVisitor.SectionNumber, currentVisitor.PreviousSeed.Position);
                    entrance.SetLeadsTo(exit);
                    exit.SetLeadsTo(entrance);

                    currentSeed.AddEntrance(entrance);
                    currentVisitor.PreviousSeed.AddExit(exit);
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
                    RoomOpening entrance = new RoomOpening(DirectionsUtil.GetOppositeDirection(currentVisitor.Direction), currentVisitor.SectionNumber, currentSeed.Position);
                    RoomOpening exit = new RoomOpening(currentVisitor.Direction, currentVisitor.SectionNumber, bossRoomCenter);
                    entrance.SetLeadsTo(exit);
                    exit.SetLeadsTo(entrance);

                    currentSeed.AddExit(exit);
                    bossRoomSeed.AddEntrance(entrance);
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

                if(UnityEngine.Random.Range(0, 100) < chanceToExpandRoom)
                {
                    width = UnityEngine.Random.Range(roomMinExpandedWidth, roomMaxExpandedWidth);
                    height = UnityEngine.Random.Range(roomMinExpandedHeight, roomMaxExpandedHeight);
                }
                
                RoomSeedComposite composite = expander.ExpandRoom(roomSeeds, width, height, position);
                roomSeedComposites.Add(composite);
            }
        }

        roomSeedComposites.Add(bossRoomComposite);

        //---------------- Now we need to generate the actual rooms. ----------------
        bool[,] generatedRoomsRepresentation = new bool[1000, 1000];
        Stack<SeedGenWrapper> roomSeedStack = new Stack<SeedGenWrapper>();
        List<SeedGenWrapper> debugSeedStack = new List<SeedGenWrapper>();
        List<RoomData> generatedRooms = new List<RoomData>();

        roomSeedStack.Push(new SeedGenWrapper(startRoomComposite, null, null));
        while (roomSeedStack.Count > 0)
        {
            SeedGenWrapper currentSGW = roomSeedStack.Pop();
            debugSeedStack.Add(currentSGW);

            RoomSeedComposite previousSeed = currentSGW.exitLeadingToThisSeed == null ? null : currentSGW.exitLeadingToThisSeed.BelongsTo.PartOf;

            bool overlaps = false;

            //Is the current room seed a base room or a connector room?
            List<SeedGenWrapper> nextSeeds = new List<SeedGenWrapper>();
            List<RoomOpening> connectorExits = new List<RoomOpening>();
            foreach (RoomOpening exit in currentSGW.thisSeed.Exits)
            {
                if (exit.LeadsTo != null)
                {
                    if (exit.LeadsTo.BelongsTo.PartOf.HasGeneratedRoomData)
                    {
                        connectorExits.Add(exit);
                    }
                    else
                    {
                        nextSeeds.Add(new SeedGenWrapper(exit.LeadsTo.BelongsTo.PartOf, exit, currentSGW));
                    }
                }
            }

            currentSGW.thisSeed.SetIsBaseRoom(connectorExits.Count == 0);

            if (currentSGW.thisSeed.IsBaseRoom)
            {
                //Debug.Log("Base room");

                //Now we need to find the base room templates that are compatible with the current room seed.
                List<RoomData> compatibleRooms = new List<RoomData>();
                foreach (RoomData baseRoom in baseRooms)
                {
                    if
                    (
                        currentSGW.thisSeed.GetOpeningsInDirection(Directions.Top).Count == baseRoom.TopOpenings.Count &&
                        currentSGW.thisSeed.GetOpeningsInDirection(Directions.Bottom).Count == baseRoom.BottomOpenings.Count &&
                        currentSGW.thisSeed.GetOpeningsInDirection(Directions.Left).Count == baseRoom.LeftOpenings.Count &&
                        currentSGW.thisSeed.GetOpeningsInDirection(Directions.Right).Count == baseRoom.RightOpenings.Count
                    )
                    {
                        compatibleRooms.Add(baseRoom);
                    }
                }

                if(compatibleRooms.Count == 0)
                {
                    Debug.LogError("No compatible rooms found for seed: ");
                    Debug.LogError(currentSGW.thisSeed);
                    break;
                }

                //Shuffle the compatible rooms list
                for (int i = 0; i < compatibleRooms.Count; i++)
                {
                    RoomData temp = compatibleRooms[i];
                    int randomIndex = UnityEngine.Random.Range(i, compatibleRooms.Count);
                    compatibleRooms[i] = compatibleRooms[randomIndex];
                    compatibleRooms[randomIndex] = temp;
                }

                //Now we see if any of the potentially compatible rooms will actually fit into the map.
                foreach (RoomData potentialRoomBase in compatibleRooms)
                {
                    RoomData potentialRoomClone = new RoomData(potentialRoomBase);

                    //What opening, if any, does this room need to connect to the previous room?
                    RoomOpening exitToThis = currentSGW.exitLeadingToThisSeed;
                    RoomOpening entranceToPrevious = exitToThis == null ? null : exitToThis.LeadsTo;

                    Vector2Int exitProperPosition = new Vector2Int(-1,-1);
                    Vector2Int entranceProperPosition = new Vector2Int(-1,-1);

                    if (entranceToPrevious == null)
                    {
                        //We must be looking at the start room, which we place by using the starting point from a while ago,
                        //taking into account that the room representation is much larger than the path representation.
                        Vector2Int roomPosition = new Vector2Int(startingPoint.x * 20, startingPoint.y * 20);//TODO: Remove magic numbers, not just from here.
                        
                        potentialRoomClone.ModifyAllPositions(roomPosition);
                    }
                    else
                    {
                        //We're looking at a non-start room here, so we need to position it so that the proper exit of the previous room
                        //lines up with the proper entrance of this room.

                        //Which exit of the previous RoomData are we going to use?
                        exitProperPosition = currentSGW.previousSGW.alignment.GetCorrespondingDataOpening(currentSGW.exitLeadingToThisSeed);

                        //Which entrance of the current RoomData are we going to use?
                        RoomSeedRoomDataAligner alignment = new RoomSeedRoomDataAligner(currentSGW.thisSeed, potentialRoomClone);
                        entranceProperPosition = alignment.GetCorrespondingDataOpening(entranceToPrevious);

                        //Position the potential room so that the proper exit of the previous room lines up with the proper entrance of this room.
                        Vector2Int roomPosition = exitProperPosition - entranceProperPosition;
                        roomPosition += DirectionsUtil.GetDirectionVector(exitToThis.TravelDirection);//Add an offset to keep rooms from overlapping at an edge.
                        potentialRoomClone.ModifyAllPositions(roomPosition);
                        entranceProperPosition += roomPosition;
                    }

                    //We've placed the room, now we need to check if it overlaps with any other rooms.

                    overlaps = false;
                    Vector2Int entranceToIgnore = entranceProperPosition;
                    Vector2Int entranceToIgnoreDirectionIndicator = entranceToPrevious == null ? new Vector2Int(-1, -1) : DirectionsUtil.GetDirectionVector(entranceToPrevious.TravelDirection) + entranceProperPosition;

                    //Debug.Log("Entrance to ignore: " + entranceToIgnore);
                    //Debug.Log("Entrance to ignore direction indicator: " + entranceToIgnoreDirectionIndicator);

                    //Vector2Int exitToIgnore = exitToThis == null ? new Vector2Int(-1, -1) : exitToThis.Position;
                    //Vector2Int exitToIgnoreDirectionIndicator = exitToThis == null ? new Vector2Int(-1, -1) : DirectionsUtil.GetDirectionVector(exitToThis.TravelDirection) + exitToThis.Position;

                    if (detectRoomCollisions)
                    { 
                        List<Vector2Int> positionsToResetToTrue = new List<Vector2Int>();
                        if(entranceProperPosition.x != -1)
                        {
                            if (generatedRoomsRepresentation[entranceToIgnore.y, entranceToIgnore.x])
                            {
                                positionsToResetToTrue.Add(entranceToIgnore);
                                generatedRoomsRepresentation[entranceToIgnore.y, entranceToIgnore.x] = false;
                            }
                            if (generatedRoomsRepresentation[entranceToIgnoreDirectionIndicator.y, entranceToIgnoreDirectionIndicator.x])
                            {
                                positionsToResetToTrue.Add(entranceToIgnoreDirectionIndicator);
                                generatedRoomsRepresentation[entranceToIgnoreDirectionIndicator.y, entranceToIgnoreDirectionIndicator.x] = false;
                            }
                        }

                        foreach (Pixel pixel in potentialRoomClone.Pixels)
                        {
                            if (generatedRoomsRepresentation[pixel.Y, pixel.X])
                            {
                                overlaps = true;
                                break;
                            }
                        }

                        foreach (Vector2Int position in positionsToResetToTrue)
                        {
                            generatedRoomsRepresentation[position.y, position.x] = true;
                        }
                    }


                    if (!overlaps)
                    {
                        foreach (Pixel pixel in potentialRoomClone.Pixels)
                        {
                            generatedRoomsRepresentation[pixel.Y, pixel.X] = true;
                        }
                        currentSGW.thisSeed.RoomData = potentialRoomClone;
                        currentSGW.SetAssociatedRoom(potentialRoomClone);

                        potentialRoomClone.sgwUsedToMakeThis = currentSGW;
                        generatedRooms.Add(potentialRoomClone);

                        //We found and placed a valid room, so we're done looking at potential rooms.
                        break;
                    }
                }
            }
            else
            {
                //We need to generate our own room data here rather than use a pre-defined one.
                RoomData connectorRoom = new RoomData(10, 10);//TODO: Magic Numbers
                List<Vector2Int> connectingPaths = new List<Vector2Int>();

                foreach (RoomOpening connectorExit in connectorExits)
                {
                    //We need the exit to use for the current room and the entrance to match with of the next room
                    Vector2Int connectorsEntranceWorldPosition = currentSGW.previousSGW.alignment.GetCorrespondingDataOpening(currentSGW.exitLeadingToThisSeed);
                    connectorsEntranceWorldPosition += DirectionsUtil.GetDirectionVector(currentSGW.exitLeadingToThisSeed.TravelDirection);

                    RoomSeedRoomDataAligner connectedRoomOpeningAlignment = connectorExit.LeadsTo.BelongsTo.PartOf.RoomData.sgwUsedToMakeThis.alignment;
                    Vector2Int connectorsExitWorldPosition = connectedRoomOpeningAlignment.GetCorrespondingDataOpening(connectorExit.LeadsTo);
                    Directions connectedRoomEntranceDirection = connectedRoomOpeningAlignment.GetCorrespondingRoomOpening(connectorsExitWorldPosition).TravelDirection; // This gives us the opening direction of the composite room that we're connecting to, rather than any sub room.
                    connectorsExitWorldPosition += DirectionsUtil.GetDirectionVector(connectedRoomEntranceDirection);

                    /* We need to find the best path to the next room. A path from the exit to the entrance that we found earlier. */
                    //First, we need to make sure that the start and end positions on the map representation are not marked as walls.
                    List<Vector2Int> positionsToResetToTrue = new List<Vector2Int>();
                    if (generatedRoomsRepresentation[connectorsEntranceWorldPosition.y, connectorsEntranceWorldPosition.x])
                    {
                        positionsToResetToTrue.Add(connectorsEntranceWorldPosition);
                        generatedRoomsRepresentation[connectorsEntranceWorldPosition.y, connectorsEntranceWorldPosition.x] = false;
                    }
                    if (generatedRoomsRepresentation[connectorsExitWorldPosition.y, connectorsExitWorldPosition.x])
                    {
                        positionsToResetToTrue.Add(connectorsExitWorldPosition);
                        generatedRoomsRepresentation[connectorsExitWorldPosition.y, connectorsExitWorldPosition.x] = false;
                    }

                    //Now we need to find the path.
                    List<Vector2Int> specificConnectingPath = FindConnectingPath(generatedRoomsRepresentation, connectorsEntranceWorldPosition, connectorsExitWorldPosition);
                    
                    foreach (Vector2Int position in positionsToResetToTrue)
                    {
                        generatedRoomsRepresentation[position.y, position.x] = true;
                    }

                    if (specificConnectingPath.Count == 0)
                    {
                        Debug.LogError("Failed to find a connecting path for a connector room.");
                        break;
                    }

                    connectingPaths.AddRange(specificConnectingPath);

                    /* Add the openings that we've been working on to the RoomData */
                    connectorRoom.AddOpening(connectorsEntranceWorldPosition, DirectionsUtil.GetOppositeDirection(currentSGW.exitLeadingToThisSeed.TravelDirection));
                    connectorRoom.AddOpening(connectorsExitWorldPosition, connectorExit.TravelDirection);
                }

                List<Vector2Int> walls = new List<Vector2Int>();
                List<Vector2Int> expandedRoom = ExpandRoom(generatedRoomsRepresentation, connectingPaths, averagePixelsPerSmallRoom, out walls);

                if (expandedRoom.Count == 0)
                {
                    Debug.LogError("Failed to expand a connector room.");
                    break;
                }

                foreach (Vector2Int position in expandedRoom)
                {
                    generatedRoomsRepresentation[position.y, position.x] = true;
                }
                foreach (Vector2Int position in walls)
                {
                    generatedRoomsRepresentation[position.y, position.x] = true;

                    connectorRoom.AddWall(position);
                }

                currentSGW.thisSeed.RoomData = connectorRoom;
                currentSGW.SetAssociatedRoom(connectorRoom);

                connectorRoom.sgwUsedToMakeThis = currentSGW;
                generatedRooms.Add(connectorRoom);

                break;//TODO: Implement connector rooms.
            }

            if (currentSGW.thisSeed.RoomData == null)
            {
                Debug.LogError("Failed to place a room: ");
                Debug.LogError(currentSGW.thisSeed);

                if (overlaps)
                {
                    Debug.LogError("Issue seems to be that there was no room that didn't overlap with another room");
                }
                break;
            }

            foreach (SeedGenWrapper nextSeed in nextSeeds)
            {
                roomSeedStack.Push(nextSeed);
            }
        }

        //Generate the walls
        int roomCounter = 0;
        foreach (RoomData room in generatedRooms)
        {
            GameObject roomParent = new GameObject("Room " + roomCounter);
            roomParent.AddComponent<RoomDebugData>();
            //roomParent.GetComponent<RoomDebugData>().roomData = room;
            //roomParent.GetComponent<RoomDebugData>().sgw = room.sgwUsedToMakeThis;
            roomParent.GetComponent<RoomDebugData>().roomSeedComposite = room.sgwUsedToMakeThis.thisSeed;
            
            //foreach(RoomOpening opening in room.sgwUsedToMakeThis.thisSeed.Exits)
            //{
            //    if (opening.LeadsTo != null)
            //    {
            //        roomParent.GetComponent<RoomDebugData>().exits.Add(opening);
            //    }
            //}

            //foreach (RoomOpening opening in room.sgwUsedToMakeThis.thisSeed.Entrances)
            //{
            //    if (opening.LeadsTo != null)
            //    {
            //        roomParent.GetComponent<RoomDebugData>().entrances.Add(opening);
            //    }
            //}

            roomParent.transform.position = new Vector3(room.WallPixels[0].X, 0, room.WallPixels[0].Y);
            //Instantiate(roomParent);

            foreach (Pixel pixel in room.WallPixels)
            {
                Instantiate(wallPrefab, new Vector3(pixel.X, 0, pixel.Y), Quaternion.identity, roomParent.transform);
            }

            roomCounter++;
        }

        //Debug.Log("--------------- Composite Seeds: -------------------");
        //int counter = 0;
        //foreach (SeedGenWrapper sgw in debugSeedStack)
        //{
        //    Debug.Log(counter + ": " + sgw.thisSeed);
        //    counter++;
        //}
        //Debug.Log("-------------------- End Composite Seeds ---------------------");
    }

    private List<Vector2Int> ExpandRoom(bool[,] map, List<Vector2Int> startingRooms, int tryForQuantityOfRooms, out List<Vector2Int> walls)
    {
        List<Vector2Int> expandedRooms = new List<Vector2Int>();
        Queue<Vector2Int> roomQueue = new Queue<Vector2Int>(startingRooms);
        while (roomQueue.Count > 0 && expandedRooms.Count < tryForQuantityOfRooms)
        {
            Vector2Int currentRoom = roomQueue.Dequeue();
            expandedRooms.Add(currentRoom);
            List<Vector2Int> neighbors = new List<Vector2Int>();
            neighbors.Add(new Vector2Int(currentRoom.x + 1, currentRoom.y));
            neighbors.Add(new Vector2Int(currentRoom.x - 1, currentRoom.y));
            neighbors.Add(new Vector2Int(currentRoom.x, currentRoom.y + 1));
            neighbors.Add(new Vector2Int(currentRoom.x, currentRoom.y - 1));
            foreach (Vector2Int neighbor in neighbors)
            {
                if (neighbor.x < 0 || neighbor.x >= map.GetLength(0) || neighbor.y < 0 || neighbor.y >= map.GetLength(1))
                {
                    continue;
                }
                if (map[(int)neighbor.y, (int)neighbor.x])
                {
                    continue;
                }
                if (expandedRooms.Contains(neighbor) || roomQueue.Contains(neighbor))
                {
                    continue;
                }
                roomQueue.Enqueue(neighbor);
            }
        }

        walls = roomQueue.ToList<Vector2Int>();
        return expandedRooms;
    }

    class AStarNode 
    {
        private float g;
        private float h;
        private float f;

        private Vector2Int position;

        private AStarNode parent;

        public AStarNode(Vector2Int position, Vector2Int goal, AStarNode parent)
        {
            this.position = position;
            this.parent = parent;
            g = parent == null ? 0 : parent.g + 1;
            h = CalcManhattanDistance(position, goal);
            f = g + h;
        }

        public float F { get { return f; } }
        public Vector2Int Position { get { return position; } }
        public AStarNode Parent { get { return parent; } }

        private float CalcManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }

    private List<Vector2Int> FindConnectingPath(bool[,] map, Vector2Int startingPoint, Vector2Int endPoint)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        List<AStarNode> open = new List<AStarNode>();
        List<AStarNode> closed = new List<AStarNode>();

        AStarNode start = new AStarNode(startingPoint, endPoint, null);
        open.Add(start);

        int attemptCounter = 0;

        while (open.Count > 0 && attemptCounter < connectionAttemptsMax)
        {
            attemptCounter++;

            AStarNode current = open[0];
            open.RemoveAt(0);
            closed.Add(current);
            if (current.Position == endPoint)
            {
                AStarNode temp = current;
                while (temp != null)
                {
                    path.Add(temp.Position);
                    temp = temp.Parent;
                }
                path.Reverse();
                break;
            }
            List<Vector2Int> neighbors = new List<Vector2Int>();
            neighbors.Add(new Vector2Int(current.Position.x + 1, current.Position.y));
            neighbors.Add(new Vector2Int(current.Position.x - 1, current.Position.y));
            neighbors.Add(new Vector2Int(current.Position.x, current.Position.y + 1));
            neighbors.Add(new Vector2Int(current.Position.x, current.Position.y - 1));
            foreach (Vector2Int neighbor in neighbors)
            {
                if (neighbor.x < 0 || neighbor.x >= map.GetLength(0) || neighbor.y < 0 || neighbor.y >= map.GetLength(1))
                {
                    continue;
                }
                if (map[neighbor.y, neighbor.x])
                {
                    continue;
                }
                AStarNode neighborNode = new AStarNode(neighbor, endPoint, current);
                if (closed.Contains(neighborNode))
                {
                    continue;
                }
                if (open.Contains(neighborNode))
                {
                    int index = open.IndexOf(neighborNode);
                    if (neighborNode.F < open[index].F)
                    {
                        open[index] = neighborNode;
                    }
                }
                else
                {
                    open.Add(neighborNode);
                }
            }
            open.Sort((a, b) => a.F.CompareTo(b.F));
        }

        return path;
    }
}

public class ExitEntranceDuo
{
    public RoomOpening exit;
    public RoomOpening entrance;
    public ExitEntranceDuo(RoomOpening exit, RoomOpening entrance)
    {
        this.exit = exit;
        this.entrance = entrance;
    }
}

[Serializable]
public class CorrespondingOpenings
{
    [SerializeField] private RoomOpening seedOpening;
    [SerializeField] private Vector2Int dataOpening;

    public CorrespondingOpenings(RoomOpening seedOpening, Vector2Int dataOpening)
    {
        this.seedOpening = seedOpening;
        this.dataOpening = dataOpening;
    }

    public RoomOpening SeedOpening { get { return seedOpening; } }

    public Vector2Int DataOpening { get { return dataOpening; } }
}


[Serializable]
public class RoomSeedRoomDataAligner
{
    [SerializeField] private RoomSeed seed;
    [SerializeField] private RoomData roomData;

    [SerializeField] private List<CorrespondingOpenings> correspondingOpenings = new List<CorrespondingOpenings>();

    public RoomSeedRoomDataAligner(RoomSeed seed, RoomData roomData)
    {
        this.seed = seed;
        this.roomData = roomData;

        //Generate corresponding top openings
        List<Vector2Int> topDataOpenings = new List<Vector2Int>(roomData.TopOpenings);
        topDataOpenings.Sort((a, b) => a.x.CompareTo(b.x));//Direction of sort doesn't really matter as long as it's consistent.

        List<RoomOpening> topSeedOpenings = new List<RoomOpening>(seed.GetOpeningsInDirection(Directions.Top));
        topSeedOpenings.Sort((a, b) => a.Position.x.CompareTo(b.Position.x));

        for (int i = 0; i < topDataOpenings.Count; i++)
        {
            correspondingOpenings.Add(new CorrespondingOpenings(topSeedOpenings[i], topDataOpenings[i]));
        }

        //Generate corresponding bottom openings
        List<Vector2Int> bottomDataOpenings = new List<Vector2Int>(roomData.BottomOpenings);
        bottomDataOpenings.Sort((a, b) => a.x.CompareTo(b.x));

        List<RoomOpening> bottomSeedOpenings = new List<RoomOpening>(seed.GetOpeningsInDirection(Directions.Bottom));
        bottomSeedOpenings.Sort((a, b) => a.Position.x.CompareTo(b.Position.x));

        for (int i = 0; i < bottomDataOpenings.Count; i++)
        {
            correspondingOpenings.Add(new CorrespondingOpenings(bottomSeedOpenings[i], bottomDataOpenings[i]));
        }

        //Generate corresponding left openings
        List<Vector2Int> leftDataOpenings = new List<Vector2Int>(roomData.LeftOpenings);
        leftDataOpenings.Sort((a, b) => a.y.CompareTo(b.y));

        List<RoomOpening> leftSeedOpenings = new List<RoomOpening>(seed.GetOpeningsInDirection(Directions.Left));
        leftSeedOpenings.Sort((a, b) => a.Position.y.CompareTo(b.Position.y));

        for (int i = 0; i < leftDataOpenings.Count; i++)
        {
            correspondingOpenings.Add(new CorrespondingOpenings(leftSeedOpenings[i], leftDataOpenings[i]));
        }

        //Generate corresponding right openings
        List<Vector2Int> rightDataOpenings = new List<Vector2Int>(roomData.RightOpenings);
        rightDataOpenings.Sort((a, b) => a.y.CompareTo(b.y));

        List<RoomOpening> rightSeedOpenings = new List<RoomOpening>(seed.GetOpeningsInDirection(Directions.Right));
        rightSeedOpenings.Sort((a, b) => a.Position.y.CompareTo(b.Position.y));

        for (int i = 0; i < rightDataOpenings.Count; i++)
        {
            correspondingOpenings.Add(new CorrespondingOpenings(rightSeedOpenings[i], rightDataOpenings[i]));
        }
    }

    public Vector2Int GetCorrespondingDataOpening(RoomOpening seedOpening)
    {
        foreach (CorrespondingOpenings co in correspondingOpenings)
        {
            if (co.SeedOpening == seedOpening)
            {
                return co.DataOpening;
            }
        }
        return new Vector2Int(-1, -1);
    }

    public RoomOpening GetCorrespondingRoomOpening(Vector2Int dataOpening)
    {
        foreach (CorrespondingOpenings co in correspondingOpenings)
        {
            if (co.DataOpening == dataOpening)
            {
                return co.SeedOpening;
            }
        }
        return null;
    }
}

[Serializable]
public class SeedGenWrapper
{
    public RoomSeedComposite thisSeed;
    public RoomData associatedRoom;
    public RoomOpening exitLeadingToThisSeed;
    public RoomSeedRoomDataAligner alignment;

    public SeedGenWrapper previousSGW;

    public SeedGenWrapper(RoomSeedComposite thisSeed, RoomOpening exitLeadingToThisSeed, SeedGenWrapper previousSGW)
    {
        this.thisSeed = thisSeed;
        this.exitLeadingToThisSeed = exitLeadingToThisSeed;
        this.previousSGW = previousSGW;
    }

    public void SetAssociatedRoom(RoomData associatedRoom)
    {
        this.associatedRoom = associatedRoom;
        this.alignment = new RoomSeedRoomDataAligner(thisSeed, associatedRoom);
    }
}