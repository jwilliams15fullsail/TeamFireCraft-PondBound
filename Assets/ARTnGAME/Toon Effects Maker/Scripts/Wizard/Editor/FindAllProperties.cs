using UnityEngine;
using UnityEditor;
using System.Collections;

public class FindAllProperties : Editor
{
	[MenuItem("Window/Find All Object Properties ")]
	
	static void Init ()
	{
		Component[] allComponent;
		allComponent = Selection.activeGameObject.GetComponents<Component>();
		
		foreach(Component go in allComponent)
		{
			SerializedObject m_Object = new SerializedObject(go);
			Debug.Log ("--------"+ go.GetType() +"-------");
			try
			{
				SerializedProperty obj = m_Object.GetIterator();
				
				foreach(SerializedProperty property in obj)
				{
					Debug.Log(property.name + " : " + property.propertyType);
				}
			}
			catch(System.Exception e)
			{
				Debug.Log (e.Message);
			}
		}
	}
}