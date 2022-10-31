

using UnityEngine;

namespace Bepop.Core
{
    public static class LayerMaskUtility
    {
        public static bool IsInLayerMask(GameObject gameObj, LayerMask layerMask)
        {
            // 根据Layer数值进行移位获得用于运算的Mask值
            var objLayerMask = 1 << gameObj.layer;
            return (layerMask.value & objLayerMask) == objLayerMask;
        }
    }
}