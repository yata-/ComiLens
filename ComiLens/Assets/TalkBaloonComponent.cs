using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalkBaloonComponent : MonoBehaviour {

    public string Text { get; set; }

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update ()
	{
	    var text = GetComponentInChildren<Text>();
	    text.text = Text;

    }
}
