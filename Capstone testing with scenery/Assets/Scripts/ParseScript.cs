using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using System.IO;

namespace latshare{

public class ParseScript : MonoBehaviour{
	//Constants for parsing server data.
	private const string TAG_SOURCE_ID = "10001";
    private const string TAG_DEST_ID = "10002";
    private const string DIST_VAL_ID = "5700";
    private const string DIST_UNITS_ID = "5701";
	
	//Location of the Log File.
	public string logFilePath;
	
	//URLs of the ranging hardware.
    private readonly string[] URLS = {"http://40.122.33.65:8080/api/clients/c91c7641ed9ad451/3330/0/", //Tag
		"http://40.122.33.65:8080/api/clients/ecee381f7777cc28/3330/0/",
		"http://40.122.33.65:8080/api/clients/91a81be2b4e9d809/3330/0/",
		"http://40.122.33.65:8080/api/clients/b7e81d56f671a601/3330/0/"};
		
	//Number of frames to pause between requests.
	private readonly int NETWORK_PAUSE_FRAMES = 30;
    private int frameCount = 0;
	
	//The factor to scale received distances by.
	//Since distances are received in mm, dividing by 1000 makes each Unity unit equivalent to 1 meter.
	public readonly double SCALING_FACTOR = 1.0/10.0;

	//Structure for parsed data.
	Queue<DataPoint> dataQueue = new Queue<DataPoint>();
	
    // Start is called before the first frame update
    void Start(){
		//Flush the log file.
		LogFlush();
    }

	
    // Update is called once per frame
    void Update()
    {
		//Only send a request if we've waited long enough since last time.
		frameCount++;
        if(frameCount > NETWORK_PAUSE_FRAMES)
        {
            frameCount = 0;
            StartCoroutine(GetPos());
        }
    }

	IEnumerator GetPos()
    {
		//Iterate through the list of endpoint URLs we have
		foreach(string URL in URLS){
			//Create and yield a request to each of them for the coroutine
			UnityWebRequest posInfoRequest = UnityWebRequest.Get(URL);
			yield return posInfoRequest.SendWebRequest();
			//If the request fails, display the error code - otherwise, parse it into the queue.
			if (posInfoRequest.isNetworkError || posInfoRequest.isHttpError)
			{
				Debug.Log("Encountered an error while reading URL "+URL);
				Debug.Log(posInfoRequest.error);
				//Continue running - this error shouldn't stop the whole program.
				yield break;
			} else{
				readString(posInfoRequest.downloadHandler.text);
			}
		}
    }
	
	public DataPoint grabData(){
		if(dataQueue.Count > 0){
			return dataQueue.Dequeue();
		} else{
			return null;
		}
	}
	private void readString(string datastring){
		//Create a new datapoint
		DataPoint dp = new DataPoint();

		//split string into words array
        string[] term = datastring.Split(new char[] { ',', ':' }, StringSplitOptions.RemoveEmptyEntries);
        for (int j = 0; j < term.Length; j++){
            //Debug.Log(term[j]);
            switch(term[j]){
                case TAG_SOURCE_ID:
                    string tag1 = term[j+2].Trim(new Char[] {'\"', '}'});
                    dp.setFirstId(tag1);
                    break;
                case TAG_DEST_ID:
                    string tag2 = term[j+2].Trim(new Char[] {'\"', '}'});
                    dp.setSecondId(tag2);
                    break;
                case DIST_VAL_ID:
                    string distStr = term[j+2].Trim(new Char[] {'}'});
                    //float distNum = float.Parse(distStr);
                    dp.setDistance(distStr,SCALING_FACTOR);
                    break;
            }
        }

		//If we have all four necessary pieces of data
		if(dp.hasAllExceptTime()){
			//Log this distance to the log file.
			LogLine(dp.getDistance().ToString());
			//Add this DataPoint to the queue
			dataQueue.Enqueue(dp);
		}
		
	}
	private void LogFlush(){
		if(logFilePath != ""){
			File.Delete(logFilePath);
		}
	}
	private void LogLine(string line){
		if(logFilePath != ""){
			StreamWriter writer = new StreamWriter(logFilePath,true);
			writer.WriteLine(line);
			writer.Close();
		}
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
	public string getFirstIdHex(){
		return firstDeviceId.ToString("x16");
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
	public string getSecondIdHex(){
		return secondDeviceId.ToString("x16");
	}
	public bool hasSecondId(){
		return getMap(4);
	}
	
	public void setDistance(string diststring,double scalar){
		distance = double.Parse(diststring)*scalar;
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