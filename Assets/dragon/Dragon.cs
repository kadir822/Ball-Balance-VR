using System.Collections;
using UnityEngine;
using System.IO.Ports;          //using Serial communication
using System;

/*
 * The Drag:on interface based on serial communication with the controlling Arduino chip.
 * 
 * This script offers functions to interface with the device.
 * Please connect the Drag:on device with the computer via USB 
 * and select the correct COM port in this script's editor interface in Unity.
 * 
 * When a connection to the Drag:on device is established, 
 * the functions in this script can be called to control the device state, 
 * e.g. moving the fans using TransformFans(A, B) - please see the code for further functionality.
 * A and B refer to the two fans onboard Drag:on.
 * 
 * The script also includes a simulation feature (commented out, as it requires the SteamVR asset) 
 * that allows to develop applications without physical access to the Drag:on prototype.
 * When simulating, the script reacts as if the Drag:on device was attached.
 * 
 * Author: André Zenner (andre.zenner@dfki.de)
 * Date: May 2018
 * Last Update: 28.05.2020
 * */
public class Dragon : MonoBehaviour {

    [Header("Drag:on Debug")]
    public bool LogTransformations = true;

    [Header("Drag:on Simulation [requires SteamVR asset and removal of code comments]")]
    public bool Simulation = false;
    //protected SteamVR_TrackedObject TrackedObj;                   //comment in for simulation feature
    //protected SteamVR_Controller.Device ViveController;           //comment in for simulation feature

    //communication with board
    protected SerialPort port;          //the COM port used to communicate with the Drag:on device
    [HideInInspector]
    public int _portIndex;
    [Header("Drag:on Communication")]
    public string portName;
    public int baud = 115200;

    //managing reading coroutine
    protected bool stopReading = false;     //flag that tells the coroutine when to stop reading the COM port
    protected Coroutine ReadingRoutine;

    [Header("Drag:on State")]
    //Drag:on Button
    public bool isPressed = false;
    public bool wasPressedThisFrame = false;
    public bool wasReleasedThisFrame = false;

    //Drag:on State
    public int StateFanA = 0;
    public int StateFanB = 0;

    [Header("Animation Values")]
    public float DurationMSFanA = 570;
    public float DurationMSFanB = 500;


    /****************************************************
     *         DRAG:ON AUTO INITIALIZATION & UPDATE
    *****************************************************/

    void Awake()
    {
        if (Simulation)
        {
            ////comment in for simulation feature
            ////find Vive controller for simulation
            //GameObject ViveControllerObject = GameObject.Find("Controller (right)");
            //if (ViveControllerObject == null)
            //    Debug.LogWarning("No Vive controller (right) for simulation found in the scene!");
            //else
            //{
            //    TrackedObj = ViveControllerObject.GetComponent<SteamVR_TrackedObject>();
            //    if (TrackedObj == null)
            //        Debug.LogWarning("No SteamVR_TrackedObject script on Vive controller (right) found!");
            //}
        }
    }

    // Use this for initialization
    void Start () {
        if (Simulation)
        {
            Debug.LogWarning("Simulating Drag:on");
        }
        else
        {
            Debug.Log("Connecting to Drag:on prototype");
            StartCommunication();
        }
	}
	
	// Update is called once per frame
	void Update () {

        //simulate button on Drag:on with Vive Controller (trigger) or Z key
        //for this, add new "Joystick Axis" (10th axis) named "openvr-r-trigger-press" under Edit->Project Settings->Input (positive button z)
        if (Simulation)
        {
            ////comment in for simulation feature
            ////update Vive controller if present
            //if (TrackedObj != null)
            //{
            //    ViveController = SteamVR_Controller.Input((int)TrackedObj.index);
            //}

            //update button states
            if (!isPressed && Input.GetAxis("openvr-r-trigger-press") > .1f)
            {
                UpdateButton("BUTTON PRESSED");
            }
            else if (isPressed && Input.GetAxis("openvr-r-trigger-press") <= .1f)
            {
                UpdateButton("BUTTON RELEASED");
            }
            else
            {
                UpdateButton(null);
            }
        }
	}

    void OnApplicationQuit()
    {
        if(!Simulation)
            StopCommunication();
    }

    /****************************************************
     *         DRAG:ON INTERFACE FUNCTIONS
    *****************************************************/
     
    //obfuscated transformation to target state
    public DragonTransformation TransformFansObfuscated(int PercentA, int PercentB)
    {
        int ObfuscationPercentA = UnityEngine.Random.Range(0, 100);
        int ObfuscationPercentB = UnityEngine.Random.Range(0, 100);

        if(LogTransformations)
            Debug.Log("Transform Drag:on: --> Obfuscation (" + ObfuscationPercentA + ", " + ObfuscationPercentB + ") --> (" + PercentA + ", " + PercentB + ")");

        DragonTransformation transformation = new DragonTransformation();
        transformation.StartPercentA = StateFanA;
        transformation.StartPercentB = StateFanB;
        transformation.EndPercentA = PercentA;
        transformation.EndPercentB = PercentB;

        long DurationToObfuscateA = (long)((Mathf.Abs(ObfuscationPercentA - transformation.StartPercentA) / 100f) * DurationMSFanA);
        long DurationToObfuscateB = (long)((Mathf.Abs(ObfuscationPercentB - transformation.StartPercentB) / 100f) * DurationMSFanB);
        long DurationToTargetA = (long)((Mathf.Abs(transformation.EndPercentA - ObfuscationPercentA) / 100f) * DurationMSFanA);
        long DurationToTargetB = (long)((Mathf.Abs(transformation.EndPercentB - ObfuscationPercentB) / 100f) * DurationMSFanB);

        if (Simulation)
        {
            StartCoroutine(ObfuscationSimulator((float)(Math.Max(DurationToObfuscateA, DurationToObfuscateB)) / 1000f));
            UpdateState("FAN-A " + transformation.EndPercentA + " FAN-B " + transformation.EndPercentB);
        }
        else
        {
            StartCoroutine(ObfuscationCommander(ObfuscationPercentA, ObfuscationPercentB, PercentA, PercentB, (float)(Math.Max(DurationToObfuscateA, DurationToObfuscateB)) / 1000f));
        }

        transformation.StartTime = Environment.TickCount;
        transformation.EndTimeA = transformation.StartTime + Math.Max(DurationToObfuscateA, DurationToObfuscateB) + DurationToTargetA;
        transformation.EndTimeB = transformation.StartTime + Math.Max(DurationToObfuscateA, DurationToObfuscateB) + DurationToTargetB;
        return transformation;

    }

    //non-obfuscated direct transformation to target state
    public DragonTransformation TransformFans(int PercentA, int PercentB)
    {
        if (LogTransformations)
            Debug.Log("Transform Drag:on: --> (" + PercentA + ", " + PercentB + ")");

        DragonTransformation transformation = new DragonTransformation();
        transformation.StartPercentA = StateFanA;
        transformation.StartPercentB = StateFanB;
        transformation.EndPercentA = PercentA;
        transformation.EndPercentB = PercentB;

        long DurationA = ((long)((Mathf.Abs(transformation.EndPercentA - transformation.StartPercentA) / 100f) * DurationMSFanA));
        long DurationB = ((long)((Mathf.Abs(transformation.EndPercentB - transformation.StartPercentB) / 100f) * DurationMSFanB));

        if (Simulation)
        {
            StartCoroutine(ObfuscationSimulator((float)(Math.Max(DurationA, DurationB)) / 1000f));
            UpdateState("FAN-A " + transformation.EndPercentA + " FAN-B " + transformation.EndPercentB);
        }
        else
        {
            WriteToSerial("FANS " + PercentA + " " + PercentB);
        }

        transformation.StartTime = Environment.TickCount;
        transformation.EndTimeA = transformation.StartTime + DurationA;
        transformation.EndTimeB = transformation.StartTime + DurationB;
        return transformation;
    }

    //non-obfuscated direct transformation to target state (moving single fan)
    public DragonTransformation TransformFan(string ID, int Percent)
    {
        if (LogTransformations)
            Debug.Log("Transform Drag:on: --> (" + (ID == "A" ? Percent : StateFanA) + ", " + (ID == "B" ? Percent : StateFanB) + ")");

        DragonTransformation transformation = new DragonTransformation();
        transformation.StartPercentA = StateFanA;
        transformation.StartPercentB = StateFanB;
        transformation.EndPercentA = StateFanA;
        transformation.EndPercentB = StateFanB;

        long DurationA = 0;
        long DurationB = 0;

        if (ID == "A")
        {
            transformation.EndPercentA = Percent;
            DurationA = ((long)((Mathf.Abs(transformation.EndPercentA - transformation.StartPercentA) / 100f) * DurationMSFanA));
            if (Simulation)
            {
                StartCoroutine(ObfuscationSimulator((float)(DurationA) / 1000f));
                UpdateState("FAN-A " + transformation.EndPercentA + " FAN-B " + transformation.EndPercentB);
            }
            else
            {
                WriteToSerial("FANS " + Percent + " X");
            }

        }
        else if (ID == "B")
        {
            transformation.EndPercentB = Percent;
            DurationB = ((long)((Mathf.Abs(transformation.EndPercentB - transformation.StartPercentB) / 100f) * DurationMSFanB));
            if (Simulation)
            {
                StartCoroutine(ObfuscationSimulator((float)(DurationB) / 1000f));
                UpdateState("FAN-A " + transformation.EndPercentA + " FAN-B " + transformation.EndPercentB);
            }
            else
            {
                WriteToSerial("FANS X " + Percent);
            }
        }
        else
            Debug.LogWarning("Invalid Fan ID " + ID);

        transformation.StartTime = Environment.TickCount;
        transformation.EndTimeA = transformation.StartTime + DurationA;
        transformation.EndTimeB = transformation.StartTime + DurationB;
        return transformation;
    }

    //requests a state update from the Drag:on
    public void RefreshState()
    {
        WriteToSerial("STATE");
    }

    //get the button states
    public bool GetButtonDownThisFrame()
    {
        return wasPressedThisFrame;
    }
    public bool GetButtonUpThisFrame()
    {
        return wasReleasedThisFrame;
    }
    public bool GetButton()
    {
        return isPressed;
    }

    //get the fan states
    public float GetFanState(string ID)
    {
        if (ID == "A")
            return StateFanA;
        else if (ID == "B")
            return StateFanB;
        else
        {
            Debug.LogWarning("Invalid Fan ID " + ID);
            return float.NegativeInfinity;
        }
    }

    /****************************************************
     *         DRAG:ON INTERNAL FUNCTIONS
    *****************************************************/

    //writes msg to the stream - fast
    protected void WriteToSerial(string msg)
    {
        port.WriteLine(msg);
        port.BaseStream.Flush();
    }

    //reads from the stream asynchronously
    protected IEnumerator AsynchronousReadFromSerial()
    {
        Debug.Log("Started reading serial port " + portName);
        string dataString = null;
        do
        {
            try
            {
                dataString = port.ReadLine();
            }
            catch (Exception e)
            {
                dataString = null;
                if(e.GetType() == typeof(System.IO.IOException))
                {
                    Debug.LogWarning("Connection to Drag:on lost!");
                    StopCommunication();
                    break;
                }
            }
            if(dataString != null)
                Debug.Log("Received: " + dataString);

            UpdateButton(dataString);
            UpdateState(dataString);

            dataString = null;

            yield return null;
        } while (!stopReading);
        Debug.Log("Stopped reading serial port " + portName);
    }

    //frame-wise update of the button state
    protected void UpdateButton(string msg)
    {
        if(msg == null)
        {
            wasPressedThisFrame = false;
            wasReleasedThisFrame = false;
        }
        if(msg == "BUTTON PRESSED")
        {
            wasPressedThisFrame = true;
            wasReleasedThisFrame = false;
            isPressed = true;
        }
        if(msg == "BUTTON RELEASED")
        {
            wasPressedThisFrame = false;
            wasReleasedThisFrame = true;
            isPressed = false;
        }
    }

    //frame-wise update of the Drag:on state
    protected void UpdateState(string msg = null)
    {
        if(msg != null)
        {
            string[] split = msg.Split(' ');
            if(split.Length >= 4)
            {
                if(split[0] == "FAN-A")
                {
                    StateFanA = int.Parse(split[1]);
                }
                if(split[2] == "FAN-B")
                {
                    StateFanB = int.Parse(split[3]);
                }
            }
        }
    }

    //simulates a transformation through Vive controller vibration for the duration of a real Drag:on transformation
    protected IEnumerator ObfuscationSimulator(float SecWait)
    {
        ////comment in for simulation feature
        //if(ViveController != null)
        //{
        //    for (float i = 0; i < SecWait; i += Time.deltaTime)
        //    {
        //        ViveController.TriggerHapticPulse((ushort)3999);
        //        yield return null;
        //    }
        //}
        yield return null;
    }

    //sequentially fires transformation commands for obfuscation
    protected IEnumerator ObfuscationCommander(int ObfuscationA, int ObfuscationB, int TargetA, int TargetB, float SecWait)
    {
        WriteToSerial("FANS " + ObfuscationA + " " + ObfuscationB);
        yield return new WaitForSecondsRealtime(SecWait);
        WriteToSerial("FANS " + TargetA + " " + TargetB);
        yield return null;
    }

    //Start/Stop communication with the Drag:on
    public void StopCommunication()
    {
        stopReading = true;
        port.Close();
        StopCoroutine(ReadingRoutine);
    }
    public void StartCommunication()
    {
        stopReading = false;
        port = new SerialPort(portName, baud);
        port.ReadTimeout = 1;
        port.Open();
        port.DiscardInBuffer();
        port.DiscardOutBuffer();
        ReadingRoutine = StartCoroutine(AsynchronousReadFromSerial());
    }
}


/*
 * Helper class to keep track in realtime of the progress of the Drag:on's transformation.
 * Can be queried for animating the transformation in VR.
 * 
 * Author: André Zenner
 * Date: July 2018
 * */
    public class DragonTransformation
{
    public float StartPercentA, StartPercentB;      //start state of the device
    public float EndPercentA, EndPercentB;          //final end state after transformation
    public long StartTime, EndTimeA, EndTimeB;      //Environment.TickCounts for start and end events (approximations)

    //some helper functions to query the progress of the transformation
    public bool isTransformingA()
    {
        return StartTime <= Environment.TickCount && Environment.TickCount <= EndTimeA;
    }
    public bool isTransformingB()
    {
        return StartTime <= Environment.TickCount && Environment.TickCount <= EndTimeB;
    }
    public float getProgressA()
    {
        return Mathf.Max(0, Mathf.Min(1, (float)(Environment.TickCount - StartTime) / (float)(EndTimeA - StartTime)));
    }
    public float getProgressB()
    {
        return Mathf.Max(0, Mathf.Min(1, (float)(Environment.TickCount - StartTime) / (float)(EndTimeB - StartTime)));
    }
    public float getPercentA()
    {
        return StartPercentA + (EndPercentA - StartPercentA) * getProgressA();
    }
    public float getPercentB()
    {
        return StartPercentB + (EndPercentB - StartPercentB) * getProgressB();
    }
    public long getDurationA()
    {
        return EndTimeA - StartTime;
    }
    public long getDurationB()
    {
        return EndTimeB - StartTime;
    }
    public bool isOver()
    {
        return Environment.TickCount > EndTimeA && Environment.TickCount > EndTimeB;
    }
}