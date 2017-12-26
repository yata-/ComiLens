using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class TitleButton : MonoBehaviour {

    private Subject<MonoBehaviour> _onClicked;
    public IObservable<MonoBehaviour> OnClicked { get { return _onClicked; } }

    // Use this for initialization
    void Start () {
		_onClicked = new Subject<MonoBehaviour>();
	}
	
	// Update is called once per frame
	void Update () {

    }
    public void Click()
    {
        this._onClicked.OnNext(this);
    }
}
