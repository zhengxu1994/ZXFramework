### Odin插件学习记录

- OdinEditorWindow 继承这个类可以创建一个自定义的window窗口

  ```C#
  #if UNITY_EDITOR
  public class SkillEditorWindow : OdinEditorWindow
  {
      [MenuItem("Tools/SkillEditor")]
      private static void OpenWindow()
      {
          var window = GetWindow<SkillEditorWindow>();
  
          // Nifty little trick to quickly position the window in the middle of the editor.
          window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1136, 640);
      }
  }
  #endif
  ```

  

- FilePath 可以将项目的资源拖入或者打开选择 读取资源的路径。

  ```c#
  [Sirenix.OdinInspector.FilePath]
  public string skillDataPath;
  ```