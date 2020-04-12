using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using ParsingJSON;
using UnityEngine.UIElements;

public class PositionAPI : MonoBehaviour
{
    public string testString;
    private readonly string localURL = "localhost:5000/";
    private int count;

    private void Start()
    {
        //default values go here
        count = 0;
        testString = "";
    }

    private void Update()
    {
        //to test and not get overloaded with info, 1 json input per second
        count++;
        if(count >= 50)
        {
            count = 0;
            Debug.Log("Running GetPos");
            StartCoroutine(GetPos());
            Debug.Log("After GetPos");
        }
    }

    IEnumerator GetPos()
    {
        string PosURL = localURL + "data"; // actual url from base

        UnityWebRequest posInfoRequest = UnityWebRequest.Get(PosURL);

        
        yield return posInfoRequest.SendWebRequest();
        string test = posInfoRequest.downloadHandler.text;
        Debug.Log(test);

        if (posInfoRequest.isNetworkError || posInfoRequest.isHttpError)
        {
            Debug.LogError(posInfoRequest.error);
            Debug.Log("EORROR");
            yield break;
        }



    }
}
