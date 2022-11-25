public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ constraint implement type
	// }} 

	// {{ AOT generic type
	//Bepop.Core.Singleton`1<System.Object>
	//System.Action`2<System.Object,System.Object>
	//System.Collections.Generic.Dictionary`2<System.Object,System.Object>
	//System.Collections.Generic.Dictionary`2<System.Int32,System.Object>
	//System.Collections.Generic.List`1<System.Object>
	// }}

	public void RefMethods()
	{
		// System.Void Bepop.Core.IEnumerableExtension::ForEach<System.Object,System.Object>(System.Collections.Generic.Dictionary`2<System.Object,System.Object>,System.Action`2<System.Object,System.Object>)
		// System.Object Bepop.Core.UI.GComponentExt::GetChildAt<System.Object>(FairyGUI.GComponent,System.Int32)
		// Bepop.Core.UI.PanelBase Bepop.Core.UI.UIManager::OpenPanel<System.Object>(Notifaction.NotifyParam)
		// System.Object[] System.Array::Empty<System.Object>()
		// System.Object UnityEngine.Component::GetComponent<System.Object>()
		// System.Object UnityEngine.GameObject::AddComponent<System.Object>()
	}
}