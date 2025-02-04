using System.Collections.Generic;
using UnityEngine;


public partial class MapGeneratorRevised
{
    public class Room
    {
        private Vector2Int bottomLeft;
        private int width;
        private int height;

        private List<Key> containedKeys = new List<Key>();

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

        public void AddKey(Key key)
        {
            containedKeys.Add(key);
        }

        public List<Key> ContainedKeys
        {
            get { return containedKeys; }
        }

        public void DetermineKeyPrerequisites(List<Key> placedKeys)
        {
            if (containedKeys.Count == 0 || containedKeys.Count == 1)
            {
                return;
            }

            bool foundFirstKey = false;
            Key previousKey = null;
            foreach (Key key in placedKeys)
            {
                if (key.RoomParent == this)
                {
                    if (!foundFirstKey)
                    {
                        foundFirstKey = true;
                    }
                    else
                    {
                        key.SetPrerequisite(previousKey);
                    }
                }

                previousKey = key;
            }
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

        public int GetNumberOfOpenings(List<Vector2Int> openings)
        {
            int openingsInRoom = 0;

            List<Vector2Int> perimeter = GetPerimeterPositions();

            foreach (Vector2Int position in openings)
            {
                if (perimeter.Contains(position))
                {
                    openingsInRoom++;
                }
            }
            return openingsInRoom;
        }

        public override string ToString()
        {
            return "Bottom Left: " + bottomLeft + " Width: " + width + " Height: " + height;
        }
    }
}