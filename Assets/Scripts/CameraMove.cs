using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraMove : MonoBehaviour
{
    private GameObject player;
    private Tilemap tilemap;
    public float Screen_Left = 9.3f;
    // Start is called before the first frame update
    void Start()
    {
        tilemap = FindObjectOfType<Tilemap>();
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        //������ʒ[�łȂ��Ȃ�
        //�v���C���[��X - (��ʕ�/2) = ���[��X��������~�߂�
        if (player.transform.position.x - Screen_Left > tilemap.cellBounds.min.x+1//�����̃u���b�N������
            &&
            player.transform.position.x + 16 < tilemap.cellBounds.max.x-1)
        {
            this.transform.position = new Vector3(player.transform.position.x, 0, -10);
        }
        //Debug.Log("t:"+tilemap.cellBounds.min.x);
        //Debug.Log("p:"+player.transform.position.x);
    }
}//-6.66 -16 16-6.7
