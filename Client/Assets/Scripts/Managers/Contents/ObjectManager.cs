using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ObjectManager
{
    public MyPlayerController MyPlayer { get; set; }
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
    Dictionary<Vector2Int, List<Item>> _items = new Dictionary<Vector2Int, List<Item>>(); // item 여부
    Dictionary<int, GameObject> _groundedItems = new Dictionary<int, GameObject>();

    public GameObject ItemRoot
    {
        get
        {
            GameObject root = GameObject.Find("@ItemRoot");
            if (root == null)
            {
                root = new GameObject { name = "@ItemRoot" };
                root.AddComponent<SortingGroup>().sortingOrder = 5;
            }

            return root;
        }
    }


    public void DropItemToMap(PositionInfo posInfo, ItemInfo itemInfo)
    {
        Vector2Int pos = new Vector2Int(posInfo.PosX, posInfo.PosY);
        List<Item> itemList = new List<Item>();

        if (!_items.ContainsKey(pos))
            _items.Add(pos, itemList);
        if (_items.TryGetValue(pos, out itemList) == false)
            _items.Add(pos, itemList);

        Item item = Item.MakeItem(itemInfo);

        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(item.TemplateId, out itemData);


        GameObject go = Managers.Resource.Instantiate(itemData.iconPath, ItemRoot.transform);
        go.name = itemData.name;

        itemList.Add(item);

        go.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
        _groundedItems.Add(item.ItemDbId, go);
    }

    public Item FindItemFromGround(Vector3Int cellPos)
    {
        Vector2Int pos = new Vector2Int(cellPos.x, cellPos.y);

        List<Item> _groundItems = null;
        if (_items.TryGetValue(pos, out _groundItems) == false)
            return null;

        // 해당 좌표의 제일 마지막에 떨어진 아이템 가져옴
        Item item = _groundItems[_groundItems.Count - 1];
        if (item == null)
            return null;

        // 리스트에서 지우는건 서버에서 허락맡고

        return item;
    }

    public void DeleteItem(int itemDbId)
    {
        foreach (List<Item> list in _items.Values)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Item item = list[i];

                if (itemDbId == item.ItemDbId)
                {
                    list.Remove(item);
                    GameObject obj = null;
                    if (_groundedItems.TryGetValue(itemDbId, out obj))
                    {
                        _groundedItems.Remove(itemDbId);
                        Managers.Resource.Destroy(obj);
                    }
                }
            }
        }
    }

    public void Add(ObjectInfo info, bool myPlayer = false)
    {
        if (MyPlayer != null && MyPlayer.Id == info.ObjectId)
            return;
        if (_objects.ContainsKey(info.ObjectId))
            return;

        GameObjectType type = GetObjectTypeById(info.ObjectId);

        switch (type)
        {
            case GameObjectType.Player:
                {
                    if (myPlayer)
                    {
                        GameObject go = Managers.Resource.Instantiate("Creature/MyPlayer");
                        go.name = info.Name;
                        _objects.Add(info.ObjectId, go);

                        MyPlayer = go.GetComponent<MyPlayerController>();
                        MyPlayer.Id = info.ObjectId;
                        MyPlayer.PosInfo = info.PosInfo;
                        MyPlayer.Stat = info.StatInfo;
                        MyPlayer.SyncPos();
                    }
                    else
                    {
                        GameObject go = Managers.Resource.Instantiate("Creature/Player");
                        go.name = info.Name;
                        _objects.Add(info.ObjectId, go);

                        PlayerController pc = go.GetComponent<PlayerController>();
                        pc.Id = info.ObjectId;
                        pc.PosInfo = info.PosInfo;
                        pc.Stat = info.StatInfo;
                        MyPlayer.SyncPos();
                    }
                }
                break;
            case GameObjectType.Monster:
                {
                    GameObject go = Managers.Resource.Instantiate($"Creature/Monster");
                    go.name = "Monster";
                    _objects.Add(info.ObjectId, go);

                    MonsterController mc = go.GetComponent<MonsterController>();
                    mc.PosInfo = info.PosInfo;
                    mc.Stat = info.StatInfo;
                    mc.SyncPos();
                }
                break;
            case GameObjectType.Projectile:
                {
                    GameObject go = Managers.Resource.Instantiate($"Creature/Arrow");
                    go.name = "Arrow";
                    _objects.Add(info.ObjectId, go);

                    ArrowController ac = go.GetComponent<ArrowController>();
                    ac.PosInfo = info.PosInfo;
                    ac.Stat = info.StatInfo;
                    ac.SyncPos();
                }
                break;
        }

        
    }

    public void Remove(int id)
    {
        if (MyPlayer != null && MyPlayer.Id == id)
            return;
        if (_objects.ContainsKey(id) == false)
            return;

        GameObject go = FindById(id);
        if (go == null)
            return;

        _objects.Remove(id);
        Managers.Resource.Destroy(go);
    }

    public GameObject FindById(int id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

    public GameObject FindCreature(Vector3Int cellPos)
    {
        foreach(GameObject obj in _objects.Values)
        {
            CreatureController cc = obj.GetComponent<CreatureController>();
            if (cc == null)
                continue;

            if (cc.CellPos == cellPos)
                return obj;
        }

        return null;
    }

    public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach(GameObject obj in _objects.Values)
        {
            if (condition.Invoke(obj))
                return obj;
        }

        return null;
    }

    public void Clear()
    {
        foreach (GameObject obj in _objects.Values)
            Managers.Resource.Destroy(obj);
        _objects.Clear();
        MyPlayer = null;
    }

    public static GameObjectType GetObjectTypeById(int id)
    {
        int type = (id >> 24) & 0x7F;
        return (GameObjectType)type;
    }
}
