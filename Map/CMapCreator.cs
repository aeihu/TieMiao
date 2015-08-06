using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RL
{
    class CMapCreator
    {
        internal class CVector2i
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
            public enum EUnitType : int
            {
                Empty = 0,
                Origin = 10000,
                //Horizontal = 10001,
                //Vertical = 10002,
                //Death,┌└┐┘
                //Burrow,
            }

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


            private List<CCrawler> _evaCrawlers = new List<CCrawler>();
            private List<CCrawler> _hatchingCrawlers = new List<CCrawler>();
            private List<CCrawler> _aliveCrawlers = new List<CCrawler>();
            public void update()
            {

                for (int i = _aliveCrawlers.Count - 1; i >= 0; i--)
                {
                    _aliveCrawlers[i].Print();
                    switch (_aliveCrawlers[i].Move())
                    {
                        case CCrawler.ECaseOfMove.Burrow:
                        case CCrawler.ECaseOfMove.Death:
                            _aliveCrawlers[i].Lay();
                            _aliveCrawlers.RemoveAt(i);
                            break;
                    }
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
                }
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

            internal bool GetCellValidSpaceInAround(CVector2i pos)
            {
                if (pos._Y >= _Height_r || pos._X >= _Width_r || pos._Y < 0 || pos._X < 0)
                    return false;

                return _space[pos._X, pos._Y] < 1;
            }
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
                for (int h = 0; h < _Height_r; h++)
                {
                    for (int w = 0; w < _Width_r; w++)
                    {
                        switch (_space[w, h])
                        {
                            case (int)CArea.EUnitType.Empty:
                                Console.Write("..");
                                break;
                            case (int)CArea.EUnitType.Origin:
                                Console.Write("**");
                                break;
                            default:
                                Console.Write(_space[w, h].ToString("D2"));
                                break;
                        }
                    }
                    
                    Console.WriteLine("");
                }
                Console.WriteLine("Count: " + _evaCrawlers[0].GetCrawlerCount().ToString());
            }

            private class CCrawler
            {
                public enum ECaseOfMove
                {
                    Normal = 0,
                    Lay,
                    Burrow,
                    Death,
                }

                #region Overmind of Crawler
                public static CCrawler CreateEva(CArea area)
                {
                    CCrawler __eva = new CCrawler(ref area);
                    __eva.Born(null, null, area._roomNum - 1);
                    return __eva;
                }
                #endregion

                private int _burrowRate;

                private int _id = 0;
                private CVector2i _position;
                private int _hp = 0;
                private int _generation = 1;
                private int _seeds = 0;
                private int _eggs = 0;
                private CArea _area = null;
                private CCrawler _mother = null;
                private Random _random = null;
                private List<CVector2i> _eggMarks = null;
                private List<CCrawler> _children = null;
                private List<CVector2i> _footMarks = null;

                private delegate bool _func(CVector2i pos);

                public int GetChildrenCount()
                {
                    if (_children.Count == 0)
                        return 0;
                    else
                    {
                        int __result = _children.Count;
                        for (int i = 0; i<_children.Count; i++)
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
                    Console.Write(" egg:" + _eggMarks.Count.ToString());
                    Console.WriteLine("");
                    Console.Write("mark:" + _eggMarks.Count.ToString());
                    for (int i = 0; i < _eggMarks.Count; i++)
                    {
                        Console.Write(string.Format("({0}:{1}) ", _eggMarks[i]._X, _eggMarks[i]._Y));
                    }
                    Console.WriteLine("");
                }
                public void SetId(int id)
                {
                    _id = id;
                }
                private List<CVector2i> GetValidSpace(_func func)
                {
                    return GetValidSpace(func, _position);
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
                    {
                        for (int i = 0; i < _footMarks.Count; i++)
                        {
                            __result.AddRange(GetValidSpace(_area.GetCellValidSpaceInAround, _footMarks[i]));
                        }
                    }

                    return __result.Distinct().ToList();
                }

                public CCrawler(ref CArea area)
                {
                    _area = area;
                    _generation = 1;
                    _random = new Random();
                    _footMarks = new List<CVector2i>();
                    _children = new List<CCrawler>();
                    _eggMarks = new List<CVector2i>();
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
                        _position = new CVector2i(_area._Width_r/2, _area._Height_r/2);
                    }
                    else
                    {
                        _position = pos;
                    }
                    _footMarks.Add(_position);
                    _area.SetCell(_position, (int)CArea.EUnitType.Origin);

                    _hp = _random.Next(1,3);
                    _hp += _random.Next(2);
                    _hp += _random.Next(2);
                    _hp += _random.Next(2);
                    _area._hatchingCrawlers.Add(this);
                    _seeds = seeds;

                    if (_seeds < 1)
                    { 
                        _eggs = 0;
                    }
                    else { 
                        if (seeds < 4)
                            _eggs = _random.Next(1, _seeds);
                        else
                            _eggs = _random.Next(1, 4); //need fixed

                        _seeds -= _eggs;
                    }
                }
                public bool Lay()
                {
                    CCrawler __crawler = this;
                    List<CVector2i> __validPos = __crawler.GetTheCrawlerValidSpace();

                    while (GetEgg() > 0)
                    {
                        if (__validPos.Count > 0)
                        {
                            int __index = _random.Next(__validPos.Count);
                            int __inheritSeeds = GetEgg() == 1 ? __crawler._seeds : _random.Next(__crawler._seeds);
                            __crawler._seeds -= __inheritSeeds;

                            CCrawler __child = new CCrawler(ref _area);
                            _children.Add(__child);
                            __child.Born(__crawler, __validPos[__index], __inheritSeeds);
                            _eggMarks.Add(__validPos[__index]);
                            __validPos.RemoveAt(__index);
                        }
                        else 
                        {
                            if (__crawler._mother != null)
                            {
                                __crawler = __crawler._mother;
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
                private bool processOfMove(List<CVector2i> validPos)
                {
                    if (validPos.Count > 0)
                    {
                        _footMarks.Add(validPos[_random.Next(validPos.Count)]);
                        _position = _footMarks[_footMarks.Count - 1];
                        _area.SetCell(_position, _id);
                        return true;
                    }

                    return false;
                }

                public ECaseOfMove Move()
                {
                    if (GetHP()>0)
                    {
                        List<CVector2i> __validPos = GetValidSpace(_area.GetCellValidSpaceInAround);

                        if (!processOfMove(__validPos))
                        {
                            int __index = _footMarks.Count - 2;

                            while (__index >= 0)
                            {
                                _position = _footMarks[__index];
                                __validPos = GetValidSpace(_area.GetCellValidSpaceInAround);

                                if (processOfMove(__validPos))
                                    return ECaseOfMove.Normal;

                                __index--;
                            }
                            return ECaseOfMove.Death;
                        }

                        return ECaseOfMove.Normal;
                    }

                    return ECaseOfMove.Death;
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
