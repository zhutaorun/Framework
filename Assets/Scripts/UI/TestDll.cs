using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class TestDll : MonoBehaviour
{
    [DllImport("DLLTest")]
    public static extern int UnityDLLTest();
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("测试下C"+ UnityDLLTest());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
