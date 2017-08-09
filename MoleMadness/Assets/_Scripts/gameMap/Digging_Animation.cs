using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Digging_Animation : MonoBehaviour {

	public Sprite[] sprites;
	public float framesPerSecond;

	private SpriteRenderer spriteRenderer;

	bool animate;
	float time_start;
	float animation_time;

	// Use this for initialization
	void Start() {
		animate = false;
		spriteRenderer = GetComponent<Renderer>() as SpriteRenderer;
		animation_time = sprites.Length / framesPerSecond;
	}

	// Update is called once per frame
	void Update () {


		if (animate)
		{
			int index = (int)((Time.timeSinceLevelLoad - time_start) * framesPerSecond);
			index = index % sprites.Length;
			spriteRenderer.sprite = sprites[index];
			if (Time.timeSinceLevelLoad >= time_start + animation_time)
			{
				animate = false;
				spriteRenderer.sprite = null;
			}
		}
	}

	public void action() {
		animate = true;
		time_start = Time.timeSinceLevelLoad;
	}
}