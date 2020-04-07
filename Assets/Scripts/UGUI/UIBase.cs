using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class UIBase : MonoBehaviour, IWindow
{
    public UI_LAYER UILayer;

    public UI_TYPE UIType;

    public void Open()
    {
        gameObject.SetActive(true);
        OnOpen();
    }
    public  void Close()
    {
        gameObject.SetActive(false);
        OnClose();
    }
    protected virtual void Initialize()
    {
        Canvas canvas = GetComponent<Canvas>();
        //UIRichText 使用要支持的部分
        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Normal;
        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Tangent;
    }
    protected virtual void OnRelease() { }

    protected virtual void OnOpen() { }
    protected virtual void OnClose() { }
    public abstract UI_TYPE GetUIType();
    public void Refresh()
    {

    }
}