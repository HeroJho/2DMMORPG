using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapEditor
{
#if UNITY_EDITOR

    [MenuItem("Tools/GenerateMap %#g")]
    private static void GenerateMap()
    {
        GenerateByPath("Assets/Resources/Map");
        GenerateByPath("../Common/MapData");
    }

    private static void GenerateByPath(string pathPrefix)
    {
        GameObject[] gameObjects = Resources.LoadAll<GameObject>("Prefabs/Map/Maps");

        foreach (GameObject go in gameObjects)
        {
            Tilemap tmBase = Util.FindChild<Tilemap>(go, "Tilemap_Base", true);
            Tilemap tm = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);

            // Spawn
            Tilemap spawn = Util.FindChild<Tilemap>(go, "Tilemap_Spawn", true);

            // Env
            Tilemap envCollsion = Util.FindChild<Tilemap>(go, "Tilemap_Env_Collision", true);
            Tilemap envNoCollsion = Util.FindChild<Tilemap>(go, "Tilemap_Env_NoCollision", true);

            using (var writer = File.CreateText($"{pathPrefix}/{go.name}.txt"))
            {
                writer.WriteLine(tmBase.cellBounds.xMin);
                writer.WriteLine(tmBase.cellBounds.xMax - 1);
                writer.WriteLine(tmBase.cellBounds.yMin);
                writer.WriteLine(tmBase.cellBounds.yMax - 1);

                for (int y = tmBase.cellBounds.yMax - 1; y >= tmBase.cellBounds.yMin; y--)
                {
                    for (int x = tmBase.cellBounds.xMin; x <= tmBase.cellBounds.xMax - 1; x++)
                    {
                        TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                        TileBase tree_tile = envCollsion.GetTile(new Vector3Int(x, y, 0));

                        if (tile != null)
                            writer.Write("1");
                        else if(tree_tile != null)
                            writer.Write("1");
                        else
                            writer.Write("0");
                    }

                    writer.WriteLine();
                }
            }

            // Spawn
            using (var writer = File.CreateText($"{pathPrefix}/{go.name}_Spawn.txt"))
            {

                for (int y = tmBase.cellBounds.yMax - 1; y >= tmBase.cellBounds.yMin; y--)
                {
                    for (int x = tmBase.cellBounds.xMin; x <= tmBase.cellBounds.xMax - 1; x++)
                    {
                        TileBase tile = spawn.GetTile(new Vector3Int(x, y, 0));
                        if (tile == null)
                            continue;

                        string name = tile.ToString();
                        string[] words = name.Split('_');
                        writer.Write(words[0]+ $"_{x}_{y}");
                        writer.WriteLine();
                    }
                }
            }

            // Env
            using (var writer = File.CreateText($"{pathPrefix}/{go.name}_Collision.txt"))
            {

                for (int y = tmBase.cellBounds.yMax - 1; y >= tmBase.cellBounds.yMin; y--)
                {
                    for (int x = tmBase.cellBounds.xMin; x <= tmBase.cellBounds.xMax - 1; x++)
                    {
                        TileBase tile = envCollsion.GetTile(new Vector3Int(x, y, 0));
                        if (tile == null)
                        {
                            writer.Write("0");
                            continue;
                        }

                        if (tile.ToString() == "RedTree (UnityEngine.Tilemaps.Tile)")
                        {
                            writer.Write("1");
                            Debug.Log(tile.ToString());
                        }
                        else if (tile.ToString() == "GreenTree (UnityEngine.Tilemaps.Tile)")
                        {
                            writer.Write("2");
                            Debug.Log(tile.ToString());
                        }

                    }

                    writer.WriteLine();
                }
            }
            using (var writer = File.CreateText($"{pathPrefix}/{go.name}_NoCollision.txt"))
            {

                for (int y = tmBase.cellBounds.yMax - 1; y >= tmBase.cellBounds.yMin; y--)
                {
                    for (int x = tmBase.cellBounds.xMin; x <= tmBase.cellBounds.xMax - 1; x++)
                    {
                        TileBase tile = envNoCollsion.GetTile(new Vector3Int(x, y, 0));
                        if (tile == null)
                        {
                            writer.Write("0");
                            continue;
                        }
 
                        if (tile.ToString() == "RedBush (UnityEngine.Tilemaps.Tile)")
                        {
                            writer.Write("1");
                            Debug.Log(tile.ToString());
                        }
                        else if (tile.ToString() == "GreenBush (UnityEngine.Tilemaps.Tile)")
                        {
                            writer.Write("2");
                            Debug.Log(tile.ToString());
                        }
                        else if (tile.ToString() == "building (UnityEngine.Tilemaps.Tile)")
                        {
                            writer.Write("3");
                            Debug.Log(tile.ToString());
                        }


                    }

                    writer.WriteLine();
                }
            }

        }
    }

#endif
}
