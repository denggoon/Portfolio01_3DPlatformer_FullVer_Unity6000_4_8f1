using UnityEngine;

public class PlayerInputModeController : MonoBehaviour
{
    [SerializeField] private CNJoystick joystick;

    public void SetJoystick(CNJoystick joystick)
    {
        this.joystick = joystick;
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
}
