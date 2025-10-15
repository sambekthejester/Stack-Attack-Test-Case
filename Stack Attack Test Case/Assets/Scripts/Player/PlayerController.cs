using UnityEngine;

public class PlayerController : MonoSingleton<PlayerController>
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;      
    [SerializeField] private float minX = -3f;
    [SerializeField] private float maxX = 3f;

    [Header("Input")]
    [SerializeField] private bool useMouse = true;    
    [SerializeField] private float dragSensitivity = 1.0f;

    [SerializeField] Animator animator;
    [SerializeField] string fireTrigger = "Fire";  
 
    private bool isDragging = false;
    private Vector2 lastPointerPos;    
    private float targetX;            

    protected override void Awake()
    {
        base.Awake();
        targetX = transform.position.x;
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void Reset()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        WeaponBase.OnAnyWeaponFired += HandleFire;
    }

    void OnDisable()
    {
        WeaponBase.OnAnyWeaponFired -= HandleFire;
    }

    private void Update()
    {
         
        if (GameManager.Instance != null && !GameManager.Instance.CanControlPlayer)
            return;

        ReadInput();
        ApplyMovement();
    }

    private void ReadInput()
    {
        
#if UNITY_EDITOR || UNITY_STANDALONE
        if (useMouse)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastPointerPos = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                var pos = (Vector2)Input.mousePosition;
                var delta = pos - lastPointerPos;
                lastPointerPos = pos;

                 
                targetX += delta.x * dragSensitivity * 0.01f;  
            }
        }
        else
#endif
        {
       
            if (Input.touchCount > 0)
            {
                var t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    isDragging = true;
                    lastPointerPos = t.position;
                }
                else if (t.phase == TouchPhase.Moved && isDragging)
                {
                    var delta = t.position - lastPointerPos;
                    lastPointerPos = t.position;

                    targetX += delta.x * dragSensitivity * 0.01f;
                }
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    isDragging = false;
                }
            }
        }

    
        targetX = Mathf.Clamp(targetX, minX, maxX);
    }

    private void ApplyMovement()
    {
      
        var p = transform.position;
        float desired = Mathf.MoveTowards(p.x, targetX, moveSpeed * Time.deltaTime);
        p.x = desired;
        transform.position = p;
    }

  
    public void HardStopMovement()
    {
        isDragging = false;
         
        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    
        targetX = transform.position.x;
    }
    void HandleFire()
    {
        if (!animator) return;
  
        animator.SetTrigger(fireTrigger);

     
    }
}
