using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.U2D;
using System.Collections;

namespace GameFrame.UI
{
    [ExecuteInEditMode]
    public partial class RichText : Text, IPointerClickHandler
    {

        [SerializeField]
        private SpriteAtlas[] m_Atlas;
        [SerializeField]
        private bool m_SupportDynamicChart = false;
        [SerializeField]
        private bool m_SupportCache = false;
        [SerializeField]
        private bool m_SupportVertexCache = false;

        private VertexHelper mVertexHelperRef;
        private Font mFont;

        private Dictionary<string, string> mParses = new Dictionary<string, string>();
        private Dictionary<string, int> mTagPos = new Dictionary<string, int>();
        private WaitForSeconds waitFrame = new WaitForSeconds(0.33f);
        private readonly string mLinkTag = "url";
        private readonly string mImgTag = "quad";

        private float heightRichText;
        public float HightRightText
        {
            get { return heightRichText; }
        }


        private int lineCount;
        public int LineCount { get { return lineCount; } }

        protected override void Awake()
        {
            base.Awake();
            if(m_Atlas!=null)
            {
                for(int i=0;i<m_Atlas.Length;++i)
                {
                    UIRichTextAtlasManager.Instance.Add(m_Atlas[i].name,m_Atlas[i]);
                }
            }
        }

        protected override void OnEnable()
        {
            this.supportRichText = true;
            base.OnEnable();
            mFont = font;
            if(m_SupportDynamicChart)
            {
                StartCoroutine(_CoUpdateSprite());
            }
        }

        private void InitImage()
        {
            var tags = GetSpriteTags();
            var count = tags.Count;
            for (int i = 0; i < count; ++i)
            {
                var tag = tags[i];
                if(tag.GetStartFrame() == tag.GetStartFrame() && tag.GetStartFrame()==0)
                {
                    tag.SetAniName(tag.GetName());
                }
            }

            SetVerticesDirty();

        }

        private IEnumerator _CoUpdateSprite()
        {
            while (true)
            {
                yield return waitFrame;

                var tags = GetSpriteTags();
                var count = tags.Count;
                for (int i = 0; i < count; i++)
                {
                    var tag = tags[i];
                    if(tag.GetStartFrame()== tag.GetStartFrame()&& tag.GetStartFrame() != 0)
                    {
                        int frameLength = tag.GetEndFrame() - tag.GetStartFrame();
                        tag.PlayerCurrentFrame = (tag.PlayerCurrentFrame++) % (frameLength) + tag.GetEndFrame();
                        // var atlas = tag.GetAtlas();
                        // var sprites = atlas.GetSprites();
                        // var sprite = sprites[UnityEngine.Random.Range(0,sprites.Length)];
                        //待优化
                        string spriteName = string.Format("{0}_{1}", tag.GetName(), tag.PlayerCurrentFrame);
                        tag.SetAniName(spriteName);
                    }
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            var manager = MaterialManager.Instance;
            var lastSpriteTexture = manager.GetSpriteAtlas(material);
            manager.DetachTexture(this, lastSpriteTexture);
            if(m_Atlas!=null)
            {
                for(int i=0;i<m_Atlas.Length;++i)
                {
                    UIRichTextAtlasManager.Instance.Remove(m_Atlas[i].name);
                }
            }
            base.OnDestroy();
        }

        public override string text 
        {
            get
            {
                return m_Text;
            }
            set
            {
                if(string.IsNullOrEmpty(value))
                {
                    if(string.IsNullOrEmpty(m_Text))
                    {
                        return;
                    }
                    m_Text = string.Empty;

                    _ParseText();
                    SetVerticesDirty();
                }
                else if(m_Text!=value)
                {
                    m_Text = value;
                    _ParseText();

                    SetVerticesDirty();
                }
            }
        }

        private void _ParseText()
        {
            _parseOutputText = text;
            ParseLinkTags(ref _parseOutputText);
            ParseSpriteTags(_parseOutputText);
            calPreferredHeightRichText();
        }

        /// <summary>
        /// 获取parse之后的字符串
        /// </summary>
        /// <returns></returns>
        public string getParsedText()
        {
            return _parseOutputText;
        }

        /// <summary>
        /// 获取parse后的高度
        /// </summary>
        private void calPreferredHeightRichText()
        {
            Vector2 extents = rectTransform.rect.size;
            var settings = GetGenerationSettings(extents);

            heightRichText = cachedTextGenerator.GetPreferredHeight(_parseOutputText, settings);
            lineCount = cachedTextGenerator.lineCount;
        }

        /// <summary>
        /// 当只有一行的时候，获取到行的宽度
        /// </summary>
        /// <returns></returns>
        public float GetSingleLineWitdh()
        {
            if (lineCount > 1)
                return -1;
            else
            {
                Vector2 extents = rectTransform.rect.size;
                var settings = GetGenerationSettings(extents);
                settings.scaleFactor = 1;
                return cachedTextGenerator.GetPreferredWidth(_parseOutputText, settings);
            }
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if(null==font)
            {
                return;
            }

            //来自官方Text源码
            //we don't care if we the font Texture changes while we are doing our Update
            //The end result of cachedTextGenerator will be valid for this instance.
            //Otherwise we can get issuse like Case 619238.
            m_DisableFontTextureRebuiltCallback = true;

            Vector2 extents = rectTransform.rect.size;

            var setting = GetGenerationSettings(extents);
            cachedTextGenerator.Populate(_parseOutputText, setting);
            if(m_SupportVertexCache)
            {
                UIVertexCache vertexCache = CacheManager.Instance.GetVertexCache(_parseOutputText.GetHashCode());
                if(vertexCache!=null)
                {
                    toFill.Clear();
                    List<UIVertex> vertexs = vertexCache.GetParams();
                    for(int  i=0;i<vertexs.Count;++i)
                    {
                        int tempVerrsIndex = i & 3;
                        _tempVerts[tempVerrsIndex] = vertexs[i];
                        _tempVerts[tempVerrsIndex].position *= vertexCache.GetUnitsPerPiexl();
                        _tempVerts[tempVerrsIndex].uv1 = new Vector2(1.0f, 0);
                        if(tempVerrsIndex==3)
                        {
                            toFill.AddUIVertexQuad(_tempVerts);
                        }
                        //mVertextHelperRef = toFill;
                        //if(mDirty)
                        {
                            _HandleSpriteTag(toFill);
                            _HandleLinkTag(toFill, vertexs, setting);
                            //mDirty = false;
                        }

                        m_DisableFontTextureRebuiltCallback = false;
                        return;
                    }
                }

                //Apply the offset to the vertices
                IList<UIVertex> verts = cachedTextGenerator.verts;
                float unitsPerPixl = 1 / pixelsPerUnit;
                int vertCount = verts.Count;

                if(vertCount<=0)
                {
                    toFill.Clear();
                    return;
                }
                Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixl;
                roundingOffset = PixelAdjustPoint(roundingOffset)- roundingOffset;

                toFill.Clear();

                if (roundingOffset != Vector2.zero)
                {
                    for (int i = 0; i < vertCount; ++i)
                    {
                        int tempVertsIndex = i & 3;
                        _tempVerts[tempVertsIndex] = verts[i];
                        _tempVerts[tempVertsIndex].position *= unitsPerPixl;
                        _tempVerts[tempVertsIndex].position.x += roundingOffset.x;
                        _tempVerts[tempVertsIndex].position.y += roundingOffset.y;
                        _tempVerts[tempVertsIndex].uv1 = new Vector2(1.0f, 0);

                        if (tempVertsIndex == 3)
                        {
                            toFill.AddUIVertexQuad(_tempVerts);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < vertCount; i++)
                    {
                        int tempVertsIndex = i & 3;
                        _tempVerts[tempVertsIndex] = verts[i];
                        _tempVerts[tempVertsIndex].position *= unitsPerPixl;
                        _tempVerts[tempVertsIndex].uv1 = new Vector2(1.0f, 0);
                        if(m_SupportVertexCache)
                        {
                            UIVertexCache vertexsCache = CacheManager.Instance.AddVertexCache(_parseOutputText.GetHashCode());
                            vertexsCache.AddParams(_tempVerts[tempVertsIndex]);
                            vertexsCache.SetUnitsPerPixel(unitsPerPixl);
                        }
                    }
                }

                //mVertexHelperRef= toFill;
                //if(mDirty)
                {
                    _HandleSpriteTag(toFill);
                    _HandleLinkTag(toFill,verts,setting);
                    //mDirty = false;
                }

                m_DisableFontTextureRebuiltCallback = false;
            }
        }

        private readonly UIVertex[] _tempVerts = new UIVertex[4];
        private string _parseOutputText = string.Empty;

    }
}
