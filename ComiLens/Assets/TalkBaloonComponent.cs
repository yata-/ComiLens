using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// 吹き出し
public class TalkBaloonComponent : MonoBehaviour
{
    private const string TailName = "BaloonTail";
    public bool IsTailVisible { get; set; }

    public string Text { get; set; }

    public Vector3? FacePosition { get; set; }

    private Image _tailImage;

    private Canvas _canvas;
    // Use this for initialization
    void Start ()
    {
        _canvas = GetComponent<Canvas>();
        _tailImage = GetComponentsInChildren<Image>().Where(p => p.name == TailName).First();
        IsTailVisible = true;
    }
	
	// Update is called once per frame
	void Update ()
	{
	    _tailImage.gameObject.SetActive( IsTailVisible);

        var text = GetComponentInChildren<Text>();
	    text.text = Text;
	    if (FacePosition == null)
	    {
	        return;
	    }
        var trans = _canvas.transform.transform;
        trans.localPosition = new Vector3(Screen.width / 8 * FacePosition.Value.x, Screen.height / 8 * FacePosition.Value.y, 600);
        Debug.Log(string.Format("[TalkBaloonComponent]x {0}, y {1}, z {2}",  FacePosition.Value.x, FacePosition.Value.y, FacePosition.Value.z));//, FacePosition.width, FacePosition.height));
    }

    
}
