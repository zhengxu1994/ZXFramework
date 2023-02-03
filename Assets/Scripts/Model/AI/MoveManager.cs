using System.Collections;
using System.Collections.Generic;
using Bepop.Core;
using TrueSync;
using RVO;
namespace MyAi
{
    public enum Dir
    {
        Down,
        Down_Left,
        Left,
        Left_Up,
        Up,
        Up_Right,
        Right,
        Right_Down
    }
    public class MoveManager : Singleton<MoveManager>
    {
        public static TSVector2 MoveInvaild = new TSVector2(-1, -1);
        private MoveManager() { }

        private FP CellR;

        private bool _enable;

        private FP groupRadius;

        public int width;
        public int height;

        public bool enable
        {
            get => _enable;
            set
            {
                if (_enable != value)
                {
                    _enable = value;
                    if (_enable)
                    {
                        //����rvo updateʱ�� 1����30��
                        RVO.Simulator.Instance.setTimeStep(1f / 30f);
                        // ����rvo���������� ��Χ ����ھ����� 
                        RVO.Simulator.Instance.setAgentDefaults(groupRadius, 10, 5, 5f, 0.5f, 0.2f, new TSVector2(0, 0));
                    }
                    else
                    {
                        //���rvoѰ·��Ϣ
                        RVO.Simulator.Instance.Clear();
                    }
                }
            }
        }
        public void Init(HashSet<(int, int)> blocks,FP cellR,FP groupRadius)
        {
            this.groupRadius = groupRadius;
            this.CellR = cellR;
            enable = true;
            SetBlocks(blocks);
        }

        /// <summary>
        /// ��ʼ���赲����Ϣ
        /// </summary>
        /// <param name="blocks"></param>
        private void SetBlocks(HashSet<(int,int)>blocks)
        {
            foreach (var block in blocks)
            {
                IList<TSVector2> obstacle = new List<TSVector2>();
                //�赲��Ϊ������ ��ȡ�ĸ���������
                var center = CellToPos(block.Item1, block.Item2);
                FP minX = center.x - CellR;
                FP minY = center.y - CellR;
                FP maxX = center.x + CellR;
                FP maxY = center.y + CellR;
                obstacle.Add(new TSVector2(minX, minY));
                obstacle.Add(new TSVector2(minX, maxY));
                obstacle.Add(new TSVector2(maxX, maxY));
                obstacle.Add(new TSVector2(minX, maxY));
                Simulator.Instance.addObstacle(obstacle);
            }
            // �����赲����Ϣ
            RVO.Simulator.Instance.processObstacles();
        }

        public TSVector2 CellToPos(int x, int y)
        {
            // *2 ����Ϊ�ҰѸ��ӷŴ���һ��
            return new TSVector2(x * CellR * 2, y * CellR *  2);
        }

        public (int,int) PosToCell(TSVector2 pos)
        {
            int x = (int)((pos.x - (CellR)) / CellR / 2) + 1;
            int y = (int)((pos.y - (CellR)) / CellR / 2) + 1;
            if (x < 0 || y < 0 || x >= width || y >= height)
                return (-1, -1);
            return (x, y);
        }

        public (int, int)[] offset = new (int, int)[] {
         (0,0),(1,0),(1,1),(1,-1),(0,1),(0,-1),(-1,1),(-1,0),(-1,-1)
        };
        public TSVector2 GetOffsetTest(TSVector2 pos,int index)
        {
            if (index >= 9)
                return TSVector2.zero;
            else
                return new TSVector2(pos.x + offset[index].Item1 * CellR * 2, pos.y + offset[index].Item2 * CellR * 2);
        }
    }

}