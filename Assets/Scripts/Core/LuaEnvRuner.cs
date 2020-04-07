using System.Text;
using System.Collections.Generic;
using UnityEngine;
using GameFrame;
using XLua;

namespace lua
{
    public class LuaEnvRuner : Singleton<LuaEnvRuner>
    {
        public LuaEnv luaEnv { get; set; }

        /// <summary>
        /// 指向_G的metatable
        /// </summary>
        public LuaTable globalMT;

        public LuaFunction createLuaUIFunc;

        LuaFunction serverToClientLua;

        /// <summary>
        /// 自定义 lua 文件加载器
        /// </summary>
        private void AddLuaFileLoader()
        {
            luaEnv.AddLoader((ref string filename) =>
            {
                string oriFileName = filename;
                byte[] luaBytes = null;

                //hotfix索引文件强制指定路径
                if (oriFileName.Contains("hotfix_id_map"))
                {
                    filename = "gamelibs/hotfix_id_map.lua.txt";
                    // luaBytes = 
                }
                else
                {
                    bool isLoadFromFile = false;
                    if ((!(filename.StartsWith("xlua") || filename.StartsWith("tdr") || filename.StartsWith("perf"))))
                    {
#if UNITY_EDITOR
                        filename = "bundle/Lua/" + filename;
#else
                        isLoadFromFile = true;
                        filename = "LuaByte/" + filename;
#endif
                    }
                    filename = filename.Replace('.', '/');
                    if (!isLoadFromFile)
                    {
                        if (!filename.EndsWith(".lua"))
                        {
                            filename = filename + ".lua";
                        }
                        //luaBytes = 
                    }
                    else
                    {
                        if(!filename.EndsWith(".lua.txt"))
                        {
                            filename = filename + ".lua.txt";
                        }
                        //luaBytes = 
                    }
                    if(luaBytes!= null)
                    {
                        //luaBytes 
                    }
                }

                Debug.LogFormat("Xlua:Load file [{0}] at path :[{1}]",oriFileName,filename);
                if (luaBytes == null)
                {
                    string errorMsg = string.Format("<color=red>XLua :load file [{0}] fail with fullPath :[{1}]</color>", oriFileName, filename);
                    Debug.LogFormat(errorMsg);
                    return null;
                }
                else
                {
                    return luaBytes;
                }
            });
        }

        private void GetCreateLuaUIFunc()
        {
            createLuaUIFunc = luaEnv.Global.GetInPath<LuaFunction>("xlua.CreateLuaUI");
            serverToClientLua = luaEnv.Global.GetInPath<LuaFunction>("xlua.ServerToClientLua");
            //恢复到原始代码
            //global::Debug.Assert(createLuaUIFunc != null);
        }

        public void Init()
        {
            luaEnv = new LuaEnv();
            globalMT = luaEnv.NewTable();
            globalMT.Set("__index",luaEnv.Global);
        }

        public void ServerToClientLua(string luafunc)
        {
            if(!string.IsNullOrEmpty(luafunc) && serverToClientLua !=null)
            {
                serverToClientLua.Action<string>(luafunc);
            }
            else
            {
                Debug.LogError("ServerToClientLua-- failed" + luafunc);
            }
        }

        public void Reload()
        {
            if(luaEnv != null)
            {
                Release();
            }
            DelayInitLuaEnv(true);
        }

        /// <summary>
        /// 必须等到ResourceMgr 加载完毕才能执行lua,否则资源无法加载
        /// </summary>
        /// <param name="isOK"></param>
        public void DelayInitLuaEnv(bool isOK)
        {
            if (!isOK)
                return;

            try
            {
                if (luaEnv == null)
                {
                    Init();
                    Debug.LogWarning("请在尽可能早的时机调用Init,否则可能会产生lua 内存申请失败问题");
                }
                AddLuaFileLoader();
                luaEnv.DoString("require'main'","main");
                GetCreateLuaUIFunc();
            }
            catch(System.Exception e)
            {
                Debug.LogError(e.Message + e.StackTrace);
            }
        }

        public void Tick()
        {
            if (luaEnv != null)
                luaEnv.Tick();
        }

        public override void Release()
        {
            try
            {
                var hot_fix_clear = luaEnv.Global.GetInPath<LuaFunction>("xlua.hotfix_clear");
                if (hot_fix_clear != null)
                {
                   // hot_fix_clear.Action();
                    hot_fix_clear.Dispose();
                }
                if (createLuaUIFunc != null)
                {
                    createLuaUIFunc.Dispose();
                }
                if (serverToClientLua != null)
                {
                    serverToClientLua.Dispose();
                }

                serverToClientLua = null;
                createLuaUIFunc = null;
                hot_fix_clear = null;
                globalMT = null;
                luaEnv.Dispose();
                luaEnv = null;
            }
            catch
            {
                luaEnv = null;
            }
        }
    }

}
