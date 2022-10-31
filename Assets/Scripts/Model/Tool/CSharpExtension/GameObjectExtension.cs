

using System;
using UnityEngine;

namespace Bepop.Core
{
    /// <summary>
    /// GameObject's Util/Static This Extension
    /// </summary>
    public static class GameObjectExtension
    {
        public static void Example()
        {
            var gameObject = new GameObject();
            var transform = gameObject.transform;
            var selfScript = gameObject.AddComponent<MonoBehaviour>();
            var boxCollider = gameObject.AddComponent<BoxCollider>();

            gameObject.Show(); // gameObject.SetActive(true)
            selfScript.Show(); // this.gameObject.SetActive(true)
            boxCollider.Show(); // boxCollider.gameObject.SetActive(true)
            gameObject.transform.Show(); // transform.gameObject.SetActive(true)

            gameObject.Hide(); // gameObject.SetActive(false)
            selfScript.Hide(); // this.gameObject.SetActive(false)
            boxCollider.Hide(); // boxCollider.gameObject.SetActive(false)
            transform.Hide(); // transform.gameObject.SetActive(false)

            selfScript.DestroyGameObj();
            boxCollider.DestroyGameObj();
            transform.DestroyGameObj();

            selfScript.DestroyGameObjGracefully();
            boxCollider.DestroyGameObjGracefully();
            transform.DestroyGameObjGracefully();

            selfScript.DestroyGameObjAfterDelay(1.0f);
            boxCollider.DestroyGameObjAfterDelay(1.0f);
            transform.DestroyGameObjAfterDelay(1.0f);

            selfScript.DestroyGameObjAfterDelayGracefully(1.0f);
            boxCollider.DestroyGameObjAfterDelayGracefully(1.0f);
            transform.DestroyGameObjAfterDelayGracefully(1.0f);

            gameObject.Layer(0);
            selfScript.Layer(0);
            boxCollider.Layer(0);
            transform.Layer(0);

            gameObject.Layer("Default");
            selfScript.Layer("Default");
            boxCollider.Layer("Default");
            transform.Layer("Default");
        }

        #region CEGO001 Show

        public static GameObject Show(this GameObject selfObj)
        {
            selfObj.SetActive(true);
            return selfObj;
        }

        public static T Show<T>(this T selfComponent) where T : Component
        {
            selfComponent.gameObject.Show();
            return selfComponent;
        }

        #endregion

        #region CEGO002 Hide

        public static GameObject Hide(this GameObject selfObj)
        {
            selfObj.SetActive(false);
            return selfObj;
        }

        public static T Hide<T>(this T selfComponent) where T : Component
        {
            selfComponent.gameObject.Hide();
            return selfComponent;
        }

        #endregion

        #region CEGO003 DestroyGameObj

        public static void DestroyGameObj<T>(this T selfBehaviour) where T : Component
        {
            selfBehaviour.gameObject.DestroySelf();
        }

        #endregion

        #region CEGO004 DestroyGameObjGracefully

        public static void DestroyGameObjGracefully<T>(this T selfBehaviour) where T : Component
        {
            if (selfBehaviour && selfBehaviour.gameObject)
            {
                selfBehaviour.gameObject.DestroySelfGracefully();
            }
        }

        #endregion

        #region CEGO005 DestroyGameObjGracefully

        public static T DestroyGameObjAfterDelay<T>(this T selfBehaviour, float delay) where T : Component
        {
            selfBehaviour.gameObject.DestroySelfAfterDelay(delay);
            return selfBehaviour;
        }

        public static T DestroyGameObjAfterDelayGracefully<T>(this T selfBehaviour, float delay) where T : Component
        {
            if (selfBehaviour && selfBehaviour.gameObject)
            {
                selfBehaviour.gameObject.DestroySelfAfterDelay(delay);
            }

            return selfBehaviour;
        }

        #endregion

        #region CEGO006 Layer

        public static GameObject Layer(this GameObject selfObj, int layer)
        {
            selfObj.layer = layer;
            return selfObj;
        }

        public static T Layer<T>(this T selfComponent, int layer) where T : Component
        {
            selfComponent.gameObject.layer = layer;
            return selfComponent;
        }

        public static GameObject Layer(this GameObject selfObj, string layerName)
        {
            selfObj.layer = LayerMask.NameToLayer(layerName);
            return selfObj;
        }

        public static T Layer<T>(this T selfComponent, string layerName) where T : Component
        {
            selfComponent.gameObject.layer = LayerMask.NameToLayer(layerName);
            return selfComponent;
        }

        public static bool IsInLayerMask(this GameObject selfObj, LayerMask layerMask)
        {
            return LayerMaskUtility.IsInLayerMask(selfObj, layerMask);
        }
        
        public static bool IsInLayerMask<T>(this T selfComponent, LayerMask layerMask) where T : Component
        {
            return LayerMaskUtility.IsInLayerMask(selfComponent.gameObject, layerMask);
        }

        #endregion

        #region CEGO007 Component

        public static T GetOrAddComponent<T>(this GameObject selfComponent) where T : Component
        {
            var comp = selfComponent.gameObject.GetComponent<T>();
            return comp ? comp : selfComponent.gameObject.AddComponent<T>();
        }
        
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            return component.gameObject.GetOrAddComponent<T>();
        }

        public static Component GetOrAddComponent(this GameObject selfComponent, Type type)
        {
            var comp = selfComponent.gameObject.GetComponent(type);
            return comp ? comp : selfComponent.gameObject.AddComponent(type);
        }

        #endregion
    }
}