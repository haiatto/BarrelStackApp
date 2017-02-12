using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using UnityEngine.VR.WSA.Input;

using UnityEngine.Windows.Speech;
using System.Linq;
using System;

[RequireComponent(typeof(AudioSource))]
public class BarrenStackMain : MonoBehaviour {

    public AudioClip Se_Decide;
    public AudioClip Se_CursorEnter;
    public AudioClip Se_BarrelPop;

    GameObject lasthit_;

    GestureRecognizer recongizer_;

    public GameObject BarrelObject;

    public GameObject ToolObject;

    public GameObject SlpashScreenObject;
    public GameObject MenuObject;

    public HoloToolkit.Unity.SpatialMapping.SpatialMappingManager SpatialMapping;


    List<GameObject> generateObjectLst = new List<GameObject>();

    KeywordRecognizer keywordRecognizer;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();


    // Use this for initialization
    void Start() {
        recongizer_ = new GestureRecognizer();
        recongizer_.SetRecognizableGestures(GestureSettings.Tap | GestureSettings.ManipulationTranslate);
        recongizer_.TappedEvent += Recongizer__TappedEvent;
        //recongizer_.ManipulationStartedEvent += Recongizer__ManipulationStartedEvent;
        //recongizer_.ManipulationCanceledEvent += Recongizer__ManipulationCanceledEvent;
        //recongizer_.ManipulationUpdatedEvent += Recongizer__ManipulationUpdatedEvent;
        //recongizer_.ManipulationCompletedEvent += Recongizer__ManipulationCompletedEvent;
        recongizer_.StartCapturingGestures();

        InteractionManager.SourceUpdated += InteractionManager_SourceUpdated;

        keywords.Add("reset", () =>
        {
            OnResetBarrel();
        });
        keywords.Add("wireframe on", () =>
        {
            OnWireframeOn();
        });
        keywords.Add("wireframe off", () =>
        {
            OnWireframeOff();
        });
        keywords.Add("gravity on", () =>
        {
            OnGravityOn();
        });
        keywords.Add("gravity off", () =>
        {
            OnGravityOff();
        });
        keywords.Add("gravity reverse", () =>
        {
            OnAntigravity();
        });
        keywords.Add("menu", () =>
        {
            SlpashScreenObject.SetActive(false);
            MenuObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1.0f;
            if (MenuObject.gameObject.activeSelf)
            {
                MenuObject.SetActive(false);
            } else
            {
                MenuObject.SetActive(true);
            }
        });
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();

        BarrelObject.SetActive(false);
        ToolObject.SetActive(false);
        MenuObject.SetActive(false);
        SlpashScreenObject.SetActive(true);
        SlpashScreenObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1.0f;
    }


    Ray handToolRay;
    private void InteractionManager_SourceUpdated(InteractionSourceState state)
    {
        if (ToolObject.activeSelf)
        {
            Vector3 handPosition;
            if (state.source.kind == InteractionSourceKind.Hand &&
                state.properties.location.TryGetPosition(out handPosition))
            {
                ToolObject.transform.position = handPosition;
                ToolObject.transform.up = handToolRay.direction;
            }
        }
    }

    void OnDestroy()
    {
        recongizer_.StopCapturingGestures();
        recongizer_.TappedEvent -= Recongizer__TappedEvent;
    }

    #region Se
    public void PlaySe_Decide()
    {
        if(Se_Decide) GetComponent<AudioSource>().PlayOneShot(Se_Decide);
    }
    public void PlaySe_CursorEnter()
    {
        if (Se_CursorEnter) GetComponent<AudioSource>().PlayOneShot(Se_CursorEnter);
    }
    public void PlaySe_BarrelPop()
    {
        if (Se_BarrelPop) GetComponent<AudioSource>().PlayOneShot(Se_BarrelPop);
    }

    #endregion

    #region Callback

    public void OnResetBarrel()
    {
        PlaySe_Decide();
        generateObjectLst.ForEach((obj) => { GameObject.Destroy(obj); });
        generateObjectLst.Clear();
    }
    public void OnWireframeOn()
    {
        PlaySe_Decide();
        SpatialMapping.DrawVisualMeshes = true;

    }
    public void OnWireframeOff()
    {
        PlaySe_Decide();
        SpatialMapping.DrawVisualMeshes = false;
    }

    public void OnGravityOff()
    {
        PlaySe_Decide();
        Physics.gravity = new Vector3(0, 0, 0);
    }

    public void OnGravityOn()
    {
        PlaySe_Decide();
        Physics.gravity = new Vector3(0, -9.81f, 0);
    }

    public void OnAntigravity()
    {
        PlaySe_Decide();
        Physics.gravity = new Vector3(0, 9.81f, 0);
    }
    #endregion

    #region GestureRecognizer

    private void Recongizer__ManipulationStartedEvent(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        ToolObject.SetActive(true);
    }
    private void Recongizer__ManipulationUpdatedEvent(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        handToolRay = headRay;
    }
    private void Recongizer__ManipulationCanceledEvent(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        ToolObject.SetActive(false);
    }
    private void Recongizer__ManipulationCompletedEvent(InteractionSourceKind source, Vector3 cumulativeDelta, Ray headRay)
    {
        ToolObject.SetActive(false);
    }

    #endregion

    #region KeywordRecognizer_OnPhraseRecognized
    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;
        // if the keyword recognized is in our dictionary, call that Action.
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }
    #endregion






    #region pop object

    void popTapObject(Vector3 pos, Vector3 dir)
    {
        PlaySe_BarrelPop();

        if (BarrelObject)
        {
            var popObj = GameObject.Instantiate(BarrelObject);
            popObj.transform.position = pos;
            var rigBody = popObj.GetComponent<Rigidbody>();
            rigBody.velocity = dir * 10 + Vector3.up * 2;
            popObj.SetActive(true);
            generateObjectLst.Add(popObj);
        }
    }

    private void Recongizer__TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
    {
        if (MenuObject.activeSelf) return;
        popTapObject(headRay.origin, headRay.direction);
    }

    public void PopBarrel()
    {
        popTapObject(Camera.main.transform.position, Camera.main.transform.forward);
    }

    #endregion

    // Update is called once per frame
    void Update() {

        RaycastHit hitInfo;
        if (Physics.Raycast(
            Camera.main.transform.position,
            Camera.main.transform.forward,
            out hitInfo,
            20.0f,
            Physics.DefaultRaycastLayers
            ))
        {
            if (hitInfo.transform.gameObject != lasthit_)
            {
                if (lasthit_)
                {
                }
                lasthit_ = hitInfo.transform.gameObject;
                {
                }
            }

        }
    }

    private void OnGUI()
    {
#if UNITY_EDITOR

        if (GUI.Button(new Rect(0, 0, 100, 100), "Test"))
        {
            PopBarrel();
        }
        if (Input.GetButtonDown("Fire1"))
        {
            //PopBarrelTest();
        }

#endif
    }
}
