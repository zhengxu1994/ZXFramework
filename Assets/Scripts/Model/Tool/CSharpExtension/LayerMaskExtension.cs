

using UnityEngine;

namespace Bepop.Core
{
    public static class LayerMaskExtension
    {
        public static bool ContainsGameObject(this LayerMask selfLayerMask, GameObject gameObject)
        {
            return LayerMaskUtility.IsInLayerMask(gameObject, selfLayerMask);
        }
    }
}