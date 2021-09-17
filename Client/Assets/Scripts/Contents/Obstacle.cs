using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public int TemplateId { get; private set; }
    public Vector2Int SpawnPos { get; private set; }
    public List<Vector2Int> ObstaclePos = new List<Vector2Int>();

    public void Init(ObstacleData obstacleData)
    {
        TemplateId = obstacleData.id;
        SpawnPos = obstacleData.spawnPos;
        ObstaclePos = obstacleData.obstaclePos;
    }

}
