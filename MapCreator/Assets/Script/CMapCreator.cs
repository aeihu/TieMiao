using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RL
{
    public class CMapCreator
    {
        public enum EWallFlag
        {
            None = 0,          // 000000000
            Space = 1,         // 000000001
            UpWall = 2,        // 000000010
            UpDoor = 4,        // 000000100
            LeftWall = 8,      // 000001000
            LeftDoor = 16,     // 000010000
            BottomWall = 32,   // 000100000
            BottomDoor = 64,   // 001000000
            RightWall = 128,   // 010000000
            RightDoor = 256,   // 100000000
            AllWall = 171,     // 010101011
            AllWallDoor = 511, // 111111111
        }
        public class CVector2i
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

            //public static bool operator ==(CVector2i lvec, CVector2i rvec)
            //{
            //    return lvec._X == rvec._X && lvec._Y == rvec._Y;
            //}
            //public static bool operator !=(CVector2i lvec, CVector2i rvec)
            //{
            //    return lvec._X != rvec._X || lvec._Y != rvec._Y;
            //}

            public CVector2i(int x, int y)
            {
                _X = x;
                _Y = y;
            }
            public CVector2i()
            {
            }
        }

        public class CArea
        {
            private int[,] _space = null;
            private int _roomNum;
            private int _index = 0;

            public int _Width_r
            {
                get;
                private set;
            }
            public int _Height_r
            {
                get;
                private set;
            }


            public List<CCrawler> _evaCrawlers = new List<CCrawler>();
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
                int __downIndex = _Height_r;
                int __LeftIndex = 0;
                int __RightIndex = _Width_r;
                bool __flag = false;

                for (int i = 0; i < _Width_r; i++)
                {
                    for (int j = 0; j < _Height_r; j++)
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
                for (int i = _Width_r - 1; i >= 0; i--)
                {
                    for (int j = 0; j < _Height_r; j++)
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
                for (int j = 0; j < _Height_r; j++)
                {
                    for (int i = 0; i < _Width_r; i++)
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
                for (int j = _Height_r - 1; j >= 0; j--)
                {
                    for (int i = 0; i < _Width_r; i++)
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

                _Width_r = __RightIndex - __LeftIndex;
                _Height_r = __downIndex - __upIndex;

                for (int i = 0; i < _Width_r; i++)
                    for (int j = 0; j < _Height_r; j++)
                        _tempSpace[i, j] = _space[__LeftIndex + i, __upIndex + j];

                _space = _tempSpace;
                
                foreach (CCrawler crawler in _evaCrawlers)
                {
                    crawler.clip(__LeftIndex, __upIndex);
                }
            }
            public CCrawler update()
            {
                CCrawler __result = null;
                for (int i = _aliveCrawlers.Count - 1; i >= 0; i--)
                {
                    while (_aliveCrawlers[i].Move() != CCrawler.ECaseOfMove.Death)
                    //switch (_aliveCrawlers[i].Move())
                    {
                      //  case CCrawler.ECaseOfMove.Burrow:
                       // case CCrawler.ECaseOfMove.Death:
                            //break;
                    }
                    __result = _aliveCrawlers[i];
                    _aliveCrawlers.RemoveAt(i);
                }

                if (_aliveCrawlers.Count < 1)
                {
                    ///_aliveCrawlers.AddRange(_hatchingCrawlers);
                    //_hatchingCrawlers.Clear();
                    if (_hatchingCrawlers.Count > 0)
                    {
                        _index++;
                        _hatchingCrawlers[0].SetId(_index);
                        _aliveCrawlers.Add(_hatchingCrawlers[0]);
                        _hatchingCrawlers.RemoveAt(0);
                    }
                    else
                    {
                        //clipArea();
                    }
                }

                return __result;
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
                if (pos._Y >= _Height_r || pos._X >= _Width_r || pos._Y < 0 || pos._X < 0)
                    return 0;

                return _space[pos._X, pos._Y];
            }
            internal bool SetCell(CVector2i pos, int val)
            {
                if (pos._Y >= _Height_r || pos._X >= _Width_r || pos._Y < 0 || pos._X < 0)
                    return false;

                _space[pos._X, pos._Y] = val;
                return true;
            }

            // 获取指定坐标四周（上下左右）为空（等于0）的坐标集
            internal bool GetCellValidSpaceInAround(CVector2i pos)
            {
                if (pos._Y >= _Height_r || pos._X >= _Width_r || pos._Y < 0 || pos._X < 0)
                    return false;

                return _space[pos._X, pos._Y] < 1;
            }

            // 获取指定坐标四周（上下左右）不为空（等于0）的坐标集
            internal bool GetCellValidValueInAround(CVector2i pos)
            {
                if (pos._Y >= _Height_r || pos._X >= _Width_r || pos._Y < 0 || pos._X < 0)
                    return false;

                return _space[pos._X, pos._Y] > 0;
            }
            public void ResetArea(int w, int h, int rooms, int crawlers)
            {
                _Width_r = w;
                _Height_r = h;
                _space = new int[w, h];
                _roomNum = rooms;
                _evaCrawlers.Clear();
                _hatchingCrawlers.Clear();
                _aliveCrawlers.Clear();

                for (int i = 0; i < crawlers; i++)
                {
                    _evaCrawlers.Add(CCrawler.CreateEva(this));
                }
            }

            public void PrintArea()
            {

                //StreamWriter stream = new StreamWriter(string.Format(@"{0}-{1}-{2} {3}{4}{5}.log",  
                //    System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day,
                //    System.DateTime.Now.Hour, System.DateTime.Now.Minute, System.DateTime.Now.Second), true);

                //for (int h = 0; h < _Height_r; h++)
                //{
                //    for (int w = 0; w < _Width_r; w++)
                //    {
                //        switch (_space[w, h])
                //        {
                //            case (int)CArea.EUnitType.Empty:
                //                stream.Write("...");
                //                break;
                //            case (int)CArea.EUnitType.Origin:
                //                stream.Write("***");
                //                break;
                //            default:
                //                stream.Write(_space[w, h].ToString("D3"));
                //                break;
                //        }
                //    }

                //    stream.WriteLine("");
                //}
                //stream.WriteLine("Count: " + _evaCrawlers[0].GetCrawlerCount().ToString());
                //stream.Flush();
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
                    public EWallFlag _Flag = EWallFlag.AllWall;
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
                public CVector2i Position
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

                private int _burrowRate;

                public int _id = 0;
                public int _hp = 0;
                public int _generation = 1;
                public int _seeds = 0;
                public int _eggs = 0;
                private CArea _area = null;
                public CCrawler _mother = null;
                public System.Random _random = null;
                public List<CCrawler> _children = null;
                public List<CCellData> _footMarks = null;

                private delegate bool _func(CVector2i pos);
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
                    //return new int[4,3]{
                    //    {0,43,0},    
                    //    {11,33,0},    
                    //    {131,1,41},    
                    //    {0,131,161}};

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

                public void clip(int left, int up)
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

                public void Print()
                {
                    Console.Write("hp:" + GetHP().ToString());
                    Console.Write(" generation:" + _generation.ToString());
                    Console.WriteLine("");
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
                                    _footMarks[0]._Flag |= EWallFlag.LeftDoor;
                                    _mother._footMarks[index]._Flag |= EWallFlag.RightDoor;
                                    _area.SetCell(_mother._footMarks[index]._Pos, (int)_mother._footMarks[index]._Flag);
                                    __isBreak = true;
                                    break;
                                }
                                if (_footMarks[0]._Pos._X + 1 == _mother._footMarks[index]._Pos._X &&
                                    _mother._footMarks[index]._Pos._Y == _footMarks[0]._Pos._Y)
                                {
                                    _footMarks[0]._Flag |= EWallFlag.RightDoor;
                                    _mother._footMarks[index]._Flag |= EWallFlag.LeftDoor;
                                    _area.SetCell(_mother._footMarks[index]._Pos, (int)_mother._footMarks[index]._Flag);
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
                                        _footMarks[0]._Flag |= EWallFlag.UpDoor;
                                        _mother._footMarks[index]._Flag |= EWallFlag.BottomDoor;
                                        _area.SetCell(_mother._footMarks[index]._Pos, (int)_mother._footMarks[index]._Flag);
                                        break;
                                    }
                                    if (_footMarks[0]._Pos._Y + 1 == _mother._footMarks[index]._Pos._Y &&
                                        _mother._footMarks[index]._Pos._X == _footMarks[0]._Pos._X)
                                    {
                                        _footMarks[0]._Flag |= EWallFlag.BottomDoor;
                                        _mother._footMarks[index]._Flag |= EWallFlag.UpDoor;
                                        _area.SetCell(_mother._footMarks[index]._Pos, (int)_mother._footMarks[index]._Flag);
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
                                if ((_footMarks[i]._Flag & EWallFlag.LeftWall) != 0 &&
                                    _footMarks[i]._Pos._X - 1 == _footMarks[j]._Pos._X &&
                                    _footMarks[i]._Pos._Y == _footMarks[j]._Pos._Y)
                                {
                                    _footMarks[i]._Flag -= EWallFlag.LeftWall;
                                    continue;
                                }

                                if ((_footMarks[i]._Flag & EWallFlag.RightWall) != 0 &&
                                    _footMarks[i]._Pos._X + 1 == _footMarks[j]._Pos._X &&
                                    _footMarks[i]._Pos._Y == _footMarks[j]._Pos._Y)
                                {
                                    _footMarks[i]._Flag -= EWallFlag.RightWall;
                                    continue;
                                }

                                if ((_footMarks[i]._Flag & EWallFlag.UpWall) != 0 &&
                                    _footMarks[i]._Pos._Y - 1 == _footMarks[j]._Pos._Y &&
                                    _footMarks[i]._Pos._X == _footMarks[j]._Pos._X)
                                {
                                    _footMarks[i]._Flag -= EWallFlag.UpWall;
                                    continue;
                                }

                                if ((_footMarks[i]._Flag & EWallFlag.BottomWall) != 0 &&
                                    _footMarks[i]._Pos._Y + 1 == _footMarks[j]._Pos._Y &&
                                    _footMarks[i]._Pos._X == _footMarks[j]._Pos._X)
                                {
                                    _footMarks[i]._Flag -= EWallFlag.BottomWall;
                                }
                            }
                        }
                        _area.SetCell(_footMarks[i]._Pos, (int)_footMarks[i]._Flag);
                        #endregion

                        #region 把房间坐标设置为房间最左上点的cell坐标
                        if (_position._X > _footMarks[i]._Pos._X) _position._X = _footMarks[i]._Pos._X;
                        if (_position._Y > _footMarks[i]._Pos._Y) _position._Y = _footMarks[i]._Pos._Y;
                        #endregion
                    }
                }
                private List<CVector2i> GetValidSpace(_func func)
                {
                    return GetValidSpace(func, Position);
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

                public CCrawler(ref CArea area)
                {
                    _area = area;
                    _generation = 1;
                    _random = new System.Random();
                    _footMarks = new List<CCellData>();
                    _children = new List<CCrawler>();
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
                    if (mother != null)
                    {
                        _mother = mother;
                        _generation = mother._generation + 1;
                    }

                    if (pos == null)
                    {
                        if (_area == null)
                            return;

                        //_position = new CVector2i(_random.Next(_area.GetWidth()), _random.Next(_area.GetHeight()));
                        Position = new CVector2i(_area._Width_r / 2, _area._Height_r / 2);
                    }
                    else
                    {
                        Position = pos;
                    }

                    CCellData __footMark = new CCellData();
                    __footMark._Pos = Position;
                    _footMarks.Add(__footMark);

                    _area.SetCell(Position, 1);

                    _hp = _random.Next(1, 15);
                    _hp += _random.Next(2);
                    _hp += _random.Next(2);
                    _hp += _random.Next(2);
                    _area._hatchingCrawlers.Add(this);
                    _area._crawlers.Add(this);
                    _seeds = seeds;

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
                }
                public bool Lay()
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
                                //need print log
                                Console.Write("oooooooooooooooooooooooooooooooooooo");
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
                        Position = _footMarks[_footMarks.Count - 1]._Pos;
                        _area.SetCell(Position, 1);
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
                                Position = _footMarks[__index]._Pos;
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
                            Lay();
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
