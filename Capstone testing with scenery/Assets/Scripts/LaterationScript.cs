﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

namespace latshare{

public class LaterationScript : MonoBehaviour{
	//Data-Parsing Constants
	private long TAG_ID;
	
	//Public-facing Anchor Objects
	public GameObject Anchor1;
	public GameObject Anchor2;
	public GameObject Anchor3;
	
	public ParseScript Parser;
	
    /*Game-Facing Functions*/
    void Start()
    {
		//Set the three passed objects as our anchors, and initialize their distances.
        ancs = new Anchor[3];
		ancs[0] = new Anchor(Anchor1,1,Int64.Parse("95f1ef60fdd32449",System.Globalization.NumberStyles.HexNumber));
		ancs[1] = new Anchor(Anchor2,1,Int64.Parse("f8c202cdc061c8bf",System.Globalization.NumberStyles.HexNumber));
		ancs[2] = new Anchor(Anchor3,1,Int64.Parse("77da1470c2e3c8ab",System.Globalization.NumberStyles.HexNumber));
		TAG_ID = Int64.Parse("98db0e5cae045b59",System.Globalization.NumberStyles.HexNumber);
		//Set our current position to the object's current position.
		currPos = new double[2] {this.transform.position[0],this.transform.position[2]};
		//Read the test JSON file
    }
    void Update(){
		//Grab the newest data from the parser
		DataPoint newdata = Parser.grabData();
		//If some new data is present, modify our distances as needed - otherwise ignore
		if(newdata != null){
			//One of the two tags should be the tracked object
			if(newdata.getFirstId() != TAG_ID && newdata.getSecondId() != TAG_ID){
				return;
			}
			//The other tag should be an anchor - find which anchor, and set its distance
			for(int i=0;i<ancs.Length;i++){
				if(newdata.getFirstId() == ancs[i].id || newdata.getSecondId() == ancs[i].id){
					ancs[i].setDist(newdata.getDistance());
					break;
				}
			}
		}
		//Run one lateration step (handles lack of new data inside)
        runStep();
    }
	
	/*Lateration Functions*/
	private readonly static double DIRTY_THRESHOLD = 0.1;
	private double[] currPos;
	private Anchor[] ancs;
	private bool changedLast = false;
	
	private void runStep(){
		bool dirtied = changedLast;
		for(int i=0;i<ancs.Length;i++){
			dirtied |= ancs[i].isDirty();
		}
		if(!dirtied){
			return;
		}
		double[] oldPos = currPos;
		currPos = descend();
		currPos = bullseye();
		//Debug.Log("New Position is ("+currPos[0]+","+currPos[1]+")");
		double diffSum = 0;
		for(int i=0;i<currPos.Length;i++){
			diffSum += Math.Abs(currPos[i] - oldPos[i]);
		}
		changedLast = (diffSum >= DIRTY_THRESHOLD);
		this.transform.position = new Vector3((float)currPos[0],this.transform.position[1],(float)currPos[1]);
	}
	
	private double[] descend(){
		double[] gradient = new double[2] {0.0,0.0};
		for(int a=0;a<ancs.Length;a++){
			double[] diff = ancs[a].gradient(currPos);
			for(int d=0;d<currPos.Length;d++){
				gradient[d] += diff[d];
			}
		}
		//Debug.Log("Gradient calculated as ("+gradient[0]+","+gradient[1]+")");
		double[] newPos = new double[2];
		for(int d=0;d<currPos.Length;d++){
			newPos[d] = currPos[d] - gradient[d]/ancs.Length;
		}
		return newPos;
	}
	private void updateTestDists(double[] target){
		for(int a=0;a<ancs.Length;a++){
			ancs[a].setDist(ancs[a].calcDistTo(target));
		}
	}
	
	private double[] bullseye(){
		double[] expected = new double[2] {0.0,0.0};
		//For each anchor, find the point at the right distance from it in the direction of the current target
		for(int a=0;a<ancs.Length;a++){
			double[] pointer = ancs[a].pointerPos(currPos);
			//Add this point to the total
			for(int d=0;d<expected.Length;d++){
				expected[d] += pointer[d];
			}
		}
		//Find the average of the summed points, and return
		for(int d=0;d<expected.Length;d++){
			expected[d] /= ancs.Length;
		}
		return expected;
	}
	
	class Anchor{
		//private readonly static int DIM = 2;
		private double[] pos;
		private double dist;
		private bool dirtied;
		private double sqrpos;
		private GameObject gobj;
		public long id;
		
		public Anchor(GameObject gobj,double dist,long id){
			this.gobj = gobj;
			this.setPos();
			this.setDist(dist);
			this.id = id;
			Debug.Log("Anchor "+this.id+" initialized.");
		}
		
		public void setPos(){
			this.pos = new double[2] {gobj.transform.position[0],gobj.transform.position[2]};
			sqrpos = 0.0;
			for(int i=0;i<pos.Length;i++){
				sqrpos += Math.Pow(this.pos[i],2);
			}
		}
		
		public void setDist(double dist){
			if(!dirtied && this.dist != dist){
			}
			dirtied = (this.dist != dist);
			this.dist = dist;
		}
		
		public bool isDirty(){
			if(this.dirtied){
				this.dirtied = false;
				return true;
			} else{
				return false;
			}
		}
		
		public double calcDistTo(double[] target){
			double sum = 0;
			for(int i=0;i<target.Length && i<pos.Length;i++){
				sum += Math.Pow(pos[i]-target[i],2);
			}
			return Math.Sqrt(sum);
		}
		
		public double[] gradient(double[] target){
			double[] returnable = new double[2] {0.0,0.0};
			double hval = Math.Sqrt(Math.Pow(target[0],2)+Math.Pow(target[1],2)+sqrpos-2*(pos[0]*target[0]+pos[1]*target[1]));
			if(hval == 0){
				return returnable;
			}
			double ival = 2*(target[0]-pos[0]);
			double jval = 2*(target[1]-pos[1]);
			returnable[0] = ival*(1-dist/hval);
			returnable[1] = jval*(1-dist/hval);
			return returnable;
		}
		
		public double[] pointerPos(double[] target){
			double[] returnable = new double[target.Length];
			double scalar = this.dist/this.calcDistTo(target);
			for(int d=0;d<target.Length;d++){
				returnable[d] = (target[d]-this.pos[d])*scalar+this.pos[d];
			}
			return returnable;
		}
	}
}

}