using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 吹き出し
public class TalkBaloonComponent : MonoBehaviour {

    public string Text { get; set; }

    public Vector3? FacePosition { get; set; }

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update ()
	{
	    var canvas = GetComponent<Canvas>();
	    var text = GetComponentInChildren<Text>();
	    text.text = Text;
	    if (FacePosition == null)
	    {
	        return;
	    }
	    var trans = canvas.transform.transform;
	    trans.localPosition = FacePosition.Value;// new Vector3(FacePosition.x * -0.6f, FacePosition.y * 0.6f, 600);
        //  + FacePosition.width / 4 , + FacePosition.height / 2 

        //Debug.Log(string.Format("[TalkBaloonComponent]x {0}, y {1}, width {2}, height {3}",  FacePosition.x, FacePosition.y, FacePosition.width, FacePosition.height));
    }
}
