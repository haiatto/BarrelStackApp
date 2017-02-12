using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;

[RequireComponent(typeof(HoloToolkit.Unity.TextToSpeechManager))]
[RequireComponent(typeof(UnityEngine.AudioSource))]
public class TextToSpeechUtil : MonoBehaviour {

    public int pitch = 0; // ピッチ：high／medium／low／±○○Hzなど
    public int rate = 0; // レート：fast／medium／slow／○○%など
    public float volume = 100; // 音量：loud／medium／soft／0.0～100.0など

    [ReadOnlyAttribute]
    public HoloToolkit.Unity.TextToSpeechManager ttsManager;
    [ReadOnlyAttribute]
    public UnityEngine.AudioSource audioSource;

    private void Awake()
    {
        ttsManager = this.GetComponent<TextToSpeechManager>();
        audioSource = this.GetComponent<UnityEngine.AudioSource>();
        ttsManager.AudioSource = audioSource;

    }
    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void Speach(string text)
    {
        Func<string, string> encode = (string data) => {
            string buffer = "";
            foreach (var ch in data)
            {
                switch (ch)
                {
                    case '&': buffer += "&amp;"; break;
                    case '\"': buffer += "&quot;"; break;
                    case '\'': buffer += "&apos;"; break;
                    case '<': buffer += "&lt;"; break;
                    case '>': buffer += "&gt;"; break;
                    default: buffer += ch; break;
                }
            }
            return buffer;
        };
        string textEncoded = encode(text);
        Debug.Log(textEncoded);

        // Get the name
        string ssml = string.Format(
            @"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
            <prosody pitch='{0}Hz' rate='{1}%' volume='{2}'>{3}</prosody>
            </speak>", pitch, rate, volume, textEncoded);
        // Speak message
        ttsManager.SpeakSsml(ssml);
    }
}