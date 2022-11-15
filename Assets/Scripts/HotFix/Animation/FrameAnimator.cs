using System.Collections;
using System.Collections.Generic;
using Bepop.Core;
using UnityEngine;

/// <summary>
/// 动画控制系统
/// </summary>
public class FrameAnimator : MonoBehaviour
{

    class ActionInfo
    {
        public string actionName;

        public ActionDirInfo[] dirInfos = new ActionDirInfo[8];

        public void ToArray()
        {
            for (int i = 0; i < dirInfos.Length; i++)
            {
                if (dirInfos[i] != null)
                    dirInfos[i].ToArray();
            }
        }
    }

    class ActionDirInfo
    {
        public int dir;

        public Dictionary<int, Sprite> sprites = new Dictionary<int, Sprite>();

        public Sprite[] spritesArr;
        public void ToArray()
        {
            spritesArr = new Sprite[sprites.Count];
            for (int i = 0; i < spritesArr.Length; i++)
            {
                spritesArr[i] = sprites[i];
            }
        }
    }

    private int tempFrame = 0;

    private string act;

    private int dir;

    private Sprite[] frames;

    public bool isPause { get; private set; }

    public bool IsOver { get; private set; }

    public SpriteRenderer spRenderer;

    public int playTimes = -1;

    private int frameRate = 20;
    public int FrameRate {
        get
        {
            return frameRate;
        }
        set
        {
            frameRate = value;
            frameInterval = 1.0f / frameRate / frameSpeed;
        }
    }

    private int frameSpeed = 1;

    public int FrameSpeed {
        get
        {
            return frameSpeed;
        }
        set
        {
            frameSpeed = value;
            frameInterval = 1.0f / frameRate / frameSpeed;
        }
    }

    public bool IsIgnoreTimeScale = false;

    private float frameInterval = 0;//帧间隔时间
    private float tick = 0;

    private void Start()
    {
        spRenderer = this.GetComponent<SpriteRenderer>();
        if (spRenderer == null)
            spRenderer = this.gameObject.AddComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isPause || IsOver || frames == null) return;
        var dt = IsIgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        tick += dt;
        if(tick >= frameInterval)
        {
            ChangeSprite();
            tick -= frameInterval;
        }
    }

    private void ChangeSprite()
    {
        if (tempFrame >= frames.Length)
        {
            tempFrame = 0;

            if (playTimes == 0)
            {
                IsOver = true;
                return;
            }

            if (playTimes > 0)
                playTimes--;
        }

        spRenderer.sprite = frames[tempFrame];
        tempFrame++;
    }

    public void ChangeFrame(string act, int dir, int playTimes = -1)
    {
        this.act = act;
        this.dir = dir;
        if (actionInfo[act].dirInfos.Length > dir)
            frames = actionInfo[act].dirInfos[dir].spritesArr;
        else
            frames = actionInfo[act].dirInfos[0].spritesArr;
        tempFrame = 0;
        this.playTimes = playTimes;
    }

    public void Play()
    {
        this.isPause = false;
    }

    public void Resume()
    {
        this.isPause = true;
    }


    private Dictionary<string,ActionInfo> actionInfo = null;

    public void Init(Object[] sprites)
    {
        if (sprites == null || sprites.Length <= 0)
        {
            Log.EX.LogError("frame animator sprites is null");
            return;
        }

        if (actionInfo == null) actionInfo = new Dictionary<string, ActionInfo>();
        
        //动作 _ 方向 _ 数组
        for (int i = 0; i < sprites.Length; i++)
        {
            var sp = sprites[i] as Sprite;
            var name = sp.name;
            var strs = name.Split('_');
            string actionName = strs[0];
            int dir = int.Parse(strs[1]);
            var index = int.Parse(strs[2]);
            if (!actionInfo.ContainsKey(actionName))
                actionInfo.Add(actionName, new ActionInfo());
            var action = actionInfo[actionName];
            action.actionName = actionName;
            if(action.dirInfos[dir] == null)
                action.dirInfos[dir]= new ActionDirInfo(); ;
            var actionDir = action.dirInfos[dir];
            actionDir.dir = dir;
            actionDir.sprites[index] = sp;
        }

        actionInfo.ForEach((k, v) => { v.ToArray(); });

        Log.BASE.LogInfo($"Sprite Init :{actionInfo.Count}");
    }
}
