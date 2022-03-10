using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Qlog;
/// <summary> Unity DebugDLL编写
/// https://mp.weixin.qq.com/s/WQcwTiX60V84pQhjwFEtFg
/// </summary>

public class QLogTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        QLog.Sample("QLog.Sample");
        QLog.Log("QLog.Log");
        QLog.LogEditor("QLog.LogEditor");
        QLog.LogError("QLog.LogError");
        QLog.LogErrorEditor("QLog.LogErrorEditor");
        QLog.LogWarning("QLog.LogWarning");
        QLog.LogWarningEditor("QLog.LogWarningEditor");
    }
}
