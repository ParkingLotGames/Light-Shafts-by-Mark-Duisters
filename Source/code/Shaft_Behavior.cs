using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This code will contain anything that has to do with "shaft settings" and or behavior. The older lightshafts performed most of these actions through a big for loop in the generator
//which was highly ineficient. LightShafts 2 moves all of these things into the shafts update loop and sets these settings through pointer variables in the "Generator"
//The generator itself will then as in older versions generator sets of shafts which can now be updated in realtime with the new aproach.
//The goal is to automate most of the settings in interactive/intuitive ways. Thus cleaning up the large and sometimes confusing setting list of previous versiosn.

//To do list: 

//Set custom shaft mesh: in development
//Raycast scaling with max cast distance: in development
//auto orient to point/lightsource: finished!
//UV offset to slide shaft texture: in development
//color settings to be fed to the shader: in development

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif

public class Shaft_Behavior : MonoBehaviour
{

	//all public variables will use Generator pointers as settings

	public Color shaftColor = new Color (255, 197, 96, 255); //sets the shader's color
	public float shaftIntensity = 22.0f; //sets the shader's intensity multiplier

	public Vector2 noiseDirection; //sets the shader's noise direction.
	public float noiseScale = 10f; //sets the shader's noise scale.
	public float noiseUpdateSpeed = 1f;

	//public	Mesh shaftMesh;//users can set custom meshes to be used as shafts
	public float maxLength = 50.0f; //used in the raycast, if nothing is hit use this as the shafts maximum length.
	public LayerMask layerMask = 1; //filter against what the raycast can collide to determine length.

	public Vector2 shaftWidthMinMax; // maps the set _shaftWidth to a min max range.

	public float shaftAdjustY = 1.0f; //set the Ytiling of the material to change texture offset
	public Texture shaftTexture; //this texture will be placed in the material.

	public Transform shaftDirection; //will be used to orient the shafts towards a point in space (lightsource or gameObjects). A standard direction object will be present as a child in the generator.

	Material shaftMat; //helper variable to acces shared materials.

	float randomRotZ; //this value is randomized once in the Start loop. This way we can have varations in shaft rotation without flipping out every frame.
	float randomWidth; //this value is randomized once in the Start loop. This way we can have varations in shaft width without flipping out every frame.

	// Use this for initialization
	void Start ()
	{

		//if there is no shaftDirection found set default vector
		if (!shaftDirection)
		{
			shaftDirection = transform;
		}

		//Randomize one time
		randomRotZ = Random.Range (-360f, 360f);

		//run every function once
		SyncVariables ();
		randomWidth = Random.Range (shaftWidthMinMax.x, shaftWidthMinMax.y);

		//use the shared material assignd from the generator.
		shaftMat = transform.GetComponent<Renderer> ().sharedMaterial;
		shaftTexture = transform.parent.parent.GetComponent<Generator> ().pointer_shaftTexture;
		shaftMat.mainTexture = shaftTexture;

		OrientShaft ();

		ShaftLength_and_Width ();

		Shaft_Color_Intensity_TexOffset ();

	}

	// Update is called once per frame
	//Only update the shafts when in editor mode (for interactivity between settings)
	void LateUpdate ()
	{

		SyncVariables ();

		OrientShaft ();

		ShaftLength_and_Width ();

		Shaft_Color_Intensity_TexOffset ();

	}

	//This function will set the shafts length based on a raycasthit.
	void ShaftLength_and_Width ()
	{
		//the width of the shaft will always be randomWidth no matter what.

		Vector3 direction = transform.forward;
		RaycastHit hit;

		//cast a ray to all colliders selected in the layermask and then adjust shaft scale to hit distance
		if (Physics.Raycast (transform.position, direction, out hit, maxLength, layerMask))
		{
			//if the hit distance is smaller than the maxLength set it, else use maxLength
			if (hit.distance <= maxLength)
			{
				transform.localScale = new Vector3 (randomWidth, transform.localScale.y, hit.distance);

			}
			else
			{
				transform.localScale = new Vector3 (randomWidth, transform.localScale.y, maxLength);
			}

		}
		//if no hit took place at all, also set the maxLength
		else
		{
			transform.localScale = new Vector3 (randomWidth, transform.localScale.y, maxLength);
		}

	}

	//This function will set all shader settings, such as color and tiling
	void Shaft_Color_Intensity_TexOffset ()
	{

		shaftMat.SetFloat ("_Intensity", shaftIntensity);
		shaftMat.SetVector ("_NoiseDirection", new Vector4 (noiseDirection.x, noiseDirection.y, 0f, 0f));
		shaftMat.SetFloat ("_Noise_Scale", noiseScale);
		shaftMat.SetFloat ("_Noise_UpdateSpeed", noiseUpdateSpeed);

		//limit the offset values
		/*	if (shaftAdjustY <= 0.85f)
		{
			shaftAdjustY = 0.85f;
		}

		shaftMat.SetTextureScale ("_MainTex", new Vector2 (1, shaftAdjustY));
*/
	}

	//This fucntion will orient the shaft based on a Transform
	void OrientShaft ()
	{

		//first get a vector of the position to orient at
		Vector3 lookPos = (2 * transform.position - shaftDirection.position);
		//orient shaft to calculated position
		transform.LookAt (lookPos);

		//We use Quaternion.Euler to tweak the z axis for randomisation

		transform.rotation = Quaternion.Euler (transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, randomRotZ);

	}

	//This fucntion contains all the variables and their pointer counter parts from Generator. This way each shaft will sync with Generator its settings

	void SyncVariables ()
	{

		Transform generator = transform.parent.parent;
		Generator getGenerator = generator.GetComponent<Generator> ();

		shaftColor = getGenerator.pointer_shaftColor; //sets the shader's color
		shaftIntensity = getGenerator.pointer_shaftIntensity; //sets the shader's intensity multiplier
		noiseDirection = getGenerator.pointer_noiseDirection;
		noiseScale = getGenerator.pointer_noiseScale;
		noiseUpdateSpeed = getGenerator.pointer_noiseUpdateSpeed;

		//shaftMesh = generator.GetComponent<Generator>().pointer_shaftMesh;//users can set custom meshes to be used as shafts
		maxLength = getGenerator.pointer_maxLength; //used in the raycast, if nothing is hit use this as the shafts maximum length.
		layerMask = getGenerator.pointer_layerMask; //filter against what the raycast can collide to determine length.

		shaftWidthMinMax = new Vector2 (getGenerator.pointer_shaftWidthMinMax.x, getGenerator.pointer_shaftWidthMinMax.y); // maps the set _shaftWidth to a min max range.

		//		shaftAdjustY = getGenerator.pointer_shaftAdjustY; //set the Ytiling of the material to change texture offset

		shaftDirection = getGenerator.pointer_shaftDirection; //will be used to orient the shafts towards a point in space (lightsource or gameObjects). A standard direction object will be present as a child in the generator.

		//if no shaft is found, search default pointer
		if (shaftDirection == null)
		{
			getGenerator.pointer_shaftDirection = GameObject.Find ("DirectionPointer").transform;

		}

	}

}