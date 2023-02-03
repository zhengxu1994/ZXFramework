using System.Collections;
using TrueSync;
using UnityEngine;

namespace MyAi
{
    /// <summary>
    /// 个体实例
    /// </summary>
    public class UnitEntity  
    {
        public FP Radius;

        public virtual FP Speed { get; set; }

        private TrueSync.TSVector2 _Position;

        public virtual TrueSync.TSVector2 Position
        {
            get
            {
                return _Position;
            }
            set
            {
                _Position = value;
            }
        }

        private Dir _dir;

        public Dir Dir
        {
            get { return _dir; }
            set
            {
                _dir = value;
            }
        }

        public bool moveEnable = true;

        public GameObject obj;

        public MoveParty moveParty;
    }
}