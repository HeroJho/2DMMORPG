using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Obstacle
    {
        public int TemplateId { get; private set; }
        private Vector2Int _spawnPos;
        public List<Vector2Int> ObstaclePos = new List<Vector2Int>();
        

        public static Obstacle MakeObstacle(ObstacleData obstacleData)
        {
            Obstacle obstacle = new Obstacle();
            {
                obstacle.TemplateId = obstacleData.id;
                obstacle._spawnPos = obstacleData.spawnPos;
                obstacle.ObstaclePos = obstacleData.obstaclePos;
            }

            return obstacle;
        }
    }
}
