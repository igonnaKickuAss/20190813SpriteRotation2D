using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OLiOYouxi
{
    [RequireComponent(typeof(Animator))]
	public class PixelRotation : MonoBehaviour
	{
        #region -- Private Data --
        private Dictionary<string, Sprite> _rotationCaches;
        private SpriteRenderer _renderer;
        private Animator _animator;
        private Rotation _rotator;

        private Sprite _spriteToRotate;
        private string _currentKey;

        private FilterMode _prevFilter;
        private int _prevPixelsPerUnit;
        private Sprite _originalSprite;
        private Texture2D _currentTexture;

        [OLiOYouxiAttributes.BoxGroup("旋转参数")] [SerializeField] private FilterMode Filter;
        [OLiOYouxiAttributes.BoxGroup("旋转参数")] [SerializeField] private int PixelsPerUnit;

        #endregion

        #region -- Public Data --
        [OLiOYouxiAttributes.BoxGroup("旋转参数")] [SerializeField] public int Angle;

        #endregion

        #region -- ShotC --
        Color[] _colorArr = null;
        Color[] testColorArr
        {
            get { return _colorArr; }
        }

        #endregion

        #region -- MONO APIMethods --
        //程序在这里开始
        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _rotator = new Rotation();

            _originalSprite = _renderer.sprite;

            _rotationCaches = new Dictionary<string, Sprite>();
            _currentKey = "";
            
            _prevPixelsPerUnit = PixelsPerUnit;
            
            _colorArr = new Color[((int)_originalSprite.rect.width * (int)_originalSprite.rect.height)];
            //设置颜色
            for (int i = 0; i < _colorArr.Length; i++)
            {
                _colorArr[i] = Color.green;
            }
        }

        private void Update()
        {
            //检查是否更新了过滤样式
            CheckFilter();
        }

        private void LateUpdate()
        {
            //计算旋转
            Rotate();
        }

        #endregion

        #region -- Private APIMethods --
        private void Rotate()
        {
            Angle = Angle % 360;
            Angle = Angle < 0 ? Angle + 360 : Angle;

            //拿到当前arm的精灵图
            _spriteToRotate = _renderer.sprite;

            //创建出属于当前状态的arm的键值
            _currentKey = $"{Angle }_{_spriteToRotate.name}";

            //检车字典中是否有缓存这个状态的arm的精灵图
            //有就拿出来用
            //没有就造一个
            if (!_rotationCaches.ContainsKey(_currentKey))
            {
                //创建一个空白的纹理(大小同第一张)
                _currentTexture = new Texture2D((int)_spriteToRotate.rect.width, (int)_spriteToRotate.rect.height);

                //设置他的名字
                _currentTexture.name = _spriteToRotate.name;

                //上色
                _currentTexture.SetPixels(_spriteToRotate.texture.GetPixels((int)_spriteToRotate.rect.position.x, (int)_spriteToRotate.rect.position.y, (int)_spriteToRotate.rect.width, (int)_spriteToRotate.rect.height));

                //测试锚点
                //_currentTexture.SetPixels(testColorArr);

                //创建一个已旋转的精灵图(附带一些预设参数)
                Sprite newSprite = _rotator.RotateTexture(ref _currentTexture, ref Filter, ref PixelsPerUnit, ref Angle, _spriteToRotate.pivot);

                //缓存到字典中
                _rotationCaches.Add(_currentKey, newSprite);
            }

            //把已经旋转的精灵图喂给spriteRender
            _renderer.sprite = _rotationCaches[_currentKey];
        }

        private void CheckFilter()
        {
            if (Filter != _prevFilter || PixelsPerUnit != _prevPixelsPerUnit)
            {
                ResetDictionary();

                _prevFilter = Filter;
                _prevPixelsPerUnit = PixelsPerUnit;
            }
        }


        #endregion

        #region -- Helper --
        private void ResetDictionary()
        {
            _rotationCaches.Clear();
            _currentKey = "";
        }

        #endregion
    }

    class Rotation
    {
        #region -- Private Data --
        private Texture2D _newTexture = null;

        private Color32[] _newColorArr = null;
        private Color32[] _prevColorArr = null;

        private int _width;
        private int _height;
        private int _prevWidth;
        private int _prevHeight;

        private Vector2 _pivot;
        private int _pixelsPerUnit;
        private FilterMode _filterMode;

        #endregion

        #region -- Private APIMethods --
        /// <summary>
        /// 返回一个被旋转的精灵图
        /// </summary>
        /// <param name="currentTexture"></param>
        /// <param name="filter"></param>
        /// <param name="pixelsPerUnit"></param>
        /// <param name="angle"></param>
        /// <param name="pivot"></param>
        /// <returns></returns>
        internal Sprite RotateTexture(ref Texture2D currentTexture, ref FilterMode filter, ref int pixelsPerUnit, ref int angle, Vector2 pivot)
        {
            _prevColorArr = currentTexture.GetPixels32();

            _prevWidth = currentTexture.width;
            _prevHeight = currentTexture.height;

            _width = _prevWidth;
            _height = _prevHeight;

            _newColorArr = new Color32[_width * _height];

            _newTexture = new Texture2D(_width, _height);
            _newTexture.filterMode = _filterMode;
            _newTexture.name = currentTexture.name;

            _pivot = pivot;
            _filterMode = filter;
            _pixelsPerUnit = pixelsPerUnit;

            //旋转滴嘢
            RotateSquare(Mathf.Deg2Rad * angle);


            //返回已经旋转的纹理
            return ApplyRotation(_newColorArr);
        }

        private Sprite ApplyRotation(Color32[] newColorArr)
        {
            _newTexture.SetPixels32(newColorArr);
            _newTexture.Apply(false);

            return Sprite.Create(_newTexture, new Rect(0f, 0f, _width, _height), new Vector2((_pivot.x / _prevWidth), (_pivot.y / _prevHeight)), _pixelsPerUnit);
        }

        /// <summary>
        /// 通过一个旋转矩阵来旋转每个像素
        /// </summary>
        /// <param name="alpha"></param>
        private void RotateSquare(float alpha)
        {
            int x, y, xc, yc, oxc, oyc;
            float sin, cos;

            x = 0;
            y = 0;
            sin = Mathf.Sin(alpha);
            cos = Mathf.Cos(alpha);

            //自定义锚点
            xc = _width / 2;
            yc = _height / 2;
            oxc = (int)_pivot.x;
            oyc = (int)_pivot.y;

            for (int j = 0; j < _height; j++)
            {
                for (int i = 0; i < _width; i++)
                {
                    x = Mathf.FloorToInt((cos * (i - xc) + sin * (j - yc) + oyc));
                    y = Mathf.CeilToInt((-sin * (i - xc) + cos * (j - yc) + oxc));

                    if ((x > -1) && (x < _prevWidth) && (y > -1) && (y < _prevHeight))
                    {
                        _newColorArr[j * _width + i] = _prevColorArr[y * _prevWidth + x];
                    }
                }
            }
        }

        #endregion

    }
}