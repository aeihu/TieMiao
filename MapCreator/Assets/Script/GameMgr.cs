﻿/*
* Copyright (C) 2015, <Aeihu.z, aeihu.z@gmail.com>.
*
* TieMiao is a free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* Version 3(GPLv3) as published by the Free Software Foundation.
*/

using UnityEngine;
using System.Collections;
using TieMiao;

public class GameMgr : MonoBehaviour {

    // Use this for initialization
    GameObject _prefab;
    CMapCreator __map = new CMapCreator();
    CRoomCellTypeManager __block;
    int __cellSize = 110;
    int __borderSize = 10;
    Color sd = new Color(0, 0, 0, 0);
	void Start () 
    {
        __block = new CRoomCellTypeManager(
            __cellSize, 10, new Color32(0, 0, 200, 200), new Color32(250, 250, 250, 255));
        //CMapCreator __map = new CMapCreator();
        __map._AreaList.Add(new CMapCreator.CArea());
        __map._AreaList[0].ResetArea(50, 50, 50, 1);

        ////int[,] __sp = 
            __map._AreaList[0].Produce();


        _prefab = (GameObject)Resources.Load("Prefabs/Room");

        foreach (CMapCreator.CArea.CCrawler cc in __map._AreaList[0]._evaCrawlers[0].GetCells())
        {
            int[,] __room = cc.GetRoomData();
            Texture2D __t2d = new Texture2D(__room.GetLength(0) * (__cellSize - __borderSize) + __borderSize,
                __room.GetLength(1) * (__cellSize - __borderSize) + __borderSize, TextureFormat.RGBA32, false);

            for (int x = 0; x < __t2d.width; x++)
            {
                for (int y = 0; y < __t2d.height; y++)
                    __t2d.SetPixel(x, y, sd);

            }

            for (int x = 0; x < __room.GetLength(0); x++)
            {
                for (int y = 0; y < __room.GetLength(1); y++)
                {
                    if (__room[x, y] > 0 &&
                        (__room[x, y] & (int)EWallFlag.UpWall) == 0 &&
                        (__room[x, y] & (int)EWallFlag.LeftWall) == 0)
                        __t2d.SetPixels32(x * (__cellSize - __borderSize), y * (__cellSize - __borderSize), __block.GetSize(),
                            __block.GetSize(), __block.GetColorBlock((EWallFlag)__room[x, y]), 0);
                }

                for (int y = 0; y < __room.GetLength(1); y++)
                {
                    if (__room[x, y] > 0 &&
                        ((__room[x, y] & (int)EWallFlag.UpWall) != 0 ||
                        (__room[x, y] & (int)EWallFlag.LeftWall) != 0))
                        __t2d.SetPixels32(x * (__cellSize - __borderSize), y * (__cellSize - __borderSize), __block.GetSize(),
                            __block.GetSize(), __block.GetColorBlock((EWallFlag)__room[x, y]), 0);
                }
            }
            __t2d.Apply();

            GameObject g = Instantiate(_prefab) as GameObject;
            CRoomRender obj = g.GetComponent<CRoomRender>();
            obj.SetTexture(__t2d);
            obj.SetData(cc);
        }
        Debug.Log("tttt:" + __map._AreaList[0]._evaCrawlers[0].GetCrawlerCount());
	}
	
	// Update is called once per frame
	void Update () {
        //return;
        //if (Input.GetKeyUp(KeyCode.Space))
        //{
        //    CMapCreator.CArea.CCrawler cc = __map._AreaList[0].update();
        //    if (cc != null)
        //    {
        //        int[,] __room = cc.GetRoomData();
        //        Texture2D __t2d = new Texture2D(__room.GetLength(0) * (__cellSize - __borderSize) + __borderSize,
        //            __room.GetLength(1) * (__cellSize - __borderSize) + __borderSize, TextureFormat.RGBA32, false);

        //        for (int x = 0; x < __t2d.width; x++)
        //        {
        //            for (int y = 0; y < __t2d.height; y++)
        //                __t2d.SetPixel(x, y, sd);

        //        }

        //        for (int x = 0; x < __room.GetLength(0); x++)
        //        {
        //            for (int y = 0; y < __room.GetLength(1); y++)
        //            {
        //                if (__room[x, y] > 0 &&
        //                    (__room[x, y] & (int)EWallFlag.UpWall) == 0 &&
        //                    (__room[x, y] & (int)EWallFlag.LeftWall) == 0)
        //                    __t2d.SetPixels32(x * (__cellSize - __borderSize), y * (__cellSize - __borderSize), __block.GetSize(),
        //                        __block.GetSize(), __block.GetColorBlock((CRoomCellTypeManager.EWallFlag)__room[x, y]), 0);
        //            }

        //            for (int y = 0; y < __room.GetLength(1); y++)
        //            {
        //                if (__room[x, y] > 0 &&
        //                    ((__room[x, y] & (int)EWallFlag.UpWall) != 0 ||
        //                    (__room[x, y] & (int)EWallFlag.LeftWall) != 0))
        //                    __t2d.SetPixels32(x * (__cellSize - __borderSize), y * (__cellSize - __borderSize), __block.GetSize(),
        //                        __block.GetSize(), __block.GetColorBlock((CRoomCellTypeManager.EWallFlag)__room[x, y]), 0);
        //            }
        //        }
        //        __t2d.Apply();

        //        GameObject g = Instantiate(_prefab) as GameObject;
        //        CRoomRender obj = g.GetComponent<CRoomRender>();
        //        obj.SetTexture(__t2d);
        //        obj.SetData(cc);

        //    }
        //}
	}

    void OnGUI()
    {
    }
}
