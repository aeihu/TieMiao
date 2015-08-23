/*
* Copyright (C) 2015, <Aeihu.z, aeihu.z@gmail.com>.
*
* TieMiao is a free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* Version 3(GPLv3) as published by the Free Software Foundation.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TieMiao
{
    public class CRoomCellTypeManager
    {
        class RoomCellType
        {
            public RoomCellType(EWallFlag flag, int size, int borderSize, Color32 spaceColor, Color32 wallColor)
            {
                _size = size;
                _doorSize = size / 3;
                _borderSize = borderSize;
                _spaceColor = spaceColor;
                _wallColor = wallColor;
                _colorblock = new Color32[_size * _size];
                _Flag = flag;
            }

            #region 成员与属性
            int _borderSize = 1;
            int _size = 16;
            int _doorSize;
            Color32 _spaceColor;
            Color32 _wallColor;
            Color32[] _colorblock;
            EWallFlag _flag = EWallFlag.None;
            public EWallFlag _Flag
            {
                get
                {
                    return _flag;
                }
                set
                {
                    _flag = value;
                    DrawSpace();
                    DrawWall();
                }
            }

            #endregion

            #region 公有方法
            public Color32[] GetColorBlock()
            {
                return (Color32[])_colorblock.Clone();
            }
            #endregion

            #region 私有方法
            private void DrawSpace()
            {
                int __total = _size * _size;

                for (int i = 0; i < __total; i++)
                {
                    _colorblock[i].r = _spaceColor.r;
                    _colorblock[i].g = _spaceColor.g;
                    _colorblock[i].b = _spaceColor.b;
                    _colorblock[i].a = _spaceColor.a;
                }
            }

            private void DrawWall()
            {
                if ((_flag & EWallFlag.UpWall) != 0)
                    DrawUpWall();

                if ((_flag & EWallFlag.BottomWall) != 0)
                    DrawBottomWall();

                if ((_flag & EWallFlag.LeftWall) != 0)
                    DrawLeftWall();

                if ((_flag & EWallFlag.RightWall) != 0)
                    DrawRightWall();
            }
            private void DrawUpWall()
            {
                for (int col = 0; col < _size; col++)
                {
                    for (int row = 0; row < _borderSize; row++)
                    {
                        if ((_flag & EWallFlag.UpDoor) != 0 && col >= _doorSize && col < _doorSize << 1)
                            continue;

                        int __index = col + row * _size;
                        _colorblock[__index].r = _wallColor.r;
                        _colorblock[__index].g = _wallColor.g;
                        _colorblock[__index].b = _wallColor.b;
                        _colorblock[__index].a = _wallColor.a;
                    }
                }
            }
            private void DrawLeftWall()
            {
                for (int col = 0; col < _borderSize; col++)
                {
                    for (int row = 0; row < _size; row++)
                    {
                        if ((_flag & EWallFlag.LeftDoor) != 0 && row >= _doorSize && row < _doorSize << 1)
                            continue;

                        int __index = col + row * _size;
                        _colorblock[__index].r = _wallColor.r;
                        _colorblock[__index].g = _wallColor.g;
                        _colorblock[__index].b = _wallColor.b;
                        _colorblock[__index].a = _wallColor.a;
                    }
                }
            }
            private void DrawBottomWall()
            {
                for (int col = 0; col < _size; col++)
                {
                    for (int row = _size - 1; row >= _size - _borderSize; row--)
                    {
                        if ((_flag & EWallFlag.BottomDoor) != 0 && col >= _doorSize && col < _doorSize << 1)
                            continue;

                        int __index = col + row * _size;
                        _colorblock[__index].r = _wallColor.r;
                        _colorblock[__index].g = _wallColor.g;
                        _colorblock[__index].b = _wallColor.b;
                        _colorblock[__index].a = _wallColor.a;
                    }
                }
            }
            private void DrawRightWall()
            {
                for (int col = _size - 1; col >= _size - _borderSize; col--)
                {
                    for (int row = 0; row < _size; row++)
                    {
                        if ((_flag & EWallFlag.RightDoor) != 0 && row >= _doorSize && row < _doorSize << 1)
                            continue;

                        int __index = col + row * _size;
                        _colorblock[__index].r = _wallColor.r;
                        _colorblock[__index].g = _wallColor.g;
                        _colorblock[__index].b = _wallColor.b;
                        _colorblock[__index].a = _wallColor.a;
                    }
                }
            }
            #endregion
        }

        int _borderSize = 1;
        int _size = 16;
        Color32 _spaceColor;
        Color32 _wallColor;
        Dictionary<EWallFlag, Color32[]> _roomCellTypes = new Dictionary<EWallFlag, Color32[]>();

        public CRoomCellTypeManager(int size, int borderSize, Color32 spaceColor, Color32 wallColor)
        {
            _size = size;
            _borderSize = borderSize;
            _spaceColor = spaceColor;
            _wallColor = wallColor;
        }
        public Color32[] GetColorBlock(EWallFlag flag)
        {
            if (!_roomCellTypes.ContainsKey(flag))
            {
                RoomCellType __newCell = new RoomCellType(flag, _size, _borderSize, _spaceColor, _wallColor);
                _roomCellTypes.Add(flag, __newCell.GetColorBlock());
            }

            return _roomCellTypes[flag];
        }
        public int GetSize()
        {
            return _size;
        }
    }
}