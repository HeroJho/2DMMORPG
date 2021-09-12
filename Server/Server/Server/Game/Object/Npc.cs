using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Npc : GameObject
    {
        public Npc()
        {
            ObjectType = GameObjectType.Npc;
        }

        public void Init(int templateId)
        {
            Id = templateId;

            NpcData npcData = null;
            DataManager.NpcDict.TryGetValue(templateId, out npcData);

            CellPos = new Vector2Int(npcData.x, npcData.y);
        }
    }
}
