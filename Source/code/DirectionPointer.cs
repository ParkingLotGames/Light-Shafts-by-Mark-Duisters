
using UnityEngine;
using System.IO;


public class DirectionPointer : MonoBehaviour {


	void OnDrawGizmosSelected () {

		if (Directory.Exists (Application.dataPath + "/Gizmos")) {

			if (File.Exists (Application.dataPath + "/Gizmos/AimIcon_tmp.png")) {

				Gizmos.DrawIcon (transform.position, "AimIcon_tmp", true);

		}
	}

	}

}