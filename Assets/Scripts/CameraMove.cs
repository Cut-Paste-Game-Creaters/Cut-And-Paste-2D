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

    /// <summary>
    /// ��ʗh��p
    public float shakeDuration = 0.25f;   // �f�t�H���g�̗h�ꎞ��
    public float shakeMagnitude = 0.25f;  // �f�t�H���g�̗h�ꋭ��

    private float _shakeTimeLeft = 0f;
    private float _shakeDurationActive = 0f;
    private float _shakeMagnitudeActive = 0f;
    /// </summary>
    void Start()
    {
        Tilemap[] maps = FindObjectsOfType<Tilemap>();
        foreach(var map in maps)
        {
            if (map.gameObject.tag == "Tilemap")
            {
                tilemap = map;
                break;
            }
        }
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (!PlayerInput.isPausing)
        {
            float posx = transform.position.x;
            float posy = transform.position.y;
            //������ʒ[�łȂ��Ȃ�
            //�v���C���[��X - (��ʕ�/2) = ���[��X��������~�߂�
            if (player.transform.position.x - Screen_Left > tilemap.cellBounds.min.x + 1//�����̃u���b�N������
                &&
                player.transform.position.x + 16 < tilemap.cellBounds.max.x - 1
                )
            {
                posx = player.transform.position.x;
            }

            if (
                player.transform.position.y - 9 >= tilemap.cellBounds.min.y      //�v���C���[�̉��[
                &&
                player.transform.position.y + 9 < tilemap.cellBounds.max.y      //�v���C���[�̏�[
                )
            {
                posy = player.transform.position.y;
            }
            this.transform.position = new Vector3(posx, posy, -10);

            ///��ʗh��p
            if (_shakeTimeLeft > 0f)
            {
                float damper = _shakeTimeLeft / _shakeDurationActive; // �I�Ղقǎキ
                float offsetX = Random.Range(-1f, 1f) * _shakeMagnitudeActive * damper;
                float offsetY = Random.Range(-1f, 1f) * _shakeMagnitudeActive * damper;

                transform.position += new Vector3(offsetX, offsetY, 0f);

                _shakeTimeLeft -= Time.deltaTime;
            }
        }
        //Debug.Log("t:"+tilemap.cellBounds.min.x);
        //Debug.Log("p:"+player.transform.position.x);
        //-6.66 -16 16-6.7



 
 
    }


    public void Shake()
    {
        Shake(shakeDuration, shakeMagnitude);
    }

    public void Shake(float duration, float magnitude)
    {

            _shakeDurationActive = Mathf.Max(0.0001f, duration);
            _shakeMagnitudeActive = Mathf.Max(0f, magnitude);
            _shakeTimeLeft = _shakeDurationActive;

    }



}
