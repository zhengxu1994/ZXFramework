using System;
using DG.Tweening;
using UnityEngine;

namespace CameraCtr
{
    public class CameraBase
    {
        public CameraType cameraType = CameraType.Perspective;

        private Camera camera;
        #region Shake Attr
        public bool IsShaking { get; private set; }

        public float shakeLevel { get; set; } = 3f;

        public float setShakeTime { get; private set; } = 0.2f;

        public float shakeFPS { get; private set; } = 45;

        private float fps;

        private float frameTime = 0.0f;
        private float shakeTime = 0.0f;
        private float shakeDelta = 0.0f;
        private Rect changeRect;
        #endregion

        public float areaWidth, areaHeight, widthHeightFactory;
        public float maxVerticalView, initVerticalView, minVerticalView;
        private float curVerticalView;
        private Tweener zoomTweener;

        public void Init(Camera camera,Vector2 areaSize,float initVertical,float minVerticalView)
        {
            this.camera = camera;
            areaWidth = areaSize.x;
            areaHeight = areaSize.y;
            widthHeightFactory = Screen.width / Screen.height;
            if (cameraType == CameraType.Perspective)
            {
                camera.orthographic = false;
                maxVerticalView = areaHeight;
            }
            else
            {
                if (widthHeightFactory > areaWidth / areaHeight)
                    maxVerticalView = areaWidth / widthHeightFactory;
                else
                    maxVerticalView = areaHeight;
                camera.orthographic = true;
            }
            initVertical = curVerticalView = ClampHeight(initVertical);
        }

        public float ClampHeight(float veritical)
        {
            return Mathf.Clamp(veritical, minVerticalView, maxVerticalView);
        }

        public virtual void MoveCamera()
        {

        }

        public void ZoomCamera(float endValue,float duration,Action cb = null)
        {
            endValue = ClampHeight(endValue);
            if(endValue == curVerticalView)
            {
                cb?.Invoke();
                return;
            }

            if(zoomTweener != null)
            {
                zoomTweener.Kill();
                zoomTweener.onComplete?.Invoke();
            }
            zoomTweener = camera.DOFieldOfView(endValue, duration);
            if (cb != null)
                zoomTweener.onComplete = ()=> { cb.Invoke(); };
        }

        public virtual void ShakeCamera()
        {
            shakeTime = setShakeTime;
            fps = shakeFPS;
            frameTime = 0.03f;
            shakeDelta = 0.005f;
            IsShaking = true;
        }

        void Update()
        {
            if(IsShaking)
            {
                if(shakeTime > 0)
                {
                    shakeTime -= Time.deltaTime;
                    if(shakeTime <= 0)
                    {
                        changeRect.xMin = 0;
                        changeRect.yMin = 0;
                        camera.rect = changeRect;
                        IsShaking = false;
                        shakeTime = setShakeTime;
                        fps = shakeFPS;
                        frameTime = 0.03f;
                        shakeDelta = 0.005f;
                    }
                    else
                    {
                        frameTime += Time.deltaTime;
                        if(frameTime > 1.0f/fps)
                        {
                            frameTime = 0;
                            changeRect.xMin = shakeDelta * (-1.0f + shakeLevel * UnityEngine.Random.value);
                            changeRect.yMin = shakeDelta * (-1.0f + shakeLevel * UnityEngine.Random.value);
                            camera.rect = changeRect;
                        }
                    }
                }
            }
        }

    }
}
