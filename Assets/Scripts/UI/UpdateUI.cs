using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
public class UpdateUI : MonoBehaviour
{
    private Text text;
    private int number = 0;
    // Start is called before the first frame update
    void Awake()
    {
        text = GetComponent<Text>();
        StartCoroutine(UpdateNumber());
    }
    IEnumerator UpdateNumber()
    {
        while (true)
        {
            text.text = number.ToString();
            number = CppInterface.Add(number, 1);
            yield return new WaitForSeconds(0.1f);
        }
    }

    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
