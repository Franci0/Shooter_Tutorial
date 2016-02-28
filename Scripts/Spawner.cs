using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
	public bool devMode;
	public Wave[] waves;
	public Enemy enemy;

	int enemiesRemainingToSpawn;
	int enemiesRemainigAlive;
	int currentWaveNumber;
	float nextSpawnTime;
	float timeBetweenCampingChecks = 2;
	float nextCampCheckTime;
	float campThresholdDistance = 1.5f;
	bool isCamping;
	bool isDisabled;
	Vector3 campPositionOld;
	Wave currentWave;
	MapGenerator map;
	LivingEntity playerEntity;
	Transform playerT;

	public event System.Action<int> OnNewWave;

	void Start ()
	{
		playerEntity = FindObjectOfType<Player> ();
		playerT = playerEntity.transform;
		nextCampCheckTime = timeBetweenCampingChecks + Time.time;
		campPositionOld = playerT.position;
		playerEntity.OnDeath += OnPlayerDeath;
		map = FindObjectOfType<MapGenerator> ();
		NextWave ();
	}

	void Update ()
	{
		if (!isDisabled) {
			if (Time.time > nextCampCheckTime) {
				nextCampCheckTime = Time.time + timeBetweenCampingChecks;
				isCamping = (Vector3.Distance (playerT.position, campPositionOld) < campThresholdDistance);
				campPositionOld = playerT.position;
			}

			if ((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime) {
				enemiesRemainingToSpawn--;
				nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;
				StartCoroutine ("SpawnEnemy");
			}
		}

		if (devMode) {
			if (Input.GetKeyDown (KeyCode.Return)) {
				StopCoroutine ("SpawnEnemy");

				foreach (Enemy enemy in FindObjectsOfType<Enemy>()) {
					GameObject.Destroy (enemy.gameObject);
				}

				NextWave ();
			}
		}
	}

	void OnPlayerDeath ()
	{
		isDisabled = true;
	}

	void OnEnemyDeath ()
	{
		//print ("Enemy Died");
		enemiesRemainigAlive--;

		if (enemiesRemainigAlive == 0) {
			NextWave ();
		}
	}

	void ResetPlayerPosition ()
	{
		playerT.position = map.GetTileFromPosition (Vector3.zero).position + Vector3.up * playerT.localScale.y;
	}

	void NextWave ()
	{
		if (currentWaveNumber > 0) {
			AudioManager.instance.PlaySound2D ("Level Complete");
		}
		currentWaveNumber++;

		if (currentWaveNumber - 1 < waves.Length) {
			print ("Wave " + currentWaveNumber);
			currentWave = waves [currentWaveNumber - 1];
			enemiesRemainingToSpawn = currentWave.enemyCount;
			enemiesRemainigAlive = enemiesRemainingToSpawn;

			if (OnNewWave != null) {
				OnNewWave (currentWaveNumber);
			}

			ResetPlayerPosition ();
		}
	}

	IEnumerator SpawnEnemy ()
	{
		float spawnDelay = 1;
		float tileFlashSpeed = 4;
		Transform spawnTile = map.GetRandomOpenTile ();

		if (isCamping) {
			spawnTile = map.GetTileFromPosition (playerT.position);
		}

		Material tileMat = spawnTile.GetComponent<Renderer> ().material;
		Color initialColor = Color.white;
		Color flashColor = Color.red;
		float spawnTimer = 0;

		while (spawnTimer < spawnDelay) {
			tileMat.color = Color.Lerp (initialColor, flashColor, Mathf.PingPong (spawnTimer * tileFlashSpeed, 1));
			spawnTimer += Time.deltaTime;
			yield return null;
		}

		Enemy spawnedEnemy = Instantiate (enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
		spawnedEnemy.OnDeath += OnEnemyDeath;
		spawnedEnemy.SetCharacteristics (currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColor);
	}

	[System.Serializable]
	public class Wave
	{
		public bool infinite;
		public int enemyCount;
		public float timeBetweenSpawns;
		public float moveSpeed;
		public int hitsToKillPlayer;
		public float enemyHealth;
		public Color skinColor;
	}

}
