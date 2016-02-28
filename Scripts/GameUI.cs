using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameUI : MonoBehaviour
{
	public Image fadePlane;
	public GameObject gameOverUI;
	public RectTransform newWaveBanner;
	public RectTransform healthBar;
	public Text newWaveTitle;
	public Text newWaveEnemyCount;
	public Text scoreUI;
	public Text gameOverScoreUI;

	Spawner spawner;
	Player player;

	//UI Input
	public void StartNewGame ()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene ("_Shooter");
	}

	public void ReturnToMainMenu ()
	{
		UnityEditor.SceneManagement.EditorSceneManager.LoadScene ("_Menu");
	}

	void Awake ()
	{
		spawner = FindObjectOfType<Spawner> ();
		spawner.OnNewWave += OnNewWave;
	}

	void Start ()
	{
		player = FindObjectOfType<Player> ();
		player.OnDeath += OnGameOver;
	}

	void Update ()
	{
		scoreUI.text = ScoreKeeper.score.ToString ("D6");
		float healthPercent = 0;

		if (player != null) {
			healthPercent = player.health / player.startingHealth;
		}

		healthBar.localScale = new Vector3 (healthPercent, 1, 1);
	}

	void OnGameOver ()
	{
		Cursor.visible = true;
		StartCoroutine (Fade (Color.clear, new Color (0, 0, 0, 230 / 256f), 1));
		gameOverScoreUI.text = scoreUI.text;
		scoreUI.gameObject.SetActive (false);
		healthBar.transform.parent.gameObject.SetActive (false);
		gameOverUI.SetActive (true);
	}

	void OnNewWave (int waveNumber)
	{
		string[] numbers = { "One", "Two", "Three", "Four", "Five" };
		newWaveTitle.text = "- Wave " + numbers [waveNumber - 1] + " -";
		string enemyCountString = (spawner.waves [waveNumber - 1].infinite) ? "Infinite" : spawner.waves [waveNumber - 1].enemyCount.ToString ();
		newWaveEnemyCount.text = "Enemies: " + enemyCountString;

		StopCoroutine ("AnimateNewWaveBanner");
		StartCoroutine ("AnimateNewWaveBanner");
	}

	IEnumerator Fade (Color from, Color to, float time)
	{
		float speed = 1 / time;
		float percent = 0;

		while (percent < 1) {
			percent += Time.deltaTime * speed;
			fadePlane.color = Color.Lerp (from, to, percent);
			yield return null;
		}
	}

	IEnumerator AnimateNewWaveBanner ()
	{
		float animationPercent = 0;
		float speed = 3f;
		float delay = 1.5f;
		int dir = 1;
		float endDelayTime = Time.time + 1 / speed + delay;

		while (animationPercent >= 0) {
			animationPercent += Time.deltaTime * speed * dir;

			if (animationPercent >= 1) {
				animationPercent = 1;

				if (Time.time > endDelayTime) {
					dir = -1;
				}
			}

			newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp (-355, 0, animationPercent);
			yield return null;
		}
	}
}
