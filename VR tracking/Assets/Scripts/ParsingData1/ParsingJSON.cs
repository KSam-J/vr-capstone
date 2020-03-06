using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ParsingJSON : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Game started");
        string json = File.ReadAllText(Application.dataPath + "/dataTest.json");
        Start1 locationData = JsonUtility.FromJson<Start1>("{\"data\":" + json + "}");
        Start2 locationData1 = JsonUtility.FromJson<Start2>("{\"data\":" + json + "}");



        /*
        string test = JsonUtility.ToJson(locationData);
        Debug.Log(test);
        string test1 = JsonUtility.ToJson(locationData1);
        Debug.Log(test1);
        
        LocationData1 locationData1 = JsonUtility.FromJson<LocationData1>(json);
        

        
        Debug.Log("ts: " + locationData.ts);
        Debug.Log("valid: " + locationData.dist.status);
        Debug.Log("id: " + locationData.dist.content.id);
        Debug.Log("id2: " + str1);
        Debug.Log("id2: " + str2);
        Debug.Log("distance: " + locationData1.dist.content.resources[2].value);
        */


        //for each data location
        for (var k = 0; k < locationData.data.Length; k++)
        {


            string str1 = "";
            string str2 = "";
            for (var i = 0; i < 16; i++)
            {
                str1 += locationData.data[k].dist.content.resources[0].value[i];
                str2 += locationData.data[k].dist.content.resources[1].value[i];
            }

            /*
             * time = locationData.data[k].ts
             *  id1 = str1
             *  id2 = str2
             *  distance = locationData1.data[k].dist.content.resources[2].value
             * 
             */

            string answer = "time: " + locationData.data[k].ts + "\n"
                            + "id1: " + str1 + "\n"
                            + "id2: " + str2 + "\n"
                            + "distance: " + locationData1.data[k].dist.content.resources[2].value;
            Debug.Log(answer);



        }


    }

    [Serializable]
    private class Start1
    {
        public LocationData[] data;
    }

    [Serializable]
    private class LocationData
    {
        public string ts;
        public DistData dist;
    }

    [Serializable]
    private class DistData
    {
        public string status;
        public bool valid;
        public bool success;
        public bool failure;
        public ContentData content;
    }

    [Serializable]
    private class ContentData
    {
        public int id;
        public ResourcesData[] resources;

    }

    [Serializable]
    private class ResourcesData
    {
        public int id;
        public string[] value;
    }



    [Serializable]
    private class Start2
    {
        public LocationData1[] data;
    }

    [Serializable]
    private class LocationData1
    {
        public string ts;
        public DistData1 dist;
    }

    [Serializable]
    private class DistData1
    {
        public string status;
        public bool valid;
        public bool success;
        public bool failure;
        public ContentData1 content;
    }

    [Serializable]
    private class ContentData1
    {
        public int id;
        public ResourcesData1[] resources;

    }

    [Serializable]
    private class ResourcesData1
    {
        public int id;
        public float value;
    }

}
