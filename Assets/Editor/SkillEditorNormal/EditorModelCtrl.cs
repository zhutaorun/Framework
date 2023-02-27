using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.IO;
using GameFrame.SkillDisplay;
using SkillnewConfig;
using GameFrame;

namespace GameEditor
{
    public class EditorModelCtrl : IDisposable
    {
        private ModelCtrl modelCtrl;

        public GameObject Model { get { return modelCtrl.Model; } }

        public string commonpath { get; private set; }//非技能动画路径

        public string skillpath { get { return modelCtrl.SkillAnimPath; } }//技能动画路径

        public string skilltestpath { get; private set; }

        public AnimationClip this[string name]
        {
            get
            {
                if (m_AnimClips.ContainsKey(name))
                    return m_AnimClips[name];
                return null;
            }
        }

        private GameObject m_EffectRoot;
        private GameObject m_ModelRoot;

        private Animation Anim
        {
            get
            {
                ModelCtrl mctrl = EditorHook.editorSkillDisplayManager.GetCtrl<ModelCtrl>();
                if (mctrl != null)
                    return mctrl.Anim;
                else
                    return null;
            }
        }

        private Dictionary<string, AnimationClip> m_AnimClips = new Dictionary<string, AnimationClip>();
        private int m_creatureid = 0;

        public EditorModelCtrl()
        {
            modelCtrl = EditorHook.editorSkillDisplayManager.GetCtrl<ModelCtrl>();
        }

        public void Dispose()
        {
            if (m_ModelRoot)
                GameObject.DestroyImmediate(m_ModelRoot);
            if (m_EffectRoot)
                GameObject.DestroyImmediate(m_EffectRoot);
        }

        public void LoadClipAsync(string name, bool isSkill, Action<AnimationClip> callback, bool test = false)
        {
            string clippath = test ? skilltestpath : skillpath;
            string path = isSkill ? clippath + name : commonpath + name;

            Hooks.ResourceManager.LoadAssetAsync<AnimationClip>(path, (animationClip) =>
             {
                 if (animationClip != null)
                 {
                     if (!m_AnimClips.ContainsKey(name))
                         m_AnimClips.Add(name, animationClip);
                 }
                 else
                 {
                     Debug.LogError("加载动画文件失败,路径:" + path);
                     return;
                 }
                 if (Anim.GetClip(name) == null)
                 {
                     Anim.AddClip(m_AnimClips[name], name);
                 }
                 callback.Invoke(animationClip);
             });
        }

        public void SetClipTime(string animName, float time)
        {
            if (Anim == null) return;
            AnimationState state = Anim[animName];
            if (state == null)
            {
                Debug.LogError("错误的动画名，名字:" + animName);
                return;
            }

            state.clip.SampleAnimation(Model, time);
        }

        public void SampleModelPosition(List<MotionInfo> infos, float time, Vector3 off, int startpoint = 0)
        {
            float timeMS = (time * 1000);
            Vector3 offset = Vector3.zero;
            if (infos != null)
            {
                foreach (var info in infos)
                {
                    if (info.MotionTime <= startpoint) continue;
                    if (info.MotionTime <= timeMS)
                    {
                        off.x += info.PosX;
                        off.y += info.PosY;
                        off.z += info.PosZ;
                    }
                }
            }
            offset *= 0.001f;
            offset += off;
            if (Model == null) return;
            Model.transform.position = offset;
        }

        public List<string> CollectAllAnimation()
        {
            List<string> list = new List<string>();
            CollectAnimation(skillpath, list);
            CollectAnimation(commonpath, list);
            return list;
        }

        public List<string> CollectSkillPath()
        {
            List<string> list = new List<string>();
            CollectAnimation(skillpath, list);
            return list;
        }

        public List<string> CollectCommonPath()
        {
            List<string> list = new List<string>();
            CollectAnimation(commonpath, list);
            return list;
        }

        public List<string> CollectSkillTestPath()
        {
            List<string> list = new List<string>();
            CollectAnimation(commonpath, list);
            return list;
        }

        public List<MotionInfo> NewCollectSkillActionMotions(string animName)
        {
            List<MotionInfo> infos = new List<MotionInfo>();

            RootMotionData rootMotionData = null;
            rootMotionData = (RootMotionData)AssetDatabase.LoadAssetAtPath(SkillDefine.ROOTMOTION_DATA_PATH, typeof(RootMotionData));
            if (rootMotionData == null)
            {
                return infos;
            }
            string name = Model.name.Replace("(Clone)", "");
            animName = animName + ".anim";
            RootMotionData.Data data = rootMotionData.Get(name);
            if (data.clips == null)
            {
                Debug.LogError(name + "没有找到位移数据，请确保统一生成了位移");
            }

            float fps = 30.0f;
            int frameTime = (int)(1000 / fps);
            RootMotionData.Clip motionClip = new RootMotionData.Clip();
            for (int i = 0; i < data.clips.Count; i++)
            {
                if (data.clips[i].name == (animName))
                {
                    motionClip = data.clips[i];
                    break;
                }
            }

            if (motionClip.positions != null)
            {
                Vector3 preVec3 = Vector3.zero;
                for (int index = 0; index < motionClip.positions.Count; index++)
                {
                    Vector3 curVec3 = motionClip.positions[index];
                    Vector3 temp = curVec3 - preVec3;
                    MotionInfo info = new MotionInfo();
                    info.MotionTime = index * frameTime;
                    info.PosX = (int)(temp.x * 1000);
                    info.PosY = (int)(temp.y * 1000);
                    info.PosZ = (int)(temp.z * 1000);
                    if (info.PosX != 0 || info.PosY != 0 || info.PosZ != 0)
                        infos.Add(info);
                    preVec3 = curVec3;
                }
            }

            return infos;
        }

        public IEnumerator CollectSkillActionMotionsAsync(int skillId, string animName, Action<List<MotionInfo>> callback, bool test = false)
        {
            List<MotionInfo> infos = new List<MotionInfo>();

            if (Anim == null) yield break;
            if(Anim[animName] == null)
            {
                string path = skillpath + animName;
                if (test)
                    path = skilltestpath + animName;

                AnimationClip animationClip = null;
                yield return Hooks.ResourceManager.LoadAssetAsync<AnimationClip>(path, (clip) =>
                {
                    animationClip = clip;
                });

                if (animationClip == null) yield break;
                Anim.AddClip(animationClip,animName);
            }

            string boneName = "root_motion";
            Transform rootMotion = null;
            Component[] hierarchy = Model.GetComponentsInChildren(typeof(Transform));
            foreach(Transform joint in hierarchy)
            {
                if(joint.name.ToUpper() == boneName.ToUpper())
                {
                    rootMotion = joint;
                    break;
                }   
            }

            if (rootMotion == null) yield break;
            AnimationState state = Anim[animName];
            float fps = 30.0f;
            int frameTime = (int)(1000 / fps);
            int totalLength = (int)(state.length * 1000);//转成ms
            state.clip.SampleAnimation(Model,0);
            Vector3 position = Model.transform.InverseTransformPoint(rootMotion.position);
            for(int i=0;i<= totalLength;i+= frameTime)
            {
                int curTime = i;
                if (curTime > totalLength) curTime = totalLength;
                float ftime = curTime / 1000.0f;
                state.clip.SampleAnimation(Model,ftime);

                Vector3 vNewPosition = Model.transform.InverseTransformPoint(rootMotion.position);
                Vector3 vTemp = vNewPosition - position;
                int offsetX = (int)(vTemp.x * 1000);
                int offsetY = (int)(vTemp.y * 1000);
                int offsetZ = (int)(vTemp.z * 1000);
                if(offsetX != 0 || offsetX!=0 || offsetZ!= 0)
                {
                    //与上个点有变化
                    MotionInfo motionInfo = new MotionInfo();
                    motionInfo.MotionTime = curTime;
                    motionInfo.PosX = offsetX;
                    motionInfo.PosY = offsetY;
                    motionInfo.PosZ = offsetZ;
                    infos.Add(motionInfo);
                }

                position = vNewPosition;
            }

            callback.Invoke(infos);
        }

        public void CollectAnimation(string path,List<string> list)
        {
            string realpath = Application.dataPath + "/AssetsPackage/" + path;
            DirectoryInfo directory = Directory.CreateDirectory(realpath);
            if(directory == null)
            {
                Debug.LogError("收集动画信息时的路径信息错误,路径:"+realpath);
                return;
            }

            FileInfo[] files = directory.GetFiles();
            if (files == null || files.Length <= 0) return;

            for(int i=0;i<files.Length;i++)
            {
                FileInfo file = files[i];
                if (file.Extension != ".anim") continue;

                list.Add(file.Name);
            }
        }
    }
}