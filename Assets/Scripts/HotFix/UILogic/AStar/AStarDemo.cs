using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using MyAi;
using TrueSync;
#region
//1.创建地图 支持动态大小 支持设置阻挡格 并导出导入

#endregion

public class AStarDemo : MonoBehaviour
{
    public int width = 100;

    public int height = 100;

    public GameObject gridPrefab;

    public Transform root;

    public float interval = 0.14f;

    public GameObject[,] mapGrids;

    public bool openSetBlock = false;

    public bool openAStar = true;

    public HashSet<(int, int)> blockIndex = new HashSet<(int, int)>();

    public AStar astar;
    public (int, int) starPos = (-1, -1);

    public (int, int) endPos = (-1, -1);


    private List<AStarNode> nodeList;

    public GameObject playerPrefab;


    public List<MoveGroup> players = new List<MoveGroup>();

    public List<MoveGroup> enemies = new List<MoveGroup>();

    public FP groupRadius = 1.5f;

    public (FP, FP)[] playerPos = new (FP, FP)[] {
    (2.7,1.1),(2.7f,2.3),(3.9,3.2),(3.4,4.6),(3.1,7)
    };

    public (FP, FP)[] enemiesPos = new (FP, FP)[] {
    (15,2.3),(15,4.3),(14,6.4),(15.2,8.1),(17.3,8.4)
    };

    public GameObject groupArea;
    private void Start()
    {
        width = 80;
        height = 40;
        ReadBlock();
        StartCoroutine(CreateMap());
        MoveManager.Instance.Init(blockIndex,interval, groupRadius);
        MoveManager.Instance.width = width;
        MoveManager.Instance.height = height;
        AStarManager.Instance.Init(width,height,blockIndex);
        for (int i = 1; i <= 5; i++)
        {
            int id = i;
            if (id != 4) continue;
           var player = new MoveGroup(id, 0.5f, new TrueSync.TSVector2(playerPos[id - 1].Item1, playerPos[id - 1].Item2), playerPrefab, groupArea);
            players.Add(player);
        }

        for (int i = 1; i <= 5; i++)
        {
            int id = i;
            if (id != 3) continue;
            var enemy = new MoveGroup(-id, 0.5f, new TrueSync.TSVector2(enemiesPos[id-1].Item1, enemiesPos[id-1].Item2), playerPrefab, groupArea);
            enemies.Add(enemy);
        }

        players[0].targetPos = enemies[0].Position;
        //enemy.moveEnable = false;
        //astar = new AStar();
        //astar.Init(width, height, blockIndex);
    }

    private void GoToTargetTest()
    {
        players.ForEach((unit) => {
            unit.UpdateState();
            unit.UpdateStep();
        });

        enemies.ForEach((unit) => {
            unit.UpdateState();
            unit.UpdateStep();
        });

    }

    IEnumerator CreateMap()
    {
        mapGrids = new GameObject[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var grid = Instantiate(gridPrefab) as GameObject;
                grid.name = string.Format("{0},{1}", i, j);
                grid.transform.position = new Vector3(i * interval * 2, j * interval * 2, 0);
                mapGrids[i, j] = grid;
                grid.transform.SetParent(root);
            }
        }

        if(blockIndex.Count > 0)
        {
            foreach (var index in blockIndex)
            {
                mapGrids[index.Item1, index.Item2].GetComponent<SpriteRenderer>().color = Color.red;
            }
        }
        yield return null;
    }
    private int updateTime = 30;
    private int nowTime = 30;
    private void Update()
    {
        //nowTime++;
        //if (nowTime >= updateTime)
        //{
        //    nowTime = 0;
        //    players.ForEach((unit) => {
        //        FP dis = FP.MaxValue;
        //        MoveGroup chooseUnit = null;
        //        for (int i = 0; i < enemies.Count; i++)
        //        {
        //            var tempDis = TSVector2.DistanceSquared(enemies[i].Position, unit.Position);
        //            if (tempDis < dis)
        //            {
        //                dis = tempDis;
        //                chooseUnit = enemies[i];
        //            }
        //        }
        //        if (chooseUnit != null)
        //            unit.AttachTarget(chooseUnit,false);
        //    });

        //    enemies.ForEach((unit) => {
        //        FP dis = FP.MaxValue;
        //        MoveGroup chooseUnit = null;
        //        for (int i = 0; i < players.Count; i++)
        //        {
        //            var tempDis = TSVector2.DistanceSquared(players[i].Position, unit.Position);
        //            if (tempDis < dis)
        //            {
        //                dis = tempDis;
        //                chooseUnit = players[i];
        //            }
        //        }
        //        if (chooseUnit != null)
        //            unit.AttachTarget(chooseUnit, false);
        //    });
        //}

        if (Input.GetKeyDown(KeyCode.K))
        {
            //player.MoveToPosition(enemy.Position, true);
        }
        if(Input.GetKeyDown(KeyCode.Space) && openAStar)
        {
            Reset();
        }

        if(Input.GetMouseButtonDown(1) && openSetBlock)
        {
            var index = PosToIndex(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (index == (-1, -1)) return;
            //设置阻挡格
            if(!blockIndex.Contains(index))
            {
                blockIndex.Add(index);
                mapGrids[index.Item1, index.Item2].GetComponent<SpriteRenderer>().color = Color.red;
            }
            else
            {
                blockIndex.Remove(index);
                mapGrids[index.Item1, index.Item2].GetComponent<SpriteRenderer>().color = Color.white;
            }
        }

        if(Input.GetMouseButtonDown(0) )
        {
            var index = PosToIndex(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (openAStar)
            {
                if (index == (-1, -1)) return;
                if (!blockIndex.Contains(index))
                {
                    if (starPos == (-1, -1))
                    {
                        starPos = index;
                        mapGrids[index.Item1, index.Item2].GetComponent<SpriteRenderer>().color = Color.yellow;
                    }
                    else
                    {
                        endPos = index;
                        mapGrids[index.Item1, index.Item2].GetComponent<SpriteRenderer>().color = Color.black;
                        nodeList = astar.GetAStar(astar.IndexToNode(starPos), astar.IndexToNode(endPos));
                        for (int i = 0; i < nodeList.Count; i++)
                        {
                            mapGrids[nodeList[i].x, nodeList[i].y].GetComponent<SpriteRenderer>().color = Color.blue;
                        }
                    }
                }
            }
        }


        GoToTargetTest();


        RVO.Simulator.Instance.doStep();
    }

    public void Reset()
    {
        mapGrids[starPos.Item1, starPos.Item2].GetComponent<SpriteRenderer>().color = Color.white;
        mapGrids[endPos.Item1, endPos.Item2].GetComponent<SpriteRenderer>().color = Color.white;
        if (nodeList != null && nodeList.Count > 0)
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                mapGrids[nodeList[i].x, nodeList[i].y].GetComponent<SpriteRenderer>().color = Color.white;
            }
            nodeList.Clear();
        }
        starPos = (-1, -1);
        endPos = (-1, -1);
    }

    public (int, int) PosToIndex(Vector2 pos)
    {
        // - (interval / 2) 是因为格子的中心在0，0 并不是格子的左下角是0，0 要换算下
        int x = Mathf.CeilToInt((pos.x - (interval)) / interval / 2);
        int y = Mathf.CeilToInt((pos.y - (interval)) / interval / 2);
        if (x < 0 || y < 0 || x >= width || y >= height)
            return (-1, -1);
        return (x, y);
    }

    void ReadBlock()
    {
        string path = Application.dataPath + "/Resources/AStar/GirdsTxt.txt";
        if (File.Exists(path))
        {
            var content = File.ReadAllText(path);
            if (!string.IsNullOrEmpty(content))
            {
                var indexs = content.Split('/');
                for (int i = 0; i < indexs.Length; i++)
                {
                    if (!string.IsNullOrEmpty(indexs[i]))
                    {
                        var strs = indexs[i].Split(',');
                        int item1 = int.Parse(strs[0].Substring(1, strs[0].Length - 1));
                        int item2 = int.Parse(strs[1].Substring(0, strs[1].Length - 1));
                        blockIndex.Add((item1, item2));
                    }
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        string path = Application.dataPath + "/Resources/AStar/GirdsTxt.txt";
        //导出Txt
        if (!File.Exists(path))
        {
            File.CreateText(path);
        }

        StringBuilder sb = new StringBuilder();
        foreach (var index in blockIndex)
        {
            sb.Append(index.ToString());
            sb.Append("/");
        }

        if (sb.Length <= 0) return;

        File.WriteAllText(path, sb.ToString());
    }
}
