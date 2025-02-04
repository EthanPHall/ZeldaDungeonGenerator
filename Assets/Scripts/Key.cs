using System.Collections.Generic;


public partial class MapGeneratorRevised
{
    public class Key
    {
        private KeyColors color;
        private Room roomParent;
        private Key prerequisite;

        private List<Room> potentialRooms = new List<Room>();
        private List<Room> invalidRooms = new List<Room>();

        private int maxRoomsDownThePath = 99;

        public Key(KeyColors color, Room roomParent = null, Key prerequisite = null)
        {
            this.color = color;
            this.roomParent = roomParent;
            this.prerequisite = prerequisite;
        }

        public KeyColors Color
        {
            get { return color; }
        }

        public Room RoomParent
        {
            get { return roomParent; }
        }

        public void SetRoomParent(Room roomParent)
        {
            this.roomParent = roomParent;
            potentialRooms.Clear();
        }

        public Key Prerequisite
        {
            get { return prerequisite; }
        }

        public void SetPrerequisite(Key prerequisite)
        {
            this.prerequisite = prerequisite;
        }

        public List<Room> PotentialRooms
        {
            get { return potentialRooms; }
        }

        public List<Room> InvalidRooms
        {
            get { return invalidRooms; }
        }

        public void AddPotentialRoom(Room room)
        {
            if(potentialRooms.Count >= maxRoomsDownThePath)
            {
                //Debug.LogWarning("Max rooms down the path reached for key " + color);
                return;
            }

            if(invalidRooms.Contains(room))
            {
                //Debug.LogWarning("Room is invalid for key " + color);
                return;
            }

            if(potentialRooms.Contains(room))
            {
                return;
            }

            potentialRooms.Add(room);
        }

        public bool CanAddPotentialRooms()
        {
            return potentialRooms.Count < maxRoomsDownThePath;
        }

        public void AddInvalidRoom(Room room)
        {
            invalidRooms.Add(room);
        }
    }
}