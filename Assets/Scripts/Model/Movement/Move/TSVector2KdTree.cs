using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using TrueSync;
using Pool;

namespace KdTree.Math
{
    [Serializable]
    public class TSVector2Math : TypeMath<FP>
    {
        public override int Compare(FP a, FP b)
        {
            return a.CompareTo(b);
        }

        public override bool AreEqual(FP a, FP b)
        {
            return a == b;
        }

        public override FP MinValue => FP.MinValue;

        public override FP MaxValue => FP.MaxValue;

        public override FP Zero => 0;

        public override FP NegativeInfinity => FP.NegativeInfinity;

        public override FP PositiveInfinity => FP.PositiveInfinity;

        public override FP Add(FP a, FP b)
        {
            return a + b;
        }

        public override FP Subtract(FP a, FP b)
        {
            return a - b;
        }

        public override FP Multiply(FP a, FP b)
        {
            return a * b;
        }

        public  bool AreEqual(TSVector2 a,TSVector2 b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public override bool AreEqual(FP[] a, FP[] b)
        {
            if (a.Length != b.Length) return false;
            for (int index = 0; index < a.Length; index++)
            {
                if (!AreEqual(a[index], b[index]))
                    return false;
            }
            return true;
        }

        public FP DistanceSquaredBetweenPoints(TSVector2 a,TSVector2 b)
        {
            return TSVector2.DistanceSquared(a, b);
        }

        public override FP DistanceSquaredBetweenPoints(FP[] a, FP[] b)
        {
            FP distance = Zero;
            int dimensions = a.Length;

            for (int dimension = 0; dimension < dimensions; dimension++)
            {
                FP distOnThisAxis = Subtract(a[dimension], b[dimension]);
                FP distOnThisAxisSquared = Multiply(distOnThisAxis, distOnThisAxis);

                distance = Add(distance, distOnThisAxisSquared);
            }
            return distance;
        }
    }
}

namespace KdTree
{
    [Serializable]
    public class TSVector2KdNode<TValue>
    {
        public TSVector2 Point;
        public TValue Value = default(TValue);

        public TSVector2KdNode() { }

        public TSVector2KdNode(TSVector2 point,TValue value)
        {
            Point = point;
            Value = value;
        }

        internal TSVector2KdNode<TValue> LeftChild = null;
        internal TSVector2KdNode<TValue> RightChild = null;

        internal TSVector2KdNode<TValue> this[int compare]
        {
            get {
                if (compare <= 0) return LeftChild;
                else
                    return RightChild;
            }
            set
            {
                if (compare <= 0) LeftChild = value;
                else
                    RightChild = value;
            }
        }

        public bool IsLeaf
        {
            get {
                return (LeftChild == null) && (RightChild == null);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("x:");
            sb.Append(Point.x.ToString());
            sb.Append(",y:");
            sb.Append(Point.y.ToString());
            sb.Append(",");
            if (Value == null)
                sb.Append("null");
            else
                sb.Append(Value.ToString());

            return sb.ToString();
        }
    }

    public struct TSVector2Rect
    {
        public TSVector2 MinPoint;

        public TSVector2 MaxPoint;

        public static TSVector2Rect Infinite(int dimensions,ITypeMath<FP> math)
        {
            var rect = new TSVector2Rect {
                MinPoint = new TSVector2(math.NegativeInfinity, math.NegativeInfinity),
                MaxPoint = new TSVector2(math.PositiveInfinity, math.PositiveInfinity)
            };

            return rect;
        }

        public TSVector2 GetClosestPoint(TSVector2 toPoint,ITypeMath<FP> math)
        {
            TSVector2 closest = new TSVector2();

            if (math.Compare(MinPoint.x, toPoint.x) > 0)
                closest.x = MinPoint.x;
            else if (math.Compare(MaxPoint.x, toPoint.x) < 0)
                closest.x = MaxPoint.x;
            else
                closest.x = toPoint.x;
            if (math.Compare(MinPoint.y, toPoint.y) > 0)
                closest.y = MinPoint.y;
            else if (math.Compare(MaxPoint.y, toPoint.y) < 0)
                closest.y = MaxPoint.y;
            else
                closest.y = toPoint.y;
            return closest;
        }

        public TSVector2Rect Clone()
        {
            var rect = new TSVector2Rect {
                MinPoint = MinPoint,
                MaxPoint = MaxPoint
            };
            return rect;
        }
    }

    [Serializable]
    public class TSVector2KdTree<TValue> : IEnumerable<TSVector2KdNode<TValue>>
    {
        private int dimensions;
        private KdTree.Math.TSVector2Math typeMath = new Math.TSVector2Math();

        private TSVector2KdNode<TValue> root = null;

        private TObjectPool<TSVector2KdNode<TValue>> TSVector2KdNodePool;

        public AddDuplicateBehavior AddDuplicateBehavior { get; private set; }

        public int Count { get; private set; }

        public TSVector2KdNode<TValue> Current => throw new NotImplementedException();

        public TSVector2KdTree(int dimensions = 2)
        {
            this.dimensions = dimensions;
            Count = 0;
            TSVector2KdNodePool = new TObjectPool<TSVector2KdNode<TValue>>(OnAlloc, OnFree, OnDestroy);
        }

        public TSVector2KdTree(int dimensions, AddDuplicateBehavior addDuplicateBehaviour)
        {
            AddDuplicateBehavior = addDuplicateBehaviour;
        }

        private TSVector2KdNode<TValue> OnAlloc()
        {
            return new TSVector2KdNode<TValue>();
        }

        public TSVector2KdNode<TValue> Alloc(TSVector2 point, TValue value)
        {
            var node = this.TSVector2KdNodePool.Alloc();
            node.Point = point;
            node.Value = value;
            return node;
        }

        private void OnFree(TSVector2KdNode<TValue> node)
        {
            node.LeftChild = null;
            node.RightChild = null;
            node.Point = TSVector2.zero;
            node.Value = default(TValue);
        }

        public void Free(TSVector2KdNode<TValue> node) {
            TSVector2KdNodePool.Free(node);
        }

        private void OnDestroy(TSVector2KdNode<TValue> node)
        {
            node.LeftChild = null;
            node.RightChild = null;
            node.Point = TSVector2.zero;
            node.Value = default(TValue);
        }

        public bool Add(TSVector2 point, TValue value)
        {
            var nodeToAdd = Alloc(point, value);

            if (root == null)
                root = Alloc(point, value);
            else
            {
                int dimension = -1;
                TSVector2KdNode<TValue> parent = root;

                do
                {
                    dimension = (dimension + 1) % dimensions;

                    if (typeMath.AreEqual(point, parent.Point))
                    {
                        switch (AddDuplicateBehavior)
                        {
                            case AddDuplicateBehavior.Skip:
                                return false;
                            case AddDuplicateBehavior.Error:
                                throw new DuplicateNodeError();
                            case AddDuplicateBehavior.Update:
                                parent.Value = value;
                                return true;
                            default:
                                throw new Exception("Unexpected AddDuplicateBehaviour");
                        }
                    }
                    int compare = typeMath.Compare(point[dimension], parent.Point[dimension]);

                    if (parent[compare] == null)
                    {
                        parent[compare] = nodeToAdd;
                        break;
                    }
                    else
                        parent = parent[compare];
                }
                while (true);
            }
            Count++;
            return true;
        }

        private void ReaddChildNodes(TSVector2KdNode<TValue> removeNode)
        {
            if (removeNode.IsLeaf)
                return;
            var nodesToReadd = new Queue<TSVector2KdNode<TValue>>();

            var nodesToReaddQueue = new Queue<TSVector2KdNode<TValue>>();

            if (removeNode.LeftChild != null)
                nodesToReaddQueue.Enqueue(removeNode.LeftChild);

            if (removeNode.RightChild != null)
                nodesToReaddQueue.Enqueue(removeNode.RightChild);

            Free(removeNode);

            while (nodesToReaddQueue.Count > 0)
            {
                var nodeToReadd = nodesToReaddQueue.Dequeue();

                nodesToReadd.Enqueue(nodeToReadd);

                for (int side = -1; side <= 1; side += 2)
                {
                    if (nodeToReadd[side] != null)
                    {
                        nodesToReaddQueue.Enqueue(nodeToReadd[side]);

                        nodeToReadd[side] = null;
                    }
                }
            }

            while (nodesToReadd.Count > 0)
            {
                var nodeToReadd = nodesToReadd.Dequeue();

                Count--;

                Add(nodeToReadd.Point, nodeToReadd.Value);
            }
        }

        public void RemoveAt(TSVector2 point)
        {
            if (root == null)
                return;
            TSVector2KdNode<TValue> node;
            if (typeMath.AreEqual(point, root.Point))
            {
                node = root;
                root = null;
                Count--;
                ReaddChildNodes(node);
                return;
            }

            node = root;
            int dimension = -1;
            do
            {
                dimension = (dimension + 1) % dimensions;
                int compare = typeMath.Compare(point[dimension], node.Point[dimension]);

                if (node[compare] == null)
                    return;
                if (typeMath.AreEqual(point, node[compare].Point))
                {
                    var nodeToRemove = node[compare];
                    node[compare] = null;
                    Count--;

                    ReaddChildNodes(nodeToRemove);
                }
                else
                    node = node[compare];
            }
            while (node != null);
        }

        public TSVector2KdNode<TValue>[] GetNearestNeighbours(TSVector2 point, int count, Func<TValue, bool> fliter = null)
        {
            if (count > Count)
                count = Count;

            if (count < 0)
                throw new ArgumentException("Number of neighbors cannot be negative");

            if (count == 0)
                return new TSVector2KdNode<TValue>[0];

            NearestNeighbourList<TSVector2KdNode<TValue>, FP> nearestNeighbours;

            if (fliter == null)
                nearestNeighbours = new NearestNeighbourList<TSVector2KdNode<TValue>, FP>(count, typeMath);
            else
                nearestNeighbours = new NearestNeighbourList<TSVector2KdNode<TValue>, FP>(typeMath);

            var rect = TSVector2Rect.Infinite(dimensions, typeMath);

            AddNearestNeighbours(root, point, rect, 0, nearestNeighbours, typeMath.MaxValue);
            count = nearestNeighbours.Count < count ? nearestNeighbours.Count : count;

            var neighbourArray = new TSVector2KdNode<TValue>[nearestNeighbours.Count];
            var arrLength = neighbourArray.Length;

            var added = 0;
            for (int index = 0; index < arrLength; index++)
            {
                var node = nearestNeighbours.RemoveFurtherest();
                if (fliter != null)
                {
                    if (fliter(node.Value))
                    {
                        neighbourArray[arrLength - added - 1] = node;
                        added++;
                    }
                }
                else
                {
                    neighbourArray[arrLength - added - 1] = node;
                    added++;
                }
            }

            if (added < arrLength)
            {
                var result = new TSVector2KdNode<TValue>[added];
                if (added > 0)
                    Array.Copy(neighbourArray, (arrLength - added), result, 0, added);
                return result;
            }
            else
                return neighbourArray;
        }

        private void AddNearestNeighbours(TSVector2KdNode<TValue> node, TSVector2 target, TSVector2Rect rect,
            int depth, NearestNeighbourList<TSVector2KdNode<TValue>, FP> nearestNeighbours, FP maxSearchRadiusSquared)
        {
            if (node == null) return;

            int dimension = depth % dimensions;

            var leftRect = rect.Clone();
            leftRect.MaxPoint[dimension] = node.Point[dimension];

            var rightRect = rect.Clone();
            rightRect.MinPoint[dimension] = node.Point[dimension];

            int compare = typeMath.Compare(target[dimension], node.Point[dimension]);

            var nearerRect = compare <= 0 ? leftRect : rightRect;
            var furtherRect = compare <= 0 ? rightRect : leftRect;

            var nearerNode = compare <= 0 ? node.LeftChild : node.RightChild;
            var furtherNode = compare <= 0 ? node.RightChild : node.LeftChild;

            if(nearerNode != null)
            {
                AddNearestNeighbours(nearerNode, target, nearerRect, depth + 1, nearestNeighbours, maxSearchRadiusSquared);
            }

            FP distanceSquaredToTarget;

            TSVector2 closestPointInFurtherRect = furtherRect.GetClosestPoint(target, typeMath);
            distanceSquaredToTarget = typeMath.DistanceSquaredBetweenPoints(closestPointInFurtherRect, target);

            if(typeMath.Compare(distanceSquaredToTarget,maxSearchRadiusSquared)<0)
            {
                if(nearestNeighbours.IsCapacityReached)
                {
                    if (typeMath.Compare(distanceSquaredToTarget, nearestNeighbours.GetFurtherestDistance()) < 0)
                        AddNearestNeighbours(furtherNode, target, furtherRect, depth + 1, nearestNeighbours, maxSearchRadiusSquared); 
                }
                else
                {
                    AddNearestNeighbours(furtherNode, target, furtherRect, depth + 1, nearestNeighbours, maxSearchRadiusSquared);
                }
            }

            distanceSquaredToTarget = typeMath.DistanceSquaredBetweenPoints(node.Point, target);

            if (typeMath.Compare(distanceSquaredToTarget, maxSearchRadiusSquared) <= 0)
                nearestNeighbours.Add(node, distanceSquaredToTarget);
        }

        public TSVector2KdNode<TValue>[] RadialSearch(TSVector2 center,FP radius)
        {
            var nearestNeighbours = new NearestNeighbourList<TSVector2KdNode<TValue>, FP>(typeMath);
            return RadialSearch(center, radius, nearestNeighbours);
        }

        public TSVector2KdNode<TValue>[] RadialSearch(TSVector2 center,FP radius,int count)
        {
            var nearestNeighbours = new NearestNeighbourList<TSVector2KdNode<TValue>, FP>(count, typeMath);
            return RadialSearch(center, radius, nearestNeighbours);
        }

        private TSVector2KdNode<TValue>[] RadialSearch(TSVector2 center,FP radius,NearestNeighbourList<TSVector2KdNode<TValue>,FP> nearestNeighbours)
        {
            AddNearestNeighbours(root, center, TSVector2Rect.Infinite(dimensions, typeMath), 0, nearestNeighbours, typeMath.Multiply(radius, radius));

            var count = nearestNeighbours.Count;

            var neighbourArray = new TSVector2KdNode<TValue>[count];

            for (int index = 0; index < count; index++)
            {
                neighbourArray[count - index - 1] = nearestNeighbours.RemoveFurtherest();
            }

            return neighbourArray;
        }

        public bool TryFindValueAt(TSVector2 point,out TValue value)
        {
            var parent = root;
            int dimension = -1;

            do
            {
                if(parent == null)
                {
                    value = default(TValue);
                    return false;
                }
                else if(typeMath.AreEqual(point,parent.Point))
                {
                    value = parent.Value;
                    return true;
                }

                dimension = (dimension + 1) % dimensions;
                int compare = typeMath.Compare(point[dimension], parent.Point[dimension]);
                parent = parent[compare];
            } while (true);
        }

        public TValue FindValueAt(TSVector2 point)
        {
            if (TryFindValueAt(point, out TValue value))
                return value;
            else
                return default(TValue);
        }

        public bool TryFindValue(TValue value,out TSVector2 point)
        {
            if (root == null) {
                point = TSVector2.zero;
                return false;
            }

            var nodesToSearch = new Queue<TSVector2KdNode<TValue>>();

            nodesToSearch.Enqueue(root);

            while (nodesToSearch.Count>0)
            {
                var nodeToSearch = nodesToSearch.Dequeue();

                if(nodeToSearch.Value.Equals(value))
                {
                    point = nodeToSearch.Point;
                    return true;
                }
                else
                {
                    for (int side = -1; side <= 1; side+=2)
                    {
                        var childNode = nodeToSearch[side];
                        if (childNode != null)
                            nodesToSearch.Enqueue(childNode);
                    }
                }
            }
            point = TSVector2.zero;
            return false;
        }

        public TSVector2 FindValue(TValue value)
        {
            if (TryFindValue(value, out TSVector2 point))
                return point;
            else
                return TSVector2.zero;
        }

        private void AddNodeToStringBuilder(TSVector2KdNode<TValue> node,StringBuilder sb,int depth)
        {
            sb.AppendLine(node.ToString());

            for (int side = -1; side <= 1; side+=2)
            {
                for (int index = 0; index <= depth; index++)
                    sb.Append("\t");
                sb.Append(side == -1 ? "L" : "R");

                if (node[side] == null)
                    sb.AppendLine("");
                else
                    AddNodeToStringBuilder(node[side], sb, depth + 1);
            }
        }

        public override string ToString()
        {
            if (root == null) return "";
            var sb = new StringBuilder();
            AddNodeToStringBuilder(root, sb, 0);
            return sb.ToString();
        }


        private void AddNodesToList(TSVector2KdNode<TValue> node,List<TSVector2KdNode<TValue>> nodes)
        {
            if (node == null) return;
            nodes.Add(node);
            for (int side = -1; side <= 1; side+=2)
            {
                if(node[side] != null)
                {
                    AddNodesToList(node[side], nodes);
                    node[side] = null;
                }
            }
        }

        private void SortNodesArray(TSVector2KdNode<TValue>[] nodes,int byDimension,int fromIndex,int toIndex)
        {
            for (int index = fromIndex; index <= toIndex; index++)
            {
                var newIndex = index;
                while (true)
                {
                    var a = nodes[newIndex - 1];
                    var b = nodes[newIndex];
                    if (typeMath.Compare(b.Point[byDimension], a.Point[byDimension]) < 0)
                    {
                        nodes[newIndex - 1] = b;
                        nodes[newIndex] = a;
                    }
                    else
                        break;
                }
            }
        }

        private void AddNodesBalanced(TSVector2KdNode<TValue>[] nodes,int byDimension,int fromIndex,int toIndex)
        {
            if(fromIndex == toIndex)
            {
                Add(nodes[fromIndex].Point, nodes[fromIndex].Value);
                nodes[fromIndex] = null;
                return;
            }

            SortNodesArray(nodes, byDimension, fromIndex, toIndex);

            int midIndex = fromIndex + (int)System.Math.Round((toIndex + 1 - fromIndex) / 2f) - 1;

            Add(nodes[midIndex].Point, nodes[midIndex].Value);
            nodes[midIndex] = null;

            int nextDimension = (byDimension + 1) % byDimension;

            if (fromIndex < midIndex)
                AddNodesBalanced(nodes, nextDimension, fromIndex, midIndex - 1);
            if (toIndex > midIndex)
                AddNodesBalanced(nodes, nextDimension, midIndex + 1, toIndex);
        }

        public void Balance()
        {
            var nodeList = new List<TSVector2KdNode<TValue>>();
            AddNodesToList(root, nodeList);

            Clear();
            AddNodesBalanced(nodeList.ToArray(), 0, 0, nodeList.Count - 1);
        }

        private void RemoveChildNodes(TSVector2KdNode<TValue> node)
        {
            for (int side = -1; side <= 1; side+=2)
            {
                if(node[side] != null)
                {
                    RemoveChildNodes(node[side]);
                    Free(node[side]);
                    node[side] = null;
                }
            }
        }

        public void Clear()
        {
            if (root != null)
                RemoveChildNodes(root);
        }


        public void SaveToFile(string fileName)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using(FileStream stream = File.Create(fileName))
            {
                formatter.Serialize(stream, this);
                stream.Flush();
            }
        }

        public static TSVector2KdNode<TValue> LoadFromFile(string fileName)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using(FileStream stream = File.Open(fileName,FileMode.Open))
            {
                return (TSVector2KdNode<TValue>)formatter.Deserialize(stream);
            }    
        }

        public IEnumerator<TSVector2KdNode<TValue>> GetEnumerator()
        {
            var left = new Stack<TSVector2KdNode<TValue>>();
            var right = new Stack<TSVector2KdNode<TValue>>();

            void addLeft(TSVector2KdNode<TValue> node)
            {
                if (node.LeftChild != null)
                    left.Push(node.LeftChild);
            }

            void addRight(TSVector2KdNode<TValue> node)
            {
                if (node.RightChild != null)
                    right.Push(node.RightChild);
            }

            if(root != null)
            {
                yield return root;

                addLeft(root);
                addRight(root);

                while (true)
                {
                    if (left.Any())
                    {
                        var item = left.Pop();

                        addLeft(item);
                        addRight(item);

                        yield return item;
                    }
                    else if (right.Any())
                    {
                        var item = right.Pop();

                        addLeft(item);
                        addRight(item);

                        yield return item;
                    }
                    else
                        break;
                }
            }

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
