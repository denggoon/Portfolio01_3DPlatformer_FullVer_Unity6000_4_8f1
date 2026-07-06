using UnityEngine;

public class PlayerAnimEventCaller : AnimatorEventCaller
{
    private PlayerMoveCC _player;
    private PlayerFX     _playerFx;

    protected override void Awake()
    {
        base.Awake();
        _player   = parentObj.GetComponent<PlayerMoveCC>();
        _playerFx = parentObj.GetComponent<PlayerFX>();
    }

    public void FootSound()                  => _player?.FootSound();
    public void ToggleCommonFx(int flag)     => _playerFx?.ToggleCommonFx(flag != 0);
    public void PlayMoveFX(int flag)         => _playerFx?.PlayMoveFX(flag != 0);
    public void ToggleAirTrail(int flag)     => _playerFx?.ToggleAirTrail(flag != 0);
    public void ToggleSpecAirTrail(int flag) => _playerFx?.ToggleSpecAirTrail(flag != 0);
    public void ToggleAllTrail(int flag)     => _playerFx?.ToggleAllTrail(flag != 0);
}
