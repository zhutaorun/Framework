using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class DepthMask : MonoBehaviour
{
    public Transform Mask;

    /// <summary>
    /// 上下左右偏移的属性 0.01约一像素
    /// </summary>
    public float offsetDownY = 0.01f;
    public float offsetUpY = -0.01f;
    public float offsetRightX = 0f;
    public float offsetLeftX = -0f;
    // Start is called before the first frame update
    void Start()
    {
        if (Mask == null) return;
        Vector3[] corners = new Vector3[4];
        RectTransform rectTransform = Mask as RectTransform;
        rectTransform.GetWorldCorners(corners);
        float minx = corners[0].x;
        float miny = corners[0].y;
        float maxx = corners[2].x;
        float maxy = corners[2].y;

        Material m = transform.GetComponent<Renderer>().material;
        m.SetFloat("_MinX",minx+ offsetLeftX);
        m.SetFloat("_MinY", miny + offsetDownY);
        m.SetFloat("_MaxX", maxx + offsetRightX);
        m.SetFloat("_MaxY",maxy + offsetUpY);

    }

    // Update is called once per frame
    void Update()
    {
        if(Refresh || Input.GetKeyDown(KeyCode.P))
        {
            Start();
            Refresh = false;
        }
    }
    public bool Refresh = false;
}
