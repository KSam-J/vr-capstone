using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;


public class ParsingJSON1 : MonoBehaviour
{
    //constant declared
    const string TAG_SOURCE_ID = "10001,";
    const string TAG_DEST_ID = "10002,";
    const string DIST_VAL_ID = "5700,";
    const string DIST_UNITS_ID = "5701,";

    //Declaring all data structure variable
    List<string> timeStamp = new List<string>();
    List<string> firstDeviceId = new List<string>();
    List<string> secondDeviceId = new List<string>();
    List<float> distance = new List<float>();

    // Start is called before the first frame update
    void Start()
    {
        //split file into words array
        System.IO.StreamReader file = new System.IO.StreamReader("Assets/data.json");
        string[] words = file.ReadToEnd().Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
      
        for (int i = 0; i < words.Length; i++)
        {
            //Debug.Log(i);
            //Debug.Log(words[i]);
            switch (words[i].Trim())
            {
                //time
                case "{\"ts\":":
                    i++;
                    string strq = words[i];
                    string strReplaceq = strq.Replace("\"", "").Trim();
                    string strReplace1q = strReplaceq.Replace(",", "").Trim();
                    timeStamp.Add(strReplace1q);
                    //Debug.Log(strReplace1q);
                    break;

                //resouces
                case "\"resources\":":
                    bool done = false;
                    while (!done)
                    {
                        i++;
                        switch (words[i].Trim())
                        {
                            //1st tag
                            case TAG_SOURCE_ID:
                                i += 2;
                                string tag1 = parseTag(words, i);
                                i += 15;
                                firstDeviceId.Add(tag1);
                                //Debug.Log(tag1);
                                break;

                            //2nd tag
                            case TAG_DEST_ID:
                                i += 2;                                
                                string tag2 = parseTag(words, i);
                                i += 15;
                                secondDeviceId.Add(tag2);
                                //Debug.Log(tag2);
                                break;

                            //distance
                            case DIST_VAL_ID:
                                i += 2;
                                string dist = words[i].Trim();
                                //Debug.Log(dist);
                                string strReplacew = dist.Replace("}", "").Trim();
                                string strReplace1w = strReplacew.Replace(",", "").Trim();
                                float distNum = float.Parse(strReplace1w);
                                distance.Add(distNum);
                                //Debug.Log(distNum);
                                break;
                            //unit
                            case DIST_UNITS_ID:
                                done = true;
                                break;
                        }
                    }
                    break;

                //all other symbols
                default:
                    break;

            }
        }


        //display results
        /*
        Debug.Log(timeStamp.Count);
        Debug.Log(firstDeviceId.Count);
        Debug.Log(secondDeviceId.Count);
        Debug.Log(distance.Count);
        */
        
        for (int i = 0; i < distance.Count; i++)
        {
            string answer = "time: " + timeStamp[i] + "\n"
                                + "id1: " + firstDeviceId[i] + "\n"
                                + "id2: " + secondDeviceId[i] + "\n"
                                + "distance: " + distance[i];
            Debug.Log(answer);
        }
     
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //private method: return tag id
    private string parseTag(string[] words, int index)
    {
        // "7"
        string strReplace, strReplace1, strReplace2, strReplace3, strReplace4, str, answer = "";
        for (int j = 0; j < 16; j++)
        {
            str = words[index + j];     
            strReplace = str.Replace("\"", "").Trim();
            strReplace1 = strReplace.Replace(",", "").Trim();
            strReplace2 = strReplace1.Replace("[", "").Trim();
            strReplace3 = strReplace2.Replace("]", "").Trim();
            strReplace4 = strReplace3.Replace("}", "").Trim();
            answer += strReplace4;
        }
        return answer;
    }
}
