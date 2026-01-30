using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
	public static InputManager I;


	public Vector2 MoveInput
	{
		get
		{
			if (Keyboard.current == null)
			{
				return Vector2.zero;
			}

			float x = 0f;
			float y = 0f;

			if (Keyboard.current.wKey.isPressed) y += 1;
			if (Keyboard.current.sKey.isPressed) y -= 1;

			if (Keyboard.current.aKey.isPressed) x -= 1;
			if (Keyboard.current.dKey.isPressed) x += 1;

			return new Vector2(x, y).normalized;
		}
	}


	private void Awake()
	{
		if (I != null && I != this)
		{
			Destroy(this);
			return;
		}
		I = this;
	}
}