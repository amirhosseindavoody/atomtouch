﻿/**
 * Class: Graph.cs
 * Created by: Justin Moeller
 * Description: This class handles the positioning and the drawing of the graph on the UI. The graph
 * is actually drawn is 3D space, but its coordinates are translated such that the lines are always 
 * facing the camera and the lines are always the same distance from the camera. The x-axis is time
 * and the y-axis is potential energy. Neither scale is a logarithm scale. The points are drawn based
 * on what percentage of the graph the current potential energy is compared to the range the it could
 * be. (i.e. from dataMinimum to dataMaximum) The graph also has its own OnGUI function to define the 
 * labels for the graph. 
 * 
 * 
 * 
 **/ 


using UnityEngine;
using System.Collections;
using System;

public class Graph : MonoBehaviour {
	
	public Material mat;
	private Queue dataPoints;
	
	//graph variables
	public float xCoord;
	public float yCoord;
	public float width = 180.0f;
	public float height = 184.0f;
	public float refreshInterval = 0.1f;
	public float lineWidth = .015f;
	private float zDepth = 5.0f;
	
	public float spacing = 0.5f;
	private float maxDataPoints;
	private float dataMaximum = 5.0f;
	private float dataMinimum = 0.0f;
	private float lowTime;
	private float highTime;
	public string yUnitLabel = "K";
	public string xUnitLabel = "ps";
	public string graphLabel = "Temperature vs Time";
	public Color axisColor = Color.red;
	public Color lineColor = Color.yellow;
	public static int numMDStepSinceLastRecord = 0;
	private float timeSpacing = 20.0f;
	
	void Start () {
		
		//these coorindates will be over written in AtomtouchGUI
		//xCoord = Screen.width - 250;
		//yCoord = 70;
		//maxDataPoints = (width / spacing) + 1;
		//dataPoints = new Queue ();
	}
	
	//This method is called from AtomTouchGUI.cs
	public void RecomputeMaxDataPoints(){
		maxDataPoints = (width / spacing) + 1;
		lowTime = 0.0f;
		highTime = maxDataPoints * StaticVariables.MDTimestepInPicosecond * timeSpacing;
		while (dataPoints.Count < maxDataPoints)
		{
			dataPoints.Enqueue(0.0f);
		}
	}

	/*
	void Update()
	{
		if ((numMDStepSinceLastRecord > (int)timeSpacing)) 
		{
			dataPoints.Dequeue ();
			dataPoints.Enqueue(StaticVariables.currentTemperature);
			highTime = (float)Math.Round(StaticVariables.currentTime,1);
			lowTime = (float)Math.Round(StaticVariables.currentTime - maxDataPoints * StaticVariables.MDTimestepInPicosecond * timeSpacing,1);
			numMDStepSinceLastRecord = 0;
		}
		
	}
	*/
	
	void OnGUI(){
		
		float minRadius = 0.0f;
		float maxRadius = PairDistributionFunction.maxR;
		
		//this function puts the labels of the graph on screen
		GUIStyle graphText = GUI.skin.label;
		graphText.alignment = TextAnchor.MiddleLeft;
		graphText.fontSize = 14;
		graphText.normal.textColor = Color.white;
		AtomTouchGUI atomGUI = Camera.main.GetComponent<AtomTouchGUI> ();
		if (atomGUI.dataPanelActive) {
			GUI.Label (new Rect (xCoord + width/2.0f - 60, Screen.height - yCoord, 200, 20), graphLabel);
			GUI.Label (new Rect (xCoord - 32, Screen.height - (Screen.height * .27f), 100, 20), (dataMaximum).ToString () + yUnitLabel);
			GUI.Label (new Rect (xCoord - 53, Screen.height - yCoord - 15, 100, 20), (PairDistributionFunction.numberOfCalculations).ToString () + yUnitLabel);
			GUI.Label (new Rect (xCoord - 5, Screen.height - yCoord, 100, 20), minRadius.ToString () + xUnitLabel);
			GUI.Label (new Rect (xCoord + width - 35.0f, Screen.height - yCoord, 100, 20), maxRadius.ToString() + xUnitLabel);
		}
		
	}
	
	//OnGUI will draw over this function, so the graph cannot be behind any GUI elements

	void OnPostRender(){
		
		AtomTouchGUI atomGUI = Camera.main.GetComponent<AtomTouchGUI> ();
		if (atomGUI.dataPanelActive) {
			//draw the background for the graph
			Vector3 upperLeft = GetComponent<Camera>().ScreenToWorldPoint (new Vector3 (xCoord, (yCoord+height), zDepth));
			Vector3 lowerLeft = GetComponent<Camera>().ScreenToWorldPoint (new Vector3(xCoord, yCoord, zDepth));
			Vector3 upperRight = GetComponent<Camera>().ScreenToWorldPoint (new Vector3 (xCoord + width, (yCoord+height), zDepth));
			Vector3 lowerRight = GetComponent<Camera>().ScreenToWorldPoint (new Vector3(xCoord + width, yCoord, zDepth));
			Color customColor = new Color (0.5f, 0.5f, 0.5f, 1.0f);
			StaticVariables.DrawQuad (upperLeft, upperRight, lowerLeft, lowerRight, customColor, mat);
			
			//horizontal axis
			StaticVariables.DrawLine (lowerLeft, lowerRight, axisColor, axisColor, lineWidth, mat);
			
			//vertical axis
			StaticVariables.DrawLine (upperLeft, lowerLeft, axisColor, axisColor, lineWidth, mat);
			
			//tick marks
			int numTicks = (int)(highTime - lowTime) + 1;
			float tickSpacing = (float)((width-10.0f) / numTicks);
			for(int i = 0; i < numTicks+1; i++){
				Vector3 top = GetComponent<Camera>().ScreenToWorldPoint(new Vector3(xCoord + (i*tickSpacing), yCoord + 10.0f, zDepth));
				Vector3 bottom = GetComponent<Camera>().ScreenToWorldPoint(new Vector3(xCoord + (i*tickSpacing), yCoord - 10.0f, zDepth));
				StaticVariables.DrawLine(top, bottom, Color.black, Color.black, lineWidth, mat);
			}
			
			//draw the lines on the graph
			//object[] dataPointArray = dataPoints.ToArray ();
			float[] dataPointArray = PairDistributionFunction.PairDistributionAverage;
			spacing = width / dataPointArray.Length;
			for (int i = 0; i < dataPointArray.Length - 1; i++) {
				
				float firstPercentage = ((float)dataPointArray[i]) / (dataMaximum - dataMinimum);
				float secondPercentage = ((float)dataPointArray[i+1]) / (dataMaximum - dataMinimum);
				
				if(firstPercentage > 1.0f){
					firstPercentage = 1.0f;
				}
				else if(firstPercentage < 0.0f){
					firstPercentage = 0.0f;
				}
				if(secondPercentage > 1.0f){
					secondPercentage = 1.0f;
				}
				else if(secondPercentage < 0.0f){
					secondPercentage = 0.0f;
				}
				
				float firstYAddition = firstPercentage * height;
				float secondYAddition = secondPercentage * height;
				
				Vector3 firstPoint = GetComponent<Camera>().ScreenToWorldPoint(new Vector3(xCoord + (i*spacing), yCoord + firstYAddition, zDepth));
				Vector3 secondPoint = GetComponent<Camera>().ScreenToWorldPoint(new Vector3(xCoord + ((i+1)*spacing), yCoord + secondYAddition, zDepth));
				StaticVariables.DrawLine(firstPoint, secondPoint, lineColor, lineColor, lineWidth, mat);
			}
		}
		
		
		
	}
}
