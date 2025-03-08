using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [SerializeField] private CinemachineVirtualCamera[] _allVirtualCamera;

    [Header("Controls for lerping Y Damping during player fall")]
    [SerializeField] private float _lerpPanAmount = 0.25f;
    [SerializeField] private float _lerpPanTime = 0.35f;
    public float _fallingSpeedDAmpingChangeThreshold = -15f;

    public bool _isLerpingFromPFalling { get; set; }
    public bool _isLerpingYDamping { get; private set; }

    private Coroutine _lerpFromPFalling;

    private CinemachineVirtualCamera _activeVirtualCamera;
    private CinemachineFramingTransposer _framingTransposer;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        for(int i = 0; i < _allVirtualCamera.Length; i++)
        {
            if (_allVirtualCamera[i].enabled)
            {
                _activeVirtualCamera = _allVirtualCamera[i];
                _framingTransposer = _activeVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }
    }   

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Lerp the Y Damping of the Virtual Camera

    public void LerpYDamping(bool targetYDamping)
    {
        _lerpFromPFalling = StartCoroutine(LerpYDampingCoroutine(targetYDamping));
    }

    private IEnumerator LerpYDampingCoroutine(bool isPlayerFalling)
    {
        /*
        float timeElapsed = 0f;
        float startValue = _framingTransposer.m_ScreenY;
        float endValue = isPlayerFalling ? 0.5f : 0.25f;
        while (timeElapsed < _lerpPanTime)
        {
            timeElapsed += Time.deltaTime;
            _framingTransposer.m_ScreenY = Mathf.Lerp(startValue, endValue, timeElapsed / _lerpPanTime);
            yield return null;
        }
        _isLerpingYDamping = false;
        */

        _isLerpingYDamping = true;

        float startDamper = _framingTransposer.m_YDamping;
        float endDamper = 0f;

        if(isPlayerFalling)
        {
            endDamper = _fallingSpeedDAmpingChangeThreshold;
            _isLerpingFromPFalling = true;
        }
        else
        {
            endDamper = 2f;
        }

        float elapsedTime = 0f;
        while (elapsedTime < _lerpPanTime)
        {
            elapsedTime += Time.deltaTime;
            _framingTransposer.m_YDamping = Mathf.Lerp(startDamper, endDamper, elapsedTime / _lerpPanTime);
            yield return null;
        }

        _isLerpingYDamping = false;
    }
    #endregion
}
