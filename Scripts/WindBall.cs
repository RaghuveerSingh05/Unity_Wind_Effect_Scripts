using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WindBall : MonoBehaviour
{
    [Header("Ball Settings")]
    public float mass = 0.5f;
    public float drag = 0.5f;
    public float windInfluence = 1.5f;
    public float activationThreshold = 0.1f;
    
    private DynamicWindController windController;
    private Rigidbody rb;
    
    void Start()
    {
        windController = DynamicWindController.Instance;
        
        if (windController == null)
        {
            Debug.LogError("DynamicWindController not found!");
            enabled = false;
            return;
        }
        
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.linearDamping = drag;
        rb.useGravity = true;
    }
    
    void FixedUpdate()
    {
        if (windController == null) return;
        
        float strength = windController.GetWindStrength();
        
        if (strength > activationThreshold)
        {
            Vector3 windForce = windController.GetWindDirection() * strength * windInfluence;
            rb.AddForce(windForce, ForceMode.Acceleration);
        }
    }
}