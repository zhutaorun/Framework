using SDGame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDGame.UITools;
using GameFrame;

public class UIManager : SingletonMBAuto<UIManager>
{

	void Start () {
        // TODO get config from xml
        IBindableUI uiA = Activator.CreateInstance(Type.GetType("UIA")) as IBindableUI;
        GameObject prefab = Resources.Load<GameObject>("UI/UIA");
        GameObject go = Instantiate(prefab);
        UIControlData ctrlData = go.GetComponent<UIControlData>();
        if(ctrlData != null)
        {
            ctrlData.BindDataTo(uiA);
        }

        (uiA as UIA).CheckBinding();
	}
	
	void Update () {
		
	}
}
