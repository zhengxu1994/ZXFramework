using System;
using UnityEngine;
using TrueSync;
namespace Movement
{
    public class MoveParty
    {
        public GameObject gameObject { get; set; }
        public Action onSetReached;
        TSVector2 _position;

        public TSVector2 position
        {
            get => _position;
            set
            {
                _position = value;
                if (gameObject != null)
                {
                    gameObject.transform.localPosition = new Vector3(
                        _position.x.AsFloat(),
                        _position.y.AsFloat(),
                        ((_position.y - _position.x * 0.0001f) * 0.0001f).AsFloat()
                        );
                }
            }
        }

        TSVector2 _target;
        public TSVector2 target
        {
            get => _target;
            set
            {
                if (_target != value && position != value)
                {
                    _target = value;
                    reached = false;
                    onSetReached?.Invoke();
                    var newDir = Calculater.GetDirByVector(_target - position);
                    if (newDir != dir || dir == -1)
                    {
                        dir = newDir;
                        dirChange?.Invoke(newDir);
                    }
                }
            }
        }

        public FP speed { get; set; } = 0;

        public FP delta { get; set; } = 1f / 30f;

        public bool enable { get; set; } = true;

        public bool reached { get; set; } = true;

        public int id { get; set; } = 0;

        int _dir = -1;

        public int dir
        {
            get => _dir;
            set
            {
                if (_dir != value)
                {
                    dirChange?.Invoke(value);
                }
                _dir = value;
            }
        }


        public Action<int> dirChange { get; set; }

        public MoveParty()
        {

        }

        public void UpdateStep()
        {
            if (!enable || reached || speed <= 0)
                return;

            var lastPos = target - position;
            var nextPos = position + TSVector2.ClampMagnitude(lastPos, speed * delta);
            //点乘判断两个向量的前后 小于0 代表在目标向量后
            if (TSVector2.Dot(lastPos, target - nextPos) <= 0)
            {
                position = target;
                reached = true;
                return;
            }
            else
                position = nextPos;
        }
    }
}