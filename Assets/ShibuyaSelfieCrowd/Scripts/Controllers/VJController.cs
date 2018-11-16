using System;
using System.Net;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using MidiJack;
using UnityOSC;

namespace VJ
{

    [System.Serializable]
    public class OSCEvent : UnityEvent<string, List<object>> {}

    [System.Serializable]
    public class MidiNoteEvent : UnityEvent<int> {}

    [System.Serializable]
    public class MidiKnobEvent : UnityEvent<int, float> {}

    public class VJController : ControllableBase {

        [SerializeField] protected int port = 8888;
        [SerializeField] protected OSCEvent onOsc;
        [SerializeField] protected MidiNoteEvent onNoteOn, onNoteOff;
        [SerializeField] protected MidiKnobEvent onKnob;

        #region OSC variables

        OSCServer server;
        protected List<OSCPacket> packets;

        #endregion

        protected void Start () {

#if UNITY_STANDALONE
            Cursor.visible = false;
#endif
#if !UNITY_EDITOR
            WindowController.SetToTopMost();
#endif
            var args = System.Environment.GetCommandLineArgs();
            int x = -1, y = -1;
            for (int i = 0, n = args.Length; i < n; i++)
            {
                switch(args[i])
                {
                    case "-x":
                        x = int.Parse(args[i + 1]);
                        break;
                    case "-y":
                        y = int.Parse(args[i + 1]);
                        break;
                }
            }
            if(x >= 0 && y >= 0)
            {
                WindowController.MoveWindow(x, y);
            }

            server = CreateServer("VJ", port);
            packets = new List<OSCPacket>();

            foreach(var controllable in Resources.FindObjectsOfTypeAll<ControllableBase>())
            {
                onNoteOn.AddListener((note) => {
                    if (IsValid(controllable)) controllable.NoteOn(note);
                });
                onNoteOff.AddListener((note) => {
                    if (IsValid(controllable)) controllable.NoteOff(note);
                });
                onKnob.AddListener((knobNumber, knobValue) => {
                    if (IsValid(controllable)) controllable.Knob(knobNumber, knobValue);
                });
                onOsc.AddListener((address, data) => {
                    if (IsValid(controllable)) controllable.OnOSC(address, data);
                });
            }

        }

        protected bool IsValid(ControllableBase controllable)
        {
            return controllable != null && controllable.gameObject.activeInHierarchy;
        }
        
        protected void Update () {
            for(int i = 0, n = packets.Count; i < n; i++)
            {
                var packet = packets[i];
                if(packet != null)
                {
                    onOsc.Invoke(packet.Address, packet.Data);
                }
            }
            packets.Clear();
        }

        void OnEnable()
        {
            MidiMaster.noteOnDelegate += NoteOn;
            MidiMaster.noteOffDelegate += NoteOff;
            MidiMaster.knobDelegate += Knob;
        }

        void OnDisable()
        {
            MidiMaster.noteOnDelegate -= NoteOn;
            MidiMaster.noteOffDelegate -= NoteOff;
            MidiMaster.knobDelegate -= Knob;
        }

        void OnApplicationQuit() 
        {
            if(server != null)
            {
                server.Close();
            }
        }

        #region Midi functions

        void NoteOn(MidiChannel channel, int note, float velocity)
        {
#if UNITY_EDITOR
            Debug.Log("NoteOn: " + channel + "," + note + "," + velocity);
#endif
            onNoteOn.Invoke(note);
        }

        void NoteOff(MidiChannel channel, int note)
        {
#if UNITY_EDITOR
            Debug.Log("NoteOff: " + channel + "," + note);
#endif
            onNoteOff.Invoke(note);
        }

        void Knob(MidiChannel channel, int knobNumber, float knobValue)
        {
#if UNITY_EDITOR
            Debug.Log("Knob: " + knobNumber + "," + knobValue);
#endif
            onKnob.Invoke(knobNumber, knobValue);
        }

        #endregion

        #region OSC functions

        OSCServer CreateServer(string serverId, int port)
        {
            OSCServer server = new OSCServer(port);
            server.PacketReceivedEvent += OnPacketReceived;

            ServerLog serveritem = new ServerLog();
            serveritem.server = server;
            serveritem.log = new List<string>();
            serveritem.packets = new List<OSCPacket>();

            return server;
        }

        void OnPacketReceived(OSCServer server, OSCPacket packet)
        {

            packets.Add(packet);
        }
               
        #endregion

        public override void Knob(int knobNumber, float knobValue)
        {
        }

        public override void NoteOff(int note)
        {
        }

        public override void NoteOn(int note)
        {
        }

        public override void OnOSC(string address, List<object> data)
        {
        }

    }

}


