using UnityEngine;
using System.Collections;

public class ScoreKeeper : MonoBehaviour
{
	public static int score{ get; private set; }

	float lastEnemyKillTime;
	int streakCount;
	float streakExpireTime = 1;

	void Start ()
	{
		Enemy.OnDeathStatic += OnEnemyKilled;
		FindObjectOfType<Player> ().OnDeath += OnPlayerDeath;
	}

	void OnEnemyKilled ()
	{
		if (Time.time < lastEnemyKillTime + streakExpireTime) {
			streakCount++;
		} else {
			streakCount = 0;
		}

		lastEnemyKillTime = Time.time;
		score += 5 + (int)Mathf.Pow (2, streakCount);
	}

	void OnPlayerDeath ()
	{
		Enemy.OnDeathStatic -= OnEnemyKilled;
	}
}
