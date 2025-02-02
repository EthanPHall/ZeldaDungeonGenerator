using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class MapGeneratorRevised : MonoBehaviour
{
    public bool generateMap = false; 
    public int usePathsFromLevel = 0;
    public int useRoomsFromLevel = 0;

    [Range(1, 10)]
    public int roomWidthMax = 4;
    [Range(1, 10)]
    public int roomHeightMax = 4;

    public int roomGridExtra = 10;

    public GameObject wallPrefab;

    private Texture2D[] paths;

    // Start is called before the first frame update
    void Start()
    {
        paths = Resources.LoadAll<Texture2D>("Paths/Level " + usePathsFromLevel + " Paths");
    }

    // Update is called once per frame
    void Update()
    {
        if (generateMap)
        {
            generateMap = false;
            GenerateMap();
        }
    }

    private void GenerateMap()
    {
        // Clear the map
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        /* Map Generation Algorithm
         * 1. Choose which critical path to use.
         * 2. Follow along the path and record each position
         *      2-1. Keep track of 4 values: lowest and highest x and y values
         *      2-2. The start point is considered 0,0 so positions may be positive or negative
         * 3. If the lowest x value is negative, shift all x values to the right by that amount. Do the same for y values.
         * 4. Multiply each position from step 2 by 2 and then add 1,1 to each position. Doing this offsets the critical path and ensures that
         *    its edges do not align with the room edges that will be generated later.
         * 5. Declare a 2D array/list of booleans to represent the map. The size of the array is the highest x and y values from step 3 multiplied by 2, plus at least 2.
         *      5-2. From there, we need to make sure that both dimensions of the map, -4, are divisible by 3. Rooms can and should share edges.
         * 6. Break that 2D list into rectangles of random sizes.
         * 7. Set every bool along the rectangle edges to true.
         * 8. Set every bool along the critical path to false. These are the openings.
         * 9. Merge openings that lead from 1 room to 1 other room.
         * 10. Generate a wall in the world for each true value in the 2D list.
         */

        /* Step 1: Randomly pick the path */
        Texture2D path = paths[UnityEngine.Random.Range(0, paths.Length)];

        /* Step 2: Follow along the path and record each position */
        Vector2Int startCenter;
        Queue<CriticalPathVisitor> branchStarters = GetCriticalPathVisitors(path, out startCenter);
        Vector2Int offsetSoStartIs00 = new Vector2Int(-startCenter.x, -startCenter.y);

        List<Vector2Int> criticalPathPositions = new List<Vector2Int>();
        Vector2Int lowest = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int highest = new Vector2Int(int.MinValue, int.MinValue);

        if (branchStarters == null)
        {
            Debug.LogError("Critical path visitor creation failed");
            return;
        }

        foreach(CriticalPathVisitor branchStarter in branchStarters)
        {
            MoveReport moveReport = null;
            do
            {
                VisitReport visitReport = branchStarter.Visit();
                if (!visitReport.WasSuccessful)
                {
                    Debug.LogError("Branch starter visit failed");
                    return;
                }

                Vector2Int positionAfterOffset = branchStarter.Position + offsetSoStartIs00;

                criticalPathPositions.Add(positionAfterOffset);
                
                if(positionAfterOffset.x < lowest.x)
                {
                    lowest.x = positionAfterOffset.x;
                }
                if (positionAfterOffset.y < lowest.y)
                {
                    lowest.y = positionAfterOffset.y;
                }
                if (positionAfterOffset.x > highest.x)
                {
                    highest.x = positionAfterOffset.x;
                }
                if (positionAfterOffset.y > highest.y)
                {
                    highest.y = positionAfterOffset.y;
                }

                moveReport = branchStarter.MoveOn();
            } while (moveReport == null || !moveReport.VisitorIsDone);
        }

        /* Step 3: If the lowest x value is negative, shift all x values to the right by that amount. Do the same for y values. */
        Vector2Int offsetForNegatives = new Vector2Int(0, 0);
        if(lowest.x < 0)
        {
            offsetForNegatives.x = -lowest.x;
        }
        if (lowest.y < 0)
        {
            offsetForNegatives.y = -lowest.y;
        }

        for(int i = 0; i < criticalPathPositions.Count; i++)
        {
            criticalPathPositions[i] += offsetForNegatives;
        }

        highest += offsetForNegatives;
        lowest += offsetForNegatives;

        /* Step 4: Multiply each position from step 2 by 2 and then add 1,1 to each position. Doing this offsets the critical path and ensures that
         * its edges do not align with the room edges that will be generated later. */
        for (int i = 0; i < criticalPathPositions.Count; i++)
        {
            criticalPathPositions[i] = criticalPathPositions[i] * 2 + Vector2Int.one;
        }

        //Also fill in the empty spaces now between critical path locations.
        List<Vector2Int> newPositions = new List<Vector2Int>();
        for (int i = 0, j = 1; i < criticalPathPositions.Count - 1; i++, j++)
        {
            Vector2Int currentPosition = criticalPathPositions[i];
            Vector2Int nextPosition = j >= criticalPathPositions.Count ? new Vector2Int(int.MinValue, int.MinValue) : criticalPathPositions[j];

            if(nextPosition.x == int.MinValue)
            {
                break;
            }

            Vector2Int difference = nextPosition - currentPosition;
            int xNormalizer = difference.x == 0 ? 1 : Mathf.Abs(difference.x); //Set to 1 in the case of 0 to avoid divide by 0 errors.
            int yNormalizer = difference.y == 0 ? 1 : Mathf.Abs(difference.y);
            Vector2Int direction = new Vector2Int(difference.x / xNormalizer, difference.y / yNormalizer); //The path only ever moves in cardinal directions, so one of x or y is 0 and we can normalize this easily. One or the other has to equal 1 or -1.

            for(Vector2Int unmarkedPosition = currentPosition + direction; unmarkedPosition != nextPosition; unmarkedPosition += direction)
            {
                newPositions.Add(unmarkedPosition);
            }
        }
        criticalPathPositions.AddRange(newPositions);

        /*5.Declare a 2D array / list of booleans to represent the map. The size of the array is the highest x and y values from step 3 multiplied by 2, plus at least 2.
         *  5 - 2.From there, we need to make sure that both dimensions of the map, -4, are divisible by 3.Rooms can and should share edges.*/

        int mapXDimension = highest.x * 2 + roomGridExtra;//The extra space is to ensure that the path is fully contained.
        int mapYDimension = highest.y * 2 + roomGridExtra;
        while ((mapXDimension - 5) % 4 != 0)
        {
            mapXDimension++;
        }

        while ((mapYDimension - 5) % 4 != 0)
        {
            mapYDimension++;
        }

        Debug.Log("Map dimensions: " + mapXDimension + "x" + mapYDimension);

        bool[,] map = new bool[mapXDimension, mapYDimension];

        /* Step 6: Break that 2D list into rectangles of random sizes. */
        List<Room> rooms = GenerateRooms(map);

        /* Step 7: Set every bool along the rectangle edges to true. */
        foreach (Room room in rooms)
        {
            foreach (Vector2Int position in room.GetPerimeterPositions())
            {
                Debug.Log(position.x + ", " + position.y);
                Debug.Log("Room: " + room.BottomLeft.x + ", " + room.BottomLeft.y + " : " + room.Width + "x" + room.Height);
                map[position.x, position.y] = true;
            }
        }

        /* Step 8: Set every bool along the critical path to false. These are the openings. */
        foreach (Vector2Int position in criticalPathPositions)
        {
            map[position.x, position.y] = false;
        }

        /* Step 9: Merge openings that lead from 1 room to 1 other room. */
        //TODO: Implement this step

        /* Step 10: Generate a wall in the world for each true value in the 2D list. */
        GameObject mapParent = new GameObject("Map Parent");
        mapParent.transform.parent = transform;
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y])
                {
                    Instantiate(wallPrefab, new Vector3(x, 0, y), Quaternion.identity, mapParent.transform);
                }
            }
        }

        GameObject pathParent = new GameObject("Path Parent");
        pathParent.transform.parent = transform;
        foreach (Vector2Int position in criticalPathPositions)
        {
            //Instantiate(wallPrefab, new Vector3(position.x, 0, position.y), Quaternion.identity, pathParent.transform);
        }
    }

    private class Room
    {
        private Vector2Int bottomLeft;
        private int width;
        private int height;

        public Room(Vector2Int bottomLeft, int width, int height)
        {
            this.bottomLeft = bottomLeft;
            this.width = width;
            this.height = height;
        }

        public Vector2Int BottomLeft
        {
            get { return bottomLeft; }
        }

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public List<Vector2Int> GetEncompasedPositions()
        {
            List<Vector2Int> positions = new List<Vector2Int>();

            for (int x = bottomLeft.x; x < bottomLeft.x + width; x++)
            {
                for (int y = bottomLeft.y; y < bottomLeft.y + height; y++)
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }

            return positions;
        }

        public List<Vector2Int> GetPerimeterPositions()
        {
            List<Vector2Int> positions = new List<Vector2Int>();

            for (int x = bottomLeft.x; x < bottomLeft.x + width; x++)
            {
                positions.Add(new Vector2Int(x, bottomLeft.y));
                positions.Add(new Vector2Int(x, bottomLeft.y + height - 1));
            }

            for (int y = bottomLeft.y; y < bottomLeft.y + height; y++)
            {
                positions.Add(new Vector2Int(bottomLeft.x, y));
                positions.Add(new Vector2Int(bottomLeft.x + width - 1, y));
            }

            return positions;
        }
    }

    private class RoomNode
    {
        private Room parent;

        public RoomNode(Room parent)
        {
            this.parent = parent;
        }

        public Room BelongsTo
        {
            get { return parent; }
            set { parent = value; }
        }
    }

    private List<Room> GenerateRooms(bool[,] map)
    {
        /* Algorithm
         * 1. Make a new RoomNode 2D array. It's sizes are (mapWidth - 4)/3 + 1 and (mapHeight - 4)/3 + 1.
         *      1-2. Initialize every RoomNode to have no parent room.
         *      1-3. As we're doing that, build up another list of Vector2Ints for each position on the RoomNode array.
         * 2. Shuffle the list of RoomNode positions.
         * 3. For each Vector2Int in the list, create a room of random size at that location on the map.
         *      3-2. Check if the room is valid; i.e. every position it encompasses does not already belong to a room.
         *      3-3. If the room is valid, add it to the list of rooms and mark every position it encompasses as belonging to the room.
         *      3-4. If the room is invalid, skrink it on one side by 1 and try again. Repeat until the room is valid or it is 1x1.
         *      3-5. If the RoomNode already has a room, skip it. this condition should end up handling the 1x1 rooms.
         * 4. For each room, multiply the width, height, and position by 4.
         * 5. Return the list of Rooms.
         */

        /* Step 1 */
        List<Room> rooms = new List<Room>();
        List<Vector2Int> potentialRoomPositions = new List<Vector2Int>();

        Debug.Log("(" + (map.GetLength(0) - 5) / 4 + ", " + (map.GetLength(1) - 5) / 4 + ")");

        RoomNode[,] roomNodes = new RoomNode[(map.GetLength(0) - 5) / 4, (map.GetLength(1) - 5) / 4];
        //RoomNode[,] roomNodes = new RoomNode[map.GetLength(0) / 4, map.GetLength(1) / 4];
        for (int x = 0; x < roomNodes.GetLength(0); x++)
        {
            for (int y = 0; y < roomNodes.GetLength(1); y++)
            {
                roomNodes[x, y] = new RoomNode(null);
                potentialRoomPositions.Add(new Vector2Int(x, y));
            }
        }

        /* Step 2 */
        for (int i = 0; i < potentialRoomPositions.Count; i++)
        {
            Vector2Int temp = potentialRoomPositions[i];
            int randomIndex = UnityEngine.Random.Range(i, potentialRoomPositions.Count);
            potentialRoomPositions[i] = potentialRoomPositions[randomIndex];
            potentialRoomPositions[randomIndex] = temp;
        }

        /* Step 3 */
        foreach (Vector2Int position in potentialRoomPositions)
        {
            if(roomNodes[position.x, position.y].BelongsTo != null)
            {
                continue;
            }

            int width = UnityEngine.Random.Range(2, roomWidthMax + 1);
            int height = UnityEngine.Random.Range(2, roomHeightMax + 1);
            width = 1;
            height = 1;
            Room room = new Room(position, width, height);

            bool validRoom = false;

            while (!validRoom)
            {
                validRoom = true;
                List<Vector2Int> encompassed = room.GetEncompasedPositions();

                foreach(Vector2Int encompassedPosition in encompassed)
                {
                    if (encompassedPosition.x >= roomNodes.GetLength(0) || encompassedPosition.y >= roomNodes.GetLength(1) || roomNodes[encompassedPosition.x, encompassedPosition.y].BelongsTo != null)
                    {
                        validRoom = false;
                        break;
                    }
                }

                if (!validRoom)
                {
                    if(room.Width == 1 && room.Height == 1)
                    {
                        Debug.LogError("Room is 1x1 and invalid");
                        break;
                    }

                    int shrinkDirection = UnityEngine.Random.Range(0, 2);

                    if (shrinkDirection == 0)
                    {
                        if (room.Width == 1)
                        {
                            room = new Room(room.BottomLeft, room.Width, room.Height - 1);
                        }
                        else
                        {
                            room = new Room(room.BottomLeft, room.Width - 1, room.Height);
                        }
                    }
                    else
                    {
                        if (room.Height == 1)
                        {
                            room = new Room(room.BottomLeft, room.Width - 1, room.Height);
                        }
                        else
                        {
                            room = new Room(room.BottomLeft, room.Width, room.Height - 1);
                        }
                    }
                }
            }

            rooms.Add(room);
            foreach (Vector2Int encompassedPosition in room.GetEncompasedPositions())
            {
                roomNodes[encompassedPosition.x, encompassedPosition.y].BelongsTo = room;
            }
        }

        /* Step 4 */
        List<Room> adjustedRooms = new List<Room>();
        foreach (Room room in rooms)
        {
            adjustedRooms.Add(new Room(new Vector2Int(room.BottomLeft.x * 4, room.BottomLeft.y * 4), room.Width * 5, room.Height * 5));
        }

        /* Step 5 */
        return adjustedRooms;
    }

    private Queue<CriticalPathVisitor> GetCriticalPathVisitors(Texture2D path, out Vector2Int startCenter)
    {
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
            startCenter = new Vector2Int(int.MinValue, int.MinValue);
            return null;
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
                startCenter = new Vector2Int(int.MinValue, int.MinValue);
                return null;
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
            startCenter = new Vector2Int(int.MinValue, int.MinValue);
            return null;
        }

        startCenter = new Vector2Int(startingPoint.x, startingPoint.y) + Vector2Int.one;//The starting room reprsentation of the path is a 3x3 white square. We have the bottom left corner.
        return branchStarters;
    }
}