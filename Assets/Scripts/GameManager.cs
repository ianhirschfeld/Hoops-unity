using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public int DefaultBasketPoints = 5;

	public static GameManager i = null;

	public int level = 0;
	public int score = 0;
	public int winningScore = 100;
	public int startingNumberOfShots = 6;
	public float ballMouseOffset = 2f;
	public float newBallDelay = 0.25f;
	public Text levelText;
	public Text scoreText;
	public Text shotsText;
	public Text youWonText;
	public Text gameOverText;
	public Button restartButton;
	public Slider powerSlider;
	public GameObject ballPrefab;
	public GameObject hoopPrefab;

	[HideInInspector]
	public bool isPlaying = true;
	[HideInInspector]
	public int numberOfShots = 0;
	[HideInInspector]
	public BallController lastBall;

	public List<GameObject> hoops = new List<GameObject> ();
	public bool hasShotsLeft { get { return numberOfShots > 0; } }
	public Vector3 mousePosition {
		get {
			Vector3 position = Input.mousePosition;
			position.z = ballMouseOffset;
			return Camera.main.ScreenToWorldPoint (position);
		}
	}

	private int basketPoints = 0;

	private void Awake() {
		if (i == null) {
			DontDestroyOnLoad(gameObject);
			i = this;
		} else if (i != this) {
			Destroy(gameObject);
		}
	}

	void Start() {
		basketPoints = DefaultBasketPoints;
		UnityEngine.RemoteSettings.Updated += new UnityEngine.RemoteSettings.UpdatedEventHandler(HandleRemoteUpdate);
		GameRestart ();
	}

	public void SetupLevel() {
		levelText.text = "Level: " + (level + 1);

		foreach (GameObject hoop in hoops) {
			Destroy (hoop);
		}
		hoops.Clear ();

		if (level == 0) {
			GameObject hoop = Instantiate (hoopPrefab, new Vector3 (0f, 2.5f, 1f), Quaternion.identity);
			HoopController controller = hoop.GetComponent<HoopController> ();
			controller.SetPoints (basketPoints);
			hoops.Add (hoop);
		} else {
			for (int i = 0; i < level; i++) {
				float yPos = Random.Range (2f, 2f + 2.5f * i);
				float zPos = 0.5f + 2.5f * i;
				GameObject hoop = Instantiate (hoopPrefab, new Vector3 (0f, yPos, zPos), Quaternion.identity);
				HoopController controller = hoop.GetComponent<HoopController> ();
				controller.SetPoints (basketPoints + basketPoints * i);
				controller.speed = Random.Range (1f, 1f + 1.5f * i);
				controller.range = 1f + 3f * i;
				hoops.Add (hoop);
			}
		}

		Dictionary<string, object> data = new Dictionary<string, object>();
		data.Add("score", GameManager.i.score);
		AnalyticsEvent.LevelStart(level, data);
	}

	public void NextLevel() {
		SendLevelCompleteEvent ();
		level++;
		numberOfShots += startingNumberOfShots * level;
		SetShotsText ();
		SetupLevel ();
	}

	public void CreateBall() {
		StartCoroutine(DoCreateBall ());
	}

	public IEnumerator DoCreateBall() {
		yield return new WaitForSeconds (newBallDelay);
		powerSlider.value = 0f;
		GameObject ball = Instantiate (ballPrefab, mousePosition, Quaternion.identity);
		lastBall = ball.GetComponent<BallController> ();
	}

	public void TakeShot() {
		numberOfShots--;
		SetShotsText ();
	}

	private void SetShotsText() {
		shotsText.text = "Shots Left: " + numberOfShots;
	}

	public void AddPoints(int points) {
		score += points;
		scoreText.text = "Score: " + score;

		Dictionary<string, object> data = new Dictionary<string, object> ();
		data.Add ("points", points);
		data.Add ("level", level);
		AnalyticsEvent.Custom ("basket", data);

		if (score >= winningScore) {
			isPlaying = false;
			youWonText.gameObject.SetActive (true);
			restartButton.gameObject.SetActive (true);
			SendLevelCompleteEvent ();
			SendGameOverEvent ();
			return;
		}

		if (level == 0 && score >= 15 ||
			level == 1 && score >= 30 ||
			level == 2 && score >= 60) {
			NextLevel ();
		}
	}

	public void GameOver() {
		print ("gameover");
		isPlaying = false;
		gameOverText.gameObject.SetActive (true);
		restartButton.gameObject.SetActive (true);
		SendGameOverEvent ();
	}

	public void GameRestart() {
		isPlaying = true;
		level = 0;
		score = 0;
		numberOfShots = startingNumberOfShots;
		scoreText.text = "Score: " + score;
		SetShotsText ();
		powerSlider.value = 0f;
		youWonText.gameObject.SetActive (false);
		gameOverText.gameObject.SetActive (false);
		restartButton.gameObject.SetActive (false);
		AnalyticsEvent.GameStart ();
		SetupLevel ();
		CreateBall ();
	}

	void SendLevelCompleteEvent() {
		Dictionary<string, object> data = new Dictionary<string, object> ();
		data.Add ("score", GameManager.i.score);
		AnalyticsEvent.LevelComplete (level, data);
	}

	public void SendLevelFailEvent() {
		Dictionary<string, object> data = new Dictionary<string, object> ();
		data.Add ("score", GameManager.i.score);
		AnalyticsEvent.LevelFail (level, data);
	}

	void SendGameOverEvent() {
		Dictionary<string, object> data = new Dictionary<string, object> ();
		data.Add ("score", GameManager.i.score);
		AnalyticsEvent.GameOver ("" + level, data);
	}

	private void HandleRemoteUpdate() {
		basketPoints = UnityEngine.RemoteSettings.GetInt ("BasketPoints", DefaultBasketPoints);
	}

}
