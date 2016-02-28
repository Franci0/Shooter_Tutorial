using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
	public LayerMask collisionMask;
	public Color trailColor;

	float speed = 10;
	float damage = 1;
	float lifeTime = 3;
	float skinWidth = .1f;

	public void SetSpeed (float newSpeed)
	{
		speed = newSpeed;
	}

	void Start ()
	{
		Destroy (gameObject, lifeTime);
		Collider[] initialCollision = Physics.OverlapSphere (transform.position, .1f, collisionMask);

		if (initialCollision.Length > 0) {
			OnHitObject (initialCollision [0], transform.position);
		}

		GetComponent<TrailRenderer> ().material.SetColor ("_TintColor", trailColor);
	}

	void Update ()
	{
		float moveDistance = speed * Time.deltaTime;
		CheckCollision (moveDistance);
		transform.Translate (Vector3.forward * moveDistance);
	}

	void CheckCollision (float moveDistance)
	{
		Ray ray = new Ray (transform.position, transform.forward);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide)) {
			OnHitObject (hit.collider, hit.point);
		}
	}

	void OnHitObject (Collider collider, Vector3 hitPoint)
	{
		IDamageable damageableObject = collider.GetComponent<IDamageable> ();

		if (damageableObject != null) {
			damageableObject.TakeHit (damage, hitPoint, transform.forward);
		}

		GameObject.Destroy (gameObject);
	}
}
