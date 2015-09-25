
/*
* Copyright (C) 2015, <Aeihu.z, aeihu.z@gmail.com>.
*
* TieMiao is a free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* Version 3(GPLv3) as published by the Free Software Foundation.
*/

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TieMiao
{
    public enum ERoomFlag : int
    {
        None = 0,          // 0000000000
        Space = 1,         // 0000000001
        UpWall = 2,        // 0000000010
        UpDoor = 4,        // 0000000100
        LeftWall = 8,      // 0000001000
        LeftDoor = 16,     // 0000010000
        BottomWall = 32,   // 0000100000
        BottomDoor = 64,   // 0001000000
        RightWall = 128,   // 0010000000
        RightDoor = 256,   // 0100000000
        Lock = 512,        // 1000000000
        AllWall = 171,     // 0010101011
        AllWallDoor = 511, // 0111111111
    }

    public class CMapCreator
    {
        public class CVector2i : IEquatable<CVector2i>  
        {
            public int _X
            {
                get;
                set;
            }
            public int _Y
            {
                get;
                set;
            }
            public static CVector2i operator +(CVector2i lvec, CVector2i rvec)
            {
                CVector2i __result = new CVector2i();
                __result._X = lvec._X + rvec._X;
                __result._Y = lvec._Y + rvec._Y;
                return __result;
            }
            public static CVector2i operator -(CVector2i lvec, CVector2i rvec)
            {
                CVector2i __result = new CVector2i();
                __result._X = lvec._X - rvec._X;
                __result._Y = lvec._Y - rvec._Y;
                return __result;
            } 
            public bool Equals(CVector2i other)  
            {  
                if (System.Object.ReferenceEquals(other, null)) return false;  
                if (System.Object.ReferenceEquals(this, other)) return true;  
  
                return _X.Equals(other._X) && _Y.Equals(other._Y);  
            }
            public override int GetHashCode()
            {
 
                int hash_X = _X.GetHashCode();
                int hash_Y = _Y.GetHashCode();

                return hash_X ^ hash_Y;
            } 

            public CVector2i(int x, int y)
            {
                _X = x;
                _Y = y;
            }
            public CVector2i()
            {
                _Y = _X = 0;
            }
        }

        public class CArea
        {
            private int[,] _space = null;
            private int _roomNum;
            private int _index = 0;

            public int _Width
            {
                get;
                private set;
            }
            public int _Height
            {
                get;
                private set;
            }

            //public List<CCrawler> _evaCrawlers = new List<CCrawler>();
            public CCrawler _evaCrawler = null;
            public List<CCrawler> _crawlers = new List<CCrawler>();
            private List<CCrawler> _hatchingCrawlers = new List<CCrawler>();
            private List<CCrawler> _aliveCrawlers = new List<CCrawler>();

            /*裁切多余的区域
             * 例：
             * 裁切前：
             * 000000
             * 001200
             * 000340
             * 000000
             * 
             * 裁切后：
             * 120
             * 034
            */
            private void clipArea()
            {
                int __upIndex = 0;
                int __downIndex = _Height;
                int __LeftIndex = 0;
                int __RightIndex = _Width;
                bool __flag = false;

                for (int i = 0; i < _Width; i++)
                {
                    for (int j = 0; j < _Height; j++)
                        if (_space[i, j] != 0)
                        {
                            __flag = true;
                            break;
                        }

                    if (__flag)
                        break;

                    __LeftIndex++;
                }

                __flag = false;
                for (int i = _Width - 1; i >= 0; i--)
                {
                    for (int j = 0; j < _Height; j++)
                        if (_space[i, j] != 0)
                        {
                            __flag = true;
                            break;
                        }

                    if (__flag)
                        break;

                    __RightIndex--;

                    if (__LeftIndex >= __RightIndex)
                        return;
                }

                __flag = false;
                for (int j = 0; j < _Height; j++)
                {
                    for (int i = 0; i < _Width; i++)
                        if (_space[i, j] != 0)
                        {
                            __flag = true;
                            break;
                        }

                    if (__flag)
                        break;

                    __upIndex++;
                }

                __flag = false;
                for (int j = _Height - 1; j >= 0; j--)
                {
                    for (int i = 0; i < _Width; i++)
                        if (_space[i, j] != 0)
                        {
                            __flag = true;
                            break;
                        }

                    if (__flag)
                        break;

                    __downIndex--;

                    if (__upIndex >= __downIndex)
                        return;
                }

                int[,] _tempSpace = new int[__RightIndex - __LeftIndex, __downIndex - __upIndex];

                _Width = __RightIndex - __LeftIndex;
                _Height = __downIndex - __upIndex;

                for (int i = 0; i < _Width; i++)
                    for (int j = 0; j < _Height; j++)
                        _tempSpace[i, j] = _space[__LeftIndex + i, __upIndex + j];

                _space = _tempSpace;

                _evaCrawler.clip(__LeftIndex, __upIndex);
            }

            public int getDistance(int crawlerAId, int crawlerBId)
            {
                if (crawlerAId == crawlerBId)
                    return 0;

                CCrawler __crawlerA = null;
                CCrawler __crawlerB = null;

                foreach (CCrawler crawler in _crawlers)
                {
                    if (crawler._Id == crawlerAId)
                    {
                        __crawlerA = crawler;
                    }

                    if (crawler._Id == crawlerBId)
                    {
                        __crawlerB = crawler;
                    }

                    if (__crawlerA != null && __crawlerB != null)
                        break;
                }

                if (__crawlerA == null || __crawlerB == null)
                    return -1;

                int __curGeneration = __crawlerA._Generation > __crawlerB._Generation ? __crawlerA._Generation : __crawlerB._Generation;
                int __count = 0;
                do
                {
                    if (__crawlerA._Generation == __curGeneration)
                    {
                        __crawlerA = __crawlerA.GetMother();
                        __count++;
                    }

                    if (__crawlerB._Generation == __curGeneration)
                    {
                        __crawlerB = __crawlerB.GetMother();
                        __count++;
                    }

                    if (__crawlerA._Id == __crawlerB._Id)
                        break;

                    __curGeneration--;

                    if (__curGeneration < 1)
                        return -1;

                }while (true);

                return __count;
            }

            public int[,] Produce()
            {
                while (true)
                {
                    for (int i = _aliveCrawlers.Count - 1; i >= 0; i--)
                    {
                        switch (_aliveCrawlers[i].Move())
                        {
                            case CCrawler.ECaseOfMove.Burrow:
                            case CCrawler.ECaseOfMove.Death:
                                _aliveCrawlers.RemoveAt(i);
                                break;
                        }
                    }

                    if (_aliveCrawlers.Count < 1)
                    {
                        if (_hatchingCrawlers.Count > 0)
                        {
                            _index++;
                            _hatchingCrawlers[0].SetId(_index);
                            _aliveCrawlers.Add(_hatchingCrawlers[0]);
                            _hatchingCrawlers.RemoveAt(0);
                        }
                        else
                        {
                            clipArea();
                            break;
                        }
                    }
                }

                return _space;
            }
            public int GetCell(CVector2i pos)
            {
                if (pos._Y >= _Height || pos._X >= _Width || pos._Y < 0 || pos._X < 0)
                    return 0;

                return _space[pos._X, pos._Y];
            }
            internal bool SetCell(CVector2i pos, ERoomFlag val)
            {
                if (pos._Y >= _Height || pos._X >= _Width || pos._Y < 0 || pos._X < 0)
                    return false;

                _space[pos._X, pos._Y] = (int)val;
                return true;
            }

            // 获取指定坐标四周（上下左右）为空（等于0）的坐标集
            internal bool GetCellValidSpaceInAround(CVector2i pos)
            {
                if (pos._Y >= _Height || pos._X >= _Width || pos._Y < 0 || pos._X < 0)
                    return false;

                return _space[pos._X, pos._Y] < 1;
            }

            // 获取指定坐标四周（上下左右）不为空（等于0）的坐标集
            internal bool GetCellValidValueInAround(CVector2i pos)
            {
                if (pos._Y >= _Height || pos._X >= _Width || pos._Y < 0 || pos._X < 0)
                    return false;

                return _space[pos._X, pos._Y] > 0;
            }
            public void ResetArea(int w, int h, int rooms)
            {
                _Width = w;
                _Height = h;
                _space = new int[w, h];
                _roomNum = rooms;
                _evaCrawler = CCrawler.CreateEva(this);
                _hatchingCrawlers.Clear();
                _aliveCrawlers.Clear();
                _crawlers.Clear();
            }

            public class CCrawler
            {
                public enum ECaseOfMove
                {
                    Normal = 0,
                    Lay,
                    Burrow,
                    Death,
                }

                public class CCellData
                {
                    public CVector2i _Pos;
                    public ERoomFlag _Flag = ERoomFlag.AllWall;
                }

                #region Overmind of Crawler
                public static CCrawler CreateEva(CArea area)
                {
                    CCrawler __eva = new CCrawler(ref area);
                    __eva.Born(null, null, area._roomNum - 1);
                    return __eva;
                }
                #endregion

                private CVector2i _position = new CVector2i();
                public CVector2i _Position
                {
                    get
                    {
                        return new CVector2i(_position._X, _position._Y);
                    }
                    set 
                    {
                        _position._X = value._X;
                        _position._Y = value._Y;
                    }
                }
                public int _Id
                {
                    get {
                        return _id;
                    }
                }
                public bool _IsEva
                {
                    get
                    {
                        return _mother == null && _motherId == -1;
                    }
                }
                public int _Generation
                {
                    get
                    {
                        return _generation;
                    }
                }
                public int _MotherId
                {
                    get
                    {
                        return _motherId;
                    }
                }

                private int _burrowRate;

                private int _id = 0;
                private int _hp = 0;
                private int _generation = 1;
                private int _seeds = 0;
                private int _eggs = 0;
                private int _lockId = -1;
                private CArea _area = null;
                private CCrawler _mother = null;
                private int _motherId = -1;
                private System.Random _random = null;
                private List<CCrawler> _children = null;
                private List<CCellData> _footMarks = null;

                private delegate bool _func(CVector2i pos);
                public CCrawler(ref CArea area)
                {
                    _area = area;
                    _generation = 1;
                    _random = new System.Random();
                    _footMarks = new List<CCellData>();
                    _children = new List<CCrawler>();
                }

                public bool Unlock(List<int> keyRing)
                {
                    if (_lockId < 0)
                        return true;

                    foreach (int key in keyRing)
                    {
                        if (key == _lockId)
                            return true;
                    }

                    return false;
                }

                public void Separation()
                {
                    if (_mother != null)
                    {
                        CCrawler __mother = _mother;
                        _mother = null;
                        __mother._children.Remove(this);
                    }
                }
                public bool Incorporation(CCrawler crawler)
                {
                    if (_mother == null)
                    {
                        if (crawler._Id == _motherId)
                        {
                            _mother = crawler;
                            crawler._children.Add(this);
                        }
                        else
                            return false;
                    }

                    return true;
                }

                public Dictionary<string, string> GetInfo()
                {
                    Dictionary<string, string> __result = new Dictionary<string, string>();

                    __result.Add("id", _id.ToString());
                    __result.Add("mother", _mother == null ? "0" : _mother._id.ToString());
                    __result.Add("generation", _generation.ToString());
                    __result.Add("position", string.Format("({0}, {1})", _Position._X, _Position._Y));
                    __result.Add("hp", _hp.ToString());
                    __result.Add("eggs", _eggs.ToString());
                    __result.Add("seeds", _seeds.ToString());
                    __result.Add("relation", string.Format("(G:{0},I:{1})", _generation, _id));

                    CCrawler __tmpCrawler = _mother;
                    while (__tmpCrawler != null)
                    {
                        __result["relation"] += string.Format("->(G:{0},I:{1})", __tmpCrawler._generation, __tmpCrawler._id);
                        __tmpCrawler = __tmpCrawler._mother;
                    }

                    __result.Add("footmarks", "");
                    for (int i = 0; i < _footMarks.Count; i++)
                    {
                        __result["footmarks"] += string.Format("  {0}: [x:{1}, y:{2}, flag:{3}]\n",
                            i.ToString("D2"),
                            _footMarks[i]._Pos._X,
                            _footMarks[i]._Pos._Y,
                            _footMarks[i]._Flag);
                    }

                    __result.Add("roomdata", "");
                    int[,] __room = GetRoomData();

                    for (int row = 0; row < __room.GetLength(1); row++)
                    {
                        for (int col = 0; col < __room.GetLength(0); col++)
                            __result["roomdata"] += string.Format(" {0} ", __room[col, row].ToString("D3"));

                        __result["roomdata"] += "\n";
                    }
                    return __result;
                }

                public CCrawler GetMother()
                {
                    return _mother;
                }

                public List<CCrawler> GetChildren()
                {
                    List<CCrawler> __result = new List<CCrawler>();
                    __result.AddRange(_children);
                    foreach (CCrawler child in _children)
                    {
                        __result.AddRange(child.GetChildren());
                    }

                    return __result;
                }
                public List<CCrawler> GetCells()
                {
                    List<CCrawler> __result = new List<CCrawler>();
                    __result.AddRange(GetChildren());
                    __result.Add(this);
                    return __result;
                }

                public int[,] GetRoomData()
                {
                    int __minX = 0;
                    int __maxX = 0;
                    int __minY = 0;
                    int __maxY = 0;
                    for (int i = 0; i < _footMarks.Count; i++)
                    {
                        if (i > 0)
                        {
                            if (__minX > _footMarks[i]._Pos._X) __minX = _footMarks[i]._Pos._X;
                            if (__minY > _footMarks[i]._Pos._Y) __minY = _footMarks[i]._Pos._Y;

                            if (__maxX < _footMarks[i]._Pos._X) __maxX = _footMarks[i]._Pos._X;
                            if (__maxY < _footMarks[i]._Pos._Y) __maxY = _footMarks[i]._Pos._Y;
                        }
                        else
                        {
                            __maxX = __minX = _footMarks[i]._Pos._X;
                            __maxY = __minY = _footMarks[i]._Pos._Y;
                        }
                    }

                    int[,] __result = new int[1 + (__maxX - __minX), 1 + (__maxY - __minY)];

                    foreach (CCellData cell in _footMarks)
                    {
                        __result[cell._Pos._X - __minX, cell._Pos._Y - __minY] = (int)cell._Flag;
                    }
                    return __result;
                }

                internal void clip(int left, int up)
                {
                    foreach (CCellData cell in _footMarks)
                    {
                        cell._Pos._X -= left;
                        cell._Pos._Y -= up;

                    }
                    _position._X -= left;
                    _position._Y -= up;

                    foreach (CCrawler crawler in _children)
                        crawler.clip(left, up);
                }
                public int GetChildrenCount()
                {
                    if (_children.Count == 0)
                        return 0;
                    else
                    {
                        int __result = _children.Count;
                        for (int i = 0; i < _children.Count; i++)
                        {
                            __result += _children[i].GetChildrenCount();
                        }

                        return __result;
                    }
                }
                public int GetCrawlerCount()
                {
                    return GetChildrenCount() + 1;
                }

                public void SetId(int id)
                {
                    _id = id;
                }
                private void afterDeath()
                {
                    for (int i = 0; i < _footMarks.Count; i++)
                    {
                        #region 设置每个cell的门
                        if (i == 0 && _mother != null)
                        {
                            bool __isBreak = false;
                            for (int index = _mother._footMarks.Count - 1; index >= 0; index--)
                            {
                                if (_footMarks[0]._Pos._X - 1 == _mother._footMarks[index]._Pos._X &&
                                    _mother._footMarks[index]._Pos._Y == _footMarks[0]._Pos._Y)
                                {
                                    _footMarks[0]._Flag |= ERoomFlag.LeftDoor;
                                    _mother._footMarks[index]._Flag |= ERoomFlag.RightDoor;
                                    _area.SetCell(_mother._footMarks[index]._Pos, _mother._footMarks[index]._Flag);
                                    __isBreak = true;
                                    break;
                                }
                                if (_footMarks[0]._Pos._X + 1 == _mother._footMarks[index]._Pos._X &&
                                    _mother._footMarks[index]._Pos._Y == _footMarks[0]._Pos._Y)
                                {
                                    _footMarks[0]._Flag |= ERoomFlag.RightDoor;
                                    _mother._footMarks[index]._Flag |= ERoomFlag.LeftDoor;
                                    _area.SetCell(_mother._footMarks[index]._Pos, _mother._footMarks[index]._Flag);
                                    __isBreak = true;
                                    break;
                                }
                            }

                            if (!__isBreak)
                                for (int index = _mother._footMarks.Count - 1; index >= 0; index--)
                                {
                                    if (_footMarks[0]._Pos._Y - 1 == _mother._footMarks[index]._Pos._Y &&
                                        _mother._footMarks[index]._Pos._X == _footMarks[0]._Pos._X)
                                    {
                                        _footMarks[0]._Flag |= ERoomFlag.UpDoor;
                                        _mother._footMarks[index]._Flag |= ERoomFlag.BottomDoor;
                                        _area.SetCell(_mother._footMarks[index]._Pos, _mother._footMarks[index]._Flag);
                                        break;
                                    }
                                    if (_footMarks[0]._Pos._Y + 1 == _mother._footMarks[index]._Pos._Y &&
                                        _mother._footMarks[index]._Pos._X == _footMarks[0]._Pos._X)
                                    {
                                        _footMarks[0]._Flag |= ERoomFlag.BottomDoor;
                                        _mother._footMarks[index]._Flag |= ERoomFlag.UpDoor;
                                        _area.SetCell(_mother._footMarks[index]._Pos, _mother._footMarks[index]._Flag);
                                        break;
                                    }
                                }
                        }
                        #endregion

                        #region 设置每个cell的墙
                        for (int j = 0; j < _footMarks.Count; j++)
                        {
                            if (i != j)
                            {
                                if ((_footMarks[i]._Flag & ERoomFlag.LeftWall) != 0 &&
                                    _footMarks[i]._Pos._X - 1 == _footMarks[j]._Pos._X &&
                                    _footMarks[i]._Pos._Y == _footMarks[j]._Pos._Y)
                                {
                                    _footMarks[i]._Flag -= ERoomFlag.LeftWall;
                                    continue;
                                }

                                if ((_footMarks[i]._Flag & ERoomFlag.RightWall) != 0 &&
                                    _footMarks[i]._Pos._X + 1 == _footMarks[j]._Pos._X &&
                                    _footMarks[i]._Pos._Y == _footMarks[j]._Pos._Y)
                                {
                                    _footMarks[i]._Flag -= ERoomFlag.RightWall;
                                    continue;
                                }

                                if ((_footMarks[i]._Flag & ERoomFlag.UpWall) != 0 &&
                                    _footMarks[i]._Pos._Y - 1 == _footMarks[j]._Pos._Y &&
                                    _footMarks[i]._Pos._X == _footMarks[j]._Pos._X)
                                {
                                    _footMarks[i]._Flag -= ERoomFlag.UpWall;
                                    continue;
                                }

                                if ((_footMarks[i]._Flag & ERoomFlag.BottomWall) != 0 &&
                                    _footMarks[i]._Pos._Y + 1 == _footMarks[j]._Pos._Y &&
                                    _footMarks[i]._Pos._X == _footMarks[j]._Pos._X)
                                {
                                    _footMarks[i]._Flag -= ERoomFlag.BottomWall;
                                }
                            }
                        }
                        _area.SetCell(_footMarks[i]._Pos, _footMarks[i]._Flag);
                        #endregion

                        #region 把房间坐标设置为房间最左上点的cell坐标
                        if (_position._X > _footMarks[i]._Pos._X) _position._X = _footMarks[i]._Pos._X;
                        if (_position._Y > _footMarks[i]._Pos._Y) _position._Y = _footMarks[i]._Pos._Y;
                        #endregion
                    }
                }
                private List<CVector2i> GetValidSpace(_func func)
                {
                    return GetValidSpace(func, _Position);
                }
                private List<CVector2i> GetValidSpace(_func func, CVector2i pos)
                {
                    List<CVector2i> __result = new List<CVector2i>();
                    if (_area == null)
                        return __result;

                    CVector2i __up = new CVector2i(0, 1) + pos;
                    CVector2i __down = new CVector2i(0, -1) + pos;
                    CVector2i __left = new CVector2i(-1, 0) + pos;
                    CVector2i __right = new CVector2i(1, 0) + pos;

                    if (func(__up)) __result.Add(__up);
                    if (func(__down)) __result.Add(__down);
                    if (func(__left)) __result.Add(__left);
                    if (func(__right)) __result.Add(__right);

                    return __result;
                }

                private List<CVector2i> GetTheCrawlerValidSpace()
                {
                    List<CVector2i> __result = new List<CVector2i>();

                    if (_footMarks != null)
                        for (int i = 0; i < _footMarks.Count; i++)
                            __result.AddRange(GetValidSpace(_area.GetCellValidSpaceInAround, _footMarks[i]._Pos));

                    return __result.Distinct().ToList();
                }

                public int GetHP()
                {
                    return _hp - _footMarks.Count;
                }
                public int GetEgg()
                {
                    return _eggs - (_children.Count);
                }

                public void Born(CCrawler mother, CVector2i pos, int seeds)
                {
                    #region 设置母节点和辈份
                    if (mother != null)
                    {
                        _mother = mother;
                        _motherId = mother._Id;
                        _generation = mother._generation + 1;
                    }
                    #endregion

                    #region 设置初始位置
                    if (pos == null)
                    {
                        if (_area == null)
                            return;

                        //_position = new CVector2i(_random.Next(_area.GetWidth()), _random.Next(_area.GetHeight()));
                        _Position = new CVector2i(_area._Width / 2, _area._Height / 2);
                    }
                    else
                    {
                        _Position = pos;
                    }
                    #endregion

                    #region 初始化移动轨迹和所在的空间的位置
                    CCellData __footMark = new CCellData();
                    __footMark._Pos = _Position;
                    _footMarks.Add(__footMark);

                    _area.SetCell(_Position, ERoomFlag.Space);
                    #endregion

                    #region 初始化生命值
                    _hp = _random.Next(1, 15);
                    _hp += _random.Next(2);
                    _hp += _random.Next(2);
                    _hp += _random.Next(2);
                    #endregion

                    _area._hatchingCrawlers.Add(this);
                    _area._crawlers.Add(this);

                    #region 设置此家族系的后代数
                    _seeds = seeds;
                    #endregion

                    #region 设置可生育child数
                    if (_seeds < 1)
                    {
                        _eggs = 0;
                    }
                    else
                    {
                        if (seeds < 4)
                            _eggs = _random.Next(1, _seeds+1);
                        else
                            _eggs = _random.Next(1, 4); //need fixed

                        _seeds -= _eggs;
                    }
                    #endregion

                    #region 设置房间锁的ID
                    _lockId = _random.Next(20) - 10;
                    _lockId = _lockId < 0 ? -1 : _lockId;
                    #endregion
                }
                private bool lay()
                {
                    CCrawler __crawler = this;
                    List<CVector2i> __validPos = __crawler.GetTheCrawlerValidSpace();

                    while (_eggs > 0)
                    {
                        if (__validPos.Count > 0)
                        {
                            int __index = _random.Next(__validPos.Count);
                            int __inheritSeeds = _eggs == 1 ? _seeds : _random.Next(_seeds+1);
                            _seeds -= __inheritSeeds;

                            CCrawler __child = new CCrawler(ref _area);
                            _eggs--;
                            __crawler._children.Add(__child);
                            __child.Born(__crawler, __validPos[__index], __inheritSeeds);
                            __validPos.RemoveAt(__index);
                        }
                        else
                        {
                            CCrawler __tmp = null;
                            for (int i = _area._crawlers.Count - 1; i >= 0; i--)
                            {
                                if (_area._crawlers[i]._footMarks.Count > 1)
                                {
                                    __validPos = _area._crawlers[i].GetTheCrawlerValidSpace();

                                    if (__validPos.Count > 0)
                                    {
                                        __tmp = _area._crawlers[i];
                                        break;
                                    }
                                }
                            }

                            if (__tmp != null)
                            {
                                __crawler = __tmp;
                                __validPos = __crawler.GetTheCrawlerValidSpace();
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }

                private bool burrow(List<CVector2i> validPos)
                {
                    if (validPos.Count > 0)
                    {
                        //CVector2i  s = validPos[_random.Next(validPos.Count)];
                        return true;
                    }

                    return false;
                }
                private bool processOfMoveing(List<CVector2i> validPos)
                {
                    if (validPos.Count > 0)
                    {
                        CCellData __footMark = new CCellData();
                        __footMark._Pos = validPos[_random.Next(validPos.Count)];
                        _footMarks.Add(__footMark);
                        _Position = _footMarks[_footMarks.Count - 1]._Pos;
                        _area.SetCell(_Position, ERoomFlag.Space);
                        return true;
                    }

                    return false;
                }
                private ECaseOfMove Moveing()
                {
                    if (GetHP() > 0)
                    {
                        List<CVector2i> __validPos = GetValidSpace(_area.GetCellValidSpaceInAround);

                        if (!processOfMoveing(__validPos))
                        {
                            int __index = _footMarks.Count - 2;

                            while (__index >= 0)
                            {
                                _Position = _footMarks[__index]._Pos;
                                __validPos = GetValidSpace(_area.GetCellValidSpaceInAround);

                                if (processOfMoveing(__validPos))
                                    return ECaseOfMove.Normal;

                                __index--;
                            }

                            return ECaseOfMove.Death;
                        }

                        return ECaseOfMove.Normal;
                    }

                    return ECaseOfMove.Death;
                }

                public ECaseOfMove Move()
                {
                    ECaseOfMove __result = Moveing();
                    switch (__result)
                    {
                        case CCrawler.ECaseOfMove.Burrow:
                        case CCrawler.ECaseOfMove.Death:
                            lay();
                            afterDeath();
                            break;
                    }

                    return __result;
                }
            }

        }

        public List<CArea> _AreaList = null;

        public CMapCreator()
        {
            _AreaList = new List<CArea>();
        }
    }
}
