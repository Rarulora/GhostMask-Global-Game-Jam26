using UnityEngine;
using Enums;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] srList;
    [SerializeField] private Animator anim;
    private float _moveSpeed;

    private Rigidbody2D _rb;

    private Vector2 _moveInput;

    private bool _isDashing = false;

	private void OnEnable()
	{
		EventManager.OnDashStatusChanged += (b) => _isDashing = b;
	}
	private void OnDisable()
	{
		EventManager.OnDashStatusChanged -= (b) => _isDashing = b;
	}
	void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if(srList.Length == 0) srList = GetComponentsInChildren<SpriteRenderer>();
    }

	private void Update()
	{
        _moveInput = InputManager.I.MoveInput;

        if (Keyboard.current.qKey.wasPressedThisFrame)
            StatsController.I.AddTimedModifier(StatType.moveSpeed, 2, StatModType.PercentMult, 3, "Speed Buff");
	}
	void FixedUpdate()
    {
        if (_isDashing) return;
        _moveSpeed = StatsController.I.GetStat(StatType.moveSpeed).Value;

        if (anim != null)
        {
			if (!Mathf.Approximately(_moveInput.magnitude, 0)) anim?.SetBool("isMoving", true);
			else anim?.SetBool("isMoving", false);
		}
        if(_moveInput.x < 0) Flip(true);
        else Flip(false);
        _rb.linearVelocity = _moveInput * _moveSpeed;
    }

    void Flip(bool isFlip)
    {
        if(srList.Length == 0) return;
        foreach(var sr in srList)
            sr.flipX = isFlip;
    }
}
