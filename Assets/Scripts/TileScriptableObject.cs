using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "TSO")]
public class TileScriptableObject : ScriptableObject
{
    public List<TileStore> tileDataList = new List<TileStore>();
}
[Serializable]
public class TileStore
{
    public string head; //タイルの頭文字
    public Tile tile; //タイル本体
    public int p_ene; //ペースト時に消費するエナジー
    public int ow_ene; //上書き時に消費するエナジー
}
