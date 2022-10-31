using System;
namespace SceneMgr
{
    /// <summary>
    /// 工厂模式
    /// </summary>
    public class SceneFactory
    {
        public SceneBase GetScene(string sceneName)
        {
            SceneBase scene = null;
            switch (sceneName)
            {

                default:
                    scene = new SceneBase(sceneName);
                    break;
            }
            return scene;
        }
    }
}
