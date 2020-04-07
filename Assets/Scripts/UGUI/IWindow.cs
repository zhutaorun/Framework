using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// UI层级
/// </summary>
public enum UI_LAYER
{
    //场景全屏特效, 拍照效果特效
    SCENEEFFECT = 0,

    //MAIN层保留，目前不用，目前游戏主界面的子模块有使用
    MAIN = 1000,

    //游戏各个系统功能模块的层
    MENU = 1500,

    //二级弹出框层
    POPUP = 2000,

    //需要显示在最上层的UI
    POPUP_TOP = 3000,

    // 新手引导的UI界面
    GUIDE_PAGE = 3500,

    // 分享界面层
    SHARE = 3750,

    // LOADING应该在所有的UI之上
    LOADING = 4000,

    // 黑边适配在所有UI上
    BLACKADAPTER = 4500,

    //播放视频应在最顶层，覆盖所有UI
    VIDEO = 5000, 

}

public enum UI_TYPE : uint
{
    _BEGIN= 0,
    _END,
}

public interface IWindow : SDGame.UITools.IBindableUI
{
    UI_TYPE GetUIType();
    void Open();
    void Close();
    void Refresh();
}