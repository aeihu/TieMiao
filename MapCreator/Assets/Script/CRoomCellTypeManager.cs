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
            public RoomCellType(ERoomFlag flag, int size, int borderSize, Color32 spaceColor, Color32 wallColor, Color32 lockColor)
            {
                _size = size;
                _doorSize = size / 3;
                _borderSize = borderSize;
                _spaceColor = spaceColor;
                _wallColor = wallColor;
                _lockColor = lockColor;
                _colorblock = new Color32[_size * _size];
                _Flag = flag;
            }

            #region 成员与属性
            int _borderSize = 1;
            int _size = 16;
            int _doorSize;
            Color32 _spaceColor;
            Color32 _wallColor;
            Color32 _lockColor;
            Color32[] _colorblock;
            ERoomFlag _flag = ERoomFlag.None;
            public ERoomFlag _Flag
            {
                get
                {
                    return _flag;
                }
                set
                {
                    _flag = value;
                    drawSpace();
                    drawWall();
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
            private void drawSpace()
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

            private void drawWall()
            {
                if ((_flag & ERoomFlag.UpWall) != 0)
                    drawUpWall();

                if ((_flag & ERoomFlag.BottomWall) != 0)
                    drawBottomWall();

                if ((_flag & ERoomFlag.LeftWall) != 0)
                    drawLeftWall();

                if ((_flag & ERoomFlag.RightWall) != 0)
                    drawRightWall();
            }

            private void drawWL(int col, int row, Color32 color)
            {
                int __index = col + row * _size;
                _colorblock[__index].r = color.r;
                _colorblock[__index].g = color.g;
                _colorblock[__index].b = color.b;
                _colorblock[__index].a = color.a;
            }

            private void drawUpWall()
            {
                for (int col = 0; col < _size; col++)
                {
                    for (int row = 0; row < _borderSize; row++)
                    {
                        if ((_flag & ERoomFlag.UpDoor) != 0 && col >= _doorSize && col < _doorSize << 1)
                        {
                            if ((_flag & ERoomFlag.Lock) != 0)
                                drawWL(col, row, _lockColor);

                            continue;
                        }

                        drawWL(col, row, _wallColor);
                    }
                }
            }
            private void drawLeftWall()
            {
                for (int col = 0; col < _borderSize; col++)
                {
                    for (int row = 0; row < _size; row++)
                    {
                        if ((_flag & ERoomFlag.LeftDoor) != 0 && row >= _doorSize && row < _doorSize << 1)
                        {
                            if ((_flag & ERoomFlag.Lock) != 0)
                                drawWL(col, row, _lockColor);

                            continue;
                        }

                        drawWL(col, row, _wallColor);
                    }
                }
            }
            private void drawBottomWall()
            {
                for (int col = 0; col < _size; col++)
                {
                    for (int row = _size - 1; row >= _size - _borderSize; row--)
                    {
                        if ((_flag & ERoomFlag.BottomDoor) != 0 && col >= _doorSize && col < _doorSize << 1)
                        {
                            if ((_flag & ERoomFlag.Lock) != 0)
                                drawWL(col, row, _lockColor);

                            continue;
                        }

                        drawWL(col, row, _wallColor);
                    }
                }
            }
            private void drawRightWall()
            {
                for (int col = _size - 1; col >= _size - _borderSize; col--)
                {
                    for (int row = 0; row < _size; row++)
                    {
                        if ((_flag & ERoomFlag.RightDoor) != 0 && row >= _doorSize && row < _doorSize << 1)
                        {
                            if ((_flag & ERoomFlag.Lock) != 0)
                                drawWL(col, row, _lockColor);
                            
                            continue;
                        }

                        drawWL(col, row, _wallColor);
                    }
                }
            }
            #endregion
        }

        int _borderSize = 1;
        int _size = 16;
        Color32 _spaceColor;
        Color32 _wallColor;
        Color32 _lockColor;
        Dictionary<ERoomFlag, Color32[]> _roomCellTypes = new Dictionary<ERoomFlag, Color32[]>();

        public CRoomCellTypeManager(int size, int borderSize, Color32 spaceColor, Color32 wallColor, Color32 lockColor)
        {
            _size = size;
            _borderSize = borderSize;
            _spaceColor = spaceColor;
            _wallColor = wallColor;
            _lockColor = lockColor;
        }
        public Color32[] GetColorBlock(ERoomFlag flag)
        {
            if (!_roomCellTypes.ContainsKey(flag))
            {
                RoomCellType __newCell = new RoomCellType(flag, _size, _borderSize, _spaceColor, _wallColor, _lockColor);
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