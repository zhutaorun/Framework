using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.UI
{
    public class LinkCache : IDisposable
    {
        public struct CacheLinkTag
        {
            public int StartIndex;
            public int EndIndex;
            public Dictionary<string, string> paramDic;
        }

        private bool _isDisposed;

        ~LinkCache()
        {
        }


        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            GC.SuppressFinalize(this);
            _isDisposed = true;
        }

        public void SetResultString(string text)
        {
            this.result = text;
        }

        public void AddParams(CacheLinkTag tag)
        {
            this.tags.Add(tag);
        }

        public List<CacheLinkTag> GetParams()
        {
            return tags;
        }

        public string GetResultString()
        {
            return this.result;
        }

        private string result = string.Empty;
        private List<CacheLinkTag> tags = new List<CacheLinkTag>();
    }


    public class ImageCache:IDisposable
    {
        public struct CacheImageTag
        {
            public int position;
            public Dictionary<string, string> paramDic;
        }

        private bool _isDisposed;

        ~ImageCache()
        {
        }

        public void Dispose()
        {
            if(_isDisposed)
            {
                return;
            }

            GC.SuppressFinalize(this);
            _isDisposed = true;
        }

        public void AddParams(CacheImageTag tag)
        {
            this.tags.Add(tag);
        }

        public List<CacheImageTag> GetParams()
        {
            return tags;
        }

        private List<CacheImageTag> tags = new List<CacheImageTag>();
    }


    public class UIVertexCache:IDisposable
    {
        private bool _isDisposed;

        ~UIVertexCache()
        { }

        public void Dispose()
        {
            if(_isDisposed)
            {
                return;
            }
            GC.SuppressFinalize(this);
            _isDisposed = true;
        }


        public void AddParams(UIVertex tag)
        {
            this.vertexs.Add(tag);
        }

        public List<UIVertex> GetParams()
        {
            return vertexs;
        }

        public void SetUnitsPerPixel(float pixel)
        {
            unitsPerPixel = pixel;
        }

        public float GetUnitsPerPiexl()
        {
            return unitsPerPixel;
        }

        public void SetRoundingOffset(Vector2 roundingOffset)
        {
            this.roundingOffset.Set(roundingOffset.x, roundingOffset.y);
        }

        public Vector2 GetRoundingOffset()
        {
            return roundingOffset;
        }

        private float unitsPerPixel = 0f;
        private Vector2 roundingOffset = Vector2.zero;
        private List<UIVertex> vertexs = new List<UIVertex>();
    }

    public class CacheManager
    {
        private readonly Dictionary<int, LinkCache> _linkMap = new Dictionary<int, LinkCache>();
        private readonly Dictionary<int, ImageCache> _imgMap = new Dictionary<int, ImageCache>();
        private readonly Dictionary<int, UIVertexCache> _vertexMap = new Dictionary<int, UIVertexCache>();

        private CacheManager()
        {

        }


        public static CacheManager Instance { get { return Nested.instance; } }


        private class Nested
        {
            //Explict static constructor to tell C# compiler
            // not to mark type as beforefieldinit

            static Nested()
            {

            }

            internal static readonly CacheManager instance = new CacheManager();
        }

        public LinkCache AddLinkCache(int source)
        {
            if(!_linkMap.ContainsKey(source))
            {
                _linkMap[source] = new LinkCache();
                return (LinkCache)_linkMap[source];
            }
            return _linkMap[source];
        }

        public LinkCache GetLinkCache(int key)
        {
            if (_linkMap.ContainsKey(key) == false) return null;
            var linkCache = _linkMap[key] as LinkCache;
            return linkCache;
        }


        public void RemoveLinkCache(int key)
        {
            if(_linkMap.ContainsKey(key))
            {
                LinkCache linkCache = _linkMap[key] as LinkCache;
                linkCache.Dispose();
                _linkMap.Remove(key);
            }
        }

        private void ClearLinkCache()
        {
            if(_linkMap.Count>0)
            {
                var iter = _linkMap.GetEnumerator();
                while(iter.MoveNext())
                {
                    var linkCache = iter.Current.Value as LinkCache;
                    if(null!= linkCache)
                    {
                        linkCache.Dispose();
                    }
                }
                _linkMap.Clear();
            }
        }

        public ImageCache AddImgCache(int source)
        {
            if(!_imgMap.ContainsKey(source))
            {
                _imgMap[source] = new ImageCache();
                return _imgMap[source];
            }
            return _imgMap[source];
        }

        public ImageCache GetImgCache(int key)
        {
            if (_imgMap.ContainsKey(key) == false) return null;
            var imageCache = _imgMap[key] as ImageCache;
            return imageCache;
        }

        public void RemoveImgCache(int key)
        {
            if(_imgMap.ContainsKey(key))
            {
                ImageCache imageCache = _imgMap[key] as ImageCache;
                imageCache.Dispose();
                _imgMap.Remove(key);
            }
        }

        private void ClearImgCache()
        {
            if(_imgMap.Count>0)
            {
                var iter = _imgMap.GetEnumerator();
                while(iter.MoveNext())
                {
                    var imgCache = iter.Current.Value as ImageCache;
                    if(null!=imgCache)
                    {
                        imgCache.Dispose();
                    }
                }
                _imgMap.Clear();
            }
        }


        public UIVertexCache AddVertexCache(int source)
        {
            if(!_vertexMap.ContainsKey(source))
            {
                _vertexMap[source] = new UIVertexCache();
                return _vertexMap[source];
            }
            return _vertexMap[source];
        }

        public UIVertexCache GetVertexCache(int key)
        {
            if (_vertexMap.ContainsKey(key) == false) return null;
            var vertexCache = _vertexMap[key] as UIVertexCache;
            return vertexCache;
        }

        public void RemoveVertexCache(int key)
        {
            if(_vertexMap.ContainsKey(key))
            {
                UIVertexCache vertexCache = _vertexMap[key] as UIVertexCache;
                vertexCache.Dispose();
                _vertexMap.Remove(key);
            }
        }

        private void ClearVertexCache()
        {
            if(_vertexMap.Count>0)
            {
                var iter = _vertexMap.GetEnumerator();
                while(iter.MoveNext())
                {
                    var vertexCache = iter.Current.Value as UIVertexCache;
                    if(null!= vertexCache)
                    {
                        vertexCache.Dispose();
                    }
                }
                _vertexMap.Clear();
            }
        }


        public void ClearAll()
        {
            ClearLinkCache();
            ClearImgCache();
            ClearVertexCache();
        }
    }

 

}
