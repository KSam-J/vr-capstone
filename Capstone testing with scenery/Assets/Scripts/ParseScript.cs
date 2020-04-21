using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using UnityEngine.Networking;


namespace latshare{

public class ParseScript : MonoBehaviour{
	private const string TAG_SOURCE_ID = "10001,";
    private const string TAG_DEST_ID = "10002,";
    private const string DIST_VAL_ID = "5700,";
    private const string DIST_UNITS_ID = "5701,";
	
	//Structures for Getting server data
	public string testString;
    private readonly string localURL = "localhost:5000/";
    private int count;
    private string test;

	//Structures for Parsed Data
	Queue<DataPoint> dataQueue = new Queue<DataPoint>();
	private DateTime firstTime = DateTime.MinValue;
	private DateTime startTime = DateTime.Now;
	
	public string fileName;
	public bool mimicTime;
	
    // Start is called before the first frame update
    void Start(){
		if(fileName.Length > 0){
			readJSON(fileName);
		}

		//default values go here
        count = 0;
        testString = "";
    }

    // Update is called once per frame
    void Update()
    {
		/*
		//The following is how networked data would be implemented:
		//Start with a string of data taken from the server
		string datastring = "{JSON formatted data}";
		//Then just pass that string to the readString method
		readString(datastring);
		*/
		//to test and not get overloaded with info, 1 json input per second
        count++;
        if(count >= 1)
        {
            count = 0;
            //Debug.Log("Running GetPos");
            StartCoroutine(GetPos());
            //Debug.Log("After GetPos");
        }
    }

	    IEnumerator GetPos()
    {
        string PosURL = localURL + "data"; // actual url from base

        UnityWebRequest posInfoRequest = UnityWebRequest.Get(PosURL);

        
        yield return posInfoRequest.SendWebRequest();
        test = posInfoRequest.downloadHandler.text;
        //Debug.Log(test);
		readString(test);

        if (posInfoRequest.isNetworkError || posInfoRequest.isHttpError)
        {
            Debug.LogError(posInfoRequest.error);
            Debug.Log("EORROR");
            yield break;
        }
		
    }
	
	public DataPoint grabData(){
		if(dataQueue.Count > 0 && (!mimicTime || (dataQueue.Peek().getTime() - firstTime) <= (DateTime.Now - startTime))){
			return dataQueue.Dequeue();
		} else{
			return null;
		}
	}
	
	private void readJSON(string filename){
		//Debug.Log("Reading file "+fileName);
		System.IO.StreamReader file = new System.IO.StreamReader(filename);
		readString(file.ReadToEnd());
		file.Close();
	}
	private void readString(string datastring){
		//Keep a datapoint
		DataPoint dp = new DataPoint();
		//split string into words array
        string[] words = datastring.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
      
        for (int i = 0; i < words.Length; i++){
            switch (words[i].Trim())
            {
                //time
                case "{\"ts\":":
                    i++;
                    string strq = words[i];
                    string strReplaceq = strq.Replace("\"", "").Trim();
                    string strReplace1q = strReplaceq.Replace(",", "").Trim();
					dp.setTime(strReplace1q);
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
                                i += 3;
                                string tag1 = parseTag(words, i);
                                i += 15;
								dp.setFirstId(tag1);
                                //Debug.Log(tag1);
                                break;

                            //2nd tag
                            case TAG_DEST_ID:
                                i += 3;                                
                                string tag2 = parseTag(words, i);
                                i += 15;
								dp.setSecondId(tag2);
                                //Debug.Log(tag2);
                                break;

                            //distance
                            case DIST_VAL_ID:
                                i += 2;
                                string dist = words[i].Trim();
                                //Debug.Log(dist);
                                string strReplacew = dist.Replace("}", "").Trim();
                                string strReplace1w = strReplacew.Replace(",", "").Trim();
								dp.setDistance(strReplace1w);
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

			//If we have all four necessary pieces of data
			if(dp.hasAllExceptTime()){
				//Debug.Log("Adding new DataPoint");
				//Add this DataPoint to the queue
				dataQueue.Enqueue(dp);
				//Reset the one we're currently working on
				dp = new DataPoint();
			}
        }
		
		firstTime = dataQueue.Peek().getTime();
		startTime = DateTime.Now;
		
		//Debug.Log("Finished enqueuing data");
	}
	private string parseTag(string[] words, int index){
        string answer = "";
        for (int j = 0; j < 16; j++){
            answer += words[index + j]
				.Replace("\"", "")
				.Replace(",", "")
				.Replace("[", "")
				.Replace("]", "")
				.Replace("}", "")
				.Trim();
        }
        return answer;
    }
}

/**
This class models a set consisting of a timestamp, a pair of identifiers, and the measured distance between the identified objects at that timestamp.

To keep track of which data has been set and which has not, a short bitmask ("datamap") is used.

In usage, each of the fields can be set with the appropriate "set_()" method, and the DataPoint can be verified complete
using the "hasAll()" method.  To check whether a particular field has already been set (and would thus be overwritten),
the appropriate "has_()" method can be used.  Finally, once the object is complete, it can be passed to the
LaterationScript for usage.
*/
public class DataPoint{
	private DateTime timeStamp;
    private long firstDeviceId;
    private long secondDeviceId;
    private double distance;
	private short datamap = 0;
	
	public void setTime(string timestring){
		timeStamp = DateTime.ParseExact(timestring, "yyyy-MM-ddTHH:mm:ss.ffffff+00:00", System.Globalization.CultureInfo.InvariantCulture);
		setMap(1);
	}
	public DateTime getTime(){
		return timeStamp;
	}
	public bool hasTime(){
		return getMap(1);
	}
	
	public void setFirstId(string longstring){
		firstDeviceId = Int64.Parse(longstring,System.Globalization.NumberStyles.HexNumber);
		setMap(2);
	}
	public long getFirstId(){
		return firstDeviceId;
	}
	public bool hasFirstId(){
		return getMap(2);
	}
	
	public void setSecondId(string longstring){
		secondDeviceId = Int64.Parse(longstring,System.Globalization.NumberStyles.HexNumber);
		setMap(4);
	}
	public long getSecondId(){
		return secondDeviceId;
	}
	public bool hasSecondId(){
		return getMap(4);
	}
	
	public void setDistance(string diststring){
		distance = double.Parse(diststring)/10.0;
		setMap(8);
	}
	public double getDistance(){
		return distance;
	}
	public bool hasDistance(){
		return getMap(8);
	}
	
	public bool hasAll(){
		return datamap == 15;
	}
	
	public bool hasAllExceptTime(){
		return datamap/2 == 7;
	}
	
	private void setMap(short val){
		if(!getMap(val)){
			datamap += val;
		}
	}
	private bool getMap(short val){
		return (datamap/val)%2 != 0;
	}
}

}