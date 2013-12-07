using UnityEngine;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.IO;

// Connects to the dialog plugin 
// For recieving data from dialog it uses a network loop 
// It keeps listening different commands issued by dialog
// Once got some commands it performs them
// Some commands are related to TheraMem game, some for Mirror box
// It also allows various camera options e.g. mirroring, or switching camera
public class ConnectToDialog : MonoBehaviour {
	
	private bool clientConnected;

	// variables for the dialog
	private string data = "";
	private string dataPath;
	
	// Thread for running the network loop in
	private Thread serverThread;
	
	// Network server and client objects
	private TcpListener serverListener;
	private TcpClient client;
	
	// variables for actions
	private string currentGame = "";
	
	public Component[] gameScripts;
	
	// variables for TheraMem actions
	private string gameMode;
	
	// Initialization function
	void Start() {
		// path where the exe should be found
		dataPath = Application.dataPath;
		
		// Thread starts and runs the 'SayHello' function within
		ThreadStart ts = new ThreadStart(GetDataFromClient);
		serverThread = new Thread(ts);
		serverThread.Start();
		
		this.SetGameMode = "";
	}
	
	public string SetGameMode
	{
		get
		{
			return this.gameMode;
		}
		set
		{
			this.gameMode = value;
		}
	}
	
	// Connects to the windows form and loops to wait for actions from the user
	void GetDataFromClient() {
		
		try {
			int port = 1234;
			serverListener = new TcpListener(port);
			serverListener.Start();
			
			print("Server started..");
			
			data = "";
			byte[] bytesFromClient = new byte[1024];
			print(dataPath);
			// calling to open application (client)
			System.Diagnostics.Process.Start(dataPath + "/ART_Application.exe");
			
			client = serverListener.AcceptTcpClient();
			clientConnected = true;
			print("Client Connected!");
			
			while (clientConnected) {
				
				NetworkStream clientStream = client.GetStream();
				
				Int32 i;
				
				while ((i = clientStream.Read(bytesFromClient, 0, bytesFromClient.Length)) != 0) {
					
					data = Encoding.ASCII.GetString(bytesFromClient, 0, i);
				}
				
				if (data == "1$shutdown") {
					clientConnected = false;
					client.Close();
					serverListener.Stop();
				}
				
			}
			
		} catch (ThreadAbortException e) {
			print(e.Message);
		}
		
		finally {
			client.Close();
			serverListener.Stop();
		}
		
	}
	
	// Update functions contantly monitoring for data from the user (Windows Form)
	// and when it does come, a switch statement will select which actions to perform
	// within Unity.
	void Update() {
		
		// the data from the Form
		string newData = data;
		// 'data' reset to empty string to wait for more commands
		data = "";
		
		if (newData != "") {
			print(newData);
		}
		
		if (newData != null && newData != "") {
			
			string numberOfCommands = newData[0].ToString();
			
			int numCommands = Int32.Parse(numberOfCommands);
			print("Number of Commands: " + numCommands);
			
			if (numCommands > 0) {
			
				newData = newData.Substring(2);
				
				string[] commands = new string[numCommands];
				int index = 0;
				int currStartPos = 0;
				
				for (int i = 0; i < newData.Length; i++) {
					
					if (newData[i] == '$') {
						int wordLength = i - currStartPos;
						commands[index] = newData.Substring(currStartPos, wordLength);
						currStartPos = i+1;
						index++;
					} else if (i == newData.Length-1) {
						int wordLength = i - currStartPos + 1;
						commands[index] = newData.Substring(currStartPos, wordLength);
					}
				}
				
				// For each command in the list from the form, an action must be performed
				foreach (string s in commands) {
				
					switch (s)
					{
					case "Shutdown":
						print("Shutdown");
						try {
							Application.Quit();
						} catch (Exception e) {
							Debug.Log("Error When Application.Quit() Called..... " + e.ToString());
						}
						break;
					case "TheraMemMemory":
						print("Starting TheraMem (Memory Version)");
						this.SetGameMode = "Memory";
						Camera.main.GetComponent<TheraMem>().enabled = true;
						currentGame = "TheraMem";
						Time.timeScale = 0;
						break;
					case "TheraMemOriginal":
						print("Starting TheraMem (Original Version)");
						this.SetGameMode = "Original";
						Camera.main.GetComponent<TheraMem>().enabled = true;
						Time.timeScale = 0;
						break;
					case "RestartTheraMemMemory":
						print("Restarting");
						Camera.main.GetComponent<TheraMem>().enabled = false;
						Thread.Sleep(50);
						this.SetGameMode = "Memory";
						Camera.main.GetComponent<TheraMem>().enabled = true;
						Time.timeScale = 0;
						break;
					case "RestartTheraMemOriginal":
						print("Restarting");
						Camera.main.GetComponent<TheraMem>().enabled = false;
						Thread.Sleep(50);
						this.SetGameMode = "Original";
						Camera.main.GetComponent<TheraMem>().enabled = true;
						Time.timeScale = 0;
						break;
					case "TheraMem":
						Camera.main.GetComponent<TheraMem>().enabled = false;
						break;
					case "PauseGame":
						Time.timeScale = 0;
						break;
					case "ResumeGame":
						Time.timeScale = 1;
						break;
					case "LeftOnLeft":
						print("LeftOnLeft");
						GameObject leftPlane = GameObject.Find("LeftPlane");
						if (leftPlane.renderer.enabled == false) {
							leftPlane.renderer.enabled = true;
						}
						leftPlane.GetComponent<Plugin_Handler>().set_cameras(0);
						break;
						
					case "RightOnLeft":
						print("RightOnLeft");
						GameObject leftPlane1 = GameObject.Find("LeftPlane");
						if (leftPlane1.renderer.enabled == false) {
							leftPlane1.renderer.enabled = true;
						}
						leftPlane1.GetComponent<Plugin_Handler>().set_cameras(1);
						break;
						
					case "NothingOnLeft":
						print("NothingOnLeft");
						GameObject leftPlane2 = GameObject.Find("LeftPlane");
						leftPlane2.GetComponent<Plugin_Handler>().set_cameras(-1);
						leftPlane2.renderer.enabled = false;
						break;
						
					case "LeftMirror":
						print("LeftMirror");
						GameObject.Find("LeftPlane").transform.localScale += new Vector3(8, 0, 0);
						break;
						
					case "LeftUnMirror":
						print("LeftUnMirror");
						GameObject.Find("LeftPlane").transform.localScale += new Vector3(-8, 0, 0);
						break;
						
					case "RightOnRight":
						print("RightOnRight");
						GameObject rightPlane = GameObject.Find("RightPlane");
						if (rightPlane.renderer.enabled == false) {
							rightPlane.renderer.enabled = true;
						}
						rightPlane.GetComponent<Plugin_Handler>().set_cameras(1);
						break;
						
					case "LeftOnRight":
						print("LeftOnRight");
						GameObject rightPlane1 = GameObject.Find("RightPlane");
						if (rightPlane1.renderer.enabled == false) {
							rightPlane1.renderer.enabled = true;
						}
						rightPlane1.GetComponent<Plugin_Handler>().set_cameras(0);
						break;
						
					case "NothingOnRight":
						print("NothingOnRight");
						GameObject rightPlane2 = GameObject.Find("RightPlane");
						rightPlane2.GetComponent<Plugin_Handler>().set_cameras(-1);
						rightPlane2.renderer.enabled = false;
						break;
						
					case "RightMirror":
						print("RightMirror");
						GameObject.Find("RightPlane").transform.localScale += new Vector3(8, 0, 0);
						break;
						
					case "RightUnMirror":
						print("RightUnMirror");
						GameObject.Find("RightPlane").transform.localScale += new Vector3(-8, 0, 0);
						break;
//					case "TheraMemMemory":
//						print("Starting TheraMem (Memory Version)");
//						this.SetGameMode = "Memory";
//						Camera.main.GetComponent<TheraMem>().enabled = true;
//						currentGame = "TheraMem";
//						Time.timeScale = 0;
//						break;
//					case "TheraMemOriginal":
//						print("Starting TheraMem (Original Version)");
//						this.SetGameMode = "Original";
//						Camera.main.GetComponent<TheraMem>().enabled = true;
//						Time.timeScale = 0;
//						break;
//					case "RestartTheraMemMemory":
//						print("Restarting");
//						Camera.main.GetComponent<TheraMem>().enabled = false;
//						Thread.Sleep(50);
//						this.SetGameMode = "Memory";
//						Camera.main.GetComponent<TheraMem>().enabled = true;
//						Time.timeScale = 0;
//						break;
//					case "RestartTheraMemOriginal":
//						print("Restarting");
//						Camera.main.GetComponent<TheraMem>().enabled = false;
//						Thread.Sleep(50);
//						this.SetGameMode = "Original";
//						Camera.main.GetComponent<TheraMem>().enabled = true;
//						Time.timeScale = 0;
//						break;
//					case "TheraMem":
//						Camera.main.GetComponent<TheraMem>().enabled = false;
//						break;
//					case "PauseGame":
//						Time.timeScale = 0;
//						break;
//					case "ResumeGame":
//						Time.timeScale = 1;
//						break;
					}
					
					
				}
			} else {
				print("Need at least one command...");
			}
		} else {
			// string is empty...do nothing while we wait for a command
		}
	}
}
