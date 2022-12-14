using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.Events;
using Vuforia;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

/*
 *======================================
 * TODO:
 * Properly handle all variations of vuforia trackable status
 * Write a proper documentation?
 * Wait for vuforia to be properly initialized(not for 2 seconds)
 *======================================
*/

/// <summary>
/// QRTrackingHandlerStatus
/// </summary>
public enum QRTrackingStatus { Initialization, NoMarker, MarkerFound, ReadSuccessfull }


public class QRTrackingHandler : MonoBehaviour, ITrackableEventHandler
{
	#region Private variables

	protected TrackableBehaviour mTrackableBehaviour;
	private QRCodeReader qrCodeReader;
	private bool cameraIsFineAndDandy;
	private Timer tQRRecognitionTimer;
	private bool tracked = false;
	private readonly PIXEL_FORMAT capturePixelFormat = PIXEL_FORMAT.GRAYSCALE;

    private string qrdata;
    private string last_qrdata;

	#endregion Private variables

	#region Public
	/// <summary>
	/// String containing data from QR code /!\ NOT PURGED UNTIL NEW DATA AQUIRED /!\
	/// </summary>
	public string QRData
    {
        get { return qrdata; }
        private set
        {
            last_qrdata = qrdata;
            qrdata = value;
            if (qrdata != "" && qrdata != last_qrdata)
            {
                Debug.Log("Fire QR Event");
                Action action = () => onReadQRSuccess.Invoke();
                UnityMainThreadDispatcher.Instance().Enqueue(action);
            }
        }
    }
	/// <summary>
	/// Status of tracker
	/// </summary>
	public QRTrackingStatus Status {get; private set;}
	/// <summary>
	/// Interval of internal timer
	/// </summary>
	public double QRReadTimer
	{
		get
		{
			return tQRRecognitionTimer.Interval;
		}

		set
		{
			tQRRecognitionTimer.Interval = value;
		}
	}
	/// <summary>
	/// Purges QRData
	/// </summary>
	public void ClearQRData()
	{
		QRData = "";
        last_qrdata = "";
	}

    /// <summary>
    /// Fire event when QR data was received
    /// </summary>
    public UnityEvent onReadQRSuccess;

    [System.Serializable]
    public class onTrackableStatusChange : UnityEvent<bool>
    {
    }

    public onTrackableStatusChange statusChange;

    #endregion Public

    #region General things

    // Use this for initialization
    private void OnEnable()
	{
		Status = QRTrackingStatus.Initialization;
		qrCodeReader = new QRCodeReader();
		QRData = "";

		tQRRecognitionTimer = new Timer(100);
		tQRRecognitionTimer.AutoReset = true;
		tQRRecognitionTimer.Elapsed += new ElapsedEventHandler(CameraQRRoutine);

		StartCoroutine(SetupCamera());

		mTrackableBehaviour = GetComponent<TrackableBehaviour>();
		if (mTrackableBehaviour)
			mTrackableBehaviour.RegisterTrackableEventHandler(this);

        onTrackableStatusChange statusChange = new onTrackableStatusChange();
	}

	// Update is called once per frame
	private void Update()
	{
	}

	protected virtual void OnDestroy()
	{
		if (mTrackableBehaviour)
			mTrackableBehaviour.UnregisterTrackableEventHandler(this);
		tQRRecognitionTimer.Stop();
	}

	#endregion General things

	#region Vuforia things
	/// <summary>
	/// Trackable EventHandler
	/// </summary>
	/// <param name="previousStatus">previous status of this trackable</param>
	/// <param name="newStatus">new status of this trackable</param>
	public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
	{
		//CameraQRRoutine();
		//bool newTrackedStatus = false;
		switch (newStatus)
		{
			case TrackableBehaviour.Status.DETECTED:
			case TrackableBehaviour.Status.TRACKED:
			//case TrackableBehaviour.Status.EXTENDED_TRACKED:
				{
					tracked = true;
					break;
				}

			case TrackableBehaviour.Status.NO_POSE:
				{
					if (previousStatus == TrackableBehaviour.Status.TRACKED)
						tracked = false;
					break;
				}
			default:
			tracked = false;
			break;
		}

		if (tracked)
		{
			if (!tQRRecognitionTimer.Enabled)
			{
				tQRRecognitionTimer.Start();
			}

            Debug.Log("FOUND");
			OnTrackingFound();
			Status = QRTrackingStatus.MarkerFound;
		}
		else
		{
			if (tQRRecognitionTimer.Enabled)
			{
				tQRRecognitionTimer.Stop();
			}

            Debug.Log("LOST");
            OnTrackingLost();
			Status = QRTrackingStatus.NoMarker;
		}

        statusChange.Invoke(tracked);
	}
	//see DefaultTrackableEventHandler
	protected virtual void OnTrackingFound()
	{
		var rendererComponents = GetComponentsInChildren<Renderer>(true);
		var colliderComponents = GetComponentsInChildren<Collider>(true);
		var canvasComponents = GetComponentsInChildren<Canvas>(true);

		// Enable rendering:
		foreach (var component in rendererComponents)
			component.enabled = true;

		// Enable colliders:
		foreach (var component in colliderComponents)
			component.enabled = true;

		// Enable canvas':
		foreach (var component in canvasComponents)
			component.enabled = true;
	}
	//see DefaultTrackableEventHandler
	protected virtual void OnTrackingLost()
	{
		var rendererComponents = GetComponentsInChildren<Renderer>(true);
		var colliderComponents = GetComponentsInChildren<Collider>(true);
		var canvasComponents = GetComponentsInChildren<Canvas>(true);

		// Disable rendering:
		foreach (var component in rendererComponents)
			component.enabled = false;

		// Disable colliders:
		foreach (var component in colliderComponents)
			component.enabled = false;

		// Disable canvas':
		foreach (var component in canvasComponents)
			component.enabled = false;
	}

	#endregion Vuforia handling

	#region Camera things
	//don't look here wanky capture format things
	
	Dictionary<PIXEL_FORMAT, RGBLuminanceSource.BitmapFormat> capTypes = new Dictionary<PIXEL_FORMAT, RGBLuminanceSource.BitmapFormat>()
	{
		{PIXEL_FORMAT.GRAYSCALE, RGBLuminanceSource.BitmapFormat.Gray8 },
		{PIXEL_FORMAT.RGB888, RGBLuminanceSource.BitmapFormat.RGB24 }
	};
	Dictionary<DecodeHintType, object> hints = new Dictionary<DecodeHintType, object>()
	{
		{DecodeHintType.TRY_HARDER,true }//,
		//{DecodeHintType.CHARACTER_SET }//If not ok add other options
	};
	// QR recognition routine
	private void CameraQRRoutine(object source, ElapsedEventArgs e)
	{
		if (!cameraIsFineAndDandy) return;

		Image img = Vuforia.CameraDevice.Instance.GetCameraImage(capturePixelFormat);
		LuminanceSource ls = new RGBLuminanceSource(img.Pixels, img.BufferWidth, img.BufferHeight, capTypes[capturePixelFormat]);
		BinaryBitmap bbmp = new BinaryBitmap(new HybridBinarizer(ls));
		
		Result qrData = qrCodeReader.decode(bbmp, hints);

		if (qrData != null)
		{

			//QR code data should be parsed as text
			//Metadata contains only technical info

			QRData = qrData.Text;
			Status = QRTrackingStatus.ReadSuccessfull;
			tQRRecognitionTimer.Stop();
		}
	}

	private IEnumerator SetupCamera()
	{
		//make this thing great again
		yield return new WaitForSeconds(2);

		if (!CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO))
			CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_INFINITY);
		cameraIsFineAndDandy = CameraDevice.Instance.SetFrameFormat(capturePixelFormat, true);
		Status = QRTrackingStatus.NoMarker;
	}

	#endregion Camera things

	#region Quality Management
	public enum QualityMode { UltraLow, Low, Def, High, UltraHigh, Custom }
	private QualityMode qualityMode;
	
	public struct QualitySetting
	{
		public CameraDevice.CameraDeviceMode cameraMode;
		public int timerInterval;
		public bool tryHarder;
	}

	public Dictionary<QualityMode, QualitySetting> qualityPresets = new Dictionary<QualityMode, QualitySetting>()
	{
		{ QualityMode.UltraLow, new QualitySetting(){
			cameraMode = CameraDevice.CameraDeviceMode.MODE_OPTIMIZE_SPEED, timerInterval = 500} },
		{ QualityMode.Low, new QualitySetting(){
			cameraMode = CameraDevice.CameraDeviceMode.MODE_OPTIMIZE_SPEED, timerInterval = 250} },
		{ QualityMode.Def, new QualitySetting(){
			cameraMode = CameraDevice.CameraDeviceMode.MODE_DEFAULT, timerInterval = 100} },
		{ QualityMode.High, new QualitySetting(){
			cameraMode = CameraDevice.CameraDeviceMode.MODE_OPTIMIZE_QUALITY, timerInterval = 50} },
		{ QualityMode.UltraHigh, new QualitySetting(){
			cameraMode = CameraDevice.CameraDeviceMode.MODE_OPTIMIZE_QUALITY, timerInterval = 25} }
	};

	public void SetQuality(QualitySetting setting)
	{
		qualityMode = QualityMode.Custom;
		QRReadTimer = setting.timerInterval;

		CameraDevice.CameraDeviceMode currentCameraMode;
		;
		if ((CameraDevice.Instance.GetSelectedVideoMode(out currentCameraMode))&&(currentCameraMode != setting.cameraMode))
		{
			CameraDevice.Instance.Stop();
			CameraDevice.Instance.Deinit();
			CameraDevice.Instance.Init();
			CameraDevice.Instance.SelectVideoMode(setting.cameraMode);
			CameraDevice.Instance.Start();
		}
	}
	public QualityMode Quality
	{
		set
		{
			if(qualityMode != value)
			{
				SetQuality(qualityPresets[value]);
				qualityMode = value;
			}
		}

		get
		{
			return qualityMode;
		}
	}

	#endregion
}