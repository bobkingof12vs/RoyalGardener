using UnityEngine;
using UnityEngine.Advertisements;
using System;
using System.Collections;

public class AdManager : MonoBehaviour {
	
	public static AdManager _ad;

	public void Awake(){
		if (_ad == null) {
			DontDestroyOnLoad (gameObject);
			_ad = this;
		} else if (_ad != this) {
			Destroy (_ad);
		}
	}

	public void showAd(Action<ShowResult> callback, string zone = ""){
		#if UNITY_EDITOR
		waitForAd ();
		#endif

		ShowOptions options = new ShowOptions ();
		options.resultCallback = callback;

		Debug.Log ("here");

		if (string.Equals (zone, ""))
			zone = null;
		
		if (Advertisement.IsReady (zone))
			Advertisement.Show (zone, options);
		
	}

	IEnumerator waitForAd(){
		float currentTimeScale = Time.timeScale;
		Time.timeScale = 0f;

		yield return null;

		while (Advertisement.isShowing)
			yield return null;

		Time.timeScale = currentTimeScale;
	}
}
