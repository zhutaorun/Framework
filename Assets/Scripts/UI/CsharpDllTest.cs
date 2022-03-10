using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://blog.csdn.net/l773575310/article/details/72461579/
public class CsharpDllTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MyRandomDLL.MyRandomDLL myRandom = new MyRandomDLL.MyRandomDLL();
        Debug.Log(myRandom.GetRandom()); ;
    }

}
