using UnityEngine;
using System.Collections;

[RequireComponent (typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
	public static event System.Action OnDeathStatic;

	public enum State
	{
		Idle,
		Chasing,
		Attacking
	}

	public ParticleSystem deathEffect;

	State currentState;
	Material skinMaterial;
	Color originalColor;
	NavMeshAgent pathfinder;
	Transform target;
	float attackDistanceThreshold = .5f;
	float timeBetweenAttacks = 1;
	float nextAttackTime;
	float myCollisionRadius;
	float targetCollisionRadius;
	LivingEntity targetEntity;
	bool hasTarget;
	float damage = 1;

	public override void TakeHit (float damage, Vector3 hitPoint, Vector3 hitDirection)
	{
		AudioManager.instance.PlaySound ("Impact", transform.position);

		if (damage >= health) {
			if (OnDeathStatic != null) {
				OnDeathStatic ();
			}

			AudioManager.instance.PlaySound ("Enemy Death", transform.position);
			Destroy (Instantiate (deathEffect.gameObject, hitPoint, Quaternion.FromToRotation (Vector3.forward, hitDirection))as GameObject, deathEffect.startLifetime);
		}

		base.TakeHit (damage, hitPoint, hitDirection);
	}

	public void SetCharacteristics (float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColor)
	{
		pathfinder.speed = moveSpeed;

		if (hasTarget) {
			damage = Mathf.Ceil (targetEntity.startingHealth / hitsToKillPlayer);
		}

		startingHealth = enemyHealth;
		deathEffect.startColor = new Color (skinColor.r, skinColor.g, skinColor.b, 1);
		skinMaterial = GetComponent<Renderer> ().material;
		skinMaterial.color = skinColor;
		originalColor = skinMaterial.color;
	}

	protected override void Start ()
	{
		base.Start ();

		if (hasTarget) {
			currentState = State.Chasing;
			targetEntity.OnDeath += OnTargetDeath;
			StartCoroutine (UpdatePath ());
		}
	}

	void Awake ()
	{
		pathfinder = GetComponent<NavMeshAgent> ();

		if (GameObject.FindGameObjectWithTag ("Player") != null) {
			hasTarget = true;
			target = GameObject.FindGameObjectWithTag ("Player").transform;
			targetEntity = target.GetComponent<LivingEntity> ();
			myCollisionRadius = GetComponent<CapsuleCollider> ().radius;
			targetCollisionRadius = target.GetComponent<CapsuleCollider> ().radius;
		}
	}

	void Update ()
	{
		if (hasTarget) {
			if (Time.time > nextAttackTime) {
				float squareDistanceToTarget = (target.position - transform.position).sqrMagnitude;

				if (squareDistanceToTarget < Mathf.Pow (attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2)) {
					nextAttackTime = Time.time + timeBetweenAttacks;
					AudioManager.instance.PlaySound ("Enemy Attack", transform.position);
					StartCoroutine (Attack ());
				}
			}
		}
	}

	void OnTargetDeath ()
	{
		hasTarget = false;
		currentState = State.Idle;
	}

	IEnumerator Attack ()
	{
		currentState = State.Attacking;
		pathfinder.enabled = false;
		Vector3 originalPosition = transform.position;
		Vector3 directionToTarget = (target.position - transform.position).normalized;
		Vector3 attackPosition = target.position - directionToTarget * (myCollisionRadius);
		float percent = 0;
		float attackSpeed = 3;
		skinMaterial.color = Color.red;
		bool hasAppliedDamage = false;

		while (percent <= 1) {
			if (percent >= .5 && !hasAppliedDamage) {
				hasAppliedDamage = true;
				targetEntity.TakeDamage (damage);
			}
			percent += Time.deltaTime * attackSpeed;
			float interpolation = (-Mathf.Pow (percent, 2) + percent) * 4;
			transform.position = Vector3.Lerp (originalPosition, attackPosition, interpolation);
			yield return null;
		}

		skinMaterial.color = originalColor;
		currentState = State.Chasing;
		pathfinder.enabled = true;
	}

	IEnumerator UpdatePath ()
	{
		float refreshRate = .25f;

		while (hasTarget) {
			if (currentState == State.Chasing) {
				Vector3 directionToTarget = (target.position - transform.position).normalized;
				Vector3 targetPosition = target.position - directionToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2);

				if (!dead) {
					pathfinder.SetDestination (targetPosition);
				}
			}

			yield return new WaitForSeconds (refreshRate);
		}
	}
}
