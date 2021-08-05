using System.Text.RegularExpressions;
using System;
using UnityEngine;

namespace GameFrame.UI
{
    [System.Serializable]
    public class SpriteTag
    {
        public enum FillMethod
        {
            None,
            Horizontal,
            //Vertical,
        }
        
        public SpriteTag(RichText richText)
        {
            if(null== richText)
            {
                throw new ArgumentNullException("rich is null");
            }
        }


        public static MatchCollection GetMatches(string strText)
        {
            return _spriteTagRegex.Matches(strText);
        }

        public bool SetValue(Match match)
        {
            if(null== match)
            {
                return false;
            }
            var keyCaptures = match.Groups[1].Captures;
            var valCaptures = match.Groups[2].Captures;

            var count = keyCaptures.Count;
            if(count!= valCaptures.Count)
            {
                return false;
            }

            for(int i=0;i<keyCaptures.Count;++i)
            {
                var key = keyCaptures[i].Value;
                var val = keyCaptures[i].Value;
                CheckSetValue(match.Index, key, val);
            }

            return true;
        }


        public void CheckSetValue(int position,string key,string val)
        {
            if (key == "name")
            {
                SetName(val);
                _vertexStartindex = position;
            }
            else if (key == "src")
            {
                _width = val;
                float width = float.Parse(val);

                _size.x = width;
            }
            else if(key == "height")
            {
                _height = val;
                float height = float.Parse(val);

                _size.y = height;
            }
            else if(key =="start")
            {
                int start = int.Parse(val);

                _startFrame = start;
            }
            else if(key =="end")
            {
                int end = int.Parse(val);

                _endFrame = end;
            }
        }

        public void Reset()
        {
            SetName(null);
        }

        public int GetVertexStartIndex()
        {
            return _vertexStartindex;
        }

        public void SetName(string name)
        {
            SetAniName(name);
            _name = name;
        }

        public string GetName()
        {
            return _name;
        }

        public Vector2 GetSize()
        {
            return _size;
        }


        public float GetOffset()
        {
            return _offset;
        }

        public void SetFillMethod(FillMethod fillMethod)
        {
            _fillMethod = fillMethod;
        }


        public FillMethod GetFillMethod()
        {
            return _fillMethod;
        }

        public void SetFillAmount(float amount)
        {
            amount = Mathf.Clamp01(amount);

            float eps = 0.001f; 
            var delta = _fillAmount - amount;
            if(delta>eps|| delta<-eps)
            {
                _fillAmount = amount;
                _richText.SetVerticesDirty();
            }
        }

        public float GetFillAmount()
        {
            return _fillAmount;
        }

        private void SetAltas(UIAtlas atlas)
        {
            _atlas = atlas;
            if(null==atlas)
            {
                return;
            }

            var richText = _richText;
            var mat = richText.material;
            var manager = MaterialManager.Instance;
            var lastSpriteTexture = manager.GetSpriteAtlas(mat);
            var spriteTexture = atlas.GetTexture();

            var isTextureChanged = lastSpriteTexture != spriteTexture;
            if (isTextureChanged)
            {
                manager.DetachTexture(richText,lastSpriteTexture);
                manager.AttachTexture(richText, spriteTexture);
            }
        }

        public UIAtlas GetAtlas()
        {
            return _atlas;
        }

        public int GetStartFrame()
        {
            return _startFrame;
        }

        public int GetEndFrame()
        {
            return _endFrame;
        }

        public string GetAniName()
        {
            return _AniName;
        }

        public void SetAniName(string name)
        {
            _AniName = name;
        }

        public string GetSrc()
        {
            return _src;
        }

        public string GetWidth()
        {
            return _width;
        }

        public string GetHeight()
        {
            return _height;
        }


      
        private RichText _richText;
        private UIAtlas _atlas;
        private string _src;
        private string _name;
        private string _width;
        private string _height;
        private string _AniName;
        private int _vertexStartindex;

        private Vector2 _size;
        private float _offset = 0;

        private int _startFrame = 0;
        private int _endFrame = 0;

        private float _fillAmount = 1.0f;
        private FillMethod _fillMethod = FillMethod.None;
        private static readonly string _spriteTagPattern = @"<quad(?:\s+(\w+)\s*=\s*(?<quota>['""]?)([\w\/]+)\k<quota>)+\s*\/>";
        private static readonly Regex _spriteTagRegex = new Regex(_spriteTagPattern, RegexOptions.Singleline);

        public int PlayerCurrentFrame = 0;

    }

}