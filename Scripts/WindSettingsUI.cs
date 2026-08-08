using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class WindSettingsUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject settingsPanel;
    public Button closeButton;
    
    [Header("Controls")]
    public Slider strengthSlider;
    public Slider speedSlider;
    public Slider directionXSlider;
    public Slider directionZSlider;
    public Toggle randomToggle;
    
    [Header("Display")]
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI directionText;
    public TextMeshProUGUI statusText;
    
    private DynamicWindController windController;
    private bool isOpen = false;
    private bool wasCursorLocked = false;
    
    private InputAction toggleAction;
    private InputAction escapeAction;
    
    void Start()
    {
        windController = FindObjectOfType<DynamicWindController>();
        
        if (windController == null)
        {
            Debug.LogError("DynamicWindController not found!");
            return;
        }
        
        SetupInputActions();
        
        settingsPanel.SetActive(false);
        
        closeButton.onClick.AddListener(ClosePanel);
        
        strengthSlider.onValueChanged.AddListener(OnStrengthChanged);
        speedSlider.onValueChanged.AddListener(OnSpeedChanged);
        directionXSlider.onValueChanged.AddListener(OnDirectionXChanged);
        directionZSlider.onValueChanged.AddListener(OnDirectionZChanged);
        
        randomToggle.onValueChanged.AddListener(OnRandomToggled);
        
        strengthSlider.value = windController.GetWindStrength();
        speedSlider.value = windController.GetWindSpeed();
        directionXSlider.value = windController.GetWindDirection().x;
        directionZSlider.value = windController.GetWindDirection().z;
        randomToggle.isOn = windController.IsRandomWind();
        
        UpdateUI();
    }
    
    void SetupInputActions()
    {
        toggleAction = new InputAction("Toggle", binding: "<Keyboard>/e");
        toggleAction.performed += ctx => TogglePanel();
        toggleAction.Enable();
        
        escapeAction = new InputAction("Escape", binding: "<Keyboard>/escape");
        escapeAction.performed += ctx => { if (isOpen) ClosePanel(); };
        escapeAction.Enable();
    }
    
    void OnDestroy()
    {
        toggleAction?.Dispose();
        escapeAction?.Dispose();
    }
    
    void TogglePanel()
    {
        isOpen = !isOpen;
        settingsPanel.SetActive(isOpen);
        
        if (isOpen)
        {
            wasCursorLocked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            DisableCameraMouseLook(true);
            
            UpdateUI();
            
            Time.timeScale = 1f;
        }
        else
        {
            if (wasCursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            
            DisableCameraMouseLook(false);
        }
    }
    
    void ClosePanel()
    {
        isOpen = false;
        settingsPanel.SetActive(false);
        
        if (wasCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        DisableCameraMouseLook(false);
    }
    
    void DisableCameraMouseLook(bool disable)
    {
        MonoBehaviour[] allBehaviours = FindObjectsOfType<MonoBehaviour>();
        
        foreach (MonoBehaviour behaviour in allBehaviours)
        {
            string typeName = behaviour.GetType().Name;
            
            if (typeName == "CameraController" || 
                typeName == "SimpleCameraController" ||
                typeName == "FreeLookCamera" ||
                typeName == "ThirdPersonCamera" ||
                typeName.Contains("Camera"))
            {
                var enabledField = behaviour.GetType().GetField("enabled");
                if (enabledField != null)
                {
                    if (disable)
                    {
                        behaviour.enabled = false;
                    }
                    else
                    {
                        behaviour.enabled = true;
                    }
                }
            }
        }
    }
    
    void OnStrengthChanged(float value)
    {
        windController.SetWindStrength(value);
        UpdateUI();
    }
    
    void OnSpeedChanged(float value)
    {
        windController.SetWindSpeed(value);
        UpdateUI();
    }
    
    void OnDirectionXChanged(float value)
    {
        windController.SetWindX(value);
        UpdateUI();
    }
    
    void OnDirectionZChanged(float value)
    {
        windController.SetWindZ(value);
        UpdateUI();
    }
    
    void OnRandomToggled(bool enabled)
    {
        windController.ToggleRandomWind(enabled);
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (windController == null) return;
        
        Vector3 dir = windController.GetWindDirection();
        
        strengthText.text = string.Format("{0:F1}", windController.GetWindStrength());
        speedText.text = string.Format("{0:F1}", windController.GetWindSpeed());
        directionText.text = string.Format("({0:F1}, {1:F1})", dir.x, dir.z);
        statusText.text = windController.IsRandomWind() ? "Random" : "Manual";
    }
    
    public bool IsPanelOpen()
    {
        return isOpen;
    }
}