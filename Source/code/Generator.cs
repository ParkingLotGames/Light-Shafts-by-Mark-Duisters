using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEditor;
#endif

[ExecuteInEditMode]
public class Generator : MonoBehaviour
{

	/// local variables for the generation/instantiation: rows or vertices to cast from

	public Vector2Int castRows = new Vector2Int (10, 10); //each vector equals the number of shafts in that axis.
	public float shaftSpacing = 0.2f; //distance between each shafts position.
	public bool meshCast = false; //toggle between castingmode.
	public Mesh castMesh; //each vertex point will cast a shaft.

	public GameObject lightShaftPrefab; //will be hidden for the user.
	public string savePath; //will be hidden for the user.
	public bool shaftSettings; //will be hidden for the user. container for the editor.

	public Material shaftMat; //helper variable to acces shared materials.

	//Pointer variables to which Shaft_behavior syncs its variables
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	[ColorUsageAttribute (true, true)]
	public Color pointer_shaftColor = new Color (255, 197, 96, 255); //sets the shader's color.
	public float pointer_shaftIntensity = 0.25f; //sets the shader's intensity multiplier.

	public Vector2 pointer_noiseDirection = new Vector2 (0.05f, 0f); //sets the shader's noise direction.
	public float pointer_noiseScale = 1f; //sets the shader's noise scale.
	public float pointer_noiseUpdateSpeed = 1f; //sets the shader's noise update rate.

	//	public	Mesh pointer_shaftMesh;//users can set custom meshes to be used as shafts
	public float pointer_maxLength = 50.0f; //used in the raycast, if nothing is hit use this as the shafts maximum length.
	public LayerMask pointer_layerMask = 0; //filter against what the raycast can collide to determine length.

	public float pointer_shaftWidth = 0.71f; //set the shat's width
	public Vector2 pointer_shaftWidthMinMax = new Vector2 (0f, 71f); // maps the set _shaftWidth to a min max range.

	//public float pointer_shaftAdjustY = 1.0f; //This value is no longer used but will stay in the generator code for now to support save file backwards compatibility.
	public Texture pointer_shaftTexture; //this texture will be placed in the material.

	public Transform pointer_shaftDirection; //will be used to orient the shafts towards a point in space (lightsource or gameObjects). A standard direction object will be present as a child in the generator.

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// 

#if UNITY_EDITOR
	// Generate either rows or vertices to cast shafts.
	[ContextMenu ("GenerateShafts")]
	public void GenerateShafts ()
	{
		//when recasting always delete the old shafts first.
		DeleteShafts ();
		//re-create the parent.
		GameObject parent = new GameObject ("shaft_Parent");
		parent.transform.position = transform.position;
		parent.transform.rotation = transform.rotation;
		parent.transform.parent = transform;

		if (!meshCast)
		{
			//nested for loop will cast a shaft based on row numbers
			for (int x = 0; x <= castRows.x - 1; x++)
			{
				for (int y = 0; y <= castRows.y - 1; y++)
				{
					GameObject tmpShaft = Instantiate (lightShaftPrefab);
					tmpShaft.transform.localPosition = transform.position;
					tmpShaft.transform.localRotation = transform.rotation;
					tmpShaft.transform.parent = parent.transform;
					tmpShaft.GetComponent<Renderer> ().sharedMaterial = shaftMat;
					tmpShaft.transform.localPosition = new Vector3 (tmpShaft.transform.localPosition.x + x * shaftSpacing, tmpShaft.transform.localPosition.y + y * shaftSpacing, 0);

				}
			}

		}
		//when a mesh is given, cast a shaft for every vertex position
		else
		{
			for (int v = 0; v <= castMesh.vertexCount - 1; v++)
			{
				GameObject tmpShaft = Instantiate (lightShaftPrefab);
				tmpShaft.transform.localPosition = parent.transform.position;
				tmpShaft.transform.localRotation = parent.transform.rotation;
				tmpShaft.transform.parent = parent.transform;
				tmpShaft.GetComponent<Renderer> ().sharedMaterial = shaftMat;
				tmpShaft.transform.localPosition = new Vector3 (castMesh.vertices[v].x * shaftSpacing, castMesh.vertices[v].y * shaftSpacing, castMesh.vertices[v].z * shaftSpacing);
			}
		}

	}

	//delete all childs/shafts except for the DirectionPointer
	[ContextMenu ("DeleteShafts")]
	public void DeleteShafts ()
	{
		for (int c = 0; c <= transform.childCount - 1; c++)
		{
			Transform child = transform.GetChild (c);
			if (child.name.Contains ("Parent"))
			{
				DestroyImmediate (child.gameObject);

			}
		}
	}

	//Bake all casted shild meshes to a single mesh.
	[ContextMenu ("BakeShafts")]
	public void BakeShafts ()
	{

		//search for the shaft_parent
		foreach (Transform shaftParent in transform)
		{
			if (shaftParent.name == "shaft_Parent")
			{
				//return if the parent has no children. No need to bake.
				if (shaftParent.childCount <= 0)
				{
					return;
				}
				//save original child(parent) position
				Vector3 shaftParentOldPos = shaftParent.transform.position;
				Quaternion shaftParentOldRot = shaftParent.transform.rotation;

				//set it to world 0 for now
				shaftParent.position = Vector3.zero;
				shaftParent.rotation = Quaternion.identity;

				//collect all shafts that are  a child of this parent
				MeshFilter[] meshFilters = shaftParent.GetComponentsInChildren<MeshFilter> ();
				//create a combine instance in which to merge all shafts
				CombineInstance[] combine = new CombineInstance[meshFilters.Length];
				for (int i = 0; i <= meshFilters.Length - 1; i++)
				{
					combine[i].mesh = meshFilters[i].sharedMesh;
					combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
					DestroyImmediate (meshFilters[i].gameObject);
				}

				//if no mesh filter is present add that mesh filter
				if (shaftParent.GetComponent<MeshFilter> () == null)
				{
					shaftParent.gameObject.AddComponent<MeshFilter> ();
				}
				if (shaftParent.GetComponent<MeshRenderer> () == null)
				{
					shaftParent.gameObject.AddComponent<MeshRenderer> ();
				}

				shaftParent.GetComponent<MeshFilter> ().sharedMesh = new Mesh ();
				shaftParent.GetComponent<MeshFilter> ().sharedMesh.CombineMeshes (combine);
				shaftParent.GetComponent<Renderer> ().sharedMaterial = new Material (shaftMat); //when baking create a unique new material for the mesh.
				shaftParent.gameObject.SetActive (true);

				shaftParent.position = shaftParentOldPos;
				shaftParent.rotation = shaftParentOldRot;

			}
		}
	}

	//save current generator settings to a file
	[ContextMenu ("SavePreset")]
	public void SavePreset ()
	{
		//let the user select a save path and name
		savePath = EditorUtility.SaveFilePanel ("Load LightShaft preset", Application.dataPath + "//LightShafts//_Presets", transform.name + "_Preset", "alsp2");

		//if path is empty, save to default
		if (string.IsNullOrEmpty (savePath))
		{
			savePath = Application.dataPath + "//LightShafts//_Presets";
		}

		//Create a formatter.
		//create a variable containing our close to copy values to.
		SaveVariables classToSave = new SaveVariables ();

		//we can use a binary or xml formatter, swap quotes of which to use.
		BinaryFormatter saveToDisk = new BinaryFormatter ();
		//XmlSerializer saveToDisk = new XmlSerializer(typeof(SaveVariables));

		//create the file selected by player on said location and open it for the stream.
		FileStream saveFile = File.Create (savePath);

		////Pointers
		//split the color variable into sperate floats for saving.
		classToSave.pointer_shaftColorR = pointer_shaftColor.r;
		classToSave.pointer_shaftColorG = pointer_shaftColor.g;
		classToSave.pointer_shaftColorB = pointer_shaftColor.b;
		classToSave.pointer_shaftColorA = pointer_shaftColor.a;

		classToSave.pointer_shaftIntensity = pointer_shaftIntensity;

		classToSave.pointer_noiseDirectionX = pointer_noiseDirection.x;
		classToSave.pointer_noiseDirectionX = pointer_noiseDirection.y;
		classToSave.pointer_noiseScale = pointer_noiseScale;
		classToSave.pointer_noiseUpdateSpeed = pointer_noiseUpdateSpeed;

		//SaveVariables.pointer_shaftMesh = pointer_shaftMesh; 
		classToSave.pointer_maxLength = pointer_maxLength;

		classToSave.pointer_shaftWidth = pointer_shaftWidth;
		classToSave.pointer_shaftWidthMinMaxX = pointer_shaftWidthMinMax.x;
		classToSave.pointer_shaftWidthMinMaxY = pointer_shaftWidthMinMax.y;

		//classToSave.pointer_shaftAdjustY = pointer_shaftAdjustY;//This value is no longer used but will stay in the generator code for now to support save file backwards compatibility.

		classToSave.pointer_shaftDirectionX = pointer_shaftDirection.localPosition.x;
		classToSave.pointer_shaftDirectionY = pointer_shaftDirection.localPosition.y;
		classToSave.pointer_shaftDirectionZ = pointer_shaftDirection.localPosition.z;

		//Local generator variables
		//split the castRows vector2 into seperate float values.
		classToSave.castRowsX = castRows.x;
		classToSave.castRowsY = castRows.y;

		classToSave.shaftSpacing = shaftSpacing;
		classToSave.meshCast = meshCast;

		//Finaly write the class away to binary .
		saveToDisk.Serialize (saveFile, classToSave);
		//when all is done close the file
		saveFile.Close ();

		Debug.Log ("saved to: " + savePath);

	}

	//load selected file to settings
	[ContextMenu ("LoadPreset")]
	public void LoadPreset ()
	{
		//let the user select a file to load
		savePath = EditorUtility.OpenFilePanel ("Load LightShaft preset", Application.dataPath + "//LightShafts//_Presets", "alsp2");

		//make a formatter to read the binary file
		BinaryFormatter loadFromDisk = new BinaryFormatter ();
		//Open te user selected file
		FileStream loadFile = File.Open (savePath, FileMode.Open);

		//create a class from the SaveVariables class to store the loaded data.
		SaveVariables classToLoad = new SaveVariables ();
		//load the class
		classToLoad = (SaveVariables) loadFromDisk.Deserialize (loadFile);

		////Pointers

		pointer_shaftColor = new Color (classToLoad.pointer_shaftColorR, classToLoad.pointer_shaftColorG, classToLoad.pointer_shaftColorB, classToLoad.pointer_shaftColorA);
		pointer_shaftIntensity = classToLoad.pointer_shaftIntensity;
		pointer_noiseDirection = new Vector2 (classToLoad.pointer_noiseDirectionX, classToLoad.pointer_noiseDirectionY);
		pointer_noiseScale = classToLoad.pointer_noiseScale;
		pointer_noiseUpdateSpeed = classToLoad.pointer_noiseUpdateSpeed;

		pointer_maxLength = classToLoad.pointer_maxLength;

		pointer_shaftWidth = classToLoad.pointer_shaftWidth;
		pointer_shaftWidthMinMax = new Vector2 (classToLoad.pointer_shaftWidthMinMaxX, classToLoad.pointer_shaftWidthMinMaxY);

		//pointer_shaftAdjustY = classToLoad.pointer_shaftAdjustY;//This value is no longer used but will stay in the generator code for now to support save file backwards compatibility.

		//merge the shaftdirection local position back from the split floats
		pointer_shaftDirection.localPosition = new Vector3 (classToLoad.pointer_shaftDirectionX, classToLoad.pointer_shaftDirectionY, classToLoad.pointer_shaftDirectionZ);

		//Local generator variables
		//merge the split castrow floats back to a vector2 on load
		castRows = new Vector2Int (classToLoad.castRowsX, classToLoad.castRowsY);

		shaftSpacing = classToLoad.shaftSpacing;
		meshCast = classToLoad.meshCast;

		//when done loading the file close it.
		loadFile.Close ();

		GenerateShafts ();
		Debug.Log ("Loaded from: " + savePath);
	}

	public string[] GetLayerList ()
	{

		string[] layerNames = new string[31];
		//user defined layers start with layer 8 and unity supports 31 layers
		for (int i = 0; i <= 31 - 1; i++)
		{

			//get the name of the layer
			string layerN = LayerMask.LayerToName (i);

			//only add the layer if it has been named 
			// if(layerN.Length>0) 
			layerNames[i] = (layerN.ToString ());
		}

		return layerNames;

	}

	void OnDrawGizmos ()
	{

		if (Directory.Exists (Application.dataPath + "/Gizmos"))
		{

			if (File.Exists (Application.dataPath + "/Gizmos/LightShaftGizmo.tif"))
			{

				Gizmos.DrawIcon (transform.position, "LightShaftGizmo", true);

			}
			else
			{
				File.Copy (Application.dataPath + "/LightShafts/Source/Resources/UI/LightShaftGizmo.tif", Application.dataPath + "/Gizmos/LightShaftGizmo.tif");
			}

			if (!File.Exists (Application.dataPath + "/Gizmos/AimIcon_tmp.png"))
			{

				File.Copy (Application.dataPath + "/LightShafts/Source/Resources/UI/AimIcon_tmp.png", Application.dataPath + "/Gizmos/AimIcon_tmp.png");
			}

		}

		else
		{
			Directory.CreateDirectory (Application.dataPath + "/Gizmos");
		}

	}

#endif

}

//this class contains a copy of all variables in this scrip, so that they can be saved to a file. Comment out variables are not serializable outside of Unity.
[System.Serializable]
public class SaveVariables
{

	//Pointer variables to which Shaft_behavior syncs its variables
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public float pointer_shaftIntensity; //sets the shader's intensity multiplier
	//public Color pointer_shaftColor;//sets the shader's color. Gets split into seperate float channels.
	public float pointer_shaftColorR;
	public float pointer_shaftColorG;
	public float pointer_shaftColorB;
	public float pointer_shaftColorA;

	public float pointer_noiseDirectionX; //sets the shader's noise direction.
	public float pointer_noiseDirectionY; //sets the shader's noise direction.

	public float pointer_noiseScale; //sets the shader's noise scale.
	public float pointer_noiseUpdateSpeed; //sets the shader's noise update rate.

	public float pointer_maxLength; //used in the raycast, if nothing is hit use this as the shafts maximum length.

	public float pointer_shaftWidth; //set the shaft's width
	public float pointer_shaftWidthMinMaxX;
	public float pointer_shaftWidthMinMaxY;

	//public float pointer_shaftAdjustY; //set the Ytiling of the material to change texture offset

	//split the pointer_shaftDirection position into floats
	public float pointer_shaftDirectionX;
	public float pointer_shaftDirectionY;
	public float pointer_shaftDirectionZ;

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// 
	/// local variables: rows to cast or vertices to cast from

	//public Vector2 castRows;// Will be split in two floats for each vector.
	public int castRowsX;
	public int castRowsY;

	public float shaftSpacing; //distance between each shafts position.
	public bool meshCast; //toggle between castingmode

}