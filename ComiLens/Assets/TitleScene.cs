using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScene : MonoBehaviour {

    private Matrix4x4 _projectionMatrix;
    private const float OverlayDistance = 1;
    private TitleButton _button;
    private InputField _inputField;

    // Use this for initialization
    void Start () {
        _projectionMatrix = ConstValue.GetProjectMatrix();
        _button = GetComponentInChildren<TitleButton>();
        _inputField = GetComponentInChildren<InputField>();
        _button.OnClicked.Subscribe(p =>
        {
            var key = _inputField.text;
            Debug.Log(key);
            SceneManager.LoadScene("MainScene");
        }).AddTo(this);
    }
	
	// Update is called once per frame
	void Update () {

    }
}
