using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using static MapGeneratorRevised;


public partial class MapGeneratorRevised : MonoBehaviour
{
    public bool generateMap = false; 
    public int usePathsFromLevel = 0;
    public int useRoomsFromLevel = 0;

    [Range(1, 10)]
    public int roomWidthMax = 4;
    [Range(1, 10)]
    public int roomHeightMax = 4;

    public int roomGridExtra = 10;

    public int minRoomSize = 5;
    private int minRoomMinus1;

    public int minGoalForKeys = 99;
    public int maxGoalForKeys = 99;

    public GameObject wallPrefab;
    public GameObject pathPrefab;
    public GameObject lockPrefab;
    public GameObject keyPrefab;
    public GameObject goalPrefab;

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
            maxGoalForKeys = Math.Min(maxGoalForKeys, KeyColorsUtil.GetNonBossColors().Count);

            minRoomMinus1 = minRoomSize - 1;
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

        /* Randomly pick the path */
        Texture2D path = paths[UnityEngine.Random.Range(0, paths.Length)];
        Debug.Log(path.name);

        /* Follow along the path and record each position */
        Vector2Int startCenter;
        List<Vector2Int> visited = new List<Vector2Int>();
        Queue<CriticalPathVisitor> branchStarters = GetCriticalPathVisitors(path, out startCenter, visited);
        List<CriticalPathVisitor> initialVisitors = new List<CriticalPathVisitor>(branchStarters);
        Vector2Int offsetSoStartIs00 = new Vector2Int(-startCenter.x, -startCenter.y);

        List<Vector2Int> criticalPathPositions = new List<Vector2Int>();
        List<Vector2Int> nonBossBranches = new List<Vector2Int>();
        List<Vector2Int> bossBranches = new List<Vector2Int>();

        List<Vector2Int> branchEndPositions = new List<Vector2Int>();

        Vector2Int lowest = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int highest = new Vector2Int(int.MinValue, int.MinValue);

        if (branchStarters == null)
        {
            Debug.LogError("Critical path visitor creation failed");
            return;
        }

        int preventInfiniteLoop = 0;
        CriticalPathVisitor currentBranchStarter = branchStarters.Dequeue();
        while (currentBranchStarter != null && preventInfiniteLoop < 9999)
        {
            preventInfiniteLoop++;
            MoveReport moveReport = null;
            Vector2Int positionAfterOffset;

            Vector2Int branchStartToCenterOffset = Vector2Int.zero;

            if (initialVisitors.Contains(currentBranchStarter))
            {
                branchStartToCenterOffset = new Vector2Int(startCenter.x - currentBranchStarter.Position.x, startCenter.y - currentBranchStarter.Position.y);
                currentBranchStarter.SetPositionsOffsetBy(offsetSoStartIs00 + branchStartToCenterOffset);
            }

            List<Vector2Int> currentBranch = new List<Vector2Int>();

            do
            {
                VisitReport visitReport = currentBranchStarter.Visit();
                if (!visitReport.WasSuccessful)
                {
                    Debug.LogError("Branch starter visit failed");
                    return;
                }

                positionAfterOffset = currentBranchStarter.Position + currentBranchStarter.PositionsOffsetBy;

                currentBranch.Add(positionAfterOffset);
                
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

                moveReport = currentBranchStarter.MoveOn();
            } while (moveReport == null || !moveReport.VisitorIsDone);
            
            Debug.LogWarning("Visited COunt in branch: " + visited.Count);

            branchEndPositions.Add(positionAfterOffset);

            if (!currentBranchStarter.ShouldDiscardBranch)
            {
                if (currentBranchStarter.ReachedBossRoom)
                {
                    bossBranches.AddRange(currentBranch);
                }
                else
                {
                    nonBossBranches.AddRange(currentBranch);
                }
            }
            else
            {
                Debug.Log("Branch discarded");
            }

            Debug.LogWarning(currentBranchStarter.NewBranchStarters.Count);
            foreach (Vector2Int position in bossBranches)
            {
                currentBranchStarter.RemovePotentialVisitorsGivenLocation(position - currentBranchStarter.PositionsOffsetBy);
            }
            foreach (Vector2Int position in nonBossBranches)
            {
                currentBranchStarter.RemovePotentialVisitorsGivenLocation(position - currentBranchStarter.PositionsOffsetBy);
            }
            foreach (CriticalPathVisitor newBranchStarter in currentBranchStarter.NewBranchStarters)
            {
                branchStarters.Enqueue(newBranchStarter);
            }

            currentBranchStarter = branchStarters.Count > 0 ? branchStarters.Dequeue() : null;
        }

        if(preventInfiniteLoop >= 9999)
        {
            Debug.LogError("Prevent infinite loop reached");
            return;
        }

        criticalPathPositions.AddRange(nonBossBranches);
        criticalPathPositions.AddRange(bossBranches);

        Debug.Log("BranchEndPositions.Count: " + branchEndPositions.Count); 
        Debug.Log("Visited.COunt: " + visited.Count);

        /* If the lowest x value is negative, shift all x values to the right by that amount. Do the same for y values. */
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
        for(int i = 0; i < branchEndPositions.Count; i++)
        {
            branchEndPositions[i] += offsetForNegatives;
        }

        highest += offsetForNegatives;
        lowest += offsetForNegatives;

        /* Multiply each position from step 2 by 2 and then add 1,1 to each position. Doing this offsets the critical path and ensures that
         * its edges do not align with the room edges that will be generated later. The rooms do need to have odd dimensions for this to work. */
        for (int i = 0; i < criticalPathPositions.Count; i++)
        {
            criticalPathPositions[i] = criticalPathPositions[i] * 2 + Vector2Int.one;
        }
        for (int i = 0; i < branchEndPositions.Count; i++)
        {
            branchEndPositions[i] = branchEndPositions[i] * 2 + Vector2Int.one;
        }

        //Also fill in the empty spaces now between critical path locations.
        Debug.Log("Critical path length: " + criticalPathPositions.Count);
        List<Vector2Int> newPlusSortedPositions = new List<Vector2Int>();
        for (int i = 0, j = 1; i < criticalPathPositions.Count - 1; i++, j++)
        {
            Vector2Int currentPosition = criticalPathPositions[i];
            Vector2Int nextPosition = j >= criticalPathPositions.Count ? new Vector2Int(int.MinValue, int.MinValue) : criticalPathPositions[j];

            newPlusSortedPositions.Add(currentPosition);

            if (nextPosition.x == int.MinValue)
            {
                break;
            }

            if (branchEndPositions.Contains(currentPosition))
            {
                continue;
            }

            Vector2Int difference = nextPosition - currentPosition;
            int xNormalizer = difference.x == 0 ? 1 : Mathf.Abs(difference.x); //Set to 1 in the case of 0 to avoid divide by 0 errors.
            int yNormalizer = difference.y == 0 ? 1 : Mathf.Abs(difference.y);
            Vector2Int direction = new Vector2Int(difference.x / xNormalizer, difference.y / yNormalizer); //The path only ever moves in cardinal directions, so one of x or y is 0 and we can normalize this easily. One or the other has to equal 1 or -1.

            for(Vector2Int unmarkedPosition = currentPosition + direction; unmarkedPosition != nextPosition; unmarkedPosition += direction)
            {
                newPlusSortedPositions.Add(unmarkedPosition);
            }
        }
        criticalPathPositions = newPlusSortedPositions;
        RemoveDuplicatePositions(criticalPathPositions);
        Debug.Log("criticalPathPositions.Count: " + criticalPathPositions.Count);


        /* Create a representation of the map */
        int mapXDimension = highest.x * 2 + roomGridExtra;//The extra space is to ensure that the path is fully contained.
        int mapYDimension = highest.y * 2 + roomGridExtra;
        while ((mapXDimension - minRoomSize) % minRoomMinus1 != 0)
        {
            //(mapXDimension - minRoomSize) % minRoomMinus1: Rooms share walls, so that needs to be taken into account when determining the map dimensions.
            mapXDimension++;
        }

        while ((mapYDimension - minRoomSize) % minRoomMinus1 != 0)
        {
            mapYDimension++;
        }

        Debug.Log("Map dimensions: " + mapXDimension + "x" + mapYDimension);

        bool[,] map = new bool[mapXDimension, mapYDimension];

        /* Break that 2D list into rectangles of random sizes. */
        List<Room> rooms = GenerateRooms(map);

        /* Set every bool along the rectangle edges that intersect with the main path to true. These are our walls. */
        foreach (Room room in rooms)
        {
            List<Vector2Int> perimeter = room.GetPerimeterPositions();

            bool atLeastOneIntersection = false;
            foreach(Vector2Int position in perimeter)
            {
                if (criticalPathPositions.Contains(position))
                {
                    atLeastOneIntersection = true;
                    break;
                }
            }

            if (!atLeastOneIntersection)
            {
                continue;
            }

            foreach (Vector2Int position in perimeter)
            {
                map[position.x, position.y] = true;
            }
        }

        //Also match critical path locations to which rooms they are in.
        List<Room> correspondingRooms = new List<Room>();
        foreach(Vector2Int position in criticalPathPositions)
        {
            correspondingRooms.Add(null);
        }
        foreach(Room room in rooms)
        {
            List<Vector2Int> encompassed = room.GetEncompasedPositions();
            foreach (Vector2Int position in encompassed)
            {
                int index = criticalPathPositions.IndexOf(position);
                if (index != -1)
                {
                    correspondingRooms[index] = room;
                }
            }
        }

        /* Set every bool along the critical path to false. These are the openings. */
        List<Vector2Int> openings = new List<Vector2Int>();
        foreach (Vector2Int position in criticalPathPositions)
        {
            if(map[position.x, position.y])
            {
                openings.Add(new Vector2Int(position.x, position.y));
            }

            map[position.x, position.y] = false;
        }

        /* Merge openings that lead from 1 room to 1 other room. */
        //TODO: Implement this step

        /* Generate a wall in the world for each true value in the 2D list. */
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
            Instantiate(pathPrefab, new Vector3(position.x, 0, position.y), Quaternion.identity, pathParent.transform);
        }

        /* Generate locks and keys */
        GenerateLocksAndKeys(criticalPathPositions, openings, correspondingRooms, rooms, branchEndPositions, mapParent);
    }

    public void RemoveDuplicatePositions(List<Vector2Int> positions)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            for (int j = i + 1; j < positions.Count; j++)
            {
                if (positions[i] == positions[j])
                {
                    positions.RemoveAt(j);
                    j--;
                }
            }
        }
    }   

    public void GenerateLocksAndKeys(List<Vector2Int> criticalPathPositions, List<Vector2Int> openings, List<Room> correspondingRooms, List<Room> allRooms, List<Vector2Int> branchEndPositions, GameObject mapParent)
    {
        Queue<Key> unplacedKeys = new Queue<Key>();
        unplacedKeys.Enqueue(new Key(KeyColorsUtil.GetBossColor()));
        
        Queue<Key> potentialKeys = new Queue<Key>();
        foreach (KeyColors color in KeyColorsUtil.GetNonBossColors())
        {
            potentialKeys.Enqueue(new Key(color));
        }

        List<Key> placedKeys = new List<Key>();

        int spacesBetweenLocks = criticalPathPositions.Count / KeyColorsUtil.GetNonBossColors().Count;

        List<Lock> locks = new List<Lock>();

        List<Vector2Int> newWallLocations = new List<Vector2Int>();

        int spacesSinceLastLock = 0;
        bool bossRoomOpeningNeedsWall = false;
        bool reachedFirstOpening = false;
        Lock toCloneForNextOpening = null;
        Room startRoom = correspondingRooms[0];
        for (int i = criticalPathPositions.Count - 1; i >= 0; i--)
        {
            Vector2Int position = criticalPathPositions[i];

            if (i == criticalPathPositions.Count - 1)
            {
                int bossRoomOpenings = correspondingRooms[i].GetNumberOfOpenings(openings);
                if (bossRoomOpenings != 1)
                {
                    bossRoomOpeningNeedsWall = true;
                }

                Room bossRoom = correspondingRooms[i];
                GameObject goal = Instantiate(goalPrefab, new Vector3(bossRoom.BottomLeft.x + bossRoom.Width / 2, 0, bossRoom.BottomLeft.y + bossRoom.Height / 2), Quaternion.identity, mapParent.transform);
            }

            if (openings.Contains(position))
            {
                int nextNonOpeningIndex = Mathf.Max(i - 1, 0);

                foreach (Key key in unplacedKeys)
                {
                    if (key.CanAddPotentialRooms())
                    {
                        key.AddPotentialRoom(correspondingRooms[nextNonOpeningIndex]);
                    }
                    if(key.CanAddPotentialRooms())
                    {
                        key.AddPotentialRoom(correspondingRooms[i]);
                    }
                }

                if (!reachedFirstOpening)
                {
                    reachedFirstOpening = true;

                    if (bossRoomOpeningNeedsWall)
                    {
                        newWallLocations.Add(position);
                    }
                    else
                    {
                        Lock bossLock = new Lock(KeyColorsUtil.GetColor(KeyColorsUtil.GetBossColor()), position);
                        locks.Add(bossLock);
                    }
                }
                else if (toCloneForNextOpening != null)
                {
                    Lock clone = toCloneForNextOpening.Clone(position);
                    locks.Add(clone);

                    toCloneForNextOpening = null;
                    //spacesSinceLastLock = 0;
                }
                else if(spacesSinceLastLock >= spacesBetweenLocks && potentialKeys.Count > 0)
                {
                    Key newKey = potentialKeys.Peek();

                    Lock newLock = new Lock(KeyColorsUtil.GetColor(newKey.Color), position);

                    Key toPlace = unplacedKeys.Count > 0 && unplacedKeys.Peek().PotentialRooms.Contains(correspondingRooms[nextNonOpeningIndex]) ? unplacedKeys.Dequeue() : null;
                    bool success = true;
                    if (toPlace != null)
                    {
                        toPlace.SetRoomParent(correspondingRooms[nextNonOpeningIndex]);
                        correspondingRooms[nextNonOpeningIndex].AddKey(toPlace);
                        placedKeys.Add(toPlace);
                    }
                    else /*if (correspondingRooms[nextNonOpeningIndex].GetNumberOfOpenings(openings) != 2)*/
                    {
                        success = false;
                    }

                    if(success)
                    {
                        locks.Add(newLock);
                        toCloneForNextOpening = newLock;
                        spacesSinceLastLock = 0;

                        newKey = potentialKeys.Dequeue();

                        if (correspondingRooms[nextNonOpeningIndex] == startRoom)
                        {
                            newKey.AddPotentialRoom(startRoom);
                        }
                        else
                        {
                            newKey.AddInvalidRoom(correspondingRooms[nextNonOpeningIndex]);
                        }
                        
                        unplacedKeys.Enqueue(newKey);
                    }
                }
            }
            else if (branchEndPositions.Contains(position))
            {
                toCloneForNextOpening = null;
            }

            spacesSinceLastLock++;
        }

        foreach(Key key in unplacedKeys)
        {
            if(key.PotentialRooms.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, key.PotentialRooms.Count);
                key.PotentialRooms[randomIndex].AddKey(key);
                key.SetRoomParent(key.PotentialRooms[randomIndex]);
                placedKeys.Add(key);
            }
        }
        unplacedKeys.Clear();

        GameObject keyParent = new GameObject("Key Parent");
        keyParent.transform.parent = transform;
        foreach (Room room in allRooms)
        {
            room.DetermineKeyPrerequisites(placedKeys);

            for(int i = 0; i < room.ContainedKeys.Count; i++)
            {
                Key key = room.ContainedKeys[i];

                GameObject keyGO = Instantiate(keyPrefab, new Vector3(room.BottomLeft.x + room.Width / 2, 0, room.BottomLeft.y + room.Height / 2), Quaternion.identity, keyParent.transform);
                keyGO.GetComponent<Renderer>().material.color = KeyColorsUtil.GetColor(key.Color);
                keyGO.GetComponent<KeyGameObject>().prerequisite = key.Prerequisite == null ? Color.clear : KeyColorsUtil.GetColor(key.Prerequisite.Color);
            }
        }

        foreach (Vector2Int wallPosition in newWallLocations)
        {
            Instantiate(wallPrefab, new Vector3(wallPosition.x, 0, wallPosition.y), Quaternion.identity, mapParent.transform);
        }

        GameObject lockParent = new GameObject("Lock Parent");
        lockParent.transform.parent = transform;
        foreach (Lock lockPosition in locks)
        {
            GameObject newLock = Instantiate(lockPrefab, new Vector3(lockPosition.Position.x, 0, lockPosition.Position.y), Quaternion.identity, lockParent.transform);
            newLock.GetComponent<Renderer>().material.color = lockPosition.UnlocksMe;
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
         * 1. Make a new RoomNode 2D array. It's sizes are condensed from the map input so that the rooms can later be scaled back up to the actual map sizes. just makes the math a little easier.
         *      1-2. Initialize every RoomNode to have no parent room.
         * 2. For each Vector2Int in the list, create a room of random size at that location on the map.
         *      3-2. Check if the room is valid; i.e. every position it encompasses does not already belong to a room.
         *      3-3. If the room is valid, add it to the list of rooms and mark every position it encompasses as belonging to the room.
         *      3-4. If the room is invalid, skrink it on one side by 1 and try again. Repeat until the room is valid or it is 1x1.
         *      3-5. If the RoomNode already has a room, skip it. this condition should end up handling the 1x1 rooms.
         * 3. For each room, scale the width, height, and position so that it fits back onto the map.
         * 4. Return the list of Rooms.
         */

        /* Step 1 */
        List<Room> rooms = new List<Room>();

        RoomNode[,] roomNodes = new RoomNode[(map.GetLength(0) - minRoomSize) / minRoomMinus1, (map.GetLength(1) - minRoomSize) / minRoomMinus1];
        for (int x = 0; x < roomNodes.GetLength(0); x++)
        {
            for (int y = 0; y < roomNodes.GetLength(1); y++)
            {
                roomNodes[x, y] = new RoomNode(null);
            }
        }

        /* Step 2 */
        int highestWidth = 1;
        for(int x = 0; x < roomNodes.GetLength(0); x += highestWidth)
        {
            highestWidth = 1;

            for (int y = 0; y < roomNodes.GetLength(1); y++)
            {
                Vector2Int position = new Vector2Int(x, y);

                if (roomNodes[position.x, position.y].BelongsTo != null)
                {
                    continue;
                }

                int width = UnityEngine.Random.Range(1, roomWidthMax + 1);
                int height = UnityEngine.Random.Range(1, roomHeightMax + 1);
                Room room = new Room(position, width, height);

                if (width > highestWidth)
                {
                    highestWidth = width;
                }

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
        }

        for(int x = 0; x < roomNodes.GetLength(0); x++)
        {
            for (int y = 0; y < roomNodes.GetLength(1); y++)
            {
                if (roomNodes[x, y].BelongsTo == null)
                {
                    Room room = new Room(new Vector2Int(x, y), 1, 1);
                    rooms.Add(room);
                }
            }
        }

        /* Step 3 */
        List<Room> adjustedRooms = new List<Room>();
        foreach (Room room in rooms)
        {
            int wAccountForWallSharing = room.Width - 1;
            int hAccountForWallSharing = room.Height - 1;
            adjustedRooms.Add(new Room(new Vector2Int(room.BottomLeft.x * minRoomMinus1, room.BottomLeft.y * minRoomMinus1), room.Width * minRoomSize - wAccountForWallSharing, room.Height * minRoomSize - hAccountForWallSharing));
        }

        /* Step 4 */
        return adjustedRooms;
    }

    private Queue<CriticalPathVisitor> GetCriticalPathVisitors(Texture2D path, out Vector2Int startCenter, List<Vector2Int> visited)
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
                    branchStarters.Enqueue(new CriticalPathVisitor(new Vector2Int(neighbor.X, neighbor.Y), branchSeedDirection, path, 0, visited));
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