using UnityEngine;

public partial class MapGeneratorRevised
{
    public class Lock
    {
        private Color unlocksMe;
        private Vector2Int position;

        public Lock(Color unlocksMe, Vector2Int position)
        {
            this.unlocksMe = unlocksMe;
            this.position = position;
        }

        public Color UnlocksMe
        {
            get { return unlocksMe; }
        }

        public Vector2Int Position
        {
            get { return position; }
        }

        public Lock Clone(Vector2Int position)
        {
            return new Lock(unlocksMe, position);
        }
    }
}