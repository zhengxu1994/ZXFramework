

using UnityEngine;

namespace Bepop.Core
{
    public static class TextureExtensions
    {
        public static Sprite CreateSprite(this Texture2D self)
        {
            return Sprite.Create(self, new Rect(0, 0, self.width, self.height), Vector2.one * 0.5f);
        }
    }
}