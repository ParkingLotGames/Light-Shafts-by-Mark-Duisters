using UnityEditor;
using UnityEngine;

public class Menu_Button : MonoBehaviour {

		
	[MenuItem("GameObject/Light/LightShafts_2")]
		private static void NewMenuOption(){
		GameObject LightShaftGenerator = Instantiate( Resources.Load("LightShaftGenerator" )as GameObject);
		LightShaftGenerator.name = "LightShaftGenerator";

		LightShaftGenerator.transform.position =SceneView.lastActiveSceneView.camera.transform.position;
		LightShaftGenerator.transform.rotation =SceneView.lastActiveSceneView.camera.transform.rotation;

		// Register root object for undo.
		Undo.RegisterCreatedObjectUndo (LightShaftGenerator, "Create object");
		}
}
