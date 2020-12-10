using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

namespace latshare{

public class LaterationScript : MonoBehaviour{
	//Data-Parsing Constants
	private long TAG_ID;
	public static readonly float Ypos = 0;
	
	//Public-facing Anchor Objects
	public GameObject Tag;
	public GameObject Anchor1;
	public GameObject Anchor2;
	public GameObject Anchor3;
	
	public ParseScript Parser;
	
    /*Game-Facing Functions*/
    void Start()
    {
		//Set the three passed objects as our anchors, and initialize their distances.
		TAG_ID = Int64.Parse("98db0e5cae045b59",System.Globalization.NumberStyles.HexNumber);
        rangers = new Ranger[4];
		rangers[0] = new Ranger(Tag,TAG_ID,false);
		rangers[1] = new Ranger(Anchor1,Int64.Parse("95f1ef60fdd32449",System.Globalization.NumberStyles.HexNumber),true);
		rangers[2] = new Ranger(Anchor2,Int64.Parse("f8c202cdc061c8bf",System.Globalization.NumberStyles.HexNumber),true);
		rangers[3] = new Ranger(Anchor3,Int64.Parse("77da1470c2e3c8ab",System.Globalization.NumberStyles.HexNumber),true);
		/*for(int i=0;i<rangers.Length;i++){
			for(int j=i+1;j<rangers.Length;j++){
				rangers[i].setDist(rangers[j].ID,DIRTY_THRESHOLD,true);
				rangers[j].setDist(rangers[i].ID,DIRTY_THRESHOLD,true);
			}
		}*/
    }
    void Update(){
		//Grab the newest data from the parser
		DataPoint newdata = Parser.grabData();
		//If some new data is present, modify our distances as needed - otherwise ignore
		if(newdata != null){
			/*//Debug.Log("Non-null data found with tags "+newdata.getFirstId()+" and "+newdata.getSecondId());
			//One of the two tags should be the tracked object
			if(newdata.getFirstId() != TAG_ID && newdata.getSecondId() != TAG_ID){
				return;
			}
			//The other tag should be an anchor - find which anchor, and set its distance
			for(int i=0;i<rangers.Length;i++){
				if(newdata.getFirstId() == rangers[i].id || newdata.getSecondId() == rangers[i].id){
					rangers[i].setDist(newdata.getDistance());
					break;
				}
			}*/
			Ranger firstRanger = null;
			Ranger secondRanger = null;
			foreach(Ranger ranger in rangers){
				if(ranger.ID == newdata.getFirstId()){
					firstRanger = ranger;
				} else if(ranger.ID == newdata.getSecondId()){
					secondRanger = ranger;
				}
			}
			if(firstRanger != null && secondRanger != null){
				if(firstRanger.anchor && secondRanger.anchor){
					firstRanger.setDist(newdata.getSecondId(),newdata.getDistance(),true);
					secondRanger.setDist(newdata.getFirstId(),newdata.getDistance(),true);
				} else{
					if(!firstRanger.anchor){
						firstRanger.setDist(newdata.getSecondId(),newdata.getDistance(),false);
					}
					if(!secondRanger.anchor){
						secondRanger.setDist(newdata.getFirstId(),newdata.getDistance(),false);
					}
				}
			}
		}
		//Run one lateration step (handles lack of new data inside)
        runStep();
    }
	
	/*Lateration Functions*/
	private readonly static double DIRTY_THRESHOLD = 0.01;
	private static Ranger[] rangers;
	private bool changedLast = false;
	
	private void runStep(){
		bool changedCurrent = false;
		foreach(Ranger ranger in rangers){
			if(changedLast || ranger.isDirty()){
				changedCurrent |= ranger.descend();
				if(!ranger.anchor){
					changedCurrent |= ranger.bullseye();
				}
			}
		}
		double[] avg = avgPos();
		foreach(Ranger ranger in rangers){
			ranger.shiftRevPos(avg);
		}
		changedLast = changedCurrent;
	}
	
	private double[] avgPos(){
		double[] totalPos = new double[2] {0.0,0.0};
		int counted = 0;
		foreach(Ranger ranger in rangers){
			if(ranger.anchor){
				counted++;
				double[] rangerPos = ranger.getPos();
				for(int i=0;i<totalPos.Length;i++){
					totalPos[i] += rangerPos[i];
				}
			}
		}
		if(counted > 0){
			for(int i=0;i<totalPos.Length;i++){
				totalPos[i] /= counted;
			}
		}
		return totalPos;
	}
	
	/*private double[] bullseye(){
		double[] expected = new double[2] {0.0,0.0};
		//For each anchor, find the point at the right distance from it in the direction of the current target
		for(int a=0;a<rangers.Length;a++){
			double[] pointer = rangers[a].pointerPos(currPos,TAG_ID);
			//Add this point to the total
			for(int d=0;d<expected.Length;d++){
				expected[d] += pointer[d];
			}
		}
		//Find the average of the summed points, and return
		for(int d=0;d<expected.Length;d++){
			expected[d] /= rangers.Length;
		}
		return expected;
	}*/
	
	class Ranger{
		private GameObject gobj;
		private double[] pos;
		public double sqrpos;
		private bool dirty;
		private Dictionary<long,double> distances;
		public long ID;
		public bool anchor;
		private int updates;
		private static readonly double PosRetention = 0.95;
		
		public Ranger(GameObject gobj,long ID, bool anchor){
			this.gobj = gobj;
			this.pos = new double[2] {gobj.transform.position[0],gobj.transform.position[2]};
			this.sqrPos();
			this.dirty = false;
			this.distances = new Dictionary<long,double>();
			this.ID = ID;
			this.anchor = anchor;
			this.updates = -1;
			Debug.Log("Ranger "+this.ID+" initialized.");
		}
		
		public double[] getPos(){
			return this.pos;
		}
		
		public void sqrPos(){
			sqrpos = 0.0;
			for(int i=0;i<pos.Length;i++){
				sqrpos += Math.Pow(this.pos[i],2);
			}
		}
		
		public double calcDistTo(double[] target){
			double sum = 0;
			for(int i=0;i<target.Length && i<pos.Length;i++){
				sum += Math.Pow(pos[i]-target[i],2);
			}
			return Math.Sqrt(sum);
		}
		
		//Failed refactor of gradient algorithm
		/*public double[] gradient(Ranger target){
			double[] targetPos = target.getPos();
			double[] returnable = new double[2] {0.0,0.0};
			double dist = this.distances[target.ID];
			double[] posdiffs = {this.pos[0]-targetPos[0],this.pos[1]-targetPos[1]};
			double combodist = Math.Sqrt(Math.Pow(posdiffs[0],2)+Math.Pow(posdiffs[1],2));
			if(combodist == 0){
				return returnable;
			}
			double invdsqr = 1/Math.Pow(dist,2);
			returnable[0] = 2*posdiffs[0]*(1 - dist/combodist);
			returnable[1] = 2*posdiffs[1]*(1 - dist/combodist);
			return returnable;
		}*/
		public double[] gradient(Ranger target){
			double[] targetPos = target.getPos();
			double[] returnable = new double[2] {0.0,0.0};
			if(this.distances.ContainsKey(target.ID)){
				double dist = this.distances[target.ID];
				double hval = Math.Sqrt(Math.Pow(pos[0],2)+Math.Pow(pos[1],2)+Math.Pow(targetPos[0],2)+Math.Pow(targetPos[1],2)-2*(targetPos[0]*pos[0]+targetPos[1]*pos[1]));
				if(hval == 0){
					return returnable;
				}
				double ival = 2*(pos[0]-targetPos[0]);
				double jval = 2*(pos[1]-targetPos[1]);
				returnable[0] = ival*(1-dist/hval);
				returnable[1] = jval*(1-dist/hval);
			}
			return returnable;
		}
		
		public bool descend(){
			this.dirty = false;
			double[] gradient = new double[2] {0.0,0.0};
			foreach(Ranger ranger in rangers){
				if(this.distances.ContainsKey(ranger.ID)){
					double[] diff = this.gradient(ranger);
					for(int d=0;d<this.pos.Length;d++){
						gradient[d] += diff[d];
					}
					//Debug.Log("Gradient calculating as ("+gradient[0]+","+gradient[1]+") from ID "+ranger.ID+" with dist "+this.distances[ranger.ID]);
				}
			}
			if(this.distances.Count > 0){
				for(int d=0;d<this.pos.Length;d++){
					gradient[d] /= this.distances.Count;
				}
				return this.shiftRevPos(gradient);
			} else{
				return false;
			}
		}
		
		public double[] pointerPos(double[] target,long targetID){
			double[] returnable = new double[target.Length];
			double scalar = this.distances[targetID]/this.calcDistTo(target);
			for(int d=0;d<target.Length;d++){
				returnable[d] = (this.pos[d]-target[d])*scalar+target[d];
			}
			return returnable;
		}
		
		public bool bullseye(){
			this.dirty = false;
			double[] expected = new double[2] {0.0,0.0};
			int counted = 0;
			//For each anchor, find the point at the right distance from it in the direction of the current target
			foreach(Ranger ranger in rangers){
				if(this.distances.ContainsKey(ranger.ID)){
					counted++;
					double[] pointer = this.pointerPos(ranger.getPos(),ranger.ID);
					//Add this point to the total
					for(int d=0;d<expected.Length;d++){
						expected[d] += pointer[d];
					}
				}
			}
			//Find the average of the summed points, and return
			double diffSum = 0.0;
			if(counted > 0){
				for(int d=0;d<expected.Length;d++){
					expected[d] /= counted;
					diffSum += Math.Abs(expected[d] - this.pos[d]);
				}
				this.pos = expected;
				return diffSum > DIRTY_THRESHOLD;
			} else{
				return false;
			}
		}
		
		/*public bool shiftPos(double[] shift){
			double diffSum = 0.0;
			for(int d=0;d<this.pos.Length;d++){
				this.pos[d] += shift[d];
				diffSum += Math.Abs(shift[d]);
			}
			//Debug.Log("Newpos calculated as ("+this.pos[0]+","+this.pos[1]+") for ID "+this.ID);
			this.sqrPos();
			this.gobj.transform.position = new Vector3((float)this.pos[0],(float)Ypos,(float)this.pos[1]);
			return diffSum > DIRTY_THRESHOLD;
		}*/
		
		public bool shiftRevPos(double[] shift){
			double diffSum = 0.0;
			for(int d=0;d<this.pos.Length;d++){
				this.pos[d] -= shift[d];
				diffSum += Math.Abs(shift[d]);
			}
			//Debug.Log("Newpos calculated as ("+this.pos[0]+","+this.pos[1]+") for ID "+this.ID);
			this.sqrPos();
			this.gobj.transform.position = new Vector3((float)this.pos[0],(float)Ypos,(float)this.pos[1]);
			return diffSum > DIRTY_THRESHOLD;
		}
		
		public bool isDirty(){
			return this.dirty;
		}
		
		public void setDist(long ID,double dist,bool anchorUpdate){
			if(!this.anchor){
				this.distances[ID] = dist;
				this.dirty = true;
			} else if(anchorUpdate){
				if(this.distances.ContainsKey(ID)){
					if(dist < this.distances[ID]/(1+Ranger.PosRetention)){
						dist = this.distances[ID]/(1+Ranger.PosRetention);
					} else if(dist > this.distances[ID]*(1+Ranger.PosRetention)){
						dist = this.distances[ID]*(1+Ranger.PosRetention);
					}
					this.distances[ID] = this.distances[ID]*Ranger.PosRetention + dist*(1 - Ranger.PosRetention);
				} else{
					this.distances[ID] = dist;
				}
				//Debug.Log("Distance from anchor "+this.ID+" to anchor "+ID+" is now "+this.distances[ID]);
				this.updates++;
				this.dirty = true;
			}
		}
	}
}

}