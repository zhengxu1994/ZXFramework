using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUpdate 
{
    public float LogicFrameDelta { get; private set; }

    /// <summary>
    /// 每秒30帧
    /// </summary>
    private const int LogicFrameNum = 30;

    public int clientSpeed { get; private set; }

    private float timeDelta { get => Time.deltaTime * clientSpeed; }

    public float currentPassTime = 0;

    private float nextFrameTime = 0;

    private bool isLogicStart = false;

    private Action hotFixUpdate = null;
    /// <summary>
    /// 初始化游戏逻辑帧率等信息
    /// </summary>
    public void Start()
    {
        LogicFrameDelta = 1.0f / LogicFrameNum;
    }

    public void OtherUpdate()
    {
        //UI update other
    }

    public void StartLogic()
    {
        currentPassTime = 0;
        isLogicStart = true;
    }

    public void LogicOver()
    {
        currentPassTime = 0;
        isLogicStart = false;
    }

    public void LogicUpdate()
    {
        if (!isLogicStart) return;
        //时间累计
        currentPassTime += timeDelta;

        if(currentPassTime >= nextFrameTime)
        {
            Update();
            //每帧间隔增加
            nextFrameTime += LogicFrameDelta;
        }
    }

    public void SetHotfixUpdate(Action update)
    {
        hotFixUpdate = update;
    }

    //游戏逻辑帧放在这里计算
    private void Update()
    {
        hotFixUpdate?.Invoke();
    }

    private void FixedUpdate()
    {

    }
}
