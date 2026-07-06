// Addressables 그룹에 등록된 모든 키를 한 곳에서 관리합니다.
// 키를 추가하거나 이름이 바뀌면 이 파일만 수정하면 됩니다.
public static class AddressableKeys
{
    // ResourcesManager.PopEffect / AttachEffect에 전달하는 FX 이름.
    // ResourcesManager 내부에서 "fxs/" 접두어를 붙여 로드합니다.
    public static class Fx
    {
        public const string JumpAttack01   = "Fx_JumpAttack_01";
        public const string Dust01         = "Fx_Dust_01";
        public const string Landing01      = "Fx_Landing_01";
        public const string Landing02      = "Fx_Landing_02";
        public const string M_Die01        = "Fx_M_Die_01";
        public const string PC_Hit01       = "Fx_PC_Hit_01";
        public const string Square01       = "Fx_Square_01";
        public const string Sparkling01    = "Fx_Sparkling_01";
        public const string PatternBox01   = "Fx_PatternBox_01";
        public const string Falling_P01    = "Fx_Falling_P_01";
        public const string Button01       = "Fx_Button_01";
        public const string DownPlatform01 = "Fx_DownPlatform_01";
        public const string Cannon_Start   = "Fx_Cannon_start";
        public const string Cannon_End     = "Fx_Cannon_end";
        public const string Bomb_Start     = "Fx_Bomb_start";
        public const string Bomb_End       = "Fx_Bomb_end";
        public const string Test00         = "Fx_00_Test";
        public const string Sunlight       = "FX_Sunlight";
        public const string Sandstem       = "FX_Sandstem";
        public const string Cave_Firefly   = "FX_Cave_Firefly";
    }

    // ResourcesManager.LoadGameObject / LoadGameObjectAsync에 전달하는 전체 주소.
    public static class Players
    {
        public const string P001_TT = "players/P_001_TT";
        public const string Test_TT = "players/Test_TT";
    }

    public static class Items
    {
        public const string GoldDroppable = "item/D_I_001_GoldDroppable";
        public const string ExitPrefab    = "item/exitprefab";
        public const string StartPrefab   = "item/startprefab";
        public const string DroppableGold = "item/droppablegold";
    }
}
