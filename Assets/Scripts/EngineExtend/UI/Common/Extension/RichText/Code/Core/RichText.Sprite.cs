using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.UI.Extension.Utils;
using UnityEngine;

namespace GameFrame.UI
{
    partial class RichText
    {
        private void _ParseSpriteTags(string strText)
        {
            if(string.IsNullOrEmpty(strText)||-1== strText.IndexOf("quad"))
            {
                _ResetSpriteTags(0);
                return;
            }

            int index = 0;
            foreach(Match match in SpriteTag.GetMatches(strText))
            {
                SpriteTag tag = _GetSpriteTag(index);
                var isOk = tag.SetValue(match);
                if(isOk)
                {
                    ++index;
                }
            }
            _ResetSpriteTags(index);
        }


        private void ParseSpriteTags(string strText)
        {
            if(string.IsNullOrEmpty(strText)||-1== strText.IndexOf("quad"))
            {
                _ResetSpriteTags(0);
                return;
            }
            ImageCache imageCache = null;
            if(m_SupportCache)
            {
                imageCache = CacheManager.Instance.GetImgCache(strText.GetHashCode());
                if(imageCache!=null)
                {
                    List<ImageCache.CacheImageTag> imageCacheParms = imageCache.GetParams();
                    for(int i=0;i<imageCacheParms.Count;++i)
                    {
                        SpriteTag tag = _GetSpriteTag(1);
                        if(imageCacheParms[i].paramDic!=null)
                        {
                            var iter = imageCacheParms[i].paramDic.GetEnumerator();
                            while(iter.MoveNext())
                            {
                                tag.CheckSetValue(imageCacheParms[i].position, iter.Current.Key, iter.Current.Value);
                            }
                            iter.Dispose();
                        }
                    }
                    _ResetSpriteTags(imageCacheParms.Count);
                }
            }

            int imgCount = 0;
            XMLIterator.Begin(strText,false);
            string textBlock = string.Empty;
            mTagPos.Clear();
            int tagLength = 0;
            int mImgTagCount = 0;
            int line = 0;
            int richXMLTagCount = 0;
            while(XMLIterator.NextTag())
            {
                mParses.Clear();
                if(XMLIterator.tagName== mImgTag)
                {
                    {
                        line = Regex.Matches(strText.Substring(0, XMLIterator.parsePos), "\n").Count;

                        mTagPos[mImgTag] = XMLIterator.parsePos + XMLIterator.tagLength;
                        XMLIterator.GetAttributes(mParses);
                        SpriteTag tag = _GetSpriteTag(imgCount);
                        tagLength += XMLIterator.tagLength;
                        foreach (var val in mParses)
                        {
                            tag.CheckSetValue(XMLIterator.parsePos + mImgTagCount - tagLength - line - richXMLTagCount, val.Key, val.Value);
                        }
                        mImgTagCount++;
                    }

                    if(m_SupportCache)
                    {
                        SpriteTag tag = _GetSpriteTag(imgCount);
                        imageCache = CacheManager.Instance.AddImgCache(strText.GetHashCode());
                        ImageCache.CacheImageTag cacheTag = new ImageCache.CacheImageTag();
                        if(cacheTag.paramDic==null)
                        {
                            cacheTag.paramDic = new Dictionary<string, string>();
                        }
                        cacheTag.position = tag.GetVertexStartIndex();
                        cacheTag.paramDic["name"] = tag.GetName();
                        cacheTag.paramDic["src"] = tag.GetSrc();
                        cacheTag.paramDic["width"] = tag.GetWidth();
                        cacheTag.paramDic["height"] = tag.GetHeight();
                        if(m_SupportDynamicChart)
                        {
                            cacheTag.paramDic["start"] = tag.GetStartFrame().ToString();
                            cacheTag.paramDic["end"] = tag.GetEndFrame().ToString();
                        }

                        imageCache.AddParams(cacheTag);
                     }
                    ++imgCount;
                }
                else
                {
                    if (XMLIterator.tagName.StartsWith("color") || XMLIterator.tagName.StartsWith("b") || XMLIterator.tagName.StartsWith("i") || XMLIterator.tagName.StartsWith("size"))
                    {
                        richXMLTagCount += XMLIterator.tagLength;
                    }
                }
            }
            _ResetSpriteTags(imgCount);
        }

        private void _HandleSpriteTag(VertexHelper toFill)
        {
            var spriteTags = _spriteTags;
            var count = spriteTags.Count;
            for(int i=0;i<count;i++)
            {
                SpriteTag spriteTag = spriteTags[i];
                var name = spriteTag.GetAniName();
                var atlas = spriteTag.GetAtlas();
                if(string.IsNullOrEmpty(name)|| null== atlas)
                {
                    continue;
                }

                var sprite = atlas.GetSprite(name);
                if(null==sprite)
                {
                    continue;
                }

                switch(spriteTag.GetFillMethod())
                {
                    case SpriteTag.FillMethod.None:
                        _SetSpriteVertex_FillMethod_None(toFill,spriteTag,sprite);
                        break;
                    case SpriteTag.FillMethod.Horizontal:
                        _SetSpriteVertex_FillMethod_Horizontal(toFill, spriteTag, sprite);
                        break;
                }
            }
        }

        private void _SetSpriteVertex_FillMethod_None(VertexHelper toFill,SpriteTag spriteTag,Sprite sprite)
        {
            UIVertex v = UIVertex.simpleVert;
            var vertexIndex = spriteTag.GetVertexStartIndex() * 4;
            var fetchIndex = vertexIndex + 3;
            if(fetchIndex>= toFill.currentIndexCount)
            {
                return;
            }
            if(m_SupportVertexCache)
            {
                UIVertexCache vertexCache = CacheManager.Instance.GetVertexCache(_parseOutputText.GetHashCode());
                if(vertexCache!=null)
                {
                    List<UIVertex> vertexs = vertexCache.GetParams();
                    v = vertexs[fetchIndex];
                }
            }
            else
            {
                toFill.PopulateUIVertex(ref v, fetchIndex);
            }


            Vector3 textPos = v.position;
            var tagSize = spriteTag.GetSize();
            float xOffset = spriteTag.GetOffset() * tagSize.x;

            var texture = sprite.texture;
            var textureWidthInv = 1.0f / texture.width;
            var textureHeightInv = 1.0f / texture.height;

            var uvRect = sprite.textureRect;
            uvRect = new Rect(uvRect.x * textureWidthInv, uvRect.y * textureHeightInv, uvRect.width * textureWidthInv, uvRect.height * textureHeightInv);

            //pos =(0,0)
            var position = new Vector3(xOffset,0,0)+ textPos;
            var uv0 = new Vector2(uvRect.x, uvRect.y);
            _SetSpriteVertex(toFill, vertexIndex, position, uv0);

            //pos = (1,0)
            position = new Vector3(xOffset + tagSize.x, 0, 0) + textPos;
            uv0 = new Vector2(uvRect.x + uvRect.width, uvRect.y);
            _SetSpriteVertex(toFill, ++vertexIndex, position, uv0);

            //pos =(1,1)
            position = new Vector3(xOffset+tagSize.x, tagSize.y, 0) + textPos;
            uv0 = new Vector2(uvRect.x+uvRect.width, uvRect.y+uvRect.height);
            _SetSpriteVertex(toFill, ++vertexIndex, position, uv0);

            //pos = (0,1)
            position = new Vector3(xOffset, tagSize.y, 0) + textPos;
            uv0 = new Vector2(uvRect.x, uvRect.y+uvRect.height);
            _SetSpriteVertex(toFill, ++vertexIndex, position, uv0);

        }

        private void _SetSpriteVertex_FillMethod_Horizontal(VertexHelper toFill,SpriteTag spriteTag,Sprite sprite)
        {
            UIVertex v = UIVertex.simpleVert;
            var vertexIndex = spriteTag.GetVertexStartIndex() * 4;
            var fetchIndex = vertexIndex + 3;
            if(fetchIndex>=toFill.currentIndexCount)
            {
                return;
            }
            if (m_SupportVertexCache)
            {
                UIVertexCache vertexCache = CacheManager.Instance.GetVertexCache(_parseOutputText.GetHashCode());
                if(vertexCache!=null)
                {
                    List<UIVertex> vertexs = vertexCache.GetParams();
                    v = vertexs[fetchIndex];
                }
            }
            else
            {
                toFill.PopulateUIVertex(ref v, fetchIndex);
            }

            Vector3 textPos = v.position;
            var tagSize = spriteTag.GetSize();
            float xOffset = spriteTag.GetOffset() * tagSize.x;

            var texture = sprite.texture;
            var textureWidthInv = 1.0f / texture.width;
            var textureHeightInv = 1.0f / texture.height;
            var uvRect = sprite.textureRect;
            uvRect = new Rect(uvRect.x * textureWidthInv, uvRect.y * textureHeightInv, uvRect.width * textureWidthInv, uvRect.height * textureHeightInv);

            //pos = (0,0)
            var position = new Vector3(xOffset, 0, 0) + textPos;
            var uv0 = new Vector2(uvRect.x,uvRect.y);
            _SetSpriteVertex(toFill, vertexIndex, position, uv0);

            var fillAmount = spriteTag.GetFillAmount();
            //pos = (1,0)
            position = new Vector3(xOffset + tagSize.x * fillAmount, 0, 0) + textPos;
            uv0 = new Vector2(uvRect.x + uvRect.width * fillAmount, uvRect.y);
            _SetSpriteVertex(toFill, ++vertexIndex, position, uv0);

            //pos = (1,1)
            position = new Vector3(xOffset + tagSize.x * fillAmount, tagSize.y, 0) + textPos;
            uv0 = new Vector2(uvRect.x + uvRect.width * fillAmount, uvRect.y + uvRect.height);
            _SetSpriteVertex(toFill, ++vertexIndex, position, uv0);

            //pos = (0,1)
            position = new Vector3(xOffset, tagSize.y, 0) + textPos;
            uv0 = new Vector2(uvRect.x ,uvRect.y + uvRect.height);
            _SetSpriteVertex(toFill, ++vertexIndex, position, uv0);
        }


        private void _SetSpriteVertex(VertexHelper toFill,int vertexIndex,Vector3 position,Vector2 uv0)
        {
            UIVertex v = new UIVertex();
            toFill.PopulateUIVertex(ref v,vertexIndex);
            v.position = position;
            v.uv0 = uv0;
            v.uv1 = new Vector2(0, 1.0f);
            toFill.SetUIVertex(v, vertexIndex);
        }

        private SpriteTag _GetSpriteTag(int index)
        {
            if(index>0)
            {
                _spriteTags.EnsureSizeEx(index + 1);
                SpriteTag tag = _spriteTags[index] ?? (_spriteTags[index] = new SpriteTag(this));
                return tag;
            }
            return null;
        }

        private void _ResetSpriteTags(int startIndex)
        {
            var count = _spriteTags.Count;
            for(int i=startIndex;i<count;++i)
            {
                var tag = _spriteTags[i];
                tag.Reset();
            }
        }
        public IList<SpriteTag> GetSpriteTags()
        {
            return _spriteTags;
        }
        private readonly List<SpriteTag> _spriteTags = new List<SpriteTag>();
    }

}

