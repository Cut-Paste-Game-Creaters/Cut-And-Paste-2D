using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "OSO")]
public class ObjectScriptableObject : ScriptableObject
{
    public List<ObjectStore> objectList = new List<ObjectStore>();
}
[Serializable]
public class ObjectStore
{
    public string name; //タイルの頭文字
    public GameObject obj; //タイル本体
    public int p_ene; //ペースト時に消費するエナジー
    public int ow_ene; //上書き時に消費するエナジー
}
