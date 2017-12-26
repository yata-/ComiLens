using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScene : MonoBehaviour {
    private TitleButton _button;

    // Use this for initialization
    void Start () {
        _button = GetComponentInChildren<TitleButton>();
        _button.OnClicked.Subscribe(p =>
        {
            SceneManager.LoadScene("MainScene");
        }).AddTo(this);
    }
	
	// Update is called once per frame
	void Update () {

    }
    private string SettingFileDirectoryPath()
    {
        string directorypath = "";
#if WINDOWS_UWP
        // HoloLens上での動作の場合、LocalAppData/AppName/LocalStateフォルダを参照する
        directorypath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
// Unity上での動作の場合、Assets/StreamingAssetsフォルダを参照する
    directorypath = UnityEngine.Application.streamingAssetsPath;
#endif
        return directorypath;
    }
}
