using System;
using Bepop.Core;
using System.Collections.Generic;
namespace Skill
{
    public class SoldierEntityMgr : Singleton<SoldierEntityMgr>
    {
        public HashSet<SoldierEntity> soldiers = new HashSet<SoldierEntity>();

        private int tempEntityId;

        private SoldierEntityMgr() { }

        public SoldierEntity CreateSoldier(int party)
        {
            SoldierEntity entity = new SoldierEntity();
            entity.entityId = tempEntityId;
            entity.campId = party;
            soldiers.Add(entity);
            tempEntityId++;
            return entity;
        }

        public void RemoveSoldier(SoldierEntity entity)
        {
            if (soldiers.Contains(entity))
                soldiers.Remove(entity);
        }
    }
}
