using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using RL;
using System.Collections.Generic;

public class CRoomRender : MonoBehaviour
{

	// Use this for initialization
    private CMapCreator.CArea.CCrawler _crawler = null;
    private int[,] _room = null;
    private float _speed = 1.1f;
    private bool _isAnime = false;

    public void SetTexture(Texture2D t2d)
    {
        #region 初始化Sprite
        SpriteRenderer __render = GetComponent<SpriteRenderer>();
        __render.sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), new Vector2(0f, 0f));
        #endregion

        #region 初始化Collider的范围
        //BoxCollider2D __collider = GetComponentInChildren<BoxCollider2D>();

        //__collider.size = new Vector2((float)t2d.width / 100, (float)t2d.height / 100);
        //__collider.offset = __collider.size / 2;
        #endregion
    }
    public void SetData(CMapCreator.CArea.CCrawler crawler)
    {
        name = "room" + crawler._id.ToString();
        _crawler = crawler;
        _room = _crawler.GetRoomData();

        Transform __renderTrf = GetComponent<Transform>();
        __renderTrf.position = new Vector3((float)_crawler.Position._X, -((float)_crawler.Position._Y));

        Transform __idTrf = transform.Find("ID").GetComponent<Transform>();
        bool __break = false;
        for (int row = 0; row < _room.GetLength(1); row++)
        {
            for (int col = 0; col < _room.GetLength(0); col++)
            {
                if (_room[col, row] > 0)
                {
                    __idTrf.position = new Vector3((float)(col + _crawler.Position._X) + 0.5f, 
                        -((float)(row + _crawler.Position._Y)) - 0.5f, -5);
                    __break = true;
                    break;
                }
            }

            if (__break)
                break;
        }

        TextMesh __txt = GetComponentInChildren<TextMesh>();
        __txt.text = _crawler._id.ToString();

        if (_crawler._mother == null)
        {
            SetChildrenRoute();
        }

        #region 初始化Collider的范围
        for (int col = 0; col < _room.GetLength(0); col++)
        {
            if (_room[col, 0] > 0)
            {
                SetColliderPath(col, 0);
                break;
            }
        }
        #endregion
    }

    private void SetColliderPath(int col, int row)
    {
        Queue<Vector2> __path = new Queue<Vector2>();
        bool __upDown = false;
        bool __leftRight = true;
        int __orgC = -1;
        int __orgR = -1;

        while (true)
        {
            if (__upDown) // 上
            {
                #region 上左
                if (__leftRight) // 上左 （从 上左点 找 上右点）
                {
                    if ((_room[col, row] & (int)CMapCreator.EWallFlag.UpWall) != 0)
                    {
                        col++;
                        if (col >= _room.GetLength(0))
                        {
                            col--;
                            __path.Enqueue(new Vector2((float)(col + 1) + 0.1f, row));
                            __leftRight = false;
                        }
                    }
                    else
                    {
                        if (_room[col, row] == 0)
                        {
                            col--;
                            __path.Enqueue(new Vector2((float)(col + 1) + 0.1f, row));
                            __leftRight = false;
                        }
                        else
                        {
                            __path.Enqueue(new Vector2(col, row)); // 上左点
                            row--;
                            __upDown = false;///////////////////////////
                        }
                    }
                }
                #endregion
                #region 上右
                else // 上右 （从 上右点 找 下右点）
                {
                    if ((_room[col, row] & (int)CMapCreator.EWallFlag.RightWall) != 0)
                    {
                        row++;
                        if (row >= _room.GetLength(1))
                        {
                            row--;
                            __path.Enqueue(new Vector2((float)(col + 1) + 0.1f, (float)(row + 1) + 0.1f));
                            __upDown = false;
                        }
                    }
                    else
                    {
                        if (_room[col, row] == 0)
                        {
                            row--;
                            __path.Enqueue(new Vector2((float)(col + 1) + 0.1f, (float)(row + 1) + 0.1f));
                            __upDown = false;
                        }
                        else
                        {
                            __path.Enqueue(new Vector2((float)(col + 1) + 0.1f, row)); // 上右点
                            col++;
                            __leftRight = true;///////////////////////////
                        }
                    }
                }
                #endregion
            }
            else  // 下
            {
                #region 下左
                if (__leftRight) // 下左 （从 下左点 找 上左点）
                {
                    if (__orgC == col && __orgR == row)
                    {
                        //__path.Enqueue(new Vector2(col, row));
                        break;
                    }

                    if (__orgC < 0 && __orgC < 0)
                    {
                        __orgC = col;
                        __orgR = row;
                    }

                    if ((_room[col, row] & (int)CMapCreator.EWallFlag.LeftWall) != 0)
                    {
                        row--;
                        if (row < 0)
                        {
                            row++;
                            __path.Enqueue(new Vector2(col, row));
                            __upDown = true;
                        }
                    }
                    else
                    {
                        if (_room[col, row] == 0)
                        {
                            row++;
                            __path.Enqueue(new Vector2(col, row));
                            __upDown = true;
                        }
                        else
                        {
                            __path.Enqueue(new Vector2(col, (float)(row + 1) + 0.1f));// 下左点
                            col--;
                            __leftRight = false;///////////////////////////
                        }
                    }
                }
                #endregion
                #region 下右
                else // 下右 （从 下右点 找 下左点）
                {
                    if ((_room[col, row] & (int)CMapCreator.EWallFlag.BottomWall) != 0)
                    {
                        col--;
                        if (col < 0)
                        {
                            col++;
                            __path.Enqueue(new Vector2(col, (float)(row + 1) + 0.1f));
                            __leftRight = true;
                        }
                    }
                    else
                    {
                        if (_room[col, row] == 0)
                        {
                            col++;
                            __path.Enqueue(new Vector2(col, (float)(row + 1) + 0.1f));
                            __leftRight = true;
                        }
                        else
                        {
                            __path.Enqueue(new Vector2((float)(col + 1) + 0.1f, (float)(row + 1) + 0.1f)); // 下右点
                            row++;
                            __upDown = true;///////////////////////////
                        }
                    }
                }
                #endregion
            }
        }

        Debug.Log("num: " + __path.Count);
        PolygonCollider2D __collider = GetComponent<PolygonCollider2D>();
        __collider.SetPath(0, __path.ToArray());
    }
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (_isAnime)
        {
            transform.localScale *= _speed;
            if (transform.localScale.x > 2f)
            {
                _speed = 0.9f;
            }
            else if (transform.localScale.x < 1f)
            {
                transform.localScale = new Vector3(1,-1,1);
                _speed = 1.1f;
                _isAnime = false;
            }
        }
	}

    private void PrintInfo()
    {
        Text __infoText = GameObject.Find("Text").GetComponent<Text>() as Text;
        __infoText.text = string.Format("ID: {0}\n", _crawler._id);
        __infoText.text += string.Format("generation: {0}\n", _crawler._generation);
        __infoText.text += string.Format("position: {0} : {1}\n", _crawler.Position._X, _crawler.Position._Y);
        __infoText.text += string.Format("HP: {0}\n", _crawler._hp);
        __infoText.text += string.Format("mother: {0}\n", _crawler._mother == null ? "0" : _crawler._mother._id.ToString());
        __infoText.text += string.Format("eggs: {0}\n", _crawler._eggs);
        __infoText.text += string.Format("seeds: {0}\n", _crawler._seeds);

        __infoText.text += string.Format("relation:\n");
        CMapCreator.CArea.CCrawler __tmpCrawler = _crawler;
        _isAnime = true;

        __infoText.text += string.Format("(G:{0},I:{1})", _crawler._generation, _crawler._id);

        CRoomRender __tmpRoomRender = null;
        while (__tmpCrawler._mother != null)
        {
            __tmpCrawler = __tmpCrawler._mother;
            __tmpRoomRender = GameObject.Find("room" + __tmpCrawler._id.ToString()).GetComponent<CRoomRender>();


            //__tmpRoomRender._IsAnime = true;
            __infoText.text += string.Format("->(G:{0},I:{1})", __tmpCrawler._generation, __tmpCrawler._id);
        }
        __infoText.text += string.Format("\nfootmarks:\n");

        for (int i = 0; i < __tmpCrawler._footMarks.Count; i++)
        {
            __infoText.text += string.Format("  {0}: [x:{1}, y:{2}, flag:{3}]\n",
                i.ToString("D2"),
                __tmpCrawler._footMarks[i]._Pos._X,
                __tmpCrawler._footMarks[i]._Pos._Y,
                __tmpCrawler._footMarks[i]._Flag);
        }

        __infoText.text += string.Format("room:\n");

        for (int row = 0; row < _room.GetLength(1); row++)
        {
            for (int col = 0; col < _room.GetLength(0); col++)
                __infoText.text += string.Format(" {0} ", _room[col, row].ToString("D3"));

            __infoText.text += "\n";
        }
    
    }

    void OnMouseOver() 
    {
        if (Input.GetMouseButtonDown(0))
        {
            CleanAllRoute();
            PrintInfo(); 
            MothersRouteOn();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            CleanAllRoute();
            ChildrenRouteOn();
        }
        else if (Input.GetMouseButtonDown(2))
        {
            CleanAllRoute();
            MothersRouteOn();
            ChildrenRouteOn();
        }
    }

    private void CleanAllRoute()
    {
        GameObject[] __objs = GameObject.FindGameObjectsWithTag("InfoForRoom");

        foreach (GameObject __obj in __objs)
        {
            LineRenderer __line = __obj.GetComponentInChildren<LineRenderer>();
            __line.SetWidth(0.0f, 0.0f);
        }
    }

    private void SetChildrenRoute()
    {
        CMapCreator.CArea.CCrawler __tmpCrawler = null;
        CRoomRender __tmpRoomRender = null;
        LineRenderer __line = null;
        Transform __trf = null;

        foreach (CMapCreator.CArea.CCrawler child in _crawler.GetChildren())
        {
            #region 设置child的路线点
            __tmpCrawler = child;
            __tmpRoomRender = GameObject.Find("room" + __tmpCrawler._id.ToString()).GetComponent<CRoomRender>();
            __trf = __tmpRoomRender.transform.Find("ID").GetComponent<Transform>();

            __line = __tmpRoomRender.GetComponentInChildren<LineRenderer>();
            __line.SetPosition(0, __trf.position);
            #endregion

            #region 设置mother的路线点
            __tmpCrawler = __tmpCrawler._mother;
            __tmpRoomRender = GameObject.Find("room" + __tmpCrawler._id.ToString()).GetComponent<CRoomRender>();
            __trf = __tmpRoomRender.transform.Find("ID").GetComponent<Transform>();

            __line.SetPosition(1, __trf.position);
            #endregion
        }
    }
    private void SetChildrenWidth(float width)
    {
        CRoomRender __tmpRoomRender = null;
        LineRenderer __line = null;

        foreach (CMapCreator.CArea.CCrawler child in _crawler.GetChildren())
        {
            __tmpRoomRender = GameObject.Find("room" + child._id.ToString()).GetComponent<CRoomRender>();
            __line = __tmpRoomRender.GetComponentInChildren<LineRenderer>();
            __line.SetWidth(width, width);
        }

    }
    private void ChildrenRouteOff()
    {
        SetChildrenWidth(0.0f);
    }
    private void ChildrenRouteOn()
    {
        SetChildrenWidth(0.1f);
    }
    private void MothersRouteOff()
    {
        SetMothersRouteWidth(0.0f);
    }
    private void MothersRouteOn()
    {
        SetMothersRouteWidth(0.1f);
    }

    private void SetMothersRouteWidth(float width)
    {
        CMapCreator.CArea.CCrawler __tmpCrawler = _crawler;
        CRoomRender __tmpRoomRender = null;
        LineRenderer __line = null;

        while (__tmpCrawler != null)
        {
            __tmpRoomRender = GameObject.Find("room" + __tmpCrawler._id.ToString()).GetComponent<CRoomRender>();
            __line = __tmpRoomRender.GetComponentInChildren<LineRenderer>();
            __line.SetWidth(width, width);
            __tmpCrawler = __tmpCrawler._mother;
        }
    
    }
}
