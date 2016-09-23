using UnityEngine;
using System.Collections;

public class crickets : MonoBehaviour {

	public AudioSource noise;
	public Transform sun;
	public float volume;
	private float time = (1f / 2f);

	// Update is called once per frame
	void Update () {
		
		if (sun.position.y < 3f && noise.volume != volume) {
			if (noise.volume < volume) {
				noise.volume += (time * Time.deltaTime);
				Debug.Log (noise.volume);
			}
			else
				noise.volume = volume;
		}
		else if (sun.position.y >= 3f && noise.volume != 0) {
			if (noise.volume > 0) {
				noise.volume -= (time * Time.deltaTime);
				Debug.Log (noise.volume);
			}
			else
				noise.volume = 0;
		}
	}
}
