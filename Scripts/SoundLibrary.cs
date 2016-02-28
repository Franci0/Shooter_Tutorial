using UnityEngine;
using System.Collections.Generic;

public class SoundLibrary : MonoBehaviour
{
	public SoundGroup[] soundGroups;

	Dictionary<string,AudioClip[]> groupDictionary = new Dictionary<string, AudioClip[]> ();

	public AudioClip GetClipFromName (string name)
	{
		if (groupDictionary.ContainsKey (name)) {
			AudioClip[] sounds = groupDictionary [name];
			return sounds [Random.Range (0, sounds.Length)];
		}

		return null;
	}

	void Awake ()
	{
		foreach (SoundGroup soundGroup in soundGroups) {
			groupDictionary.Add (soundGroup.groupID, soundGroup.group);
		}
	}

	[System.Serializable]
	public class SoundGroup
	{
		public string groupID;
		public AudioClip[] group;
	}
}
