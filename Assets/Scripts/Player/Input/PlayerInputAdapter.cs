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

    public void RegisterDefaultInputMode()
    {
        if (PlayerPrefs.HasKey("DefaultInputMode"))
        {
            ApplyInputMode((INPUT_MODE)PlayerPrefs.GetInt("DefaultInputMode"));
            return;
        }

        INPUT_MODE defaultInputMode = GetDefaultInputMode();
        ApplyInputMode(defaultInputMode);
        PlayerPrefs.SetInt("DefaultInputMode", (int)defaultInputMode);
    }

    public void LoadSavedInputMode()
    {
        if (PlayerPrefs.HasKey("InputMode"))
            ApplyInputMode((INPUT_MODE)PlayerPrefs.GetInt("InputMode"));
    }

    private INPUT_MODE GetDefaultInputMode()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            return INPUT_MODE.STATIC_JOYSTICK;

        if (joystick != null && joystick.SnapsToFinger)
            return INPUT_MODE.FREE_JOYSTICK;

        if (UIManager.instance != null && UIManager.instance.optionPanel != null &&
            UIManager.instance.optionPanel.gamePadOverlay)
        {
            return INPUT_MODE.GAMEPAD;
        }

        return INPUT_MODE.STATIC_JOYSTICK;
    }

    private void ApplyInputMode(INPUT_MODE inputMode)
    {
        if (joystick != null)
        {
            switch (inputMode)
            {
                case INPUT_MODE.FREE_JOYSTICK:
                    joystick.SnapsToFinger = true;
                    joystick.TouchZoneSize = new Vector2(14F, 16F);
                    break;

                case INPUT_MODE.STATIC_JOYSTICK:
                case INPUT_MODE.GAMEPAD:
                    joystick.SnapsToFinger = false;
                    joystick.TouchZoneSize = new Vector2(5F, 5F);
                    break;
            }
        }

        if (UIManager.instance == null || UIManager.instance.optionPanel == null)
            return;

        bool useGamePadOverlay = inputMode == INPUT_MODE.GAMEPAD;
        UIManager.instance.optionPanel.gamePadOverlay = useGamePadOverlay;
        UIManager.instance.optionPanel.eConfirmedInputMode = inputMode;

        if (UIManager.instance.optionPanel.gamePadSys != null)
            UIManager.instance.optionPanel.gamePadSys.ToggleGamePadSprites(useGamePadOverlay);
    }

    private float ReadJoystickAxis(string axisName)
    {
        if (joystick.DragRadius == 0F)
            return 0F;

        return Mathf.Clamp(joystick.GetAxis(axisName) / joystick.DragRadius, -1F, 1F);
    }
}
