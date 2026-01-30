using UnityEngine;
using Enums;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    private float _moveSpeed;

    private Rigidbody2D _rb;

    private Vector2 _moveInput;
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

	private void Update()
	{
        _moveInput = InputManager.I.MoveInput;

        if (Keyboard.current.qKey.wasPressedThisFrame)
            StatsController.I.AddTimedModifier(StatType.moveSpeed, 2, StatModType.PercentMult, 3, "Speed Buff");
	}
	void FixedUpdate()
    {
        _moveSpeed = StatsController.I.GetStat(StatType.moveSpeed).Value;

        _rb.linearVelocity = _moveInput * _moveSpeed;
    }
}
