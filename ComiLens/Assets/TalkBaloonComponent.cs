using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalkBaloonComponent : MonoBehaviour {

    public string Text { get; set; }

    public OpenCVForUnity.Rect FaceRect { get; set; }

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update ()
	{
	    var text = GetComponentInChildren<Text>();
	    text.text = Text;
	    if (FaceRect == null)
	    {
	        return;
	    }
	    var transform = text.transform;
	    transform.localPosition = new Vector3(FaceRect.x + FaceRect.width / 2 - 0.5f, 0.5f - FaceRect.y - FaceRect.height / 2, 0);
	    transform.localScale = new Vector3(FaceRect.width, FaceRect.height, 1);
    }
}
