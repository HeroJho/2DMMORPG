using Google.Protobuf.Protocol;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleEquipItem(Player player, C_EquipItem equipPacket)
        {
            if (player == null)
                return;

            player.HandleEquipItem(equipPacket);
        }
    }
}
