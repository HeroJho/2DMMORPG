using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct Pos
{
    public Pos(int y, int x) { Y = y; X = x; }
    public int Y;
    public int X;
}

public struct PQNode : IComparable<PQNode>
{
    public int F;
    public int G;
    public int Y;
    public int X;

    public int CompareTo(PQNode other)
    {
        if (F == other.F)
            return 0;
        return F < other.F ? 1 : -1;
    }
}

public class MapManager
{
    private Dictionary<Vector2Int, Obstacle> _checkObstacle = new Dictionary<Vector2Int, Obstacle>();

    public Grid CurrentGrid { get; private set; }

    public int MinX { get; set; }
    public int MaxX { get; set; }
    public int MinY { get; set; }
    public int MaxY { get; set; }

    public int SizeX { get { return MaxX - MinX + 1; } }
    public int SizeY { get { return MaxY - MinY + 1; } }

    bool[,] _collision;

    public bool CanGo(Vector3Int cellPos)
    {
        if (cellPos.x < MinX || cellPos.x > MaxX)
            return false;
        if (cellPos.y < MinY || cellPos.y > MaxY)
            return false;

        // 장애물 확인
        Obstacle obstacle = null;
        Vector2Int pos = (Vector2Int)cellPos;
        if (_checkObstacle.TryGetValue(pos, out obstacle) == true)
            return false;

        int x = cellPos.x - MinX;
        int y = MaxY - cellPos.y;
        return !_collision[y, x];
    }

    public void LoadMap(int mapId)
    {
        DestroyMap();

        string mapName = "Map_" + mapId.ToString("000");
        GameObject go = Managers.Resource.Instantiate($"Map/Maps/{mapName}");
        go.name = "Map";

        GameObject collision = Util.FindChild(go, "Tilemap_Collision", true);
        if (collision != null)
            collision.SetActive(false);

        // Env
        GameObject tree = Util.FindChild(go, "Tilemap_Env_Collision", true);
        if (collision != null)
            tree.SetActive(false);
        GameObject bush = Util.FindChild(go, "Tilemap_Env_NoCollision", true);
        if (collision != null)
            bush.SetActive(false);
        GameObject treeLoot = Util.FindChild(go, "EnvObjects", true);

        CurrentGrid = go.GetComponent<Grid>();

        // Collision 관련 파일
        TextAsset collision_txt = Managers.Resource.Load<TextAsset>($"Map/{mapName}");
        StringReader coliision_reader = new StringReader(collision_txt.text);

        // Env
        TextAsset collisionEnv_txt = Managers.Resource.Load<TextAsset>($"Map/{mapName}_Collision");
        StringReader collisionEnv_reader = new StringReader(collisionEnv_txt.text);
        TextAsset noCollisionEnv_txt = Managers.Resource.Load<TextAsset>($"Map/{mapName}_NoCollision");
        StringReader noCollisionEnv_reader = new StringReader(noCollisionEnv_txt.text);

        MinX = int.Parse(coliision_reader.ReadLine());
        MaxX = int.Parse(coliision_reader.ReadLine());
        MinY = int.Parse(coliision_reader.ReadLine());
        MaxY = int.Parse(coliision_reader.ReadLine());

        int xCount = MaxX - MinX + 1;
        int yCount = MaxY - MinY + 1;
        _collision = new bool[yCount, xCount];

        for(int y = 0; y < yCount; y++)
        {
            string line = coliision_reader.ReadLine();
            string collisionEnv_line = collisionEnv_reader.ReadLine();
            string noCollisionEnv_line = noCollisionEnv_reader.ReadLine();

            for(int x = 0; x < xCount; x++)
            {
                _collision[y, x] = (line[x] == '1' ? true : false);

                // 나무 깔아주기
                if(collisionEnv_line[x] == '1')
                {
                    Vector3 cellPos = Pos2Cell(new Pos(y, x));

                    GameObject obj = Managers.Resource.Instantiate("Map/Env/RedTree", treeLoot.transform);
                    obj.transform.position = new Vector3(cellPos.x + 0.5f, cellPos.y + 0.5f, 0);
                }
                else if (collisionEnv_line[x] == '2')
                {
                    Vector3 cellPos = Pos2Cell(new Pos(y, x));

                    GameObject obj = Managers.Resource.Instantiate("Map/Env/GreenTree", treeLoot.transform);
                    obj.transform.position = new Vector3(cellPos.x + 0.5f, cellPos.y + 0.5f, 0);
                }

                // 부쉬 깔아주기
                if (noCollisionEnv_line[x] == '1') 
                {
                    Vector3 cellPos = Pos2Cell(new Pos(y, x));

                    GameObject obj = Managers.Resource.Instantiate("Map/Env/RedBush", treeLoot.transform);
                    obj.transform.position = new Vector3(cellPos.x + 0.5f, cellPos.y + 0.5f, 0);
                }
                else if(noCollisionEnv_line[x] == '2')
                {
                    Vector3 cellPos = Pos2Cell(new Pos(y, x));

                    GameObject obj = Managers.Resource.Instantiate("Map/Env/GreenBush", treeLoot.transform);
                    obj.transform.position = new Vector3(cellPos.x + 0.5f, cellPos.y + 0.5f, 0);
                }
                else if (noCollisionEnv_line[x] == '3')
                {
                    Vector3 cellPos = Pos2Cell(new Pos(y, x));

                    GameObject obj = Managers.Resource.Instantiate("Map/Env/Building_1", treeLoot.transform);
                    obj.transform.position = new Vector3(cellPos.x + 0.5f, cellPos.y + 0.5f, 0);
                }

            }
        }
    }

    public void DestroyMap()
    {
        GameObject map = GameObject.Find("Map");
        if(map != null)
        {
            GameObject.Destroy(map);
            CurrentGrid = null;
        }
    }

    public void AddObstacle(Obstacle obstacle)
    {
        foreach (Vector2Int pos in obstacle.ObstaclePos)
        {
            _checkObstacle.Add(pos, obstacle);
        }
    }

    public void RemoveObstacle(Obstacle obstacle)
    {
        foreach (Vector2Int pos in obstacle.ObstaclePos)
        {
            _checkObstacle.Remove(pos);
        }
    }

    #region A* PathFinding

    int[] _deltaY = new int[] { 1, -1, 0, 0 };
    int[] _deltaX = new int[] { 0, 0, -1, 1 };
    int[] cost = new int[] { 10, 10, 10, 10 };

    public List<Vector3Int> FindPath(Vector3Int startCellPos, Vector3Int destCellPos, bool ignoreDestCollision = false)
    {
        List<Pos> path = new List<Pos>();

        // 점수 매기기
        // F = G + H
        // F = 최종 점수 (작을 수록 좋음, 경로에 따라 달라짐)
        // G = 시작점에서 해당 좌표까지 이동하는데 드는 비용 (작을 수록 좋음, 경로에 따라 달라짐)
        // H = 목적지에서 얼마나 가까운지 (작을 수록 좋음, 고정)

        // 방문 여부 CloseList
        bool[,] closed = new bool[SizeY, SizeX];

        // 발견 여부 OpenList
        int[,] open = new int[SizeY, SizeX];
        for (int y = 0; y < SizeY; y++)
            for (int x = 0; x < SizeX; x++)
                open[y, x] = Int32.MaxValue;

        Pos[,] parent = new Pos[SizeY, SizeX];

        PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>();

        // CellPos -> ArrayPos
        Pos pos = Cell2Pos(startCellPos);
        Pos dest = Cell2Pos(destCellPos);

        // 시작점 발견 (예약 진행)
        open[pos.Y, pos.X] = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X));
        pq.Push(new PQNode() { F = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)), G = 0, Y = pos.Y, X = pos.X });
        parent[pos.Y, pos.X] = new Pos(pos.Y, pos.X);

        while (pq.Count > 0)
        {
            // 제일 좋은 후보를 찾는다
            PQNode node = pq.Pop();
            // 동일한 좌표를 여러 경로로 찾아서, 더 빠른 경로로 인해서 이미 방문(closed)된 경우 스킵
            if (closed[node.Y, node.X])
                continue;

            // 방문한다
            closed[node.Y, node.X] = true;
            // 목적지 도착했으면 바로 종료
            if (node.Y == dest.Y && node.X == dest.X)
                break;

            // 상하좌우 등 이동할 수 있는 좌표이지 확인해서 예약(open)한다
            for(int i =0; i < _deltaY.Length; i++)
            {
                Pos next = new Pos(node.Y + _deltaY[i], node.X + _deltaX[i]);

                // 유효 범위 체크
                // 벽 체크
                if (!ignoreDestCollision || next.Y != dest.Y || next.X != dest.X)
                {
                    if (CanGo(Pos2Cell(next)) == false)
                        continue;
                }

                // 이미 방문한 곳이면 스킵
                if (closed[next.Y, next.X])
                    continue;

                // 비용 계산
                int g = 0; // node.G + _cost[i];
                int h = 10 * ((dest.Y - next.Y) * (dest.Y - next.Y) + (dest.X - next.X) * (dest.X - next.X));
                // 다른 경로에서 더 빠른 길 이미 찾았으면 스킵
                if (open[next.Y, next.X] < g + h)
                    continue;

                // 예약 진행
                open[next.Y, next.X] = g + h;
                pq.Push(new PQNode() { F = g + h, G = g, Y = next.Y, X = next.X });
                parent[next.Y, next.X] = new Pos(node.Y, node.X);
            }
        }

        return CalcCellPathFromParent(parent, dest);     
    }

    List<Vector3Int> CalcCellPathFromParent(Pos[,] parent, Pos dest)
    {
        List<Vector3Int> cells = new List<Vector3Int>();

        int y = dest.Y;
        int x = dest.X;
        while (parent[y, x].Y != y || parent[y, x].X != x)
        {
            cells.Add(Pos2Cell(new Pos(y, x)));
            Pos pos = parent[y, x];
            y = pos.Y;
            x = pos.X;
        }
        cells.Add(Pos2Cell(new Pos(y, x)));
        cells.Reverse();

        return cells;
    }

    Pos Cell2Pos(Vector3Int cell)
    {
        // CellPos -> ArrayPos
        return new Pos(MaxY - cell.y, cell.x - MinX);
    }

    Vector3Int Pos2Cell(Pos pos)
    {
        // ArrayPos -> CellPos
        return new Vector3Int(pos.X + MinX, MaxY - pos.Y, 0);
    }

    #endregion
}
