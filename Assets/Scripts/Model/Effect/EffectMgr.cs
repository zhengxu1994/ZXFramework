using System;
using Bepop.Core;
using System.Collections.Generic;
using UnityEngine;
using Resource;
namespace Effect
{
    public enum SortingLayerType
    {
        Under,
        Middle,
        Default,
        Skill,
        Above,
        Top
    }
    public class EffectMgr : Singleton<EffectMgr>
    {
        public static readonly int SortingLayer_Under = SortingLayer.NameToID("Under");

        public static readonly int SortingLayer_Middle = SortingLayer.NameToID("Middle");

        public static readonly int SortingLayer_Default = SortingLayer.NameToID("Default");

        public static readonly int SortingLayer_Skill = SortingLayer.NameToID("Skill");

        public static readonly int SortingLayer_Above = SortingLayer.NameToID("Above");

        public static readonly int SortingLayer_Top = SortingLayer.NameToID("Top");

        private Dictionary<int, ParticleNode> playingNodes = new Dictionary<int, ParticleNode>();

        private Dictionary<string, Queue<ParticleNode>> cacheNodes = new Dictionary<string, Queue<ParticleNode>>();

        private List<int> removeList = new List<int>();

        private int particleNodeId = -1;

        public EffectMgr()
        {

        }

        public void Update(float deltaTime)
        {
            if (playingNodes.Count > 0)
            {
                playingNodes.ForEach((k, v) => {
                    if (!v.pause)
                    {
                        v.Update(deltaTime);
                        if (!v.IsPlaying)
                            removeList.Add(k);
                    }
                });
            }
            if (removeList.Count > 0)
            {
                for (int i = 0; i < removeList.Count; i++)
                {
                    playingNodes.Remove(removeList[i]);
                }
                removeList.Clear();
            }
        }

        public void SetSortingOrder(ParticleNode node,SortingLayerType layerType,int layer =0)
        {
            int sortingOrder = 0;
            int layerId = SortingLayer_Skill;
            if (layerType == SortingLayerType.Skill)
            {
                layerId = SortingLayer_Skill;
            }
            else if (layerType == SortingLayerType.Under)
                layerId = SortingLayer_Under;
            else if (layerType == SortingLayerType.Middle)
                layerId = SortingLayer_Middle;
            else if (layerType == SortingLayerType.Default)
                layerId = SortingLayer_Default;
            else if (layerType == SortingLayerType.Top)
                layerId = SortingLayer_Top;
            else
                layerId = SortingLayer_Above;
            node.gameObject.layer = layer;
            node.sortingGroup.sortingLayerID = layerId;
            node.sortingGroup.sortingOrder = sortingOrder;
        }

        public ParticleNode CreateNode(string path, SortingLayerType sortingLayer, Transform root = null, float time = -1)
        {
            ParticleNode node = null;
            if (cacheNodes.ContainsKey(path) && cacheNodes[path].Count > 0)
            {
                node = cacheNodes[path].Dequeue();
            }
            else
            {
                var obj = ResourceManager.Instance.LoadAsset(path, typeof(GameObject)) as GameObject;
                node = new ParticleNode(obj, path);
            }
            particleNodeId++;
            node.nodeId = particleNodeId;
            node.gameObject.transform.SetParent(root);
            node.Play(time);
            SetSortingOrder(node, sortingLayer, 0);
            playingNodes.Add(particleNodeId, node);
            return node;
        }

        public void RemoveNode(int id)
        {
            if(playingNodes.ContainsKey(id))
            {
                var node = playingNodes[id];
                node.Recycle();
                cacheNodes[node.path].Enqueue(node);
                playingNodes.Remove(id);
            }
        }
        
    }
}