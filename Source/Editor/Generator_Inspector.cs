using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (Generator))]

public class Generator_Inspector : Editor
{

	Texture _save;
	Texture _load;
	Texture _generate;
	Texture _delete;
	Texture _bake;

	float guiScale = 1f;

	public override void OnInspectorGUI ()
	{
		//load the ui textures
		loadTextures ();

		//get selected object
		Generator selected = Selection.activeTransform.gameObject.GetComponent<Generator> ();

		GUILayout.MaxWidth (Screen.width * guiScale);

		EditorGUILayout.BeginHorizontal ();

		if (GUILayout.Button (new GUIContent (_save, "Save your current generator settings to a (.alsp2) file."), GUILayout.MaxWidth (Screen.width * guiScale)))
		{
			selected.SavePreset ();
		}

		if (GUILayout.Button (new GUIContent (_load, "Loads generator settings from a preset (.alsp2) file."), GUILayout.MaxWidth (Screen.width * guiScale)))
		{
			selected.LoadPreset ();
		}

		EditorGUILayout.EndHorizontal ();

		selected.meshCast = EditorGUILayout.Toggle (new GUIContent ("Enable mesh cast", "use a mesh(vertices) instead of rows to cast shafts"), selected.meshCast);
		if (selected.meshCast)
		{
			selected.castMesh = EditorGUILayout.ObjectField (new GUIContent ("Mesh", " The mesh itself will not be used. Each vertex represents a shaft cast location. Using smart vertex placement, one can create complex scenes. Such as light breaking through a forest."), selected.castMesh, typeof (Mesh), true) as Mesh;
		}
		else
		{
			selected.castRows = EditorGUILayout.Vector2IntField (new GUIContent ("Shaft rows", "Cast shafts along the axis."), selected.castRows);
		}
		selected.shaftSpacing = EditorGUILayout.FloatField (new GUIContent ("Shaft spacing", "Set distance between each shaft."), selected.shaftSpacing);

		selected.pointer_shaftTexture = EditorGUILayout.ObjectField (new GUIContent ("Shaft Texture", "Set the image texture used for each shaft. Custom textures should be oriented bottom, up. The upside will be casted towards the shafts end. For multi-colored shafts, such as stained glass, you can use colored textures (Example: LightShaft_wide_RainBow)"), selected.pointer_shaftTexture, typeof (Texture), true) as Texture;

		selected.shaftMat = EditorGUILayout.ObjectField (new GUIContent ("Material", "All systems use a shared material. If you want a system to use different colors, create a new material with the lightshaft shader, then place that material in this slot."), selected.shaftMat, typeof (Material), true) as Material;

		selected.shaftSettings = EditorGUILayout.Foldout (selected.shaftSettings, "Shaft settings");

		if (selected.shaftSettings)
		{
			selected.pointer_shaftColor = EditorGUILayout.ColorField (new GUIContent ("Shaft color", "Set the main color of the shafts."), selected.pointer_shaftColor, true, true, true);
			selected.pointer_shaftIntensity = EditorGUILayout.FloatField (new GUIContent ("Shaft intensity", "Set the intensity ontop of the shafts color."), selected.pointer_shaftIntensity);

			selected.pointer_noiseDirection = EditorGUILayout.Vector2Field (new GUIContent ("Noise direction", "Set the direction the noise should move along the shaft."), selected.pointer_noiseDirection);
			selected.pointer_noiseScale = EditorGUILayout.FloatField (new GUIContent ("Noise scale", "Set the scale of the noise and Voronoi nosie."), selected.pointer_noiseScale);
			selected.pointer_noiseUpdateSpeed = EditorGUILayout.FloatField (new GUIContent ("Noise update speed", "Set the update speed of the caustic Noise. This can be used to stop the variable light effect. Note that Noise scale is a multiplier ontop of this value, so setting the update speed to 0 will also freeze the noise direction."), selected.pointer_noiseUpdateSpeed);

			selected.pointer_maxLength = EditorGUILayout.FloatField (new GUIContent ("Max shaft length", " Max range for the raycast to detect collisions. If nothing is hit, this value will be the shafts length"), selected.pointer_maxLength);
			selected.pointer_layerMask = EditorGUILayout.MaskField (new GUIContent ("Hit layers", "Select the layers to which the shafts can collide."), selected.pointer_layerMask, selected.GetLayerList ());

			selected.pointer_shaftWidth = EditorGUILayout.FloatField (new GUIContent ("Shaft max width", "Set the width of each shaft."), selected.pointer_shaftWidth);
			EditorGUILayout.MinMaxSlider (new GUIContent ("Shaft width range", "Set the width variation for each shaft based on max width."), ref selected.pointer_shaftWidthMinMax.x, ref selected.pointer_shaftWidthMinMax.y, 0f, selected.pointer_shaftWidth);

			//selected.pointer_shaftAdjustY = EditorGUILayout.FloatField (new GUIContent ("Shaft uv offset", "Adjust the shafts texturing along its length."), selected.pointer_shaftAdjustY);

			selected.pointer_shaftDirection = EditorGUILayout.ObjectField (new GUIContent ("Shaft direction", " A transform is used to set the shaft direction and angle. Default is DirectionPointer, but anything such as a lightsource can be used."), selected.pointer_shaftDirection, typeof (Transform), true) as Transform;

		}

		EditorGUILayout.BeginHorizontal ();

		if (GUILayout.Button (new GUIContent (_generate, "Generate a set of shafts based on the above settings."), GUILayout.MaxWidth (Screen.width * guiScale)))
		{
			selected.GenerateShafts ();
		}

		if (GUILayout.Button (new GUIContent (_delete, "Delete the current shaft set (dynamic or baked). This happens automatically when generating a new set."), GUILayout.MaxWidth (Screen.width * guiScale)))
		{
			selected.DeleteShafts ();
		}

		EditorGUILayout.EndHorizontal ();

		if (GUILayout.Button (new GUIContent (_bake, "Bake the current shafts into a single mesh. This greatly improves performance at the cost of losing all dynamic properties. If users want shafts to behave dynamically simply do not bake and leave the system as is."), GUILayout.MaxWidth (Screen.width * guiScale)))
		{
			selected.BakeShafts ();
		}

		//	base.OnInspectorGUI ();

	}

	void loadTextures ()
	{
		if (_save == null)
		{
			_save = Resources.Load ("UI/save@0.5x", typeof (Texture)) as Texture;
		}
		if (_load == null)
		{
			_load = Resources.Load ("UI/load@0.5x", typeof (Texture)) as Texture;
		}
		if (_generate == null)
		{
			_generate = Resources.Load ("UI/generate@0.5x", typeof (Texture)) as Texture;
		}
		if (_delete == null)
		{
			_delete = Resources.Load ("UI/delete@0.5x", typeof (Texture)) as Texture;
		}
		if (_bake == null)
		{
			_bake = Resources.Load ("UI/bake@0.5x", typeof (Texture)) as Texture;
		}

	}

}