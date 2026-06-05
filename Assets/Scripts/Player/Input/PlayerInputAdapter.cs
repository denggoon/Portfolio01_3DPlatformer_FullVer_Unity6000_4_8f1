using UnityEngine;

public class PlayerInputAdapter : MonoBehaviour
{
    [SerializeField] private CNJoystick joystick;

    void Awake()
    {
        if (joystick == null)
            joystick = FindAnyObjectByType<CNJoystick>();
    }

    public void SetJoystick(CNJoystick joystick)
    {
        this.joystick = joystick;
    }

    public PlayerInputState ReadInput()
    {
        PlayerInputState state = new PlayerInputState();

        if (joystick == null)
            return state;

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (UIManager.instance != null)
            {
                state.Horizontal = ReadJoystickAxis("Horizontal");
                state.Vertical = ReadJoystickAxis("Vertical");
                state.JumpPressed = UIManager.instance.buttonPressed;
            }
        }
        else
        {
            if (UIManager.instance != null)
            {
                state.Horizontal = ReadJoystickAxis("Horizontal");

                if (state.Horizontal == 0F)
                    state.Horizontal = Input.GetAxis("Horizontal");

                state.Vertical = ReadJoystickAxis("Vertical");

                if (state.Vertical == 0F)
                    state.Vertical = Input.GetAxis("Vertical");

                if (UIManager.instance.GetRRect().Contains(Input.mousePosition))
                {
                    state.JumpPressed = Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump");
                }
                else if (Input.GetKeyDown(KeyCode.Space))
                {
                    state.JumpPressed = true;
                }
            }
        }

        return state;
    }

    private float ReadJoystickAxis(string axisName)
    {
        if (joystick.DragRadius == 0F)
            return 0F;

        return Mathf.Clamp(joystick.GetAxis(axisName) / joystick.DragRadius, -1F, 1F);
    }
}
