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
    public string name; //�^�C���̓�����
    public GameObject obj; //�^�C���{��
    public int p_ene; //�y�[�X�g���ɏ����G�i�W�[
    public int ow_ene; //�㏑�����ɏ����G�i�W�[
}
