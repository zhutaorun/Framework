using GameFrame.Pool;

namespace GameFrame.SkillDisplay
{
    public class BaseItem:PoolClass
    {
       public T GetCtrl<T>() where T :class
       {
            return SkillDisplayManager.Instance.GetCtrl<T>();
       }
    }
}
