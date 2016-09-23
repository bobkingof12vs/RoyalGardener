using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class mainmenu : MonoBehaviour {

	public static mainmenu m;

	public Text mWeek, mDetail;
	public GameObject mStart;
	public GameObject[] mDays = new GameObject[7];

	public int dayNum = -1, weekNum = 0, weekdayNum = 1;

	void Awake(){
		if (m == null) {
			DontDestroyOnLoad (gameObject);
			m = this;
		} else if (m != this) {
			Destroy (m);
		}
	}

	void Start(){
		for (int i = 0; i < 49; i++) {
			if (_saveData.saveData.sd.highScore [i] == 0) {
				dayNum = 0;
				incDay (i);
				break;
			}
		}
	}

	public void setMainMenu(int selectedDay = -1) {
		Debug.Log ("start main menu");
		bool found = false, daySet = false;

		for (int i = weekNum * 7; i < (weekNum * 7) + 7; i++) {
			Debug.Log ("i: " + i + " dayNum: " + dayNum + " weekNum: " + weekNum + " dayNum: " + weekdayNum + " highscore[i]: " + _saveData.saveData.sd.highScore [i]);
			if (!found && (weekNum == 0 || _saveData.saveData.sd.highScore [(weekNum * 7) - 1] != 0)) {
				if (_saveData.saveData.sd.highScore [i] == 0) {
					found = true;
					if (selectedDay == -1)
						selectedDay = (i % 7);
				}
				if (selectedDay == (i % 7)) {
					daySet = true;

					dayNum = (weekNum * 7) + selectedDay;
					weekdayNum = selectedDay;

					_level._l.setLevel (dayNum + 1);

					weekdayNum = selectedDay;
					mDays [(i % 7)].GetComponent<Image> ().color = new Color(0.45f, 0.60f, 0.19f, 1f);
					mDetail.text = "Week " + (weekNum + 1) + " - day " + (selectedDay + 1) +
						"\nGoal: " + _level._l.goal +
						"\nFlowers: " + _level._l.types.Count() +
						"\nHigh Score: " + _saveData.saveData.sd.highScore [dayNum] +
						"\nMoves: " + _level._l.moves;
					mStart.gameObject.SetActive (true);
				} else {
					mDays [(i % 7)].GetComponent<Image> ().color = new Color(0.70f, 0.57f, 0.29f, 1f);
				}
			} else {
				mDays [(i % 7)].GetComponent<Image> ().color = new Color(0.7f, 0.7f, 0.7F, 1F);
			}

			if (found && ((i % 7) == 6))
				break;
		}
		if (!daySet) {
			mDetail.text = "Week " + (weekNum + 1) + " - day " + (selectedDay + 1) + 
				"\nYou haven't made it to this day yet";
			mStart.gameObject.SetActive (false);
		}
			
	}

	public void incDay(int i){
		Debug.Log ("setting day: " + dayNum + " i: " + i);
		if (dayNum + i >= 0 && dayNum + i < 49) {
			dayNum += i;
			weekNum = (int)Mathf.Floor (dayNum / 7);
			mWeek.text = "Week " + (weekNum + 1);
			weekdayNum = (dayNum % 7);
			setMainMenu (weekdayNum);
		}
	}

	public int getDay(){
		return dayNum;
	}

	public void startLevel(){
		Debug.Log ("started level: " + mainmenu.m.dayNum);
		_main._m.loadLevel(mainmenu.m.dayNum + 1);
	}
}
