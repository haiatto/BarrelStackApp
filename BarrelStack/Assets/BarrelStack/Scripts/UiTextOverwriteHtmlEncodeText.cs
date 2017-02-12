using System;
using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

/// <summary>
/// UnityエディタでUnicodeのコード指定で入力する為に便利なコンポーネントです
/// （一応コピペでも入力はできましたが…インスペクタ上では指定したフォントでは描画してないようなので文字が入ってるようには見えなかったりするので微妙）
/// http://modernicons.io/segoe-mdl2/cheatsheet/
/// ↑入力したかった文字達
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(UnityEngine.UI.Text))]
public class UiTextOverwriteHtmlEncodeText : MonoBehaviour {

    public string htmlEncodeText;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        var text = GetComponent< UnityEngine.UI.Text>();
        var unitext = Uri.UnescapeDataString(htmlEncodeText);
        if (text)
        {
            var decText = DecodeHtmlChars(unitext);
            if (text.text != decText)
            {
                text.text = decText;
            }
        }

    }

    //http://answers.unity3d.com/questions/244911/decode-html-charactersin-c.html
    string DecodeHtmlChars(string aText)
    {
        string[] parts = aText.Split(new string[] { "&#x" }, StringSplitOptions.None);
        for (int i = 1; i < parts.Length; i++)
        {
            int n = parts[i].IndexOf(';');
            string number = parts[i].Substring(0, n);
            try
            {
                int unicode = Convert.ToInt32(number, 16);
                parts[i] = ((char)unicode) + parts[i].Substring(n + 1);
            }
            catch { }
        }
        return String.Join("", parts);
    }
}
