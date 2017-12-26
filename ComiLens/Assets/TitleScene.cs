using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScene : MonoBehaviour {
    private TitleButton _button;

    // Use this for initialization
    void Start ()
    {
        _button = GetComponentInChildren<TitleButton>();
        _button.OnClicked.Subscribe(p =>
        {
            SceneManager.LoadScene("MainScene");
        }).AddTo(this);

        try
        {
            StateManager.Key = GetKey();
        }
        catch (Exception e)
        {
            _button.enabled = false;
            this.GetComponentInChildren<Text>().text = "API Keyファイルが見つかりません。";
        }
        
    }
	
	// Update is called once per frame
	void Update () {

    }

    private string GetKey()
    {

#if UNITY_EDITOR
       return "";
#else
        var task = Windows.Storage.ApplicationData.Current.LocalFolder.OpenStreamForReadAsync("key");
        task.Wait();
        using (var stream = task.Result)
        {
            var sr = new System.IO.StreamReader(stream);
            return sr.ReadToEnd();
        }
#endif

    }

}
