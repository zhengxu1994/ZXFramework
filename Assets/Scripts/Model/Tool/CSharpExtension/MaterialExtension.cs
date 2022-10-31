

using UnityEngine;

namespace Bepop.Core
{
    public static class MaterialExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        public static void SetStandardMaterialToTransparentMode(this Material self)
        {
            self.SetFloat("_Mode", 3);
            self.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            self.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            self.SetInt("_ZWrite", 0);
            self.DisableKeyword("_ALPHATEST_ON");
            self.EnableKeyword("_ALPHABLEND_ON");
            self.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            self.renderQueue = 3000;
        }
    }
}