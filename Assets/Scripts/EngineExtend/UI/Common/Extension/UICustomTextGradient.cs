using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Extension
{
    [AddComponentMenu("UI/Effects/TextGradient")]
    [RequireComponent(typeof(Text))]
    public class UICustomTextGradient : BaseMeshEffect
    {
        [SerializeField]
        public Color32 topColor = Color.white;
        [SerializeField]
        public Color32 bottomColor = Color.black;
        [RangeAttribute(0, 1)]
        public float center = 0.5f;
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
            {
                return;
            }
            var count = vh.currentVertCount;
            if (count == 0)
                return;

            var vertexs = new List<UIVertex>();
            for (var i = 0; i < count; i++)
            {
                var vertex = new UIVertex();
                vh.PopulateUIVertex(ref vertex, i);
                vertexs.Add(vertex);
            }

            var topY = vertexs[0].position.y;
            var bottomY = vertexs[0].position.y;

            for (var i = 1; i < count; i++)
            {
                var y = vertexs[i].position.y;
                if (y > topY)
                {
                    topY = y;
                }
                else if (y < bottomY)
                {
                    bottomY = y;
                }
            }

            var height = topY - bottomY;
            for (var i = 0; i < count; i++)
            {
                var vertex = vertexs[i];

                //使用处理过后的颜色
                var color = CenterColor(bottomColor, topColor, (vertex.position.y - bottomY) / height);

                vertex.color = color;

                vh.SetUIVertex(vertex, i);
            }
        }

        //加了一个对颜色处理的函数，主要是调整中心的位置
        private Color32 CenterColor(Color32 bc,Color32 tc,float time)
        {
            if(center==0)
            {
                return bc;
            }
            else if(center==1)
            {
                return tc;
            }
            else
            {
                var centerColor = Color32.Lerp(bottomColor,topColor,0.5f);
                var resultColor = tc;
                if(time<center)
                {
                    resultColor = Color32.Lerp(bottomColor, centerColor, time/center);
                }
                else
                {
                    resultColor = Color32.Lerp(centerColor, topColor, (time - center) / (1 - center));
                }
                return resultColor;
            }
        }
    }
}
