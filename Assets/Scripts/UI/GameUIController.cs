using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameUIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text_HP;
    [SerializeField] TextMeshProUGUI text_nowCost;
    [SerializeField] TextMeshProUGUI text_nowRank;
    [SerializeField] TextMeshProUGUI text_nextRank;
    [SerializeField] TextMeshProUGUI text_duplicateCost;
    [SerializeField] TextMeshProUGUI text_writeCost;
    [SerializeField] TextMeshProUGUI text_writeCost2;
    [SerializeField] TextMeshProUGUI text_overwriteCost;
    [SerializeField] TextMeshProUGUI text_overwriteCost2;
    [SerializeField] private TMP_FontAsset customFont;
    [SerializeField] Image icon_copycut;
    [SerializeField] Sprite s_copy;
    [SerializeField] Sprite s_cut;
    [SerializeField] ObjectScriptableObject objSB;
    [SerializeField] float paddingX = 100.0f;        //�}�E�X�J�[�\������image�̋߂����̒[�܂ł̋���
    [SerializeField] float paddingY = 80.0f;         //��ʒ[����ǂꂾ���]�����c���ĕ\�����邩
    [SerializeField] GameObject nowCostUI;
    [SerializeField] GameObject RankUI;


    private Tilemap tilemap;
    private StageManager stageManager;
    private RankJudgeAndUpdateFunction judgeFunc;
    //private Tilemap tilemap;
    private Vector3 initPos;
    //costDisplay�p
    private GameObject CostDisplay;
    private RectTransform rectT;
    private Canvas costDisplayCanvas;
    private float baseWidth = 150.0f;
    //�R�X�g�ꗗdisplay�p
    private GameObject allCostDisplay;
    private Vector2 startPos;
    private RectTransform targetUI;
    private Camera uiCamera;
    bool isLeft = false; //UI���E�[�ɂ��邩�ǂ���

    [Header("��������v���n�u")]
    public GameObject itemPrefab;

    [Header("�eRectTransform�iUI���̔z�u�͈́j")]
    public RectTransform spawnArea;

    /*[Header("���E�}�[�W���ƊԊu")]
    public float horizontalMargin = 20f;*/

    void Start()
    {
        uiCamera = FindObjectOfType<Camera>();
        Tilemap[] maps = FindObjectsOfType<Tilemap>();
        foreach (var map in maps)
        {
            if (map.gameObject.tag == "Tilemap")
            {
                tilemap = map;
            }
        }
        CalcDisplayAllObjectCost();
        InitData();

        icon_copycut.enabled = false;
        text_duplicateCost.gameObject.SetActive(false);
    }

    //�Ȃ���start�Ń��[�h����Ȃ��l�����@Update�ł����s����
    void InitData()
    {
        if(stageManager == null) stageManager = FindObjectOfType<StageManager>();
        if(judgeFunc==null) judgeFunc = FindObjectOfType<RankJudgeAndUpdateFunction>();
        if (CostDisplay == null)
        {
            CostDisplay = gameObject.transform.Find("CostDisplay").gameObject;
            CostDisplay.gameObject.SetActive(false);
        }
        if(allCostDisplay==null) allCostDisplay = gameObject.transform.Find("AllCostDisplay").gameObject;
        if(targetUI==null)
        {
            targetUI = allCostDisplay.GetComponent<RectTransform>();
            startPos = targetUI.anchoredPosition;
        }

        if(SceneManager.GetActiveScene().name == "StageSelectScene")
        {
            RankUI.SetActive(false);
            nowCostUI.SetActive(true);
        }
        else
        {
            RankUI.SetActive(true);
            nowCostUI.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        InitData();
        text_HP.text = "HP:" + stageManager.player_HP;
        text_nowCost.text = ""+stageManager.have_ene;
        //�W���b�W����֐��������Ă��Ă�B2�ڂ̕ϐ��͐�΂�false�B
        //2�ڂ̕ϐ���true�ɂ���ƍŏ�����R�X�g���X�V����A���ɃW���b�W���I������Ɣ��肳���
        text_nowRank.text = judgeFunc.JudgeAndUpdateRank(stageManager.all_sum_cos - stageManager.player_HP, false);
        text_nextRank.text = "�����N\n�_�E��\n�܂� " + judgeFunc.culcCostToNextRank();
        if (stageManager.tileData.hasData || stageManager.objectData.Count > 0)
        {
            icon_copycut.enabled = true;
            if (stageManager.all_isCut)
            {
                text_duplicateCost.text = ":" + stageManager.write_cost*0.5f;
                icon_copycut.sprite = s_cut; //�J�b�g�̃X�v���C�g��ݒ�
            }
            else
            {
                text_duplicateCost.text = ":" + stageManager.write_cost;
                icon_copycut.sprite = s_copy;
            }

                text_duplicateCost.gameObject.SetActive(true);
        }
        else
        {
            text_duplicateCost.gameObject.SetActive(false);
            icon_copycut.enabled = false;
        }
        

        

        //AppearAllCostDisplay(); //�R�X�g�ꗗ�\�\��
    }

    public void DisplayObjectCost(int writeCost, int eraseCost)
    {
        if (CostDisplay == null)
        {
            return;
        }
        if (rectT == null)
        {
            rectT = CostDisplay.GetComponent<RectTransform>();
        }
        if(costDisplayCanvas == null)
        {
            costDisplayCanvas = CostDisplay.transform.parent.GetComponent<Canvas>();
        }

        /*
        //���₷�R�X�g�A�����R�X�g���Ђ傤������
        text_writeCost.text = writeCost.ToString();
        text_writeCost2.text = writeCost.ToString();
        text_overwriteCost.text = eraseCost.ToString();
        text_overwriteCost2.text = eraseCost.ToString();

        //������̕����擾���A�������ɍ��킹��image�̕������߂�
        Canvas.ForceUpdateCanvases();
        float longestWidth = text_writeCost.preferredWidth >= text_overwriteCost.preferredWidth
            ? text_writeCost.preferredWidth : text_overwriteCost.preferredWidth;

        rectT.sizeDelta = new Vector2(baseWidth + longestWidth,rectT.sizeDelta.y);

        //�}�E�X�J�[�\���ɒǏ]����
        Vector2 localPoint;         //image�̍��W
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            costDisplayCanvas.transform as RectTransform,
            Input.mousePosition,
            costDisplayCanvas.worldCamera, // �������d�v�I
            out localPoint
        );
        //�܂��E���ɏo���Ƒz�肵�Čv�Z
        float rectPosX = rectT.rect.width * 0.5f + paddingX;
        float rectPosY = paddingY;

        //�����E���ɏo���ĉ�ʊO�ɍs���Ȃ獶��
        if(Input.mousePosition.x + paddingX + rectT.rect.width > Screen.width)
        {
            rectPosX *= -1;
        }

        rectT.anchoredPosition = localPoint + new Vector2(rectPosX,rectPosY);

        //�����I�u�W�F�N�g�����i�܂��͏�j�̃M���M���ɂ������ʓ���image������悤�ɂ���
        if (Input.mousePosition.y - rectT.rect.height*0.5f - paddingY < 0)
        {
            //�A���J�[�͉�ʒ����Ȃ̂ŁA(0,0)�͉�ʒ����ɂȂ�
            rectT.anchoredPosition = new Vector2(
                rectT.anchoredPosition.x, rectT.rect.height*0.5f+paddingY - Screen.height*0.5f);
        }
        else if(Input.mousePosition.y + rectT.rect.height*0.5f + paddingY > Screen.height)
        {
            rectT.anchoredPosition = new Vector2(
                rectT.anchoredPosition.x, Screen.height*0.5f - rectT.rect.height * 0.5f - paddingY);
        }*/

        //���₷�R�X�g�A�����R�X�g���Ђ傤������
        text_writeCost.text = writeCost.ToString();
        text_writeCost2.text = writeCost.ToString();
        text_overwriteCost.text = eraseCost.ToString();
        text_overwriteCost2.text = eraseCost.ToString();

        //������̕����擾���A�������ɍ��킹��image�̕������߂�
        Canvas.ForceUpdateCanvases();
        float longestWidth = text_writeCost.preferredWidth >= text_overwriteCost.preferredWidth
            ? text_writeCost.preferredWidth : text_overwriteCost.preferredWidth;

        rectT.sizeDelta = new Vector2(baseWidth + longestWidth, rectT.sizeDelta.y);

        //�}�E�X�J�[�\���ɒǏ]����
        Vector2 localPoint;         //image�̍��W
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            costDisplayCanvas.transform as RectTransform,
            Input.mousePosition,
            costDisplayCanvas.worldCamera, // �������d�v�I
            out localPoint
        );

        //�܂��E���ɏo���Ƒz�肵�Čv�Z,����x,y�͉�ʒ��S����}�E�X�J�[�\���܂ł̍���
        float rectPosX = localPoint.x + paddingX;
        float rectPosY = localPoint.y;

        //�����E���ɏo���ĉ�ʊO�ɍs���Ȃ獶��
        if (rectPosX + rectT.rect.width + paddingX > Screen.width*0.5f)//��canvas�͉�ʒ��S��(0,0)�Ȃ̂�
        {
            rectPosX = localPoint.x - rectT.rect.width - paddingX;
        }

        if (rectPosY - rectT.rect.height * 0.5f - paddingY < -Screen.height*0.5f)
        {
            rectPosY = -Screen.height*0.5f + rectT.rect.height * 0.5f + paddingY;
        }
        else if (rectPosY + rectT.rect.height * 0.5f + paddingY > Screen.height * 0.5f)
        {
            rectPosY = Screen.height * 0.5f - rectT.rect.height * 0.5f;
        }

        rectT.anchoredPosition = new Vector2(rectPosX, rectPosY);
        CostDisplay.SetActive(true);
    }

    public void CalcDisplayAllObjectCost()
    {
        tilemap.CompressBounds(); //�^�C�����ŏ��܂ň��k
        var b = tilemap.cellBounds; //�^�C���̑��݂���͈͂��擾 ���[����̍��W

        List<string> tags = new List<string>(); //���݂��Ă���I�u�W�F�N�g�̃^�O��ۑ�����p�̃��X�g

        int objCount = 0;

        Collider2D[] cols = Physics2D.OverlapAreaAll(new Vector2(b.x, b.y), new Vector2(b.size.x, b.size.y));
        foreach(var col in cols)
        {
            if(col.gameObject.tag != "Tilemap"
                && col.gameObject.tag != "Player"
                && col.gameObject.tag != "Uncuttable")
            {
                foreach(ObjectStore obj_s in objSB.objectList) //�V�[����̃I�u�W�F�N�g��tag���擾
                {
                    if(obj_s.obj.tag.Equals(col.gameObject.tag))
                    {
                        if(!tags.Contains(col.gameObject.tag))
                        {
                            tags.Add(col.gameObject.tag);
                            objCount++;
                            Debug.Log(col.gameObject.tag + "�̑��₷�R�X�g��" + obj_s.p_ene + " �����R�X�g��" + obj_s.ow_ene);
                            if(obj_s.obj.tag.Equals("Cannon")) //Canon������Ƃ���bullet��ǉ�
                            {
                                tags.Add("Bullet");
                            }
                        }
                    }
                }
            }
        }

        // �v���n�u�̕����ꎞ�I�Ɏ擾�i�������j
        GameObject temp = Instantiate(itemPrefab);
        RectTransform tempRT = temp.GetComponent<RectTransform>();
        float itemWidth = tempRT.rect.width;
        Destroy(temp);

        /*// �z�u�͈͂̕����擾
        float areaWidth = spawnArea.anchoredPosition.x;

        // 1������̕����v�Z�i�S�̂���}�[�W���ƊԊu�������j
        float totalSpacing = spacing * (objCount - 1);
        float availableWidth = areaWidth - (horizontalMargin * 2) - totalSpacing;
        float itemWidth = availableWidth / objCount;*/

        float areaWidth = spawnArea.rect.width;
        float oneItemWidth = areaWidth / objCount;
        Vector2 centerPos = new Vector2(spawnArea.anchoredPosition.x, spawnArea.anchoredPosition.y);
        Vector2 startPos = centerPos - new Vector2(areaWidth / 2, 0) /*+ new Vector2(oneItemWidth / 2, 0)*/;
        //Debug.Log("�\���G���A��x�T�C�Y��" + areaWidth);
        //float usableWidth = areaWidth - (horizontalMargin * 2);

        // spacing�������v�Z�i�󔒂��u�A�C�e���ԁv�ɋϓ��Ɋ���j
        

        /*float totalItemsWidth = itemWidth * objCount;
        float totalSpacing = usableWidth - totalItemsWidth;
        float spacing = (objCount > 1) ? totalSpacing / (objCount - 1) : 0f;*/

        for (int i = 0; i < objCount; i++)
        {
            GameObject item = Instantiate(itemPrefab, spawnArea);
            Image plusIcon_s = item.transform.Find("PlusCostIcon").gameObject.GetComponent<Image>();
            TextMeshProUGUI plusText = item.transform.Find("PlusCostText").gameObject.GetComponent<TextMeshProUGUI>();
            Image eraseIcon_s = item.transform.Find("EraseCostIcon").gameObject.GetComponent<Image>();
            TextMeshProUGUI eraseText = item.transform.Find("EraseCostText").gameObject.GetComponent<TextMeshProUGUI>();
            RectTransform itemRT = item.GetComponent<RectTransform>();

            foreach(ObjectStore obj_s in objSB.objectList) //�V�[����̃I�u�W�F�N�g��tag���擾
            {
                if(obj_s.obj.tag.Equals(tags[i])/* && !obj_s.obj.tag.Equals("Cannon")*/)
                {
                    plusText.text = obj_s.p_ene.ToString();
                    eraseText.text = obj_s.ow_ene.ToString();
                    plusText.color = Color.black;
                    eraseText.color = Color.black;
                    plusText.font = customFont;
                    eraseText.font = customFont;
                    plusText.fontSize = 36;   
                    eraseText.fontSize = 36;
                    obj_s.obj.GetComponent<SpriteRenderer>().color += new Color(0, 0, 0, 255);
                    plusIcon_s.sprite = obj_s.obj.GetComponent<SpriteRenderer>().sprite;
                    eraseIcon_s.sprite = obj_s.obj.GetComponent<SpriteRenderer>().sprite;
                    if(obj_s.obj.tag.Equals("Cannon"))
                    {
                        obj_s.obj.GetComponent<SpriteRenderer>().color -= new Color(0, 0, 0, 255);
                    }
                    //Debug.Log(col.gameObject.tag + "�̑��₷�R�X�g��" + obj_s.p_ene + " �����R�X�g��" + obj_s.ow_ene);
                }
            }

            // �T�C�Y�w��
            //itemRT.sizeDelta = new Vector2(oneItemWidth, itemRT.sizeDelta.y);

            // �A���J�[�ƈʒu�w��i���񂹌Œ�j
            /*itemRT.anchorMin = new Vector2(0, 0.5f);
            itemRT.anchorMax = new Vector2(0, 0.5f);
            itemRT.pivot = new Vector2(0, 0.5f);*/

            /*float xPos = horizontalMargin + (itemWidth / 2f) + i * (itemWidth + spacing);
            itemRT.anchoredPosition = new Vector2(xPos, 0f);*/

            // �z�u�ʒu�i�����珇�ɕ��ׂ�j
            //float xPos = horizontalMargin + (i + 1) * (oneItemWidth / 2f);
            itemRT.anchoredPosition = startPos + new Vector2(i * oneItemWidth, 0);
        }
    }

    public void AppearAllCostDisplay(bool isSelectZone) //�J�[�\�����킹������display����ʏ�ɏo��������
    {
        //����4�s��UI�Ƃ̏d�Ȃ蔻��, �����肽���z��GraphicRayCaster�R���|�[�l���g�t����
        /*PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);*/

        Vector2 mousePosition = Input.mousePosition;

        bool isOver = RectTransformUtility.RectangleContainsScreenPoint(
            targetUI,
            mousePosition,
            uiCamera // World Space��Camera�̏ꍇ�͎w��BScreen Space - Overlay�Ȃ� null �ɂ���
        );

        //�V�[�����܂����ƎQ�Ƃ��r�؂��̂�
        if(allCostDisplay==null) allCostDisplay = gameObject.transform.Find("AllCostDisplay").gameObject;

        Vector2 size = allCostDisplay.GetComponent<RectTransform>().sizeDelta; //UI�̃T�C�Y
        Vector2 hiddenPos = startPos; //�B��Ă���UI�̈ʒu
        Vector2 appearPos = hiddenPos + new Vector2(-size.x + 50, 0); //50�̓J�[�\�����킹�镝, UI�̏o���ʒu
        Vector2 leftPos = appearPos + new Vector2(-(size.x - 70), 0); //���[�̈ʒu

        if(isSelectZone)
        {
            //allCostDisplay.GetComponent<RectTransform>().anchoredPosition = appearPos;

            if(!isLeft)
            {
                allCostDisplay.GetComponent<RectTransform>().anchoredPosition = appearPos;
                if(isOver && PlayerInput.GetMouseButtonDown(0))
                {
                    allCostDisplay.GetComponent<RectTransform>().anchoredPosition = leftPos;
                    isLeft = true;
                }
            }
            else
            {
                if(isOver && PlayerInput.GetMouseButtonDown(0))
                {
                    allCostDisplay.GetComponent<RectTransform>().anchoredPosition = appearPos;
                    isLeft = false;
                }
            }

            /*if(isOver)
            {
                if(!isLeft)
                {
                    allCostDisplay.GetComponent<RectTransform>().anchoredPosition = leftPos;
                    isLeft = true;
                }
                else
                {
                    allCostDisplay.GetComponent<RectTransform>().anchoredPosition = appearPos;
                    isLeft = false;
                }
            }*/
        }
        else
        {
            if (isOver)
            {
                if(allCostDisplay.GetComponent<RectTransform>().anchoredPosition == hiddenPos)
                {
                    allCostDisplay.GetComponent<RectTransform>().anchoredPosition = appearPos;
                }
            }
            else
            {
                allCostDisplay.GetComponent<RectTransform>().anchoredPosition = hiddenPos;
            }
        }
    }

    public void UnDisplayObjectCost()
    {
        if (CostDisplay != null)
        {
            CostDisplay.SetActive(false);
        }
    }
}

/* void Start()
 * Tilemap[] maps = FindObjectsOfType<Tilemap>();
        foreach (var map in maps)
        {
            if (map.gameObject.tag == "Copy_Tilemap")
            {
                tilemap = map;
                break;
            }
        }
        initPos = tilemap.transform.localPosition;*/
/*
 * void Update()
         ���݃R�s�[���Ă���f�[�^��UI�ɕ\���������B�ǂ����H
        �܂��A�R�s�[���Ă���^�C���f�[�^�����ƂɐV����tilemap���쐬����B
        �쐬����tilemap�̃T�C�Y���A������v�Z����UI�̂Ƃ���Ɏ��܂�悤�ɂ���B
         
        if (stageManager.tileData.hasData)
        {
            tilemap.ClearAllTiles();
            tilemap.gameObject.transform.position = initPos;
            int w = stageManager.tileData.width;
            int h = stageManager.tileData.height;
            float scaleRateX = 6.0f / (float)w;
            float scaleRateY = 6.0f / (float)h;
            float scaleRate = scaleRateX < scaleRateY ? scaleRateX : scaleRateY;
            tilemap.gameObject.transform.localScale = new Vector3(scaleRate,scaleRate,1);
            float newX = 1;
            float newY = 1;
            switch (stageManager.tileData.direction)
            {
                case 0:
                    newX = -w * 0.5f * scaleRate;
                    newY = -h * 0.5f * scaleRate;
                    break;
                case 1:
                    newX = -w * 0.5f * scaleRate;
                    newY = -h * 0.5f * -scaleRate - scaleRate;
                    break;
                case 2:
                    newX = -w * 0.5f * -scaleRate - scaleRate;
                    newY = -h * 0.5f * -scaleRate - scaleRate;
                    break;
                case 3:
                    newX = -w * 0.5f * -scaleRate - scaleRate;
                    newY = -h * 0.5f * scaleRate;
                    break;
                    default:break;
            }
            tilemap.gameObject.transform.localPosition = new Vector3(initPos.x + newX,initPos.y + newY,initPos.z);
            for(int i = 0; i < h; i++)
            {
                for(int j = 0; j < w; j++)
                {
                    //�R�s�[�����Ƃ��̕����ɂ���Č��_���قȂ�
                    Vector3Int _p = Vector3Int.zero;
                    switch (stageManager.tileData.direction)
                    {
                        case 0:
                            _p = new Vector3Int( j,i,0);
                            break;
                        case 1:
                            _p = new Vector3Int( j,-i, 0);
                            break;
                        case 2:
                            _p = new Vector3Int( -j,-i, 0);
                            break;
                        case 3:
                            _p = new Vector3Int( -j,i,0);
                            break;
                        default: break;
                    }

                    tilemap.SetTile(_p, stageManager.tileData.tiles[i][j]);
                }
            }
        }
        else
        {
            tilemap.ClearAllTiles();
        }
        */