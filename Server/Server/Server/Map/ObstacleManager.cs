using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class ObstacleManager
    {
        public Dictionary<int, Obstacle> Obstacles { get; private set; } = new Dictionary<int, Obstacle>();
        private Dictionary<Vector2Int, Obstacle> _checkObstacle = new Dictionary<Vector2Int, Obstacle>();

        public void Add(Obstacle obstacle)
        {
            Obstacles.Add(obstacle.TemplateId, obstacle);

            foreach (Vector2Int obstaclePos in obstacle.ObstaclePos)
            {
                _checkObstacle.Add(obstaclePos, obstacle);
            }
        }

        public Obstacle RemoveObstacle(int templateId)
        {
            Obstacle obstacle = null;
            if (Obstacles.Remove(templateId, out obstacle) == false)
                return null;

            foreach (Vector2Int pos in obstacle.ObstaclePos)
            {
                _checkObstacle.Remove(pos);
            }

            return obstacle;
        }

        public bool CheckObstaclePos(Vector2Int dest)
        {
            Obstacle obstacle = null;
            if (_checkObstacle.TryGetValue(dest, out obstacle) == false)
                return true;

            return false;
        }

    }
}
