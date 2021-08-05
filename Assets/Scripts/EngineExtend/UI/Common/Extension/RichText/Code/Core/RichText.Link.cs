using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extension.Utils;
using System.Text.RegularExpressions;
using UnityEngine.UI;

namespace GameFrame.UI
{
    public struct BoxStruct
    {
        public int line;
        public int start;
        public int end;
    }

    partial class RichText
    {
        public float LineOffset = 1f;
        public float LineThikcness = 2;
        public Color UnderlineColor = Color.black;

        private static readonly UIVertex[] data = new UIVertex[4];
        private static readonly StringBuilder mTextBuilder = new StringBuilder();
        private readonly List<LinkTag> mUnderlineTagInfos = new List<LinkTag>();
        List<UILineInfo> m_Lines = new List<UILineInfo>();

        private IList<UIVertex> verts = null;

        List<Rect> DrawLineRect = new List<Rect>();
        [SerializeField]
        public class RichTextClickEvent : UnityEvent<Dictionary<string, string>> { }

        [SerializeField]
        private RichTextClickEvent m_OnClick = new RichTextClickEvent();

        public RichTextClickEvent onClick
        {
            get { return m_OnClick; }
            set { m_OnClick = value; }
        }

        public void GetLines(List<UILineInfo> lines)
        {
            cachedTextGenerator.GetLines(lines);
        }

        private Vector2 GetUnderlineCharUV()
        {
            var ch = '*';
            CharacterInfo info;
            if(font.GetCharacterInfo(ch,out info,fontSize,fontStyle))
            {
                return (info.uvBottomLeft + info.uvBottomRight + info.uvTopLeft + info.uvTopRight) * 0.25f;
            }
            Debug.LogWarning("GetCharacterInof failed");
            return Vector2.zero;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Vector2 lp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out lp);

            foreach(var hrefInfo in mUnderlineTagInfos)
            {
                var boxes = hrefInfo.Boxes;
                for(var i=0;i<boxes.Count;++i)
                {
                    if(boxes[i].Contains(lp))
                    {
                        m_OnClick.Invoke(hrefInfo.GetParams());
                        return;
                    }
                }
            }
        }

        private void ParseLinkTags(ref string strText)
        {
            if(string.IsNullOrEmpty(strText))              {
                _ResetLinkTags(0);
                return;
            }
            mUnderlineTagInfos.Clear();
            LinkCache linkCache = null;
            if(m_SupportCache)
            {
                linkCache = CacheManager.Instance.GetLinkCache(strText.GetHashCode());
                if(linkCache!=null)
                {
                    for(int i=0;i<linkCache.GetParams().Count;++i)
                    {
                        LinkCache.CacheLinkTag tag = linkCache.GetParams()[i];
                        LinkTag selftag = _GetLinkTag(i);
                        selftag.SetStartIndex(tag.StartIndex);
                        selftag.SetEndIndex(tag.EndIndex);
                        mUnderlineTagInfos.Add(selftag);

                        if(tag.paramDic!=null)
                        {
                            var iter = tag.paramDic.GetEnumerator();
                            while(iter.MoveNext())
                            {
                                selftag.CheckSetValue(iter.Current.Key,iter.Current.Value);
                            }
                            iter.Dispose();
                        }
                    }

                    strText = linkCache.GetResultString();
                    _ResetSpriteTags(linkCache.GetParams().Count);
                    return;
                }
            }

            int linkCount = 0;
            mTextBuilder.Length = 0;
            XMLIterator.Begin(strText, false);
            string textBlock = string.Empty;
            mTagPos.Clear();
            while(XMLIterator.NextTag())
            {
                mParses.Clear();
                if (XMLIterator.tagName == mLinkTag)
                {
                    textBlock = XMLIterator.GetText(false);
                    if (!mTagPos.ContainsKey(mLinkTag))
                    {
                        XMLIterator.tagOffset += XMLIterator.tagOffset;
                        mTagPos[mLinkTag] = (XMLIterator.parsePos - XMLIterator.tagOffset) * 4;
                        LinkTag tag = _GetLinkTag(lineCount);
                        mUnderlineTagInfos.Add(tag);
                        tag.SetStartIndex(mTagPos[mLinkTag]);
                        XMLIterator.GetAttributes(mParses);
                        foreach (var val in mParses)
                        {
                            tag.CheckSetValue(val.Key, val.Value);
                        }
                    }
                    else
                    {
                        LinkTag tag = _GetLinkTag(lineCount);
                        XMLIterator.tagOffset += XMLIterator.tagLength;
                        tag.SetEndIndex((XMLIterator.parsePos - XMLIterator.tagOffset) * 4);
                        mTagPos.Remove(mLinkTag);

                        if (m_SupportCache)
                        {
                            linkCache = CacheManager.Instance.AddLinkCache(strText.GetHashCode());
                            LinkCache.CacheLinkTag cacheTag = new LinkCache.CacheLinkTag();
                            cacheTag.StartIndex = tag.GetStartIndex();
                            cacheTag.EndIndex = tag.GetEndIndex();

                            if (cacheTag.paramDic == null)
                            {
                                cacheTag.paramDic = new Dictionary<string, string>();
                            }
                            var iter = tag.GetParams().GetEnumerator();
                            while (iter.MoveNext())
                            {
                                cacheTag.paramDic.Add(iter.Current.Key, iter.Current.Value);
                            }
                            linkCache.AddParams(cacheTag);
                        }
                        ++lineCount;
                    }
                }
                else
                {
                    textBlock = XMLIterator.GetText(false);
                    mTextBuilder.Append(textBlock);
                    mTextBuilder.Append(XMLIterator.GetTagSource());
                }
            }
            textBlock = XMLIterator.GetText(false);
            mTextBuilder.Append(textBlock);
            string result = mTextBuilder.ToString();
            if(m_SupportCache)
            {
                linkCache = CacheManager.Instance.GetLinkCache(strText.GetHashCode());
                if (linkCache != null)
                    linkCache.SetResultString(result);
            }
            strText = result;

            _ResetLinkTags(linkCount);
        }

        private void _ParseLinkTags(ref string strText)
        {
            if(string.IsNullOrEmpty(strText))
            {
                _ResetLinkTags(0);
                return;
            }
            mUnderlineTagInfos.Clear();
            int index = 0;
            mTextBuilder.Length = 0;
            var indexText = 0;
            foreach(Match match in LinkTag.GetMatches(strText))
            {
                mTextBuilder.Append(strText.Substring(indexText,match.Index-indexText));

                var group = match.Groups[1];
                LinkTag tag = _GetLinkTag(index);
                tag.SetStartIndex(mTextBuilder.Length*4);
                tag.SetEndIndex((mTextBuilder.Length+match.Groups["text"].Length-1)*4+3);
                mUnderlineTagInfos.Add(tag);
                ++index;
                mTextBuilder.Append(match.Groups["text"].Value);
                indexText = match.Index + match.Length;
                tag.SetValue(match);
            }
            mTextBuilder.Append(strText.Substring(indexText, strText.Length - indexText));

            strText = mTextBuilder.ToString();

            _ResetLinkTags(index);
        }
           
        private void _HandleLinkTag(VertexHelper toFill,IList<UIVertex> verts,TextGenerationSettings setting)
        {
            this.verts = verts;
            GetLines(m_Lines);
            GetBounds(toFill, mUnderlineTagInfos);
            //绘制underLine 实现有点bug，先不开启
            //TextGenerator textGenerator = new TextGenerator();
            //textGenerator.Populate("_",setting);
            //IList<UIVertex> underlineVerts = textGenerator.verts;
            //for(int m=0;m<mUnderlineTagInfos.Count;++m)
            //{
            //    var underLineInfo = mUnderlineTagInfos[m];
            //    if(!underLineInfo.IsValid())
            //    {
            //        continue;
            //    }
            //    if(underLineInfo.GetStartIndex()>=mVertexHelperRef.currentVertCount)
            //    {
            //        continue;
            //    }
            //    for(int i=0;i<underLineInfo.Boxes.Count;i++)
            //    {
            //        Vector3 startBoxPos = new Vector3(underLineInfo.Boxes[i].x,underLineInfo.Boxes[i].y-1,0.0f);
            //        Vector3 endBoxPos = startBoxPos + new Vector3(underLineInfo.Boxes[i].width,0.0f,0.0f);
            //        AddUnderlineQuad(underlineVerts,startBoxPos,endBoxPos);
            //    }
            //}
        }
        #region 添加下划线
        //private void AddUnderlineQuad(IList<UIVertex> underlineVerts,Vector3 startBoxPos,Vector3 endBoxPos)
        //{
        //    Vector3[] underlinePos = new Vector3[4];
        //    underlinePos[0] = startBoxPos + new Vector3(0, fontSize * -0.1f, 0);
        //    underlinePos[1] = endBoxPos + new Vector3(0, fontSize * -0.1f, 0);
        //    underlinePos[2] = endBoxPos + new Vector3(0, fontSize * 0, 0);
        //    underlinePos[3] = startBoxPos + new Vector3(0, fontSize * 0, 0);
        //    for(int i=0;i<4;++i)
        //    {
        //        int tempVertsIndex = i & 3;
        //        _tempVerts[tempVertsIndex] = underlineVerts[i];
        //        _tempVerts[tempVertsIndex].color = Color.blue;
        //        _tempVerts[tempVertsIndex].position = underlinePos[i];
        //        if(tempVertsIndex==3)
        //        {
        //            mVertexHelperRef.AddUIVertexQuad(_tempVerts);
        //        }
        //    }
        //}

        private int GetCharLine(int charIndex)
        {
            int line = 0;
            for(int i=0;i<m_Lines.Count;i++)
            {
                if(m_Lines[i].startCharIdx>charIndex)
                {
                    return line;
                }
                line = i;
            }
            return line;
        }
        List<BoxStruct> box = new List<BoxStruct>();
        private List<BoxStruct> ConstructBox(int startIndex,int endIndex)
        {
            box.Clear();
            startIndex = startIndex / 4;
            endIndex = (endIndex - 3) / 4;
            BoxStruct boxStruct;
            int preLine = 0;
            boxStruct = new BoxStruct();
            int line = GetCharLine(startIndex);
            preLine = line;
            boxStruct.line = line;
            boxStruct.start = startIndex;
            for(int i=startIndex;i<=endIndex;++i)
            {
                line = GetCharLine(i);
                if(preLine!=line)
                {
                    box.Add(boxStruct);
                    preLine = line;
                    boxStruct.line = line;
                    boxStruct.start = i;
                }
                else
                {
                    boxStruct.end = i;
                }
            }
            box.Add(boxStruct);
            return box;
        }
        #endregion


        UIVertex vertStart3 = new UIVertex();
        UIVertex vertEnd1 = new UIVertex();
        private void GetBounds(VertexHelper toFill,List<LinkTag> m_HrefInfos)
        {
            SetNativeSize();

            DrawLineRect.Clear();
            //处理超链接包围框

            foreach(var hrefInfo in m_HrefInfos)
            {
                hrefInfo.Boxes.Clear();
                if(hrefInfo.GetStartIndex()>=toFill.currentVertCount)
                {
                    continue;
                }
                var pos = verts[hrefInfo.GetStartIndex()].position;

                List<BoxStruct> box = ConstructBox(hrefInfo.GetStartIndex(), hrefInfo.GetEndIndex());

                for(int i=0;i<box.Count;++i)
                {
                    BoxStruct boxStruct = box[i];
                    Rect rect = new Rect();

                    int startVer = boxStruct.start * 4 + 3;
                    int endVert = boxStruct.end * 4 + 1;
                    if(endVert>toFill.currentVertCount)
                    {
                        endVert = toFill.currentVertCount - 2;
                    }
                    if(startVer>=toFill.currentVertCount)
                    {
                        break;
                    }
                    if(m_SupportVertexCache)
                    {
                        UIVertexCache vertexCache = CacheManager.Instance.GetVertexCache(_parseOutputText.GetHashCode());
                        if(vertexCache!=null)
                        {
                            List<UIVertex> vertexs = new List<UIVertex>();
                            vertStart3 = verts[startVer];
                            vertEnd1 = vertexs[endVert];
                        }
                    }
                    else
                    {
                        toFill.PopulateUIVertex(ref vertStart3, startVer);
                        toFill.PopulateUIVertex(ref vertEnd1, endVert);
                    }

                    rect.Set(vertStart3.position.x, vertStart3.position.y, Mathf.Abs(vertEnd1.position.x - vertStart3.position.x), m_Lines[boxStruct.line].height);
                    hrefInfo.Boxes.Add(rect);
                    DrawLineRect.Add(rect);
                }
            }
        }


        private LinkTag _GetLinkTag(int index)
        { 
            if(index>0)
            {
                _linkTags.EnsureSizeEx(index + 1);
                LinkTag tag = _linkTags[index] ?? (_linkTags[index] = new LinkTag(this));
                return tag;
            }
            return null;
        }


        private void _ResetLinkTags(int startIndex)
        {
            var count = _linkTags.Count;
            for(int i= startIndex;i<count;++i)
            {
                var tag = _linkTags[i];
                tag.Reset();
            }
        }
        private readonly List<LinkTag> _linkTags = new List<LinkTag>();
    }
}
