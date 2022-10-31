using System;
using UnityEngine;
using System.Collections.Generic;
using Skill;
using Bepop.Core;
using Notifaction;
using Skill.SkillTrigger;

public class SkillTest : MonoBehaviour
{
    public GameObject player;

    public GameObject enemy;

    private LinkedList<SkillBase> skills = new LinkedList<SkillBase>();

    private void Start()
    {
        Test();
    }

    private void Test()
    {
        var playerEntity = SoldierEntityMgr.Instance.CreateSoldier(1);

        playerEntity.atk = 100;
        playerEntity.def = 10;
        playerEntity.entityId = 1;
        playerEntity.hp = 1000;
        playerEntity.isAlive = true;
        playerEntity.SetGameObject(player);
        playerEntity.sex = true;
        playerEntity.skillCD = 10;

        var enemyEntity = SoldierEntityMgr.Instance.CreateSoldier(-1);
        enemyEntity.atk = 100;
        enemyEntity.def = 10;
        enemyEntity.hp = 1000;
        enemyEntity.isAlive = true;
        enemyEntity.SetGameObject(enemy);
        enemyEntity.sex = true;
        enemyEntity.skillCD = 10;

        SkillData skillData = new SkillData();
        skillData.TestSkill();

        var skill = playerEntity.AddSkill(skillData);
        var canUse = skill.CheckCanUse();
        Log.BASE.LogInfo($"canuse :{canUse}");

        if (canUse)
        {
            var getTargets = skill.GetSkillTargets(SkillVector.Zero, SoldierEntityMgr.Instance.soldiers);
            Log.BASE.LogInfo($"target count:{getTargets.Count}");
            skill.Use(SkillVector.Zero);
            //模拟下
            skills.AddFirst(skill);
        }
    }

    private void Update()
    {
        if (skills.Count <= 0) return;
        var skill = skills.First;
      
        if (skill != null)
        {
            skill.Value.UpdateFrame();
            skill = skill.Next;
        }

        NotifactionCenter.Instance.PostNotification(SkillEvent.Trigger_BuffUpdate);
    }
}
