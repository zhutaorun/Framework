using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using Common;
using GameFrame.Interface;
using GameFrame.Config;

namespace GameFrame
{
    public static class Hooks
    {
        #region 属性
        private static List<ManagerBase> allManagerList = new List<ManagerBase>();
        private static List<IUpdate> updateManagerList = new List<IUpdate>();
        private static List<ILateUpdate> lateUpdateManagerList = new List<ILateUpdate>();
        private static List<IFixedUpdate> fixedUpdateManagerList = new List<IFixedUpdate>();
        #endregion

        /// <summary>
        /// 游戏启动
        /// </summary>
        //public static Launcher Launcher = new Launcher();

        #region 公共及基础库(Common & Plugins)
        //public static EventManager<EventEnum> GlobalEvent;
        #endregion

        #region 持久层
        /// <summary>
        /// 本地状态数据管理器
        /// </summary>
        //public static LocalStatusManager LocalStatusManager;

        /// <summary>
        /// 配置管理器
        /// </summary>
        public static IConfig ConfigManager;

        /// <summary>
        /// 多语言管理器
        /// </summary>
        //public static LanguageManager LanguageManager;
        #endregion

        #region 功能层(Function)
        public static DSFramework.Function.Resource.IResourceManager ResourceManager;

        /// <summary>
        /// UI管理器
        /// </summary>
        //public static UI.UIManager UIManager;

        /// <summary>
        /// 技能系统工具
        /// </summary>
        public static SkillModuleMain SkillSystemTool;
        /// <summary>
        /// 音效管理器
        /// </summary>
        //public static Audio.IAudioManager AudioManager;
        /// <summary>
        /// UI上显示模型管理器
        /// </summary>
        //public static UIModelManager UIModelManager;


        #endregion


        #region 服务层（Service）
        //public static GameStateManager GameStateManager;
        #endregion

        #region 应用层(Application)
        #region --业务(Bussines)
        //private static Dictionary<Type, Container<IViewSystemBase>> _viewSystems = new Dictionary<Type, System.ComponentModel.Container<IViewSystemBase>>();
        #endregion
        #region --表现(Presentation)
        //private static Container<ILogicSystem> _logicSystem = new Container<ILogicSystem>();
        #endregion

        public static bool initialized;

        public static void CreateManager()
        {

            //GlobalEvent = CreateInstance<EventManager<EventEnum>>();

            //LocalStatusManager = CreateInstance<LocalStatusManager>();
            ConfigManager = CreateInstance<ConfigManager>();
            //LanguageManager = CreateInstance<LanguageManager>();

            ResourceManager = CreateInstance<DSFramework.Function.Resource.ResourceManager>();
            //AudioManager = CreateInstance<Audio.AudioManager>();
            //UIManager = CreateInstance<UIManager>();
            //UIModelManager = CreateInstance<UIModelManager>();

            SkillSystemTool = new SkillModuleMain();

            //_viewSystems[typeof(IInitializeViewSystem)] = new Container<IViewSystemBase>();
            initialized = true;
        }
        #endregion

        private static T CreateInstance<T>() where T:ManagerBase,new()
        {
            T manager = new T();
            allManagerList.Add(manager);

            if(manager is IUpdate)
            {
                updateManagerList.Add(manager as IUpdate);
            }

            if(manager is ILateUpdate)
            {
                lateUpdateManagerList.Add(manager as ILateUpdate);
            }

            if(manager is IFixedUpdate)
            {
                fixedUpdateManagerList.Add(manager as IFixedUpdate);
            }

            return manager;
        }
    }
}
