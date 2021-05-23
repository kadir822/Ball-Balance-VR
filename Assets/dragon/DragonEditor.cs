using UnityEngine;
using UnityEditor;
using System.IO.Ports;

/*
 * The custom Editor view for the Dragon script
 * 
 * Author: André Zenner (andre.zenner@dfki.de)
 * Date: May 2018
 * last Zpdated: 28.05.2020
 * 
 * */
[CustomEditor(typeof(Dragon))]
[CanEditMultipleObjects]
public class DragonEditor : Editor
{
    protected string[] _availablePorts;
    GUIStyle LabelStyle;

    private void OnEnable()
    {
        //define label style
        LabelStyle = new GUIStyle();
        LabelStyle.alignment = TextAnchor.MiddleLeft;
        LabelStyle.fontStyle = FontStyle.Bold;

        //load list of available ports
        RefreshCOMPorts();
    }

    public override void OnInspectorGUI()
    {
        //get connected Drag:on
        Dragon dragon = ((Dragon)target);
        
        //header
        EditorGUILayout.LabelField("Drag:on Interface", LabelStyle);

        //reconnect
        if (GUILayout.Button("Reconnect"))
        {
            dragon.StopCommunication();
            dragon.StartCommunication();
        }

        //show available COM ports
        dragon._portIndex = EditorGUILayout.Popup("Drag:on COM Port", dragon._portIndex, _availablePorts);
        if (_availablePorts.Length > 0)
            dragon.portName = _availablePorts[dragon._portIndex];
        else
            dragon.portName = "NO COM PORT AVAILABLE";

        //refresh available COM port list
        if (GUILayout.Button("Refresh COM Ports"))
        {
            RefreshCOMPorts();
        }

        //show normal editor below
        DrawDefaultInspector();
    }

    //load list of available COM ports
    protected void RefreshCOMPorts()
    {
        _availablePorts = SerialPort.GetPortNames();
        if(((Dragon)target)._portIndex >= _availablePorts.Length)
        {
            ((Dragon)target)._portIndex = 0;
        }
    }

}
