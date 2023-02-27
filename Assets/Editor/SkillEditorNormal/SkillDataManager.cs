using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using SkillnewConfig;
using UnitConfig;

namespace GameEditor
{
    public class SkillDataManager :  IDisposable
    {

        private int maxSkillId = 0;
        public List<skillnew_data> SkillConfigList;
        public skillnew_data CurSkillConfig = null;
        public unit_data CurUnitConfig = null;

        public string ShowName { get; private set; }
        public bool ConfigLoaded { get; private set; }
        public void Dispose()
        {

        }

        /// <summary>
        /// 初始技能数据
        /// </summary>
        /// <param name="data"></param>
        public void InitSkill(unit_data data)
        {
            SkillConfigList = new List<skillnew_data>();

            if(CurSkillConfig == null && data == null)
            {
                ShowName = string.Empty;
                CurSkillConfig = null;
                return;
            }

            if (data != null)
                CurUnitConfig = data;
            ShowName = CurSkillConfig.SkillName;
            List<int> skillids;
            if (CurUnitConfig.Skills == null)
                skillids = new List<int>();
            else
                skillids = new List<int>(CurUnitConfig.Skills);

            foreach(var id in skillids)
            {
                bool find = false;
                List<object> skill_all = EditorHook.configCtrl.GetListData<skillnew_data>();
                if(skill_all != null)
                {
                    foreach(var item in skill_all)
                    {
                        skillnew_data skill = item as skillnew_data;
                        if (skill.Id > maxSkillId)
                            maxSkillId = skill.Id;
                        if(skill.Id == id)
                        {
                            SkillConfigList.Add(skill);
                            find = true;
                            break;
                        }
                    }
                }

                if(!find)
                {
                    skillnew_data newskill = EditorHook.configCtrl.CreateData<skillnew_data>();
                    newskill.Id = id;
                    SkillConfigList.Add(newskill);
                }
            }

            
            if(SkillConfigList != null && SkillConfigList.Count>0)
            {
                SkillConfigList.Sort((a, b) => 
                {
                    return a.Id - b.Id;
                });

                CurSkillConfig = SkillConfigList[0];
            }
        }
        
        //获取技能的帧数

        public int GetSkillTotleFrame(skillnew_data skill)
        {
            if (skill.SkillActionList == null || skill.SkillActionList.Count <= 0) return 0;
            int time = 0;
            foreach(var item in skill.SkillActionList)
            {
                time += item.ActionTime;
            }
            return EditorConfigUtil.ConvertTime2Frame(time);
        }

        //获取技能总伤害百分比
        
        public int GetSkillTotalDamage(skillnew_data skill)
        {
            if (skill.SkillActionList == null || skill.SkillActionList.Count <= 0) return 0;
            int total = 0;
            foreach(var item in skill.SkillActionList)
            {
                if (item.FrameEvents == null || item.FrameEvents.Count == 0)
                    continue;

                foreach(var eventInfo in item.FrameEvents)
                {
                    if (eventInfo.DamageData == null || eventInfo.DamageData.Count == 0)
                        continue;
                    foreach(var damageInfo in eventInfo.DamageData)
                    {
                        if (damageInfo == null || damageInfo.Outputs == null || damageInfo.Outputs.Count == 0)
                            continue;
                        foreach(var outputInfo in damageInfo.Outputs)
                        {
                            if (outputInfo.OutputType == EOutputType.OptTotalDamagePercent)
                                total += outputInfo.Value;
                        }
                    }
                }
            }
            return total;
        }

        public void CreateNewSkill()
        {
            skillnew_data skill = EditorHook.configCtrl.CreateData<skillnew_data>();
            skill.Id = ++maxSkillId;
            SkillConfigList.Add(skill);

            if(CurUnitConfig != null)
            {
                List<int> skillids = new List<int>();
                if (CurUnitConfig.Skills != null)
                    skillids.AddRange(CurUnitConfig.Skills);
                if (!skillids.Contains(skill.Id))
                    skillids.Add(skill.Id);
            }
            EditorHook.configCtrl.FreshConfigData<skillnew_data>("id");
        }

        public void AddExistSkill(skillnew_data skill)
        {
            if(!EditorHook.configCtrl.ContainsID<skillnew_data>(skill.Id,"Id"))
            {
                Debug.LogError("Id error");
                return;
            }

            SkillConfigList.Add(skill);
            if(CurUnitConfig != null)
            {
                List<int> skillIds = new List<int>();
                if (CurUnitConfig.Skills != null)
                    skillIds.AddRange(CurUnitConfig.Skills);
                if (!skillIds.Contains(skill.Id))
                    skillIds.Add(skill.Id);
            }
        }


        public void DeleteSkill(int index)
        {
            if(index < 0 || index >= SkillConfigList.Count)
            {
                Debug.LogErrorFormat("skill index error,index:{0},length:{1}",index,SkillConfigList.Count);
            }

            skillnew_data skill = SkillConfigList[index];

            SkillConfigList.RemoveAt(index);
        }

        #region 保存技能配置

        public void SaveSkillConfig(bool savepart = true)
        {
            List<object> skill_all = EditorHook.configCtrl.GetListData<skillnew_data>();
            if (skill_all == null) return;

            SkillnewConfig.container container = new SkillnewConfig.container();
            List<skillnew_data> list = new List<skillnew_data>();
            foreach(var item in skill_all)
            {
                skillnew_data skill = item as skillnew_data;
                container.Infos.Add(skill.Id,skill);
                list.Add(skill);
            }
            SkillController.SkillCfg.SaveFile(SkillDefine.SkillConfigPath,container);
            if (savepart)
                SaveSkill2Part();
            AssetDatabase.Refresh();
           
        }

        /// <summary>
        /// 保存所有技能散表配置
        /// </summary>
        private void SaveSkill2Part()
        {
            List<object> unit_all = EditorHook.configCtrl.GetListData<unit_data>();
            List<object> skill_all = EditorHook.configCtrl.GetListData<skillnew_data>();
            if (skill_all == null) return;
            if (unit_all == null) return;

            foreach(var item in unit_all)
            {
                unit_data unit = item as unit_data;
                int id = unit.Id;
                foreach(var skillid in unit.Skills)
                {
                    skillnew_data target = null;
                    foreach(var skillobj in skill_all)
                    {
                        skillnew_data skill = skillobj as skillnew_data;
                        if(skill.Id == skillid)
                        {
                            target = skill;
                            break;
                        }
                    }
                    if (target == null)
                    {
                        Debug.LogErrorFormat("find skill failed,unitid:{0},skillId:{1}", id, skillid);
                    }
                    else
                    {
                        string folder = string.Format("{0}/{1}/{2}", Application.dataPath, SkillDefine.SeparatedSkillFolder, id);
                        if (!System.IO.Directory.Exists(folder))
                        {
                            System.IO.Directory.CreateDirectory(folder);
                        }
                        string configpath = string.Format("{0}/{1}/{2}@skill.bytes", SkillDefine.SeparatedSkillFolder, id, target.Id);
                        skillnew_data skill = target;
                        SkillnewConfig.container container = new SkillnewConfig.container();
                        container.Infos.Add(skill.Id, skill);

                        SkillController.SkillCfg.SaveFile(configpath, container);
                    }
                }
            }
            AssetDatabase.Refresh();
        }


        /// <summary>
        /// 保存单个技能
        /// </summary>
        public void SavePartSkill()
        {
            string folder = Application.dataPath + "/" + SkillDefine.SeparatedSkillFolder;
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            skillnew_data skill = CurSkillConfig;
            if(skill == null)
            {
                Debug.LogError("当前技能为空");
                return;
            }

            List<object> unit_all = EditorHook.configCtrl.GetListData<unit_data>();
            int targetUnit = -1;
            foreach(var item in unit_all)
            {
                var unit = (unit_data)item;
                foreach(var skillid in unit.Skills)
                {
                    if(skillid == skill.Id)
                    {
                        targetUnit = unit.Id;
                        break;
                    }
                }
                if (targetUnit > 0)
                    break;
            }

            if(targetUnit<=0)
            {
                EditorUtility.DisplayDialog("错误", "没有找到对应的角色，保存失败", "确定");
                return;
            }
            folder = string.Format("{0}/{1}/{2}", Application.dataPath, SkillDefine.SeparatedSkillFolder, targetUnit);
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            SkillnewConfig.container container = new SkillnewConfig.container();

            container.Infos.Add(skill.Id, skill);
            string bytePath = string.Format("{0}/{1}/{2}@skill.bytes", SkillDefine.SeparatedSkillFolder, targetUnit, skill.Id);

            SkillController.SkillCfg.SaveFile(bytePath,container);
            EditorHook.configCtrl.LoadSkillConfig(targetUnit,true);
            SaveSkillConfig(false);
            AssetDatabase.Refresh();
        }
        #endregion
    }
}