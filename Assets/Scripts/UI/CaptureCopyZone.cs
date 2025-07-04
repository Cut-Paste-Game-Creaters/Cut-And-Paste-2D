using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class CaptureCopyZone : MonoBehaviour
{
    private Rect captureArea = new Rect(100, 100, 300, 200); // x, y, width, height
    private Camera cam;
    private int originalMask;

    [SerializeField] Image image;

    void Start()
    {
        image.enabled = false; 
    }

    public void CaptureImage(Vector3 startPos, Vector3 endPos)
    {
        int dir = culcDir(startPos,endPos);

        /*
         �@Startpos��endpos�������Ŏ擾����
        �A�����dir�ɂ���Ĉʒu�𒲐�����
        ������ChangeVecToInt��4�����̃x�N�g��������
        �B�������X�N���[�����W�ɂ���x,y������Ƃ�����
        �C�����start��end��x,y����width��height���v�Z����
         */
        Vector3Int temp_start = ChangeVecToInt(startPos);
        temp_start.z = 0;
        Vector3Int temp_end = ChangeVecToInt(endPos);
        temp_end.z = 0;
        Vector3Int tempVec_start = Vector3Int.zero;
        Vector3Int tempVec_end = Vector3Int.zero;

        switch (dir)
        {
            case 0:
                tempVec_start = new Vector3Int(temp_start.x, temp_start.y, temp_start.z);
                tempVec_end = new Vector3Int(temp_end.x+1, temp_end.y+1, temp_end.z);
                break;
            case 1:
                tempVec_start = new Vector3Int(temp_start.x, temp_start.y+1, temp_start.z);
                tempVec_end = new Vector3Int(temp_end.x + 1, temp_end.y, temp_end.z);
                break;
            case 2:
                tempVec_start = new Vector3Int(temp_start.x+1, temp_start.y + 1, temp_start.z);
                tempVec_end = new Vector3Int(temp_end.x, temp_end.y, temp_end.z);
                break;
            case 3:
                tempVec_start = new Vector3Int(temp_start.x + 1, temp_start.y, temp_start.z);
                tempVec_end = new Vector3Int(temp_end.x, temp_end.y+1, temp_end.z);
                break;
            default:break;
        }

        float startX_screen = Camera.main.WorldToScreenPoint(tempVec_start).x;
        float startY_screen = Camera.main.WorldToScreenPoint(tempVec_start).y;
        float endX_screen = Camera.main.WorldToScreenPoint(tempVec_end).x;
        float endY_screen = Camera.main.WorldToScreenPoint(tempVec_end).y;
        captureArea.x = startX_screen < endX_screen ? startX_screen : endX_screen;
        captureArea.y = startY_screen < endY_screen ? startY_screen : endY_screen;
        captureArea.width = Mathf.Abs(startX_screen - endX_screen);
        captureArea.height = Mathf.Abs(startY_screen - endY_screen);


        // ���̃J�����O�}�X�N��ۑ�
        cam = Camera.main;
        originalMask = cam.cullingMask;

        // �w�背�C���[�𖳎��i�r�b�g���Z�ŏ��O�j
        int ignoreLayer = LayerMask.NameToLayer("UI");
        cam.cullingMask &= ~(1 << ignoreLayer);

        StartCoroutine(CaptureAndSave());
    }

    //�͈͑I�����āA�R�s�[�܂��̓J�b�g��I�������u�Ԃɂ��̊֐����Ăяo��

    System.Collections.IEnumerator CaptureAndSave()
    {
        // �t���[���̃����_�����O������҂�
        yield return new WaitForEndOfFrame();

        // �e�N�X�`�����쐬
        Texture2D tex = new Texture2D((int)captureArea.width, (int)captureArea.height, TextureFormat.RGB24, false);
        tex.ReadPixels(captureArea, 0, 0); // �X�N���[���̈ꕔ��ǂݍ���
        tex.Apply();

        // Texture2D �� Sprite �ɕϊ�
        Sprite capturedSprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f) // pivot (����)
        );

        // �J�����O�}�X�N�����ɖ߂�
        cam.cullingMask = originalMask;

        // UI Image �ɔ��f
        image.sprite = capturedSprite;
        SetImageScaleToFit(image,capturedSprite);
        image.enabled = true;

        ///////////////////////////////////////////
        /*
        // PNG�f�[�^�ɕϊ�
        byte[] pngData = tex.EncodeToPNG();

        // �ۑ���p�X
        string path = Application.dataPath + "/CapturedImage.png";
        File.WriteAllBytes(path, pngData);

        Debug.Log("�摜��ۑ����܂���: " + path);
        */
    }

    void SetImageScaleToFit(Image image, Sprite sprite)
    {
        // �摜�̌��̃T�C�Y�i�s�N�Z���P�ʁj
        float imgWidth = sprite.rect.width;
        float imgHeight = sprite.rect.height;

        Vector3 scale = Vector3.one;

        if (imgHeight >= imgWidth)
        {
            // �c���FY����ɂ���3.6�AX�͔䗦�ŏk��
            scale.y = 3.6f;
            scale.x = 3.6f * (imgWidth / imgHeight);
        }
        else
        {
            // �����FX����ɂ���3.6�AY�͔䗦�ŏk��
            scale.x = 3.6f;
            scale.y = 3.6f * (imgHeight / imgWidth);
        }

        // scale��Image�I�u�W�F�N�g�ɓK�p
        image.transform.localScale = scale;
    }


    //�}�E�X�̍��W���^�C���̍��W�ɕϊ�����֐�
    private Vector3Int ChangeVecToInt(Vector3 v)
    {
        Vector3Int pos = Vector3Int.zero;
        pos.x = (int)Mathf.Floor(v.x);
        pos.y = (int)Mathf.Floor(v.y);
        //pos.z = (int)Mathf.Floor(v.z);

        return pos;
    }

    private int culcDir(Vector3 _startPos,Vector3 _endPos)
    {
        //�R�s�[�̌������擾����
        int direction = 0;
        if (_endPos.x - _startPos.x >= 0)//�E����
        {
            if (_endPos.y - _startPos.y >= 0) //�����
            {
                direction = 0;
            }
            else
            {
                direction = 1;
            }
        }
        else  //������
        {
            if (_endPos.y - _startPos.y >= 0) //�����
            {
                direction = 3;
            }
            else
            {
                direction = 2;
            }
        }
        return direction;
    }

    public void disableImage()
    {
        image.enabled = false;
    }
}
