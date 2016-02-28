using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
	public enum AudioChannel
	{
		Master,
		Sfx,
		Music
	}

	public static AudioManager instance;

	public float masterVolumePercent { get; private set; }

	public float sfxVolumePercent { get; private set; }

	public float musicVolumePercent { get; private set; }

	AudioSource sfx2DSource;
	AudioSource[] musicSources;
	int activeMusicSourceIndex;
	Transform audioListener;
	Transform playerT;
	SoundLibrary soundLibrary;

	public void PlayMusic (AudioClip clip, float fadeDuration = 1)
	{
		activeMusicSourceIndex = 1 - activeMusicSourceIndex;
		musicSources [activeMusicSourceIndex].clip = clip;
		musicSources [activeMusicSourceIndex].Play ();
		StartCoroutine (AnimateMusicCrossfade (fadeDuration));
	}

	public void PlaySound (AudioClip clip, Vector3 position)
	{
		if (clip != null) {
			AudioSource.PlayClipAtPoint (clip, position, sfxVolumePercent * masterVolumePercent);
		}
	}

	public void PlaySound (string soundName, Vector3 position)
	{
		PlaySound (soundLibrary.GetClipFromName (soundName), position);
	}

	public void PlaySound2D (string soundName)
	{
		sfx2DSource.PlayOneShot (soundLibrary.GetClipFromName (soundName), sfxVolumePercent * masterVolumePercent);
	}

	public void SetVolume (float volumePercent, AudioChannel channel)
	{
		switch (channel) {
		case AudioChannel.Master:
			masterVolumePercent = volumePercent;
			break;
		case AudioChannel.Music:
			musicVolumePercent = volumePercent;
			break;
		case AudioChannel.Sfx:
			sfxVolumePercent = volumePercent;
			break;
		}

		musicSources [0].volume = musicVolumePercent * masterVolumePercent;
		musicSources [1].volume = musicVolumePercent * masterVolumePercent;

		PlayerPrefs.SetFloat ("master vol", masterVolumePercent);
		PlayerPrefs.SetFloat ("sfx vol", sfxVolumePercent);
		PlayerPrefs.SetFloat ("music vol", musicVolumePercent);
		PlayerPrefs.Save ();
	}

	void Awake ()
	{
		if (instance != null) {
			Destroy (gameObject);
		} else {
			instance = this;
			DontDestroyOnLoad (gameObject);
			soundLibrary = GetComponent<SoundLibrary> ();
			musicSources = new AudioSource[2];

			for (int i = 0; i < 2; i++) {
				GameObject newMusicSource = new GameObject ("Music source " + (i + 1));
				musicSources [i] = newMusicSource.AddComponent<AudioSource> ();
				newMusicSource.transform.parent = transform;
			}

			GameObject newsfx2DSource = new GameObject ("2D Sfx source");
			sfx2DSource = newsfx2DSource.AddComponent<AudioSource> ();
			newsfx2DSource.transform.parent = transform;
			audioListener = FindObjectOfType<AudioListener> ().transform;

			if (FindObjectOfType<Player> () != null) {
				playerT = FindObjectOfType<Player> ().transform;
			}

			masterVolumePercent = PlayerPrefs.GetFloat ("master vol", 1);
			sfxVolumePercent = PlayerPrefs.GetFloat ("sfx vol", 1);
			musicVolumePercent = PlayerPrefs.GetFloat ("music vol", 1);
		}
	}

	void Update ()
	{
		if (playerT != null) {
			audioListener.position = playerT.position;
		}
	}

	IEnumerator AnimateMusicCrossfade (float duration)
	{
		float percent = 0;

		while (percent < 1) {
			percent += Time.deltaTime * 1 / duration;
			musicSources [activeMusicSourceIndex].volume = Mathf.Lerp (0, musicVolumePercent * masterVolumePercent, percent);
			musicSources [1 - activeMusicSourceIndex].volume = Mathf.Lerp (musicVolumePercent * masterVolumePercent, 0, percent);
			yield return null;
		}
	}
}
