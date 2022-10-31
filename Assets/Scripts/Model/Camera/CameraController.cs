using System;
using System.Collections.Generic;
using Bepop.Core;
using UnityEngine;

namespace CameraCtr
{
    public enum CameraType
    {
        /// <summary>
        /// 透视
        /// </summary>
        Perspective,
        /// <summary>
        /// 正焦
        /// </summary>
        Orthographic,
    }
    public class Test1 {
        public string name;
        public int age;

        public Test1(string name,int age)
        {
            this.name = name;
            this.age = age;
        }
    }
    /// <summary>
    /// 相机管理
    /// </summary>
    public class CameraController : MonoSingleton<CameraController>
    {
        private CameraBase tempCamera;
        private void Start()
        {
            tempCamera = new CameraBase();
            tempCamera.cameraType = CameraType.Orthographic;
            tempCamera.Init(Camera.main, new Vector2(1136, 640), 100, 100);
        }

        private void Update()
        {
            
        }

        public void InitCamera()
        {

        }

        public void RemoveCamera()
        {

        }
    }
}
