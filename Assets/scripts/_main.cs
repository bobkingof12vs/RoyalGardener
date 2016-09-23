using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class _main : MonoBehaviour {

	//public objects
	//ingame objects
	public GameObject planePrefab;
	public List<GameObject> flowerPrefab;
	public Light sun;
	public ParticleSystem fireFlies;
	public AudioSource moveSound, dingSound;
	//ingame menu
	public GameObject igMenu;
	public Text bankText, valueText, moveText, goalText;
	public Button igHome, igHelp;
	//tutorial menu
	public Canvas tutorial;
	public RectTransform tutBackground, tutInstructions;
	public Button tutContinueButton;
	//gameover menu
	public Canvas gameover;
	public Text ggTitleTxt, ggReasonTxt, ggGoalTxt;
	public Button ggMain, ggNext, ggReplay;
	//in development menu
	public Canvas ComeBackLaterMenu;
	//ad menu
	public GameObject adMenu;
	//main menu
	public GameObject mainMenuGO;

	public static string gameDataLocation;

	//private variables
	private bool moving = false, highlighted = false, clicked = false, shownAd = false;
	private float lerpT = 3f;
	private float lerpDt = 1;
	private _lump startlump;
	private Vector2 startclickf = new Vector2();
	private int countLumpsSelected, lumpSelectedValue, moves;
	private int countTaken = 0, numberOfRocks = 0, bank = 0;
	private Vector3 sunPosFrom, sunPosTo;
	private int welcomeInt;

	//structs
	private class _lump{
		public GameObject obj;
		public GameObject plane;
		public byte color;
		public int x, y;
		public bool movable, highlighted;
		public _lump(int _x, int _y, byte _color){
			x = _x;
			y = _y;
			color = _color;
		}
	}
	private class _mover{
		public _lump lump;
		public Vector3 from;
		public Vector3 to;
	}

	//create lists
	private List<_mover> movement = new List<_mover>();
	private List<_lump> lumps = new List<_lump>();

	//control functions
	public static _main _m;
	public void Awake(){
		Screen.SetResolution (800, ((800 * Screen.currentResolution.height) / Screen.currentResolution.width), true);
		Debug.Log ("awake");
		gameDataLocation = Application.persistentDataPath + "/the_kings_gardener.dat";
		if (_m == null) {
			DontDestroyOnLoad (gameObject);
			_m = this;
		} else if (_m != this) {
			Destroy (_m);
		}
	}

	void Start () {
		Debug.Log ("start");
		tutorial.gameObject.SetActive (false);
		adMenu.gameObject.SetActive (false);
		tutContinueButton.onClick.AddListener(delegate {showHelpMenu();});
		ggMain.onClick.AddListener(delegate {loadLevel (0); mainmenu.m.setMainMenu (_level._l.level); mainMenuGO.SetActive(true);});
		ggReplay.onClick.AddListener(delegate {loadLevel(mainmenu.m.getDay());});
		ggNext.onClick.AddListener(delegate {mainmenu.m.incDay(1); loadLevel(mainmenu.m.getDay());});
		igHelp.onClick.AddListener(delegate {showHelpMenu(); clicked = false;});
		igHome.onClick.AddListener(delegate {loadLevel(0); mainMenuGO.SetActive(true);});
		loadLevel (0);
		mainMenuGO.SetActive(true);
		Debug.Log ("startd");
	}

	void OnApplicationQuit(){
		_saveData.saveData.sd.save ();
	}

	//Update is called once per frame
	void Update () {
		if (welcomeInt != 0)
			return; 
		
		if (!moving) {
			string action = "";
			if (Input.GetMouseButtonDown (0))
				startClick ();
			else if (Input.GetMouseButtonUp (0))
				action = endClick ();
			else if (Input.GetKeyDown (KeyCode.DownArrow) || Input.GetKeyDown (KeyCode.S))
				action = "down";
			else if (Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown (KeyCode.W))
				action = "up";
			else if (Input.GetKeyDown (KeyCode.LeftArrow) || Input.GetKeyDown (KeyCode.A))
				action = "left";
			else if (Input.GetKeyDown (KeyCode.RightArrow) || Input.GetKeyDown (KeyCode.D))
				action = "right";

			//Debug.Log ("click: " + startlump.x + ", " + startlump.y);
			if (action == "click")
				handleClick ();
			
			Vector2 spot = new Vector2 ();
			if (checkSpaces (out spot) <= 0 || moves <= 0)
				return;
			
			if(action == "down")
				shiftLumps (0, -1);
			else if(action == "up")
				shiftLumps (0, 1);
			else if(action == "left")
				shiftLumps (-1, 0);
			else if(action == "right")
				shiftLumps (1, 0);
				
		} else {
			moveLumps ();
			moveSun ();
			if (!moving) {
				Vector2 spot = new Vector2 ();
				int countRemaining = checkSpaces (out spot);
				if (countRemaining >= 1)
					createLump ((int)spot.x, (int)spot.y, (byte)_level._l.types[Random.Range (0, _level._l.countFlowers)]);

				if (countRemaining <= 1 && !canSellAnything ())
					showGameover (1);

			}
		}
	}
		
	public void loadLevel(int levelNum){
		Debug.Log (levelNum);

		gameover.gameObject.SetActive(false);
		if (levelNum != 0) {
			mainMenuGO.SetActive (false);
			igMenu.SetActive (true);
		} else {
			mainMenuGO.SetActive (true);
			igMenu.SetActive (false);
		}
			
		foreach (_lump l in lumps) {
			Destroy (l.obj);
			Destroy (l.plane);
		}

		lumps.RemoveAll (l => 1==1);
		movement.Clear ();

		lerpDt = 1;
		countTaken = 0; 
		numberOfRocks = 0;
		bank = 0;
		clicked = false;
		shownAd = false;

		if (levelNum == 1) {
			welcomeInt = 0;
			showHelpMenu ();
		} else if (levelNum == 0) {
			welcomeInt = -1;
		} else {
			welcomeInt = 0;
		}

		if(_level._l.moves == -1)
			ComeBackLaterMenu.gameObject.SetActive (true);
		else
			ComeBackLaterMenu.gameObject.SetActive (false);
		
		moves = _level._l.moves;
		moveText.text = "Moves\n" + moves;
		goalText.text = "Goal\n" + _level._l.goal;
		bankText.text = "Bank\n0";

		for (int i = 0; i < 6; i++)
			for (int j = 0; j < 6; j++) {
				if (_level._l.lumps [i, j] != ' ') {
					if (_level._l.lumps [i, j] == '@') {
						createLump (j, 5 - i, (byte)(flowerPrefab.Count - 1));	
						numberOfRocks++;
					} else if (_level._l.lumps [i, j] == 'f') {
						createLump (j, 5 - i, (byte)_level._l.types[Random.Range (0, _level._l.countFlowers)]);	
					} else {
						createLump (j, 5 - i, byte.Parse (_level._l.lumps [i, j].ToString ()));
					}
				}
			}

		setSun (0);
		moveSun ();

	}
	//SceneManager.
	void createLump(int x, int y, byte color){
		//create the lump
		_lump lump = new _lump(x, y, color);

		//create the flower list and add this new flower
		lump.obj = (GameObject)Instantiate (flowerPrefab[(int)color]);

		//move it to the right spot
		//lump.flowers.transform.Rotate (90, 0, 0);
		if (color == flowerPrefab.Count - 1) {
			lump.obj.transform.Rotate (0, 0, 0);
			lump.obj.transform.localPosition = new Vector3 ((x * 2) - 5, 0, (y * 2) - 5);
		} else {
			lump.obj.transform.localPosition = new Vector3 ((x * 2) - 5, 0.38f, (y * 2) - 5);
		}

		lump.plane = (GameObject)Instantiate (planePrefab);
		lump.plane.transform.position = new Vector3 ((x * 2) - 5, 0.002f, (y * 2) - 5);
		lump.plane.GetComponent<Renderer>().enabled = false;

		lump.movable = (lump.color < flowerPrefab.Count - 1);

		//add the lump to the list
		lumps.Add (lump);

	}

	void shiftLumps(int shiftX, int shiftY){
		
		if (highlighted)
			removeHighlights ();

		int xStart = (shiftX == -1 ? 1 : (shiftX == 1 ? 4 : 5));
		int yStart = (shiftY == -1 ? 1 : (shiftY == 1 ? 4 : 5));
		int xInc = (shiftX == -1 ? -1 : 1);
		int yInc = (shiftY == -1 ? -1 : 1);
			
		for (int x = xStart; x > -1 && x < 6; x -= xInc) {
			for (int y = yStart; y > -1 && y < 6; y -= yInc) { 
				//nothing assinged to this square? simply continue
				_lump l = lumps.Find(delegate(_lump _l) {return _l.x == x && _l.y == y;});
				if (l == null || !l.movable)
					continue;

				int subX = x, subY = y;

				if (shiftX != 0) {
					for (subX = x + shiftX; subX > -1 && subX < 6; subX += xInc)
						if (lumps.Find(delegate(_lump _l) {return _l.x == subX && _l.y == y;}) != null)
							break;

					subX -= xInc;
				}
				if (shiftY != 0) {
					for (subY = y + shiftY; subY > -1 && subY < 6; subY += yInc)
						if (lumps.Find(delegate(_lump _l) {return _l.x == x && _l.y == subY;}) != null)
							break;

					subY -= yInc;
				}

				l.x = subX;
				l.y = subY;

				_mover m = new _mover ();
				m.from = new Vector3 ((x * 2) - 5, 0.38f, (y * 2) - 5);
				m.to = new Vector3 ((subX * 2) - 5, 0.38f, (subY * 2) - 5);
				m.lump = l;
				if(Vector3.Distance(m.from,m.to) > .01)
				movement.Add (m);

				lerpDt = 0;
				if(!(x == subX && y == subY))
					moving = true;
			}
		}

		if (movement.Count > 0) {

			moveSound.Play ();

			moveText.text = "Moves\n" + --moves;
			setSun (1 - (float)moves / (float)_level._l.moves);

			if (moves == 0 && !canSellAnything ()) {
				showGameover (2);
				return;
			}

			if (moves == 1 && bank < _level._l.goal && !shownAd)
				adMenu.SetActive (true);
		}
	}

	void moveLumps(){
		if ((lerpDt += (Time.deltaTime * lerpT)) >= 1f)
			lerpDt = 1;

		foreach(_mover m in movement)
			if(m.lump.obj != null)
				m.lump.obj.transform.position = Vector3.Lerp (m.from, m.to, lerpDt);

		if (lerpDt == 1) {
			moving = false;
			movement.Clear ();
		}
	}

	int checkSpaces(out Vector2 val){
		List<Vector2> available = new List<Vector2> ();

		countTaken = 0;
		for (int x = 0; x < 6; x++)
			for (int y = 0; y < 6; y++)
				if (lumps.Find (l => ((l.x == x) && (l.y == y))) == null)
					available.Add (new Vector2 (x, y));
				else
					countTaken++;

		if (available.Count == 0) {
			val = new Vector2 (-1, -1);
			return 0;
		}
		
		val = available [Random.Range(0, available.Count)];
		return available.Count;

	}

	void startClick(){

		clicked = true;

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		startclickf = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);

		if (Physics.Raycast (ray, out hit, 100))
			startlump = lumps.Find (_l => _l.obj.transform == hit.transform);
		else
			startlump = new _lump(-1, -1, 0);
	}

	string endClick (){
		if (!clicked)
			return "";
		
		clicked = false;
		
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, 100)) {
			_lump l = lumps.Find (_l => _l.obj.transform == hit.transform);
			if (l.x != -1 && l.x == startlump.x && l.y == startlump.y)
				return "click";
		}


		int dx = Mathf.RoundToInt(Input.mousePosition.x - startclickf.x);
		int dy = Mathf.RoundToInt(Input.mousePosition.y - startclickf.y);

		if (dx == dy || Mathf.Abs(dx) + Mathf.Abs(dy) < 30)
			return "";

		if (Mathf.Abs (dx) > Mathf.Abs (dy)) {
			if (dx < 0)
				return "left";
			else
				return "right";
		} else {
			if (dy < 0)
				return "down";
			else
				return "up";
		}
	}

	void handleClick(){
		if (startlump.plane.GetComponent<Renderer> ().enabled) {
			//removeHighlights ();
			sellFlowers();
		} else {
			removeHighlights ();
			countLumpsSelected = 0;
			checkBuddies (startlump);

			if (countLumpsSelected < 3) {
				removeHighlights ();
				return;
			} else {
				highlighted = true;
				lumpSelectedValue = (5 * countLumpsSelected * countLumpsSelected);
				valueText.text = "Value\n" + lumpSelectedValue;
			}
		}
	}

	bool canSellAnything(){
		foreach (_lump l in lumps) {
			countLumpsSelected = 0;
			if(l.color != (byte)(flowerPrefab.Count - 1))
				checkBuddies (l, false);
			removeHighlights();
			if (countLumpsSelected > 2)
				return true;
		}
		return false;
	}

	void checkBuddies(_lump l_start, bool highlight = true){

		countLumpsSelected++;


		if (highlight) {
			l_start.plane.transform.position = new Vector3 ((l_start.x * 2) - 5, 0.002f, (l_start.y * 2) - 5);
			l_start.plane.GetComponent<Renderer> ().enabled = true;
		}

		l_start.highlighted = true;
			

		//check left
		_lump l_next = lumps.Find(_l => (_l.x == (l_start.x-1) && _l.y == l_start.y));
		if (l_next != null && l_next.color == l_start.color && l_next.highlighted == false)
			checkBuddies (l_next, highlight);

		//check right
		l_next = lumps.Find(_l => (_l.x == (l_start.x+1) && _l.y == l_start.y));
		if (l_next != null && l_next.color == l_start.color && l_next.highlighted == false)
			checkBuddies (l_next, highlight);

		//check up
		l_next = lumps.Find(_l => (_l.x == l_start.x && _l.y == (l_start.y-1)));
		if (l_next != null && l_next.color == l_start.color && l_next.highlighted == false)
			checkBuddies (l_next, highlight);

		//check down
		l_next = lumps.Find(_l => (_l.x == l_start.x && _l.y == (l_start.y+1)));
		if (l_next != null && l_next.color == l_start.color && l_next.highlighted == false)
			checkBuddies (l_next, highlight);
		
	}

	void sellFlowers(){
		Debug.Log ("selling flowers");

		dingSound.Play ();

		bank += lumpSelectedValue;
		lumpSelectedValue = 0;
		bankText.text = "Bank\n" + bank;

		highlighted = false;

		foreach (_lump l in lumps) {
			if (!l.plane.GetComponent<Renderer> ().enabled)
				continue;
			
			Destroy (l.obj);
			Destroy (l.plane);
			l.x = -1;
		}

		lumps.RemoveAll (l => (l.x == -1));


		if (moves == 0 && !canSellAnything())
			showGameover (2);
		
		Vector2 val;
		checkSpaces (out val);
		if(countTaken - numberOfRocks == 0)
			createLump ((int) val.x, (int) val.y, (byte)Random.Range (0, (int) _level._l.types.Count()));
	}

	void removeHighlights (){
		highlighted = false;
		foreach (_lump l in lumps) {
			l.plane.GetComponent<Renderer> ().enabled = false;
			l.highlighted = false;
		}
	}

	void setSun (float t){
		Vector3 a1 = new Vector3 (-10,   -2,  -9);
		Vector3 a2 = new Vector3 (-10,   20,  -9);
		Vector3 b1 = new Vector3 ( 10,   -2,  -9);
		Vector3 b2 = new Vector3 ( 10,   20,  -9);

		sunPosFrom = sun.transform.position;
		sunPosTo = Vector3.Lerp (Vector3.Lerp (a1, a2, t), Vector3.Lerp (b2, b1, t), t);

	}  

	void moveSun(){
		sun.transform.position = Vector3.Lerp (sunPosFrom, sunPosTo, lerpDt);
		sun.transform.LookAt (Vector3.zero);
		fireFlies.enableEmission = ((sunPosTo.y < 2));// ? 5 : 0);
	}

	void showGameover(byte reason){

		Debug.Log ("game over");

		gameover.gameObject.SetActive(true);
		if (bank >= _level._l.goal) {
			ggTitleTxt.text = "Great Job!";
			ggReasonTxt.text = "The King's Party will look wonderful!";
			if (bank > _saveData.saveData.sd.highScore [_level._l.level - 1]) {
				_saveData.saveData.sd.highScore [_level._l.level - 1] = bank;
				_saveData.saveData.sd.save ();
			}
			ggNext.interactable = true;
		}
		else{
			ggNext.interactable = false;
			ggTitleTxt.text = "Game Over";
			ggReasonTxt.text = "The King seems upset with these flowers";
		}

		if(reason == 1)
			ggReasonTxt.text += "\n<size=18>(You ran out of space in your garden)</size>";
		
		ggGoalTxt.text = "Goal: "+_level._l.goal+
			"\nFlower Value: "+bank+
			"\nHigh Score: "+_saveData.saveData.sd.highScore[_level._l.level - 1];

	}

	void showHelpMenu (){
		
		welcomeInt = (++welcomeInt % 3);
		
		tutorial.gameObject.SetActive (welcomeInt > 0);

		tutBackground.gameObject.SetActive (welcomeInt == 1);
		tutInstructions.gameObject.SetActive (welcomeInt == 2);

	}

	public void showAdYes(){
		shownAd = true;
		clicked = false;
		AdManager._ad.showAd (delegate(ShowResult sr){
			Debug.Log ("made it here");
		}, "rewardedVideo");
		moves += (int)Mathf.Floor ((float)_level._l.moves / 4);
		moves = (moves <= 10 ? 10 : moves);
		moveText.text = "Moves\n" + moves;
		setSun (1 - (float)moves / (float)_level._l.moves);
		moveSun ();
		adMenu.SetActive (false);
	}

	public void showAdNo(){
		clicked = false;
		adMenu.SetActive (false);
	}

	public void quit(){
		Application.Quit ();
	}
}
