using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace _saveData {
	public class saveData : MonoBehaviour{
		public static _saveData.saveData sd;

		public int[] highScore = new int[49];

		public void Awake(){
			if (sd == null) {
				DontDestroyOnLoad (gameObject);
				sd = this;
				load ();
			} else if (sd != this) {
				Destroy (sd);
			}
		}

		//Application.persistentDataPath + "/the_kings_gardener.dat"
		private void load (){
			if (File.Exists (_main.gameDataLocation)) {
				BinaryFormatter bf = new BinaryFormatter ();
				FileStream file = File.Open (_main.gameDataLocation, FileMode.Open); 
				highScore = (int[])bf.Deserialize (file);
				file.Close();
			} else {
				highScore = Enumerable.Repeat(0, 49).ToArray();
			}
		}

		public void save(){
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Create (_main.gameDataLocation);
			bf.Serialize (file, highScore);
			file.Close();
			Debug.Log ("game saved");
		}
	}
}

