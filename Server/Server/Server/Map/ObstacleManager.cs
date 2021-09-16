using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class ObstacleManager
    {
        private Dictionary<int, Obstacle> _obstacle = new Dictionary<int, Obstacle>();
        private Dictionary<Vector2Int, Obstacle> _checkObstacle = new Dictionary<Vector2Int, Obstacle>();

        public void Add(Obstacle obstacle)
        {
            _obstacle.Add(obstacle.TemplateId, obstacle);

            foreach (Vector2Int obstaclePos in obstacle.ObstaclePos)
            {
                _checkObstacle.Add(obstaclePos, obstacle);
            }
        }

        public void RemoveObstacle(int templateId)
        {
            Obstacle obstacle = null;
            if (_obstacle.TryGetValue(templateId, out obstacle) == false)
                return;

            obstacle.RemoveObstacle();
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
