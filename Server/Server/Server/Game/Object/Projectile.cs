using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Projectile : GameObject
    {
        public Data.Skill Data { get; set; }

        public Projectile()
        {
            ObjectType = GameObjectType.Projectile;
        }

        public override void Update()
        {

        }
    }
}
