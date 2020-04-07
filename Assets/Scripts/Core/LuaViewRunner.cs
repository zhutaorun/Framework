using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SDGame.UITools;
using XLua;
using lua;


#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

/// <summary>
/// 所有lua实现的UI的容器
/// </summary>
public class LuaViewRunner : UIBase
{
    /// <summary>
    /// 类名，需要在AddComponent后Start被调用前设置
    /// </summary>
    public string viewClassName { get; set; }

    /// <summary>
    /// luaUI 实例，可以用来进行主动调用或者传递给其他UI
    /// </summary>
    public LuaTable luaUI { get; set; }

    #region LuaHandler

    private LuaFunction _lua_OnEnable;
    private LuaFunction _lua_OnDisable;
    private LuaFunction _lua_GetUIType;
    private LuaFunction _lua_Destroy;
    private LuaFunction _lua_Initialize;
    private LuaFunction _lua_OnOpen;
    private LuaFunction _lua_OnClose;
    private LuaFunction _lua_OnHide;
    private LuaFunction _lua_OnRefresh;
    private LuaFunction _lua_Update;
    private LuaFunction _lua_FixedUpdate;
    private LuaFunction _lua_GetLayer;
    private LuaFunction _lua_OnRelease;

    #endregion

    public LuaTable BindLua(string viewClassName)
    {
        this.viewClassName = viewClassName;

        ClearLua();
        if (!LoadLuaCode())
        {
            return luaUI;
        }
        UIControlData ctrl = GetComponent<UIControlData>();
        //创建成功并初始化子节点后调用Init,给lua初始化变量的机会
        if (ctrl != null)
        {
            ctrl.BindDataToLua(this, luaUI);
        }
        if (_lua_Initialize != null)
        {
            Debug.Log("_lua_Initialize -- successed--" + viewClassName);
            _lua_Initialize.Func<LuaTable, LuaTable>(luaUI);
        }
        else
        {
            Debug.Log("_lua_Initialize -- failed--" + viewClassName);
        }

        if (_lua_GetUIType != null)
        {
            UILayer = _lua_GetLayer.Func<LuaTable, UI_LAYER>(luaUI);
        }
        return luaUI;
    }

    #region 提供给lua使用
    public void SetLayer(UI_LAYER layer)
    {
        UILayer = layer;
    }


    public void SetImage(Image img, string imgPath)
    {

    }
    public void SetRawImage(RawImage img, string imgPath)
    {

    }
    #endregion


    private void OnEnable()
    {
        if (_lua_OnEnable != null)
            _lua_OnEnable.Func<LuaTable, LuaTable>(luaUI);
    }

    private void OnDisable()
    {
        if (_lua_OnDisable != null)
            _lua_OnDisable.Func<LuaTable, LuaTable>(luaUI);
    }

    #region Virtal Func

    public override UI_TYPE GetUIType()
    {
        if (_lua_GetUIType != null)
            return _lua_GetUIType.Func<LuaTable, UI_TYPE>(luaUI);
        else
            return UI_TYPE._BEGIN;
    }


    protected override void OnOpen()
    {
        if (_lua_OnOpen != null)
            _lua_OnOpen.Func<LuaTable, LuaTable>(luaUI);
    }

    protected override void OnClose()
    {
        if (_lua_OnClose != null)
            _lua_OnClose.Func<LuaTable, LuaTable>(luaUI);
    }

    protected override void OnRelease()
    {
        if (_lua_OnRelease != null)
            _lua_OnRelease.Func<LuaTable, LuaTable>(luaUI);
        base.OnRelease();
    }

    #endregion


    #region Private
    private void Update()
    {
        if (_lua_Update != null)
            _lua_Update.Func<LuaTable, LuaTable>(luaUI);
    }

    private void FixedUpdate()
    {
        if (_lua_FixedUpdate != null)
            _lua_FixedUpdate.Func<LuaTable, LuaTable>(luaUI);
    }

    bool LoadLuaCode()
    {
        try
        {
            luaUI = lua.LuaEnvRuner.Instance.createLuaUIFunc.Func<string, LuaViewRunner, LuaTable>(viewClassName, this);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        if (luaUI == null)
        {
            string errorMsg = string.Format("Get LuaTable fail when load LuaUI[{0}],pls check lua file!", viewClassName);

            Debug.LogErrorFormat(errorMsg);
            return false;
        }

        SetupCallbacks();
        return true;
    }

    /// <summary>
    /// 设置Lua的回调
    /// </summary>
    private bool SetupCallbacks()
    {
        if (luaUI == null)
            return false;

        luaUI.Get("OnEnable", out _lua_OnEnable);
        luaUI.Get("OnDisable", out _lua_OnDisable);
        luaUI.Get("GetUIType", out _lua_GetUIType);
        luaUI.Get("OnDestroy", out _lua_Destroy);
        luaUI.Get("OnOpen", out _lua_OnOpen);
        luaUI.Get("OnClose", out _lua_OnClose);
        luaUI.Get("OnHide", out _lua_OnHide);
        luaUI.Get("OnRefresh", out _lua_OnRefresh);
        luaUI.Get("Update", out _lua_Update);
        luaUI.Get("FixedUpdate", out _lua_FixedUpdate);
        luaUI.Get("GetLayer", out _lua_GetLayer);
        luaUI.Get("OnRelease", out _lua_OnRelease);

        return true;
    }

    private void OnDestroy()
    {
        UIControlData.UnBindUI(gameObject);
        ClearLua();
    }

    void ClearLua()
    {
        if (_lua_Destroy != null)
            _lua_Destroy.Func<LuaTable, LuaTable>(luaUI);
        RemoveCallbacks();
        if(luaUI != null)
        {
            luaUI.Dispose();
            luaUI = null;
        }
    }

    /// <summary>
    ///置空所有回调
    /// </summary>
    /// <remarks> 请务必置空。否则会报"try to dispose a LuaEnv with C# CallBack"错误</remarks>
    private void RemoveCallbacks()
    {
        if (_lua_OnEnable != null)      _lua_OnEnable.Dispose();
        if (_lua_OnDisable != null)     _lua_OnDisable.Dispose();
        if (_lua_GetUIType != null)     _lua_GetUIType.Dispose();
        if (_lua_Destroy != null)       _lua_Destroy.Dispose();
        if (_lua_Initialize != null)    _lua_Initialize.Dispose();
        if (_lua_OnOpen != null)        _lua_OnOpen.Dispose();
        if (_lua_OnClose != null)       _lua_OnClose.Dispose();
        if (_lua_OnHide != null)        _lua_OnHide.Dispose();
        if (_lua_OnRefresh != null)     _lua_OnRefresh.Dispose();
        if (_lua_Update != null)        _lua_Update.Dispose();
        if (_lua_FixedUpdate != null)   _lua_FixedUpdate.Dispose();
        if (_lua_GetLayer != null)      _lua_GetLayer.Dispose();
        if (_lua_OnRelease != null)     _lua_OnRelease.Dispose();
        _lua_OnEnable       = null;
        _lua_OnDisable      = null;
        _lua_GetUIType      = null;
        _lua_Destroy        = null;
        _lua_Initialize     = null;
        _lua_OnOpen         = null;
        _lua_OnClose        = null;
        _lua_OnHide         = null;
        _lua_OnRefresh      = null;
        _lua_Update         = null;
        _lua_FixedUpdate    = null;
        _lua_GetLayer       = null;
        _lua_OnRelease      = null;
    }
    #endregion
}
