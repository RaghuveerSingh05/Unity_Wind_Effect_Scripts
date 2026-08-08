using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class DynamicWindController : MonoBehaviour
{
    public static DynamicWindController Instance;
    
    [Header("Wind Settings")]
    public Vector3 windDirection = new Vector3(1, 0, 0);
    [Range(0f, 3f)] public float windStrength = 1f;
    [Range(0.1f, 5f)] public float windSpeed = 2f;
    [Range(0f, 1f)] public float windMotion = 0.5f;
    
    [Header("Random Wind")]
    public bool randomWind = false;
    [Range(0.3f, 1.8f)] public float strengthMin = 0.3f;
    [Range(0.3f, 1.8f)] public float strengthMax = 1.8f;
    public float changeIntervalMin = 3f;
    public float changeIntervalMax = 8f;
    [Range(0f, 1f)] public float windVariation = 0.5f;
    
    [Header("Force Settings")]
    public List<Rigidbody> affectedPhysics = new List<Rigidbody>();
    public List<ParticleSystem> affectedParticles = new List<ParticleSystem>();
    public float physicsForce = 2f;
    public float particleForce = 3f;
    
    public UnityEvent<Vector3, float> OnWindChanged;
    public UnityEvent<float> OnWindStrengthChanged;
    
    private Vector3 targetDirection;
    private float targetStrength;
    private float timer = 0f;
    private float changeInterval = 5f;
    private bool isInitialized = false;
    private bool wasInEditMode = false;
    
    private static readonly string SEASON = "_SeasonChangeGlobal";
    private static readonly string WIND_STRENGTH = "_GlobalWindStrength";
    private static readonly string WIND_SPEED = "_StrongWindSpeed";
    private static readonly string WIND_MOTION = "_WindMotion";
    private static readonly string WIND_DIRECTION = "_WindDirection";
    private static readonly string GENTLEBREEZE = "_WINDTYPE_GENTLEBREEZE";
    private static readonly string WINDOFF = "_WINDTYPE_WINDOFF";
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        FindPhysicsObjects();
        FindParticles();
        
        targetDirection = windDirection.normalized;
        targetStrength = windStrength;
        
        ApplyWindToShaders();
        ApplyForceToParticles();
        
        isInitialized = true;
    }
    
    void Update()
    {
        if (!Application.isPlaying)
        {
            ForceWindToZero();
            return;
        }
        
        if (wasInEditMode)
        {
            wasInEditMode = false;
            targetDirection = windDirection.normalized;
            targetStrength = windStrength;
        }
        
        if (randomWind)
        {
            timer += Time.deltaTime;
            if (timer >= changeInterval)
            {
                targetDirection = Random.insideUnitSphere;
                targetDirection.y = 0;
                targetDirection.Normalize();
                targetStrength = Random.Range(strengthMin, strengthMax);
                timer = 0f;
                changeInterval = Random.Range(changeIntervalMin, changeIntervalMax);
            }
            
            windDirection = Vector3.Lerp(windDirection, targetDirection, Time.deltaTime * windSpeed * 0.5f);
            windStrength = Mathf.Lerp(windStrength, targetStrength, Time.deltaTime * windSpeed * 0.5f);
        }
        else
        {
            windDirection = targetDirection;
            windStrength = targetStrength;
        }
        
        ApplyWindToShaders();
        ApplyForceToParticles();
        
        OnWindChanged?.Invoke(windDirection, windStrength);
        OnWindStrengthChanged?.Invoke(windStrength);
    }
    
    void FixedUpdate()
    {
        if (!Application.isPlaying) return;
        ApplyForceToObjects();
    }
    
    void ForceWindToZero()
    {
        wasInEditMode = true;
        
        Shader.DisableKeyword(GENTLEBREEZE);
        Shader.EnableKeyword(WINDOFF);
        
        Shader.SetGlobalFloat(WIND_STRENGTH, 0f);
        Shader.SetGlobalFloat(WIND_SPEED, 0f);
        Shader.SetGlobalFloat(WIND_MOTION, 0f);
        Shader.SetGlobalFloat(SEASON, 0f);
        Shader.SetGlobalVector(WIND_DIRECTION, Vector4.zero);
        
        foreach (ParticleSystem ps in affectedParticles)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var emission = ps.emission;
                emission.rateOverTime = 0f;
            }
        }
        
        foreach (Rigidbody rb in affectedPhysics)
        {
            if (rb != null && !rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
    
    void FindPhysicsObjects()
    {
        Rigidbody[] all = FindObjectsOfType<Rigidbody>();
        affectedPhysics.Clear();
        foreach (Rigidbody rb in all)
        {
            if (rb.mass < 2f && !rb.isKinematic)
                affectedPhysics.Add(rb);
        }
    }
    
    void FindParticles()
    {
        ParticleSystem[] all = FindObjectsOfType<ParticleSystem>();
        affectedParticles.Clear();
        foreach (ParticleSystem ps in all)
        {
            if (ps.name.Contains("Leaf") || ps.name.Contains("leaf") ||
                ps.name.Contains("Falling") || ps.name.Contains("Particle"))
                affectedParticles.Add(ps);
        }
    }
    
    void ApplyWindToShaders()
    {
        Shader.SetGlobalFloat(SEASON, 0f);
        Shader.SetGlobalFloat(WIND_STRENGTH, windStrength);
        Shader.SetGlobalFloat(WIND_SPEED, windSpeed);
        Shader.SetGlobalFloat(WIND_MOTION, windMotion);
        Shader.SetGlobalVector(WIND_DIRECTION, 
            new Vector4(windDirection.x, windDirection.y, windDirection.z, 0f));
        
        Shader.DisableKeyword(WINDOFF);
        Shader.EnableKeyword(GENTLEBREEZE);
    }
    
    void ApplyForceToParticles()
    {
        Vector3 force = windDirection * windStrength * particleForce;
        
        foreach (ParticleSystem ps in affectedParticles)
        {
            if (ps == null) continue;
            
            var fm = ps.forceOverLifetime;
            if (fm.enabled)
            {
                var x = fm.x;
                var y = fm.y;
                var z = fm.z;
                
                x.constantMin = force.x * 0.5f;
                x.constantMax = force.x * 1.5f;
                y.constantMin = force.y - 0.5f;
                y.constantMax = force.y - 0.3f;
                z.constantMin = force.z * 0.5f;
                z.constantMax = force.z * 1.5f;
                
                fm.x = x;
                fm.y = y;
                fm.z = z;
            }
        }
    }
    
    void ApplyForceToObjects()
    {
        Vector3 force = windDirection * windStrength * physicsForce;
        
        foreach (Rigidbody rb in affectedPhysics)
        {
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForce(force * Time.fixedDeltaTime, ForceMode.Acceleration);
                float turbX = Random.Range(-0.2f, 0.2f) * windStrength;
                float turbZ = Random.Range(-0.2f, 0.2f) * windStrength;
                rb.AddForce(new Vector3(turbX, 0, turbZ) * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
        }
    }
    
    public void SetWindDirection(Vector3 dir)
    {
        targetDirection = dir.normalized;
        randomWind = false;
    }
    
    public void SetWindStrength(float value)
    {
        targetStrength = Mathf.Clamp(value, 0f, 3f);
        randomWind = false;
        OnWindStrengthChanged?.Invoke(targetStrength);
    }
    
    public void SetWindSpeed(float value)
    {
        windSpeed = Mathf.Clamp(value, 0.1f, 5f);
    }
    
    public void SetWindMotion(float value)
    {
        windMotion = Mathf.Clamp(value, 0f, 1f);
    }
    
    public void SetWindX(float x)
    {
        Vector3 dir = windDirection;
        dir.x = Mathf.Clamp(x, -1f, 1f);
        dir.y = 0;
        dir.Normalize();
        SetWindDirection(dir);
    }
    
    public void SetWindZ(float z)
    {
        Vector3 dir = windDirection;
        dir.z = Mathf.Clamp(z, -1f, 1f);
        dir.y = 0;
        dir.Normalize();
        SetWindDirection(dir);
    }
    
    public void SetWindVariation(float value)
    {
        windVariation = Mathf.Clamp(value, 0f, 1f);
    }
    
    public void ToggleRandomWind(bool enabled)
    {
        randomWind = enabled;
        if (enabled)
        {
            targetDirection = Random.insideUnitSphere;
            targetDirection.y = 0;
            targetDirection.Normalize();
            targetStrength = Random.Range(strengthMin, strengthMax);
            timer = 0f;
            changeInterval = Random.Range(changeIntervalMin, changeIntervalMax);
        }
        else
        {
            targetDirection = windDirection;
            targetStrength = windStrength;
        }
    }
    
    public Vector3 GetWindDirection() => windDirection;
    public float GetWindStrength() => windStrength;
    public float GetWindSpeed() => windSpeed;
    public float GetWindMotion() => windMotion;
    public bool IsRandomWind() => randomWind;
    public float GetWindVariation() => windVariation;
}