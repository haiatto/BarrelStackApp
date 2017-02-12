using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;

public class BarrelGestureMove : MonoBehaviour,
                                IFocusable,
                                IInputHandler,
                                ISourceStateHandler,
                                IManipulationHandler
{

    public GameObject PickTool;

    Vector3 lastPosition_ = new Vector3();
    Vector3 diffPosition_ = new Vector3();


    void start_(HoloToolkit.Unity.InputModule.BaseInputEventData eventData)
    {
        if (currentInputSource != null)
        {
            Debug.Log("barance err?");
        }
        currentInputSource = eventData.InputSource;
        currentInputSourceId = eventData.SourceId;

        var rigidBody = this.GetComponent<Rigidbody>();
        if (rigidBody)
        {
            rigidBody.useGravity = false;
        }
        lastPosition_ = this.transform.position;
        diffPosition_ = new Vector3();
        InputManager.Instance.PushModalInputHandler(gameObject);

        Debug.Log(string.Format("start_ {0} : {1}", eventData.selectedObject.name, eventData.SourceId));

        PickTool.SetActive(true);
    }
    void finsh_()
    {
        if (currentInputSource != null)
        {
            var rigidBody = this.GetComponent<Rigidbody>();
            if (rigidBody)
            {
                rigidBody.useGravity = true;
            }
            InputManager.Instance.PopModalInputHandler();

            currentInputSource = null;
            currentInputSourceId = 0;

            PickTool.SetActive(false);
            Debug.Log(string.Format("finish"));
        }
    }
    void update_()
    {
    }

    #region event handler
    public void OnManipulationCanceled(ManipulationEventData eventData)
    {
        finsh_();
    }

    public void OnManipulationCompleted(ManipulationEventData eventData)
    {
        finsh_();
    }

    public void OnManipulationStarted(ManipulationEventData eventData)
    {
        start_(eventData);
    }

    public void OnManipulationUpdated(ManipulationEventData eventData)
    {
        diffPosition_ += eventData.CumulativeDelta;
        this.transform.position = eventData.CumulativeDelta + lastPosition_;


        if (eventData != null) Debug.Log(string.Format("move {0} : {1}", eventData.selectedObject.name, eventData.SourceId));


        Vector3 handPosition;
        currentInputSource.TryGetPosition(currentInputSourceId, out handPosition);
        PickTool.transform.position = handPosition;

    }


    public void OnSourceDetected(SourceStateEventData eventData)
    {
    }

    public void OnSourceLost(SourceStateEventData eventData)
    {
        if (currentInputSource != null && eventData.SourceId == currentInputSourceId)
        {
            finsh_();
        }
    }
    public void OnFocusEnter()
    {

    }

    public void OnFocusExit()
    {

    }

    public void OnInputUp(InputEventData eventData)
    {
    }

    public void OnInputDown(InputEventData eventData)
    {
    }
    #endregion

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    private void OnDestroy()
    {
        finsh_();
    }



    private IInputSource currentInputSource = null;
    private uint currentInputSourceId;





#if false


















    /// <summary>
    /// Event triggered when dragging starts.
    /// </summary>
    public event Action StartedDragging;

    /// <summary>
    /// Event triggered when dragging stops.
    /// </summary>
    public event Action StoppedDragging;

    [Tooltip("Transform that will be dragged. Defaults to the object of the component.")]
    public Transform HostTransform;

    [Tooltip("Scale by which hand movement in z is multipled to move the dragged object.")]
    public float DistanceScale = 2f;

    [Tooltip("Should the object be kept upright as it is being dragged?")]
    public bool IsKeepUpright = false;

    [Tooltip("Should the object be oriented towards the user as it is being dragged?")]
    public bool IsOrientTowardsUser = true;

    public bool IsDraggingEnabled = true;

    private Camera mainCamera;
    private bool isDragging;
    private bool isGazed;
    private Vector3 objRefForward;
    private float objRefDistance;
    private Quaternion gazeAngularOffset;
    private float handRefDistance;
    private Vector3 objRefGrabPoint;

    private Vector3 draggingPosition;
    private Quaternion draggingRotation;

    private IInputSource currentInputSource = null;
    private uint currentInputSourceId;

    private void Start()
    {
        if (HostTransform == null)
        {
            HostTransform = transform;
        }

        mainCamera = Camera.main;
    }

    private void OnDestroy()
    {
        if (isDragging)
        {
            StopDragging();
        }

        if (isGazed)
        {
            OnFocusExit();
        }
    }

    private void Update()
    {
        if (IsDraggingEnabled && isDragging)
        {
            UpdateDragging();
        }
    }

    /// <summary>
    /// Starts dragging the object.
    /// </summary>
    public void StartDragging()
    {
        if (!IsDraggingEnabled)
        {
            return;
        }

        if (isDragging)
        {
            return;
        }

        // Add self as a modal input handler, to get all inputs during the manipulation
        InputManager.Instance.PushModalInputHandler(gameObject);

        isDragging = true;
        //GazeCursor.Instance.SetState(GazeCursor.State.Move);
        //GazeCursor.Instance.SetTargetObject(HostTransform);

        Vector3 gazeHitPosition = GazeManager.Instance.HitInfo.point;
        Vector3 handPosition;
        currentInputSource.TryGetPosition(currentInputSourceId, out handPosition);

        Vector3 pivotPosition = GetHandPivotPosition();
        handRefDistance = Vector3.Magnitude(handPosition - pivotPosition);
        objRefDistance = Vector3.Magnitude(gazeHitPosition - pivotPosition);

        Vector3 objForward = HostTransform.forward;

        // Store where the object was grabbed from
        objRefGrabPoint = mainCamera.transform.InverseTransformDirection(HostTransform.position - gazeHitPosition);

        Vector3 objDirection = Vector3.Normalize(gazeHitPosition - pivotPosition);
        Vector3 handDirection = Vector3.Normalize(handPosition - pivotPosition);

        objForward = mainCamera.transform.InverseTransformDirection(objForward);       // in camera space
        objDirection = mainCamera.transform.InverseTransformDirection(objDirection);   // in camera space
        handDirection = mainCamera.transform.InverseTransformDirection(handDirection); // in camera space

        objRefForward = objForward;

        // Store the initial offset between the hand and the object, so that we can consider it when dragging
        gazeAngularOffset = Quaternion.FromToRotation(handDirection, objDirection);
        draggingPosition = gazeHitPosition;

        StartedDragging.RaiseEvent();
    }

    /// <summary>
    /// Gets the pivot position for the hand, which is approximated to the base of the neck.
    /// </summary>
    /// <returns>Pivot position for the hand.</returns>
    private Vector3 GetHandPivotPosition()
    {
        Vector3 pivot = Camera.main.transform.position + new Vector3(0, -0.2f, 0) - Camera.main.transform.forward * 0.2f; // a bit lower and behind
        return pivot;
    }

    /// <summary>
    /// Enables or disables dragging.
    /// </summary>
    /// <param name="isEnabled">Indicates whether dragging shoudl be enabled or disabled.</param>
    public void SetDragging(bool isEnabled)
    {
        if (IsDraggingEnabled == isEnabled)
        {
            return;
        }

        IsDraggingEnabled = isEnabled;

        if (isDragging)
        {
            StopDragging();
        }
    }

    /// <summary>
    /// Update the position of the object being dragged.
    /// </summary>
    private void UpdateDragging()
    {
        Vector3 newHandPosition;
        currentInputSource.TryGetPosition(currentInputSourceId, out newHandPosition);

        Vector3 pivotPosition = GetHandPivotPosition();

        Vector3 newHandDirection = Vector3.Normalize(newHandPosition - pivotPosition);

        newHandDirection = mainCamera.transform.InverseTransformDirection(newHandDirection); // in camera space
        Vector3 targetDirection = Vector3.Normalize(gazeAngularOffset * newHandDirection);
        targetDirection = mainCamera.transform.TransformDirection(targetDirection); // back to world space

        float currenthandDistance = Vector3.Magnitude(newHandPosition - pivotPosition);

        float distanceRatio = currenthandDistance / handRefDistance;
        float distanceOffset = distanceRatio > 0 ? (distanceRatio - 1f) * DistanceScale : 0;
        float targetDistance = objRefDistance + distanceOffset;

        draggingPosition = pivotPosition + (targetDirection * targetDistance);

        if (IsOrientTowardsUser)
        {
            draggingRotation = Quaternion.LookRotation(HostTransform.position - pivotPosition);
        }
        else
        {
            Vector3 objForward = mainCamera.transform.TransformDirection(objRefForward); // in world space
            draggingRotation = Quaternion.LookRotation(objForward);
        }

        // Apply Final Position
        HostTransform.position = draggingPosition + mainCamera.transform.TransformDirection(objRefGrabPoint);
        HostTransform.rotation = draggingRotation;

        if (IsKeepUpright)
        {
            Quaternion upRotation = Quaternion.FromToRotation(HostTransform.up, Vector3.up);
            HostTransform.rotation = upRotation * HostTransform.rotation;
        }
    }

    /// <summary>
    /// Stops dragging the object.
    /// </summary>
    public void StopDragging()
    {
        if (!isDragging)
        {
            return;
        }

        // Remove self as a modal input handler
        InputManager.Instance.PopModalInputHandler();

        isDragging = false;
        currentInputSource = null;
        StoppedDragging.RaiseEvent();
    }

    public void OnFocusEnter()
    {
        if (!IsDraggingEnabled)
        {
            return;
        }

        if (isGazed)
        {
            return;
        }

        isGazed = true;
    }

    public void OnFocusExit()
    {
        if (!IsDraggingEnabled)
        {
            return;
        }

        if (!isGazed)
        {
            return;
        }

        isGazed = false;
    }

    public void OnInputUp(InputEventData eventData)
    {
        if (currentInputSource != null &&
            eventData.SourceId == currentInputSourceId)
        {
            StopDragging();
        }
    }

    public void OnInputDown(InputEventData eventData)
    {
        if (isDragging)
        {
            // We're already handling drag input, so we can't start a new drag operation.
            return;
        }

        if (!eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
        {
            // The input source must provide positional data for this script to be usable
            return;
        }

        currentInputSource = eventData.InputSource;
        currentInputSourceId = eventData.SourceId;
        StartDragging();
    }

    public void OnSourceDetected(SourceStateEventData eventData)
    {
        // Nothing to do
    }

    public void OnSourceLost(SourceStateEventData eventData)
    {
        if (currentInputSource != null && eventData.SourceId == currentInputSourceId)
        {
            StopDragging();
        }
    }
#endif

}

