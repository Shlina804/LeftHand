using System.Text;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace LeftHand;

public class LeftHandConfig : BasePluginConfig
{
    [JsonPropertyName("Enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("Debug")] public bool Debug { get; set; } = false;

    // ===== 隔墙预瞄范围 =====
    [JsonPropertyName("PreaimMaxDistance")] public float PreaimMaxDistance { get; set; } = 1100f;
    [JsonPropertyName("PreaimHardRadius")] public float PreaimHardRadius { get; set; } = 380f;      // 这个距离内必定锁
    [JsonPropertyName("PreaimContactTime")] public float PreaimContactTime { get; set; } = 0.9f;    // 预测"下一秒碰面"
    [JsonPropertyName("PreaimEngageDistance")] public float PreaimEngageDistance { get; set; } = 700f;
    [JsonPropertyName("PreaimMaxHeightDiff")] public float PreaimMaxHeightDiff { get; set; } = 140f;
    [JsonPropertyName("PreaimFrontConeDeg")] public float PreaimFrontConeDeg { get; set; } = 120f;
    [JsonPropertyName("PreaimLeadTime")] public float PreaimLeadTime { get; set; } = 0.15f;
    [JsonPropertyName("HolderTrackThroughWalls")] public bool HolderTrackThroughWalls { get; set; } = true;
    // 站桩架点的 bot 用多宽的正面锥角去挑墙后目标（移动中的 bot 用 PreaimFrontConeDeg）
    [JsonPropertyName("HolderPreaimConeDeg")] public float HolderPreaimConeDeg { get; set; } = 100f;
    // true = 双方都静止且距离在 PreaimMaxDistance 内也算"要接战"，这样 bot 才会隔着墙预瞄站着不动的你
    [JsonPropertyName("TrackStaticTargets")] public bool TrackStaticTargets { get; set; } = true;
    // true = 只在看不见目标时改 bot 的朝向/瞄点（隔墙预瞄）；一旦可见就交还给它自己的 AI 瞄准+开火。
    // 设 false 会全程硬改朝向，副作用是 bot 侧身对人、并且不开枪。
    [JsonPropertyName("PreaimOnlyWhenInvisible")] public bool PreaimOnlyWhenInvisible { get; set; } = true;

    // ===== 与 BotAimImprover 等瞄准类插件的分工 =====
    // LeftHand 只负责走位/peek/蹲拉/急停 + 隔墙锁定目标；真正的"瞄哪、开不开枪"交给 BotAimImprover。
    // 下面三个开关决定 LeftHand 到底往 bot 内存里写什么：
    //   PreaimLookAtSpot = 隔墙锁头（只写 m_lookAtSpot，看不见目标时才写）—— 默认开
    //   WriteLookYaw      = 写 m_lookYaw / m_lookPitch（会顶掉 BotAimImprover 的瞄准）—— 默认关
    //   WriteBodyAngles   = 用 Teleport 拧 pawn 朝向（会侧身对人 + 顶掉瞄准）—— 默认关
    [JsonPropertyName("PreaimLookAtSpot")] public bool PreaimLookAtSpot { get; set; } = true;
    // 隔墙时把"瞄准部位"(m_targetSpot) 也压到对手头上：这是玩家肉眼看到的那条枪口/准星线。
    // m_lookAtSpot 只管 bot 下一步想瞄哪，不改它当前枪口，所以只写那个的话看起来像"没锁我"。
    [JsonPropertyName("PreaimTargetSpotWhenHidden")] public bool PreaimTargetSpotWhenHidden { get; set; } = true;
    [JsonPropertyName("WriteLookYaw")] public bool WriteLookYaw { get; set; } = false;
    [JsonPropertyName("WriteBodyAngles")] public bool WriteBodyAngles { get; set; } = false;

    // ===== bot 难度（决定原生瞄头部位选择，BotAimImprover 的智能瞄头也依赖它） =====
    // 0~4，4 = 专家：bot 会挑头部；2 及以下基本只瞄身体。
    [JsonPropertyName("ApplyBotDifficulty")] public bool ApplyBotDifficulty { get; set; } = true;
    [JsonPropertyName("BotDifficulty")] public int BotDifficulty { get; set; } = 4;
    // bot_difficulty=5（自定义）时生效的 0~100 档位，这里给满，避免被 cfg 里的 5 拖低
    [JsonPropertyName("CustomBotDifficulty")] public int CustomBotDifficulty { get; set; } = 100;

    // ===== 瞄准拟人 =====
    [JsonPropertyName("AimTurnSpeedDeg")] public float AimTurnSpeedDeg { get; set; } = 480f;
    [JsonPropertyName("AimTurnSpeedVisibleDeg")] public float AimTurnSpeedVisibleDeg { get; set; } = 1100f;
    [JsonPropertyName("AimHeadOffsetZ")] public float AimHeadOffsetZ { get; set; } = -1.5f;
    [JsonPropertyName("AimJitterDeg")] public float AimJitterDeg { get; set; } = 1.2f;

    // ===== 急停 =====
    [JsonPropertyName("CounterStrafe")] public bool CounterStrafe { get; set; } = true;
    [JsonPropertyName("CounterStrafeFactor")] public float CounterStrafeFactor { get; set; } = 0f; // 0=完全停
    [JsonPropertyName("RunSpeedScale")] public float RunSpeedScale { get; set; } = 1.0f;

    // ===== 拉枪通用 =====
    [JsonPropertyName("MinMoveSpeedToPeek")] public float MinMoveSpeedToPeek { get; set; } = 10f;
    [JsonPropertyName("MinLateralRatio")] public float MinLateralRatio { get; set; } = 0.1f;
    [JsonPropertyName("EngageTimeout")] public float EngageTimeout { get; set; } = 6.0f;   // 一波对枪最长时长（要留够四拍：探出→回缩→等→大拉）
    // bot 的 m_isEnemyVisible 刷新慢会闪，只认它有时会漏；但完全不看"看得见"也不行 ——
    // peek 的意义是骗枪，必须在你能看到它的时候做。这里给"刚失去视线"一个很短的宽限窗口。
    [JsonPropertyName("InvisibleCommitTime")] public float InvisibleCommitTime { get; set; } = 0.25f;
    [JsonPropertyName("JustSeenWindow")] public float JustSeenWindow { get; set; } = 0.30f;
    [JsonPropertyName("LostSightTime")] public float LostSightTime { get; set; } = 0.7f;
    [JsonPropertyName("FireHoldMax")] public float FireHoldMax { get; set; } = 2.0f;
    [JsonPropertyName("ReengageCooldown")] public float ReengageCooldown { get; set; } = 0.6f;

    // ===== 模式1：peek接大拉 =====
    // 一次完整的 peek 流程：探出约1/3 → 停住确认对手位置/爆头线 → 收回 → 墙后调整 → 整身大拉开打
    [JsonPropertyName("ShoulderSpeed")] public float ShoulderSpeed { get; set; } = 140f;      // v1.01：140 比 150 稳、比 115 快，探身/缩回有"抽一下"的感觉
    [JsonPropertyName("ShoulderOutTime")] public float ShoulderOutTime { get; set; } = 0.17f;  // 140×0.17 ≈ 24 单位 ≈ 1/3 个身位
    [JsonPropertyName("ConfirmHoldMin")] public float ConfirmHoldMin { get; set; } = 0.10f;    // 探出后停留 0.1s（让对手看见你露头）
    [JsonPropertyName("ConfirmHoldMax")] public float ConfirmHoldMax { get; set; } = 0.10f;
    [JsonPropertyName("ShoulderBackTime")] public float ShoulderBackTime { get; set; } = 0.26f; // 140×0.26 ≈ 36：探出只有 ~20（起步是加速的），多走一截才能真退回掩体
    // 探出之前的"墙前准备"：先站定刹停、把准星压到人头上，再探出去（不然看起来是"看见你才动"）
    [JsonPropertyName("PeekSetupTime")] public float PeekSetupTime { get; set; } = 0.18f;
    [JsonPropertyName("BaitPauseMin")] public float BaitPauseMin { get; set; } = 0.20f;         // 缩回掩体后停 0.2s 再整身大拉
    [JsonPropertyName("BaitPauseMax")] public float BaitPauseMax { get; set; } = 0.20f;
    [JsonPropertyName("SwingRampTime")] public float SwingRampTime { get; set; } = 0.18f;      // 大拉从 0 加速上去的时间
    [JsonPropertyName("SwingSpeedScale")] public float SwingSpeedScale { get; set; } = 0.55f;    // v1.01：0.70 仍偏"瞬移"，0.55 ≈ 132/秒
    // 大拉速度不再跟武器跑速挂钩：所有武器统一用这个基准速度 × SwingSpeedScale，
    // 狙/步枪/微冲拉出来的速度和身位完全一样（AWP 跑速 200、M4 225，原样乘系数狙会明显慢一拍）。
    [JsonPropertyName("SwingBaseSpeed")] public float SwingBaseSpeed { get; set; } = 240f;
    [JsonPropertyName("SwingMinTime")] public float SwingMinTime { get; set; } = 0.16f;
    // 大拉一看到人就硬停：速度归零后还要等这么多秒才开火，等弹道扩散归零（第一枪才准）
    [JsonPropertyName("StopSettleTime")] public float StopSettleTime { get; set; } = 0.06f;
    // 拉枪后的冷却：默认 0 = 完全关闭，不再限制同一波对枪里能拉几次。
    // 想恢复限制就把值调成想要的秒数（例如 2）。
    [JsonPropertyName("PeekCooldown")] public float PeekCooldown { get; set; } = 0f;

    // 瞄准全部交给 BotAimImprover / bot 自己的 AI，这里只保留模式2 的"别朝滑行方向看"补偿。
    // 之前试过 WriteTargetSpotHead / WriteLookPitchToHead / AimControl / SprayComp，
    // 它们会跟 BotAimImprover 抢同一个瞄准部位，反而让它不生效、也不压枪，已全部删除。
    [JsonPropertyName("M2ForceLook")] public bool M2ForceLook { get; set; } = true;
    // 蹲拉/大拉时把瞄准部位（m_targetSpot）压到玩家身上：玩家看到的"它是不是在看我"由这个决定。
    [JsonPropertyName("M2AimAtEnemy")] public bool M2AimAtEnemy { get; set; } = true;
    [JsonPropertyName("FireForceLook")] public bool FireForceLook { get; set; } = false;
    [JsonPropertyName("FireForceLookTime")] public float FireForceLookTime { get; set; } = 0.15f;
    [JsonPropertyName("M2ForceLookTime")] public float M2ForceLookTime { get; set; } = 0.15f;
    [JsonPropertyName("SwingMaxTime")] public float SwingMaxTime { get; set; } = 1.00f;
    // 大拉至少要拉出这么远才允许急停开枪：否则"刚露个头就停"，玩家看到的是 peek 了一下又缩回去
    [JsonPropertyName("SwingMinTravel")] public float SwingMinTravel { get; set; } = 45f;   // 大拉至少要拉出这么多（≈一个身位）才算拉到位
    // v1.01 新动作（玩家指定）：大拉途中一看见对手，先不停、边跑边开枪；
    // 然后继续往外拉 SwingPostMoveMin~Max 秒、整身位拉到 SwingFullTravel，才急停，第二枪打头。
    [JsonPropertyName("SwingPostMoveMin")] public float SwingPostMoveMin { get; set; } = 0.20f;
    [JsonPropertyName("SwingPostMoveMax")] public float SwingPostMoveMax { get; set; } = 0.30f;
    [JsonPropertyName("SwingFullTravel")] public float SwingFullTravel { get; set; } = 55f;  // 2/3 身位（34 单位是半身，55 ≈ 2/3）
    // false = 模式1 只骗枪不开枪：整套动作走完就把开火权交回 bot 自己的 AI（打头/压枪交给 BotAimImprover）
    [JsonPropertyName("M1FireAfterSwing")] public bool M1FireAfterSwing { get; set; } = true;
    [JsonPropertyName("BaitOnlyIfEnemyAiming")] public bool BaitOnlyIfEnemyAiming { get; set; } = false;
    [JsonPropertyName("EnemyAimConeDeg")] public float EnemyAimConeDeg { get; set; } = 20f;
    // true = 第一次照面（已经看得见）且距离够远时，也先做一次 shoulder/大拉或蹲拉再开火，
    // 否则对玩家永远是"一看见就 Fire"，看不到模式1/模式2。
    [JsonPropertyName("BaitOnFirstContact")] public bool BaitOnFirstContact { get; set; } = true;
    [JsonPropertyName("PeekBaitMinDistance")] public float PeekBaitMinDistance { get; set; } = 100f; // 再近就直接开打，不做拉枪
    // 墙后盲 peek（v1.01）：双方还互相看不见时，靠隔墙预瞄知道位置，就先探半个身位 → 缩回 → 整身拉出
    [JsonPropertyName("BlindPeek")] public bool BlindPeek { get; set; } = true;
    [JsonPropertyName("BlindPeekDelay")] public float BlindPeekDelay { get; set; } = 0.20f;
    [JsonPropertyName("BlindPeekMaxDistance")] public float BlindPeekMaxDistance { get; set; } = 450f;
    // true = 目标已锁定但还没开始做 peek 时，bot 在墙后原地不动（不往前蹭、不露身位），
    // 只把准星对到对手头上；所有动作都从 M1Setup 才开始。这就是"一点不漏、对好头再出来"。
    [JsonPropertyName("HoldPositionWhileHidden")] public bool HoldPositionWhileHidden { get; set; } = true;
    // true = bot 自己有真实视线（说明身边没掩体、藏不住）时，直接进入对枪，不再套"准备→peek"那套。
    // 那一套在偷看相位会把开火权按住，没掩体时就会表现成"站着不开枪"。
    [JsonPropertyName("FightWhenBotSees")] public bool FightWhenBotSees { get; set; } = true;

    // ===== 模式2：蹲拉 =====
    // 模式1/模式2 现在纯随机挑（每次拉枪重新掷一次），不再根据进攻方/敌人速度判定。
    [JsonPropertyName("Mode2Chance")] public float Mode2Chance { get; set; } = 0.5f;
    [JsonPropertyName("CrouchSlideTime")] public float CrouchSlideTime { get; set; } = 0.10f;
    // 模式2 正确形态：先站着拉出去（对手看到"开始移动"），过一小会儿才下蹲，
    // 蹲下的过程被对手看到，同时继续带一点横向位移 —— 这就是"蹲拉"，不是一出来就蹲。
    // 蹲下要先原地顿一下：带速写蹲姿会被引擎取消，人就站着跑出来
    [JsonPropertyName("M2CrouchSettleTime")] public float M2CrouchSettleTime { get; set; } = 0.12f;
    // 偷看那几拍不许开火：按住 bot 的"正在瞄准敌人"，骗枪才是纯骗枪。
    [JsonPropertyName("SuppressAimDuringPeek")] public bool SuppressAimDuringPeek { get; set; } = true;
    // 步枪 AI 就算 m_isAimingAtEnemy 被按成 false，也有可能在探出那一拍抢一枪；
    // 再把它当帧的"敌人可见"也按掉，步枪就不会在偷看时开枪了（这一帧不参与判定，下一帧自动恢复）。
    [JsonPropertyName("SuppressVisibleDuringPeek")] public bool SuppressVisibleDuringPeek { get; set; } = true;
    // 偷看/蹲拉时身体正对敌人（否则侧步会让人侧着身子看你）
    [JsonPropertyName("ForceFaceDuringPeek")] public bool ForceFaceDuringPeek { get; set; } = false;
    [JsonPropertyName("M2StandOutTime")] public float M2StandOutTime { get; set; } = 0.14f;   // 站姿段：先把身位拉出来
    [JsonPropertyName("M2CrouchTime")] public float M2CrouchTime { get; set; } = 0.22f;        // 蹲滑段：边蹲边继续拉，拉完才开火
    // 蹲拉必须先拉出身位再开火：位移不到这么多就不许开枪（45 ≈ 大半个身位）
    [JsonPropertyName("M2MinTravel")] public float M2MinTravel { get; set; } = 45f;
    [JsonPropertyName("M2MaxExtraTime")] public float M2MaxExtraTime { get; set; } = 0.30f;
    [JsonPropertyName("M2CrouchSpeedScale")] public float M2CrouchSpeedScale { get; set; } = 0.52f; // 蹲姿速度 = 跑速 × 这个（蹲着本来就慢，0.52 ≈ 111）
    [JsonPropertyName("CrouchSlideDecay")] public float CrouchSlideDecay { get; set; } = 0.88f;
    [JsonPropertyName("CrouchHoldMax")] public float CrouchHoldMax { get; set; } = 2.5f;
}

public class LeftHandPlugin : BasePlugin, IPluginConfig<LeftHandConfig>
{
    public override string ModuleName => "LeftHand 左手插件";
    public override string ModuleVersion => "1.0.1";
    public override string ModuleAuthor => "LeftHand";
    public override string ModuleDescription => "Bot 隔墙预瞄 / peek接大拉 / 蹲拉 / 急停";

    public LeftHandConfig Config { get; set; } = new();
    public void OnConfigParsed(LeftHandConfig config) => Config = config;

    private const float R2D = 57.29578f;

    private enum Phase { None, Track, M1Setup, M1ShoulderOut, M1Confirm, M1ShoulderBack, M1Pause, M1Swing, M1Stop, M2Slide, M2Crouch, Fire, Cooldown }

    private sealed class BotState
    {
        public Phase Phase = Phase.None;
        public int TargetSlot = -1;
        public float PhaseStart, EngageStart, LastSeen, PauseDur, CooldownUntil, FireStart;
        public bool HasAim, PeekPlanned, ModeDecided, Mode2, Crouch;
        public float PeekX, PeekY, CurYaw, CurPitch, JitYaw, JitPitch, NextJitter;
        public float PeekCooldownUntil;   // 这一波打过拉枪后，要等这么久才允许下一次
        public float SwingX0, SwingY0;    // 大拉起点（用来保证"拉满一个身位"才停）
        public float SwingSightAt;        // 大拉途中"真正看见对手"的时刻（0 = 还没看见）
        public float SwingPostMove;       // 看见后还要继续往外拉多久（0.2~0.3s）才急停
        public bool DbgBotNull, DbgTarget, DbgVisible, DbgSees, DbgHeadAim;
        public float DbgSpeed;   // 这一帧实际水平速度（验证速度写入到底有没有生效）

        public void Reset()
        {
            Phase = Phase.None; TargetSlot = -1; HasAim = false; PeekPlanned = false;
            ModeDecided = false; Mode2 = false; Crouch = false;
            SwingSightAt = 0f; SwingPostMove = 0.25f;
        }
    }

    private sealed class Snap
    {
        public CCSPlayerController Ctrl = null!;
        public CCSPlayerPawn Pawn = null!;
        public int Slot, Team;
        public float X, Y, Z, VX, VY, VZ, EyeOff, Yaw, Pitch;
        public bool OnGround;
        public float Speed2D => MathF.Sqrt(VX * VX + VY * VY);
    }

    private readonly Dictionary<int, BotState> _states = new();
    private readonly List<Snap> _alive = new();
    private readonly Random _rng = new();
    private bool _bombPlanted;
    private float _lastTick;
    private bool _botFieldsOk = true;
    private bool _headAimFlag;   // 本帧是否把视线写到了敌人（forceLook 诊断用）
    private float _diagNext;
    private float _diagChatNext;
    private int _writeCount;
    private bool _chatOn;
    private readonly StringBuilder _diagErr = new();
    private readonly StringBuilder _diagBots = new();
    private Vector _vel = null!;
    private QAngle _ang = null!;

    private static readonly Dictionary<string, float> WeaponSpeeds = new()
    {
        ["weapon_ak47"] = 215, ["weapon_m4a1"] = 225, ["weapon_m4a1_silencer"] = 225, ["weapon_famas"] = 220,
        ["weapon_galilar"] = 215, ["weapon_aug"] = 220, ["weapon_sg556"] = 210, ["weapon_awp"] = 200,
        ["weapon_ssg08"] = 230, ["weapon_scar20"] = 215, ["weapon_g3sg1"] = 215, ["weapon_deagle"] = 230,
        ["weapon_revolver"] = 220, ["weapon_glock"] = 240, ["weapon_hkp2000"] = 240, ["weapon_usp_silencer"] = 240,
        ["weapon_p250"] = 240, ["weapon_fiveseven"] = 240, ["weapon_tec9"] = 240, ["weapon_cz75a"] = 240,
        ["weapon_elite"] = 240, ["weapon_mp9"] = 240, ["weapon_mac10"] = 240, ["weapon_mp7"] = 220,
        ["weapon_mp5sd"] = 235, ["weapon_ump45"] = 230, ["weapon_p90"] = 230, ["weapon_bizon"] = 240,
        ["weapon_nova"] = 220, ["weapon_xm1014"] = 215, ["weapon_mag7"] = 225, ["weapon_sawedoff"] = 210,
        ["weapon_m249"] = 195, ["weapon_negev"] = 150,
    };

    public override void Load(bool hotReload)
    {
        _vel = new Vector(0, 0, 0);
        _ang = new QAngle(0, 0, 0);
        RegisterListener<Listeners.OnTick>(OnTick);
        RegisterListener<Listeners.OnMapStart>(_ =>
        {
            _states.Clear();
            _bombPlanted = false;
            ApplyBotDifficulty();
        });
        Console.WriteLine("[LeftHand] 左手插件已加载");
        try { File.AppendAllText(Path.Combine(ModuleDirectory, "LeftHand-debug.log"), $"=== LOADED hotReload={hotReload} at {DateTime.Now:HH:mm:ss} ===\n"); } catch { }
        SelfCheckSchema();
        // 热重载时地图已经在跑，OnMapStart 不会触发，这里补一次（下一帧执行，保证服务端已就绪）
        try
        {
            if (!string.IsNullOrEmpty(Server.MapName)) Server.NextFrame(ApplyBotDifficulty);
        }
        catch { }
    }

    // 加载时把所有依赖的 schema 字段偏移打出来：偏移 <=0 或抛异常 = 这条写入不可用
    private void ApplyBotDifficulty()
    {
        if (!Config.ApplyBotDifficulty) return;
        try
        {
            int d = Math.Clamp(Config.BotDifficulty, 0, 4);
            Server.ExecuteCommand($"bot_difficulty {d}");
            Server.ExecuteCommand($"custom_bot_difficulty {Math.Clamp(Config.CustomBotDifficulty, 0, 100)}");
            Console.WriteLine($"[LeftHand] bot_difficulty -> {d}, custom_bot_difficulty -> {Config.CustomBotDifficulty}");
        }
        catch { }
    }

    private void SelfCheckSchema()
    {
        var pairs = new (string Cls, string Fld)[]
        {
            ("CCSPlayerPawn", "m_pBot"),
            ("CCSPlayerPawn", "m_entitySpottedState"),
            ("CCSPlayerPawn", "m_bBotAllowActive"),
            ("CCSBot", "m_lookYaw"),
            ("CCSBot", "m_lookPitch"),
            ("CCSBot", "m_lookAtSpot"),
            ("CCSBot", "m_isEnemyVisible"),
            ("CCSBot", "m_enemy"),
            ("CCSBot", "m_isAimingAtEnemy"),
            ("CBot", "m_isCrouching"),
        };
        var sb = new StringBuilder();
        sb.Append($"[{DateTime.Now:HH:mm:ss}] LOAD schema-check\n");
        foreach (var pr in pairs)
        {
            string r;
            try
            {
                var off = Schema.GetSchemaOffset(pr.Cls, pr.Fld);
                r = off > 0 ? $"off={off}" : $"BAD_OFFSET={off}";
            }
            catch (Exception ex) { r = "ERR:" + ex.GetType().Name; }
            sb.Append($"   {pr.Cls}.{pr.Fld} -> {r}\n");
        }
        try { File.AppendAllText(Path.Combine(ModuleDirectory, "LeftHand-debug.log"), sb.ToString()); } catch { }
    }

    // ================= 事件 =================
    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart e, GameEventInfo info)
    {
        _bombPlanted = false;
        foreach (var s in _states.Values) s.Reset();
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnBombPlanted(EventBombPlanted e, GameEventInfo info)
    {
        _bombPlanted = true;
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath e, GameEventInfo info)
    {
        var v = e.Userid;
        if (v != null && _states.TryGetValue(v.Slot, out var s)) s.Reset();
        return HookResult.Continue;
    }

    // ================= 命令 =================
    // 本地/单人服默认没配 admins.json，@css/root 谁都拿不到，命令会被静默拒绝，
    // 所以这两个命令不再挂权限，改为配置驱动的开关（要在多人公开服用请自行加回 RequiresPermissions）。
    [ConsoleCommand("css_lefthand", "开关左手插件: css_lefthand 0/1")]
    public void CmdToggle(CCSPlayerController? player, CommandInfo cmd)
    {
        Config.Enabled = cmd.ArgCount > 1 ? cmd.GetArg(1) != "0" : !Config.Enabled;
        cmd.ReplyToCommand($"[LeftHand] Enabled = {Config.Enabled}");
    }

    [ConsoleCommand("css_lefthand_debug", "左手插件调试输出开关")]
    public void CmdDebug(CCSPlayerController? player, CommandInfo cmd)
    {
        Config.Debug = !Config.Debug;
        cmd.ReplyToCommand($"[LeftHand] Debug = {Config.Debug}");
    }

    // 一键开关聊天框输出：状态行 + 换模式/换阶段提示
    [ConsoleCommand("css_lefthand_chat", "聊天框显示开关: css_lefthand_chat 0/1（不带参数即取反）")]
    public void CmdChat(CCSPlayerController? player, CommandInfo cmd)
    {
        _chatOn = cmd.ArgCount > 1 ? cmd.GetArg(1) != "0" : !_chatOn;
        cmd.ReplyToCommand($"[LeftHand] 聊天框显示 = {(_chatOn ? "开" : "关")}");
    }

    [ConsoleCommand("css_lefthand_bots", "补 bot: css_lefthand_bots [数量]，默认 10（只设 quota+fill，不叠 bot_add）")]
    public void CmdBots(CCSPlayerController? player, CommandInfo cmd)
    {
        int n = 10;
        if (cmd.ArgCount > 1) int.TryParse(cmd.GetArg(1), out n);
        n = Math.Clamp(n, 1, 20);
        Server.ExecuteCommand($"bot_quota {n}");
        Server.ExecuteCommand("bot_quota_mode fill");
        cmd.ReplyToCommand($"[LeftHand] 已设 bot_quota {n} + fill");
        Server.PrintToChatAll($" \x01[左手] 已设 bot_quota {n} + fill");
    }

    [ConsoleCommand("css_lefthand_status", "把手里的 bot 检测结果打到控制台/聊天框")]
    public void CmdStatus(CCSPlayerController? player, CommandInfo cmd)
    {
        var lines = new StringBuilder();
        lines.Append($"[LeftHand] enabled={Config.Enabled} fieldsOk={_botFieldsOk} alive={_alive.Count} 快照:\n");
        foreach (var c in GetAllCtrls())
        {
            if (c == null || !c.IsValid) continue;
            var pawn = c.PlayerPawn.Value;
            bool hasPtr = false;
            try { hasPtr = pawn != null && pawn.IsValid && pawn.Bot != null; } catch { }
            lines.Append($"   slot={c.Slot} team={c.TeamNum} alive={(c.PawnIsAlive ? 1 : 0)} isBot={(c.IsBot ? 1 : 0)} pBot={(hasPtr ? 1 : 0)} {c.PlayerName}\n");
        }
        Server.PrintToChatAll("[LeftHand] status 已输出到服务器控制台");
        Console.WriteLine(lines.ToString());
        cmd.ReplyToCommand("[LeftHand] 已输出玩家快照到服务器控制台");
    }

    // ================= 主循环 =================
    private void OnTick()
    {
        if (!Config.Enabled) return;
        float now = Server.CurrentTime;
        float dt = now - _lastTick;
        _lastTick = now;
        if (dt <= 0f || dt > 0.1f) dt = 1f / 64f;

        BuildSnapshots();
        foreach (var me in _alive)
        {
            // 有些 bot 框架会给 bot 套真实玩家的 Steam 档案，此时 IsBot 可能为 false，
            // 但 m_pBot 指针一定非空，所以两者取或。
            if (!me.Ctrl.IsBot && me.Pawn.Bot == null) continue;
            if (!_states.TryGetValue(me.Slot, out var st)) { st = new BotState(); _states[me.Slot] = st; }
            try { ThinkBot(me, st, now, dt); }
            catch (Exception ex)
            {
                if (_diagErr.Length < 3000)
                    _diagErr.Append($"ERR slot={me.Slot} {ex.GetType().Name}: {ex.Message}\n");
                if (Config.Debug) Console.WriteLine($"[LeftHand] {me.Ctrl.PlayerName}: {ex.Message}");
                // 相位流水里也记一笔：能对上"序列断在哪一拍、为什么断"
                try
                {
                    File.AppendAllText(Path.Combine(ModuleDirectory, "LeftHand-phase.log"),
                        $"{DateTime.Now:HH:mm:ss.fff} slot={me.Slot} ERR-in-{st.Phase} {ex.GetType().Name}: {ex.Message}\n");
                }
                catch { }
                st.Reset();
            }
        }

        // ---------- 诊断落盘：每秒一行 ----------
        if (now >= _diagNext)
        {
            _diagNext = now + 1f;
            try
            {
                _diagBots.Clear();
                int total = 0, bots = 0, withTgt = 0, sees = 0, botNull = 0, m1 = 0, m2 = 0;
                var idxToSlot = new Dictionary<uint, int>();
                foreach (var s in _alive) idxToSlot[s.Pawn.Index] = s.Slot;
                foreach (var c in GetAllCtrls())
                {
                    if (c == null || !c.IsValid) continue;
                    total++;
                    var pawn = c.PlayerPawn.Value;
                    bool alive = c.PawnIsAlive && pawn != null && pawn.IsValid;
                    var isBot = c.IsBot;
                    bool hasPtr = false;
                    try { hasPtr = pawn != null && pawn.IsValid && pawn.Bot != null; } catch { }
                    if (isBot || hasPtr) bots++;
                    var bs = _states.TryGetValue(c.Slot, out var t) ? t : null;
                    if (bs != null)
                    {
                        if (bs.DbgBotNull) botNull++;
                        if (bs.DbgTarget) withTgt++;
                        if (bs.DbgSees) sees++;
                        if (bs.PeekPlanned) { if (bs.Mode2) m2++; else m1++; }
                    }
                    _diagBots.Append($" [{c.Slot}|T{c.TeamNum}|alive={(alive ? 1 : 0)}|isBot={(isBot ? 1 : 0)}|pBot={(hasPtr ? 1 : 0)}|{c.PlayerName}");
                    // bot 自己锁的敌人是谁（按 player slot 显示，-1 表示没有，? 表示是我们枚举外的实体）
                    if (hasPtr)
                    {
                        try
                        {
                            uint h = Schema.GetRef<uint>(pawn!.Bot!.Handle, "CCSBot", "m_enemy");
                            if (h == uint.MaxValue) _diagBots.Append("|own=-1");
                            else
                            {
                                int oslot = idxToSlot.TryGetValue((uint)(h & 0x7FFF), out var sl) ? sl : -2;
                                _diagBots.Append($"|own={(oslot == -2 ? "?" : oslot.ToString())}");
                            }
                        }
                        catch { _diagBots.Append("|own=!"); }
                    }
                    if (bs != null) _diagBots.Append($"|{bs.Phase}|tgt={bs.TargetSlot}|vis={(bs.DbgSees ? 1 : 0)}|ha={(bs.DbgHeadAim ? 1 : 0)}|sp={(int)bs.DbgSpeed}");
                    else _diagBots.Append("|noState");
                    // 只读诊断：BotAimImprover 到底把瞄准部位设在多高。
                    // dz≈60~72 = 瞄头（pawn 原点 + 视高）；dz≈30~45 = 瞄胸/身体。
                    if (hasPtr && alive)
                    {
                        try
                        {
                            var tp = Schema.GetDeclaredClass<Vector>(pawn!.Bot!.Handle, "CCSBot", "m_targetSpot");
                            float dz = tp.Z - pawn.AbsOrigin.Z;
                            _diagBots.Append($"|dz={(int)dz}");
                        }
                        catch { _diagBots.Append("|dz=!"); }
                    }
                    _diagBots.Append("]");
                }
                var sb = new StringBuilder();
                sb.Append($"[{DateTime.Now:HH:mm:ss}] total={total} bots={bots} alive={_alive.Count} tgt={withTgt} sees={sees} writes={_writeCount} botPtrNull={botNull} fieldsOk={_botFieldsOk} enabled={Config.Enabled}\n");
                if (_diagBots.Length > 0) sb.Append("   ").Append(_diagBots).Append('\n');
                if (_diagErr.Length > 0) sb.Append("   ").Append(_diagErr.Replace("\n", "\n   ")).Append('\n');
                try
                {
                    int entCtrl = 0, entPawn = 0;
                    foreach (var e in Utilities.GetAllEntities())
                    {
                        var dn = e.DesignerName;
                        if (dn.Contains("player_controller")) entCtrl++;
                        else if (dn == "player") entPawn++;
                    }
                    var q = ConVar.Find("bot_quota");
                    var qm = ConVar.Find("bot_quota_mode");
                    var stop = ConVar.Find("bot_stop");
                    var diff = ConVar.Find("bot_difficulty");
                    sb.Append($"   cvars: bot_quota={(q == null ? "null" : q.StringValue)} bot_quota_mode={(qm == null ? "null" : qm.StringValue)} bot_stop={(stop == null ? "null" : stop.StringValue)} bot_difficulty={(diff == null ? "null" : diff.StringValue)} map={Server.MapName} | entCtrl={entCtrl} entPawn={entPawn}\n");
                }
                catch (Exception ex) { sb.Append($"   cvars ERR: {ex.GetType().Name}\n"); }
                File.AppendAllText(Path.Combine(ModuleDirectory, "LeftHand-debug.log"), sb.ToString());

                // 聊天框状态行：由 css_lefthand_chat 一键开关
                if (_chatOn && now >= _diagChatNext)
                {
                    _diagChatNext = now + 5f;
                    Server.PrintToChatAll($" \x01[左手] bot={bots}/{total} 参战={_alive.Count} 目标={withTgt} 可见={sees} 模式1={m1} 模式2={m2} 字段={(_botFieldsOk ? "OK" : "失效")}");
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// CS# 的 Utilities.GetPlayers() 在本机 listen server 上只返回真人（bot 被过滤掉），
    /// 这里直接遍历实体层把 bot 控制器也捞出来。
    /// </summary>
    private static List<CCSPlayerController> GetAllCtrls()
    {
        var list = new List<CCSPlayerController>();
        try
        {
            foreach (var e in Utilities.GetAllEntities())
            {
                if (e == null) continue;
                if (!e.DesignerName.Contains("player_controller")) continue;
                try
                {
                    var c = new CCSPlayerController(e.Handle);
                    if (c != null && c.IsValid) list.Add(c);
                }
                catch { }
            }
        }
        catch { }
        return list;
    }

    private void BuildSnapshots()
    {
        _alive.Clear();
        foreach (var c in GetAllCtrls())
        {
            if (c == null || !c.IsValid || !c.PawnIsAlive) continue;
            if (c.TeamNum != 2 && c.TeamNum != 3) continue;
            var pawn = c.PlayerPawn.Value;
            if (pawn == null || !pawn.IsValid) continue;
            var o = pawn.AbsOrigin;
            if (o == null) continue;
            var v = pawn.AbsVelocity;
            float eye = pawn.ViewOffset.Z;
            if (eye < 10f) eye = 64f;
            _alive.Add(new Snap
            {
                Ctrl = c, Pawn = pawn, Slot = c.Slot, Team = c.TeamNum,
                X = o.X, Y = o.Y, Z = o.Z, VX = v.X, VY = v.Y, VZ = v.Z, EyeOff = eye,
                Yaw = pawn.EyeAngles.Y, Pitch = pawn.EyeAngles.X,
                OnGround = (pawn.Flags & 1u) != 0 // FL_ONGROUND
            });
        }
    }

    private Snap? FindSlot(int slot)
    {
        foreach (var s in _alive) if (s.Slot == slot) return s;
        return null;
    }

    private void ThinkBot(Snap me, BotState st, float now, float dt)
    {
        IntPtr bot = me.Pawn.Bot?.Handle ?? IntPtr.Zero;
        st.DbgBotNull = bot == IntPtr.Zero;

        if (st.Phase == Phase.Cooldown)
        {
            if (now < st.CooldownUntil) { GenericCounterStrafe(me, bot); return; }
            st.Reset();
        }

        bool engaged = st.Phase is not (Phase.None or Phase.Track);
        Snap? tgt;

        if (!engaged)
        {
            // 优先用 bot 自己锁定的敌人：只有瞄同一个人，它的 m_isAimingAtEnemy 才会为真、才会开枪。
            // 看不见时（墙后预瞄）它通常没有 enemy，这时才用我们的隔墙选人逻辑。
            tgt = OwnTarget(me, bot);
            if (tgt == null) tgt = PickTarget(me, bot);
            st.DbgTarget = tgt != null;
            if (tgt == null)
            {
                st.Phase = Phase.None; st.HasAim = false; st.ModeDecided = false;
                GenericCounterStrafe(me, bot);
                return;
            }
            if (st.TargetSlot != tgt.Slot)
            {
                st.TargetSlot = tgt.Slot; st.HasAim = false; st.ModeDecided = false;
            }
            if (st.Phase == Phase.None) { SetPhase(me, st, Phase.Track, now); st.EngageStart = now; }
        }
        else
        {
            tgt = FindSlot(st.TargetSlot);
            if (tgt == null || tgt.Team == me.Team)
            {
                // 目标在拉枪中途死了/消失：这一套动作已经不需要它了（方向早就记在 PeekX/PeekY 里），
                // 直接掐断会导致"缩回掩体就没下文"、蹲拉走一半重来。让它按时间把剩下几拍走完。
                bool seqPhase = st.Phase is Phase.M1Setup or Phase.M1ShoulderOut or Phase.M1Confirm or Phase.M1ShoulderBack
                                or Phase.M1Pause or Phase.M1Swing or Phase.M1Stop
                                or Phase.M2Slide or Phase.M2Crouch;
                if ((st.PeekPlanned || seqPhase) && st.Phase is not (Phase.Fire or Phase.Cooldown)) tgt = null;
                else { EndEngage(me, st, bot, now); return; }
            }
        }

        // bot 自己有没有看见敌人（单向）：隔墙时也要继续把视线摆到他的头上，
        // 这样一拉出来就是"瞄着我头出来的"。
        bool hasTgt = tgt != null;
        bool botSees = hasTgt && Sees(me, tgt!, bot);
        bool visible = hasTgt && (botSees || Sees(tgt!, me, IntPtr.Zero));
        st.DbgSees = botSees;
        st.DbgVisible = visible;
        if (visible) st.LastSeen = now;

        if (st.Phase != Phase.Fire && now - st.EngageStart > Config.EngageTimeout)
        { EndEngage(me, st, bot, now); return; }

        // ---------- 瞄准 ----------
        // 看不见敌人（隔墙）时继续把视线摆到他头上；看得见就交还 AI（否则它判定没在瞄敌人，不开枪）
        bool doAim = !(botSees && Config.PreaimOnlyWhenInvisible);
        float pitch = me.Pitch, yaw = me.Yaw, hx = 0f, hy = 0f, hz = 0f;
        // 瞄点始终算：看得见时不写视角（交还 bot 自己的 AI），但 m_targetSpot 要压到爆头线
        if (hasTgt) ComputeAim(me, tgt!, st, visible, now, dt, out pitch, out yaw, out hx, out hy, out hz);

        // ---------- 移动状态机 ----------
        bool overrideVel = false;
        float vx = 0f, vy = 0f;
        float runSpeed = WeaponSpeed(me.Pawn) * Config.RunSpeedScale;
        float elapsed = now - st.PhaseStart;

        switch (st.Phase)
        {
            case Phase.Track:
                // v1.01：这里不能调 PlanPeek —— 它会在 bot 横移时提前把 PeekPlanned 置真，
                // 于是下面"先站定准备再探"的唯一触发点被跳过，表现就是"有时候压根没准备、看见我才动"。
                bool contact = visible || (st.LastSeen > 0f && now - st.LastSeen <= Config.JustSeenWindow);
                // ---- 没有掩体就直接对枪（v1.01）----
                // bot 自己有视线（botSees = 它有真实 LOS），说明这个位置根本藏不住，
                // 这时候不该再做"隐蔽→准备→peek"那套（那套会把开火权按住，结果就是不开枪）。
                // 交给它自己的 AI 打：原地急停、准星在头上、正常扣扳机。
                if (Config.FightWhenBotSees && botSees && !st.PeekPlanned && st.Phase == Phase.Track)
                {
                    EnterFire(me, st, now);
                    break;
                }
                // 旧版 Track 这段 bot 仍按自己的寻路全速往玩家的方向走，所以玩家先看到它整个人露出来，
                // 然后才看到它做 peek —— 观感是"被我发现了才开始动"。
                // 这里：只要目标已锁定、距离在盲 peek 范围内、这波还没开始动作，就把水平速度直接写成 0，
                // 让它在墙后原地对头（PreaimLookAtSpot），一点身位都不露，动只从 M1Setup 开始。
                if (Config.HoldPositionWhileHidden && !st.PeekPlanned && !visible && tgt != null && me.OnGround)
                {
                    float dxh = tgt.X - me.X, dyh = tgt.Y - me.Y;
                    float dh = MathF.Sqrt(dxh * dxh + dyh * dyh);
                    if (dh <= Config.BlindPeekMaxDistance)
                    {
                        overrideVel = true; vx = 0f; vy = 0f;
                    }
                }
                // ---- 墙后盲 peek（v1.01，玩家要求）----
                // bot 隔墙就已经知道人在哪（PreaimMaxDistance 的隔墙选人 + 隔墙瞄头）。
                // 关键：**不要求"玩家看得见我"**，也不要求 bot 自己有视线 —— 只要距离合适就先站定准备再探，
                // 这样每一波都是"先在墙边站好、再探出"，而不是"看到你了才开始动"。
                if (Config.BlindPeek && !st.PeekPlanned && tgt != null && now >= st.PeekCooldownUntil)
                {
                    float dxb = tgt.X - me.X, dyb = tgt.Y - me.Y;
                    float db = MathF.Sqrt(dxb * dxb + dyb * dyb);
                    if (db >= Config.PeekBaitMinDistance && db <= Config.BlindPeekMaxDistance &&
                        now - st.EngageStart >= Config.BlindPeekDelay &&
                        PlanPeekAtContact(me, tgt, st))
                    {
                        if (st.Mode2) { st.Crouch = false; SetPhase(me, st, Phase.M2Slide, now); }
                        // 模式1：先在墙前站定"准备好"（PeekSetup 那拍），再探出 —— 不是看见你了才动作
                        else { st.PauseDur = Config.PeekSetupTime; SetPhase(me, st, Phase.M1Setup, now); }
                        break;
                    }
                }
                // 蹲拉（21:51 那版机制）：还没照面时沿着自己的寻路方向用满速往外走，
                // 一露头就立刻蹲下滑一步 —— 玩家看到的是"蹲着滑出来的"。
                if (!contact && st.PeekPlanned && st.Mode2 && me.OnGround)
                {
                    float sp = me.Speed2D;
                    if (sp > 1f && sp < runSpeed)
                    {
                        overrideVel = true; vx = me.VX / sp * runSpeed; vy = me.VY / sp * runSpeed;
                    }
                }
                if (contact)
                {
                    if (!st.PeekPlanned)
                    {
                        // 第一次照面且距离够远：按随机模式先做一次诱枪/蹲拉再开火。
                        // 但同一波对枪只拉一次：冷却没到就直接开打，不再 repeat。
                        bool cooled = now >= st.PeekCooldownUntil;
                        bool baited = false;
                        if (Config.BaitOnFirstContact && cooled && tgt != null)
                        {
                            float dx0 = tgt.X - me.X, dy0 = tgt.Y - me.Y;
                            float dist0 = MathF.Sqrt(dx0 * dx0 + dy0 * dy0);
                            if (dist0 >= Config.PeekBaitMinDistance && PlanPeekAtContact(me, tgt, st))
                            {
                                baited = true;
                                if (st.Mode2) { st.Crouch = false; SetPhase(me, st, Phase.M2Slide, now); } // 先站着拉，一会儿才蹲
                                else { st.PauseDur = Config.PeekSetupTime; SetPhase(me, st, Phase.M1Setup, now); }
                            }
                        }
                        if (!baited) EnterFire(me, st, now);
                    }
                    else if (now < st.PeekCooldownUntil) { EnterFire(me, st, now); } // 冷却中，直接打
                    else if (st.Mode2) { st.Crouch = false; SetPhase(me, st, Phase.M2Slide, now); } // 先站着拉，一会儿才蹲
                    // 对手没在瞄我也照拉：拉枪只需要"接敌"，不需要"它正在瞄我"。
                    // 只有刚照面的极短窗口（还在判断要不要拉）才看这个条件。
                    else if (!Config.BaitOnlyIfEnemyAiming || IsAimingAt(tgt, me) || now - st.EngageStart >= 0.35f)
                    { st.PauseDur = Config.PeekSetupTime; SetPhase(me, st, Phase.M1Setup, now); }
                    else EnterFire(me, st, now); // 对手没瞄我，直接打
                }
                else if (st.PeekPlanned && st.Mode2 && me.OnGround)
                {
                    // 模式2：沿寻路方向拉满地速
                    float sp = me.Speed2D;
                    if (sp > 1f && sp < runSpeed)
                    {
                        overrideVel = true; vx = me.VX / sp * runSpeed; vy = me.VY / sp * runSpeed;
                    }
                }
                break;

            case Phase.M1Setup: // 墙前准备：先站定、刹住、把准星压到对手头上，再从掩体后探出去
                overrideVel = me.OnGround;
                if (me.OnGround) { vx = me.VX * 0.15f; vy = me.VY * 0.15f; }   // 刹停，站在掩体前"就位"
                if (elapsed >= st.PauseDur) SetPhase(me, st, Phase.M1ShoulderOut, now);
                break;

            case Phase.M1ShoulderOut: // 探出约1/3身位（加速度起步，别一帧弹出）
                {
                    float r = Math.Clamp(elapsed / MathF.Max(0.02f, Config.ShoulderOutTime), 0.35f, 1f);
                    overrideVel = true; vx = st.PeekX * Config.ShoulderSpeed * r; vy = st.PeekY * Config.ShoulderSpeed * r;
                    if (elapsed >= Config.ShoulderOutTime)
                    {
                        st.PauseDur = Lerp(Config.ConfirmHoldMin, Config.ConfirmHoldMax, (float)_rng.NextDouble());
                        SetPhase(me, st, Phase.M1Confirm, now);
                    }
                }
                break;

            case Phase.M1Confirm: // 停在探出的位置，用这一小段时间确认对手在哪、把准星压在爆头线
                overrideVel = me.OnGround;
                if (me.OnGround) { vx = me.VX * 0.35f; vy = me.VY * 0.35f; }
                if (elapsed >= st.PauseDur) SetPhase(me, st, Phase.M1ShoulderBack, now);
                break;

            case Phase.M1ShoulderBack: // 收回，骗枪：必须真正退回掩体后面，所以不能减速
                overrideVel = true;
                vx = -st.PeekX * Config.ShoulderSpeed;
                vy = -st.PeekY * Config.ShoulderSpeed;
                if (elapsed >= Config.ShoulderBackTime)
                {
                    st.PauseDur = Lerp(Config.BaitPauseMin, Config.BaitPauseMax, (float)_rng.NextDouble());
                    SetPhase(me, st, Phase.M1Pause, now);
                }
                break;

            case Phase.M1Pause: // 墙后停顿，等对手空枪/松准星
                overrideVel = me.OnGround;
                if (me.OnGround) { vx = me.VX * 0.55f; vy = me.VY * 0.55f; }   // 缓停，不要瞬间钉住
                if (elapsed >= st.PauseDur) SetPhase(me, st, Phase.M1Swing, now);
                break;

            case Phase.M1Swing: // 大拉：从 0 加速到满速，避免看起来像瞬移
                {
                    float r = Math.Clamp(elapsed / MathF.Max(0.03f, Config.SwingRampTime), 0.2f, 1f);
                    float swingSpeed = Config.SwingBaseSpeed * Config.SwingSpeedScale * Config.RunSpeedScale;
                    overrideVel = true; vx = st.PeekX * swingSpeed * r; vy = st.PeekY * swingSpeed * r;
                    float travelled = MathF.Sqrt((me.X - st.SwingX0) * (me.X - st.SwingX0) + (me.Y - st.SwingY0) * (me.Y - st.SwingY0));

                    // ---- v1.01 玩家要的"大拉" ----
                    // 1) 拉出去一看见对手（有视线）就记录这一刻，此时不停、继续跑，AI 边跑边开第一枪
                    if (visible && st.SwingSightAt <= 0f) st.SwingSightAt = now;
                    // 2) 看见之后再往外拉 SwingPostMove 秒（0.2~0.3s），并把身位拉到 2/3 个
                    // 3) 两个条件都满足才急停，停稳 0.06s 后再开火（这一枪才是"急停爆头"）
                    bool postShotDone = st.SwingSightAt > 0f && (now - st.SwingSightAt) >= st.SwingPostMove;
                    if (postShotDone && travelled >= Config.SwingFullTravel)
                    {
                        st.PauseDur = Config.StopSettleTime;
                        SetPhase(me, st, Phase.M1Stop, now);
                    }
                    else if (st.SwingSightAt > 0f && (now - st.SwingSightAt) >= st.SwingPostMove + 0.25f)
                    {
                        // 兜底：看见很久了还被墙/人卡住拉不动，别再傻站外面送枪
                        st.PauseDur = Config.StopSettleTime;
                        SetPhase(me, st, Phase.M1Stop, now);
                    }
                    else if (elapsed >= Config.SwingMaxTime)
                    {
                        if (Config.M1FireAfterSwing) EnterFireFromPeek(me, st, now);
                        else { st.PauseDur = Config.StopSettleTime; SetPhase(me, st, Phase.M1Stop, now); }
                    }
                }
                break;

            case Phase.M1Stop: // 急停：模式1 是纯骗枪，停稳后把开火权交回 bot 自己的 AI（打头/压枪由 BotAimImprover 负责）
                overrideVel = me.OnGround;
                vx = 0f; vy = 0f;
                if (elapsed >= st.PauseDur)
                {
                    if (Config.M1FireAfterSwing) EnterFireFromPeek(me, st, now);
                    else EndEngage(me, st, bot, now);   // 骗完就撒手，不替它开枪
                }
                break;

            case Phase.M2Slide: // 模式2 蹲拉（回退到 21:51 那版的机制）：露头那一刻就蹲下，借惯性滑一步再打
                st.Crouch = true;
                overrideVel = true;
                vx = me.VX * Config.CrouchSlideDecay;
                vy = me.VY * Config.CrouchSlideDecay;
                if (elapsed >= Config.CrouchSlideTime) EnterFireFromPeek(me, st, now);
                break;

            case Phase.M2Crouch: // 第二阶段：边蹲边滑 —— 对手看到"站→蹲"这个动作，同时还在往外出
                {
                    overrideVel = true;
                    vx = st.PeekX * runSpeed * Config.M2CrouchSpeedScale;
                    vy = st.PeekY * runSpeed * Config.M2CrouchSpeedScale;
                    float m2travel = MathF.Sqrt((me.X - st.SwingX0) * (me.X - st.SwingX0) + (me.Y - st.SwingY0) * (me.Y - st.SwingY0));
                    // 必须"先拉出来再开火"：滑到时间且身位够了才允许急停开火
                    if (elapsed >= Config.M2CrouchTime && m2travel >= Config.M2MinTravel)
                    {
                        overrideVel = me.OnGround;
                        vx = 0f; vy = 0f;
                        if (elapsed >= Config.M2CrouchTime + Config.StopSettleTime) EnterFireFromPeek(me, st, now);
                    }
                    else if (elapsed >= Config.M2CrouchTime + Config.M2MaxExtraTime)
                    {
                        // 兜底：被墙卡住拉不动也不能一直滑
                        overrideVel = me.OnGround;
                        vx = 0f; vy = 0f;
                        if (elapsed >= Config.M2CrouchTime + Config.M2MaxExtraTime + Config.StopSettleTime)
                            EnterFireFromPeek(me, st, now);
                    }
                }
                break;

            case Phase.Fire: // 急停射击
                if (Config.CounterStrafe && me.OnGround && WeaponAllowsStop(me.Pawn))
                {
                    overrideVel = true; vx = me.VX * Config.CounterStrafeFactor; vy = me.VY * Config.CounterStrafeFactor;
                }
                if (st.Mode2 && now - st.FireStart > Config.CrouchHoldMax) st.Crouch = false;
                if ((!visible && now - st.LastSeen > Config.LostSightTime) || now - st.FireStart > Config.FireHoldMax)
                {
                    EndEngage(me, st, bot, now); return;
                }
                break;
        }

        // ---------- 写入 ----------
        // 模式2：滑出阶段全程盯住敌人；开火后头 M2ForceLookTime 秒也继续盯着，之后就交还 bot
        bool forceLook = Config.M2ForceLook && st.Mode2 && (st.Phase == Phase.M2Slide || st.Phase == Phase.M2Crouch);
        if (Config.FireForceLook && st.Phase == Phase.Fire)
        {
            float hold = st.Mode2 ? Config.M2ForceLookTime : Config.FireForceLookTime;
            if (now - st.FireStart <= hold) forceLook = true;
        }
        // 瞄准完全交还 bot / BotAimImprover：这里不再做任何后坐力补偿
        // 蹲拉/大拉这段不光要"朝向对着人"，还要把瞄准部位压到人身上，否则它只是"头转过来、准星还朝外面"
        bool aimSpot = forceLook && st.Phase != Phase.Fire && Config.M2AimAtEnemy &&
                       (MathF.Abs(hx) + MathF.Abs(hy) + MathF.Abs(hz) > 0.01f);
        _headAimFlag = false;
        WriteBot(bot, pitch, yaw, hx, hy, hz, st.Crouch, doAim, forceLook, aimSpot);
        // 偷看那几拍不许开火：bot 自己的 AI 一有视线就会扣扳机，
        // 这里把它的"正在瞄准敌人"标志按住，只骗枪、不还手。
        if (Config.SuppressAimDuringPeek && bot != IntPtr.Zero && _botFieldsOk &&
            (st.Phase == Phase.M1Setup || st.Phase == Phase.M1ShoulderOut || st.Phase == Phase.M1Confirm ||
             st.Phase == Phase.M1ShoulderBack || st.Phase == Phase.M1Pause))
        {
            try { Schema.GetRef<bool>(bot, "CCSBot", "m_isAimingAtEnemy") = false; }
            catch { BotFieldsFailed(); }
            // 步枪抢枪：连"这一帧的敌人可见"一起按掉，AI 就不会扣扳机；下一帧它自己会重新算回来
            if (Config.SuppressVisibleDuringPeek)
            {
                try { Schema.GetRef<bool>(bot, "CCSBot", "m_isEnemyVisible") = false; }
                catch { BotFieldsFailed(); }
            }
        }
        st.DbgHeadAim = _headAimFlag;
        st.DbgSpeed = overrideVel ? MathF.Sqrt(vx * vx + vy * vy) : me.Speed2D;
        // 朝向只在显式开启时才拧，否则保持 bot 自己的视角（BotAimImprover 要用）
        bool bodyAngles = (doAim && Config.WriteBodyAngles);
        // 偷看/蹲拉那几拍：身体必须正对敌人。侧步会让模型侧着朝人，看起来不像在偷瞄。
        bool peekFace = Config.ForceFaceDuringPeek && hasTgt &&
                        (st.Phase == Phase.M1ShoulderOut || st.Phase == Phase.M1Confirm ||
                         st.Phase == Phase.M1ShoulderBack || st.Phase == Phase.M1Pause ||
                         st.Phase == Phase.M1Swing || st.Phase == Phase.M2Slide || st.Phase == Phase.M2Crouch);
        if (peekFace)
        {
            float fdx = tgt!.X - me.X, fdy = tgt.Y - me.Y;
            yaw = MathF.Atan2(fdy, fdx) * R2D;
            pitch = Math.Clamp(-MathF.Atan2(tgt.Z + tgt.EyeOff - me.Z, MathF.Max(1f, MathF.Sqrt(fdx * fdx + fdy * fdy))) * R2D, -89f, 89f);
            bodyAngles = true;
        }
        if (bodyAngles) { _ang.X = pitch; _ang.Y = yaw; _ang.Z = 0f; }
        else { _ang.X = me.Pitch; _ang.Y = me.Yaw; _ang.Z = 0f; }   // 只为了让 Teleport 参数合法，值就是它当前视角
        if (overrideVel) { _vel.X = vx; _vel.Y = vy; _vel.Z = me.VZ; }
        // 关键：CS# 的 Teleport 在 position/angles 全为 null 时直接抛
        // ArgumentException("At least one parameter must be specified")，速度根本写不进去。
        // 所以这里始终把 angles 传进去（不是想改视角，而是让这一帧的速度真正生效）。
        if (bodyAngles || overrideVel)
        {
            try { me.Pawn.Teleport(null, _ang, overrideVel ? _vel : null); }
            catch (Exception ex) { _diagErr.Append($"\nSLOT {me.Slot} Teleport: {ex.GetType().Name}: {ex.Message}"); }
        }
    }

    /// <summary>
    /// 取 bot 自己锁定的敌人（m_enemy 是实体 handle，低 15 位是索引）。
    /// 只有我们瞄的目标和它自己锁的一致，它的 m_isAimingAtEnemy 才会为真、才会开火。
    /// </summary>
    private Snap? OwnTarget(Snap me, IntPtr bot)
    {
        if (bot == IntPtr.Zero || !_botFieldsOk) return null;
        try
        {
            uint h = Schema.GetRef<uint>(bot, "CCSBot", "m_enemy");
            if (h == uint.MaxValue) return null;
            int idx = (int)(h & 0x7FFF);
            foreach (var s in _alive)
            {
                if (s.Team == me.Team) continue;
                if (s.Pawn.Index != (uint)idx) continue;
                float dx = s.X - me.X, dy = s.Y - me.Y;
                if (MathF.Sqrt(dx * dx + dy * dy) > Config.PreaimMaxDistance) return null;
                return s;
            }
        }
        catch { BotFieldsFailed(); }
        return null;
    }

    // ================= 目标选择（隔墙范围限定） =================
    private Snap? PickTarget(Snap me, IntPtr bot)
    {
        Snap? best = null;
        float bestScore = float.MaxValue;
        float msp = me.Speed2D;
        float cosHalf = MathF.Cos(Config.PreaimFrontConeDeg * 0.5f / R2D);
        float T = Config.PreaimContactTime;

        foreach (var e in _alive)
        {
            if (e.Team == me.Team) continue;
            if (MathF.Abs(e.Z - me.Z) > Config.PreaimMaxHeightDiff) continue;
            float dx = e.X - me.X, dy = e.Y - me.Y;
            float d = MathF.Sqrt(dx * dx + dy * dy);
            if (d < 1f || d > Config.PreaimMaxDistance) continue;

            bool vis = Sees(me, e, bot) || Sees(e, me, IntPtr.Zero);
            float ux = dx / d, uy = dy / d;

            if (!vis && d > Config.PreaimHardRadius)
            {
                // 站桩（架点）的 bot 用"当前朝向"判断正面，移动中的 bot 用速度方向，
                // 并且用更宽的锥角，这样它才会把墙后静止的你当成预瞄对象。
                float dirX, dirY, cone;
                if (msp > 60f) { dirX = me.VX / msp; dirY = me.VY / msp; cone = Config.PreaimFrontConeDeg; }
                else { float yr = me.Yaw / R2D; dirX = MathF.Cos(yr); dirY = MathF.Sin(yr); cone = Config.HolderPreaimConeDeg; }
                float cos = dirX * ux + dirY * uy;
                if (cos < MathF.Cos(cone * 0.5f / R2D)) continue;
            }

            float pdx = (e.X + e.VX * T) - (me.X + me.VX * T);
            float pdy = (e.Y + e.VY * T) - (me.Y + me.VY * T);
            float pd = MathF.Sqrt(pdx * pdx + pdy * pdy);
            bool moving = msp > 80f || e.Speed2D > 80f;
            bool soon = vis || d <= Config.PreaimHardRadius ||
                        (pd <= Config.PreaimEngageDistance && (pd < d - 15f || moving)) ||
                        (Config.TrackStaticTargets && d <= Config.PreaimMaxDistance);
            if (!soon) continue;

            float score = vis ? pd * 0.5f : pd;
            if (score < bestScore) { bestScore = score; best = e; }
        }
        return best;
    }

    // ================= 拉枪规划 =================

    /// <summary>
    /// 第一次照面（已经看得见）时也规划一次拉枪：不看横向速度，直接取垂直连线方向。
    /// 有横向速度就用它的方向，没有就随机一侧，这样站着相遇也会做出 shoulder/大拉。
    /// </summary>
    private bool PlanPeekAtContact(Snap me, Snap tgt, BotState st)
    {
        float dx = tgt.X - me.X, dy = tgt.Y - me.Y;
        float d = MathF.Sqrt(dx * dx + dy * dy);
        if (d < 1f) return false;
        float ux = dx / d, uy = dy / d;
        float px = -uy, py = ux;
        float sp = me.Speed2D;
        float lat = sp > 1f ? (me.VX * px + me.VY * py) / sp : 0f;
        if (lat < 0f) { px = -px; py = -py; }
        else if (MathF.Abs(lat) < 1e-3f && _rng.NextDouble() < 0.5f) { px = -px; py = -py; }

        st.PeekX = px; st.PeekY = py; st.PeekPlanned = true;
        if (!st.ModeDecided)
        {
            st.Mode2 = _rng.NextDouble() < Config.Mode2Chance;
            st.ModeDecided = true;
            if (_chatOn)
                Server.PrintToChatAll($" \x01[左手] {me.Ctrl.PlayerName} -> {(st.Mode2 ? "模式2 蹲拉" : "模式1 peek大拉")}（首次接敌）");
        }
        return true;
    }
    private void PlanPeek(Snap me, Snap tgt, BotState st)
    {
        st.PeekPlanned = false;
        float sp = me.Speed2D;
        if (!me.OnGround) return;
        if (sp < Config.MinMoveSpeedToPeek) return;

        float dx = tgt.X - me.X, dy = tgt.Y - me.Y;
        float d = MathF.Sqrt(dx * dx + dy * dy);
        if (d < 1f) return;
        float ux = dx / d, uy = dy / d;
        float px = -uy, py = ux; // 垂直于敌我连线 = 横拉方向

        float lat = (me.VX * px + me.VY * py) / sp;
        if (MathF.Abs(lat) < Config.MinLateralRatio) return; // 正对着走，不做侧拉
        if (lat < 0) { px = -px; py = -py; }

        st.PeekX = px; st.PeekY = py; st.PeekPlanned = true;

        if (!st.ModeDecided)
        {
            st.Mode2 = _rng.NextDouble() < Config.Mode2Chance;
            st.ModeDecided = true;
            if (_chatOn)
                Server.PrintToChatAll($" \x01[左手] {me.Ctrl.PlayerName} -> {(st.Mode2 ? "模式2 蹲拉" : "模式1 peek大拉")}");
        }
    }

    // ================= 瞄准 =================
    private void ComputeAim(Snap me, Snap tgt, BotState st, bool visible, float now, float dt,
        out float pitch, out float yaw, out float hx, out float hy, out float hz)
    {
        float ex = me.X, ey = me.Y, ez = me.Z + me.EyeOff;
        hx = tgt.X; hy = tgt.Y; hz = tgt.Z + tgt.EyeOff + Config.AimHeadOffsetZ;
        if (!visible) { hx += tgt.VX * Config.PreaimLeadTime; hy += tgt.VY * Config.PreaimLeadTime; }

        float dx = hx - ex, dy = hy - ey, dz = hz - ez;
        float yawT = MathF.Atan2(dy, dx) * R2D;
        float pitchT = -MathF.Atan2(dz, MathF.Sqrt(dx * dx + dy * dy)) * R2D;

        if (!st.HasAim) { st.CurYaw = me.Yaw; st.CurPitch = me.Pitch; st.HasAim = true; }

        if (now >= st.NextJitter)
        {
            st.JitYaw = ((float)_rng.NextDouble() * 2f - 1f) * Config.AimJitterDeg;
            st.JitPitch = ((float)_rng.NextDouble() * 2f - 1f) * Config.AimJitterDeg * 0.5f;
            st.NextJitter = now + 0.2f + (float)_rng.NextDouble() * 0.15f;
        }
        if (!visible) { yawT += st.JitYaw; pitchT += st.JitPitch; }

        float turn = (visible ? Config.AimTurnSpeedVisibleDeg : Config.AimTurnSpeedDeg) * dt;
        st.CurYaw = NormYaw(st.CurYaw + Math.Clamp(NormYaw(yawT - st.CurYaw), -turn, turn));
        st.CurPitch = Math.Clamp(st.CurPitch + Math.Clamp(pitchT - st.CurPitch, -turn, turn), -89f, 89f);
        pitch = st.CurPitch; yaw = st.CurYaw;
    }

    // ================= 工具 =================
    private bool Sees(Snap viewer, Snap target, IntPtr viewerBot)
    {
        // 有 bot 指针时以引擎自己的判定为准：它算的是真实视线（遮挡、高低差都算进去）。
        // 之前优先用 m_bSpottedByMask，那个掩码是"被谁标记过"，隔墙/隔层也会为真，
        // 结果 bot 以为看得见 → 直接开火打墙，而且一进 Track 就跳 Fire、从不做 peek。
        if (viewerBot != IntPtr.Zero && _botFieldsOk)
        {
            try
            {
                bool vis = Schema.GetRef<bool>(viewerBot, "CCSBot", "m_isEnemyVisible");
                uint h = Schema.GetRef<uint>(viewerBot, "CCSBot", "m_enemy");
                if (!vis) return false;
                if (h == uint.MaxValue) return false;
                return (h & 0x7FFF) == target.Pawn.Index;
            }
            catch { BotFieldsFailed(); }
        }

        // 拿不到 bot 指针（比如判断"对方是否看得见我"）时才退回掩码
        try
        {
            var mask = target.Pawn.EntitySpottedState.SpottedByMask;
            int s = viewer.Slot;
            if ((mask[s >> 5] & (1u << (s & 31))) != 0) return true;
        }
        catch { }

        return false;
    }

    private void WriteBot(IntPtr bot, float pitch, float yaw, float hx, float hy, float hz, bool crouch, bool doAim, bool forceLook = false, bool aimSpot = false)
    {
        if (bot == IntPtr.Zero || !_botFieldsOk) return;
        try
        {
            if (doAim)
            {
                // 隔墙锁头：只挪"瞄点"，不碰 bot 的视角值，这样 BotAimImprover 的瞄准仍然接管
                if (Config.PreaimLookAtSpot)
                {
                    _writeCount++;
                    var spot = Schema.GetDeclaredClass<Vector>(bot, "CCSBot", "m_lookAtSpot");
                    spot.X = hx; spot.Y = hy; spot.Z = hz;
                }
                // 隔墙锁头（看得见的那条线）：把"瞄准部位"也压到对手头上，
                // 否则 bot 的枪口/视线仍是它自己 AI 指的地方，看上去就是"没锁我了"。
                if (Config.PreaimTargetSpotWhenHidden)
                {
                    var ts = Schema.GetDeclaredClass<Vector>(bot, "CCSBot", "m_targetSpot");
                    ts.X = hx; ts.Y = hy; ts.Z = hz;
                    _headAimFlag = true;
                }
                if (Config.WriteLookYaw)
                {
                    Schema.GetRef<float>(bot, "CCSBot", "m_lookYaw") = yaw;
                    Schema.GetRef<float>(bot, "CCSBot", "m_lookPitch") = pitch;
                }
            }            // 模式2 滑出阶段强制视线盯住敌人，否则它会朝滑行方向看（只管朝向，不管瞄哪个部位）
            if (forceLook)
            {
                Schema.GetRef<float>(bot, "CCSBot", "m_lookYaw") = yaw;
                Schema.GetRef<float>(bot, "CCSBot", "m_lookPitch") = pitch;
                _headAimFlag = true;
            }
            // 蹲拉/大拉同时把"瞄准部位"压到玩家身上：这是玩家真正看得见的那条视线。
            // 只写角度的话，bot AI 下一帧就用自己的朝向覆盖掉，观感还是"没看着我"。
            if (aimSpot)
            {
                var ts = Schema.GetDeclaredClass<Vector>(bot, "CCSBot", "m_targetSpot");
                ts.X = hx; ts.Y = hy; ts.Z = hz;
                _headAimFlag = true;
            }
            Schema.GetRef<bool>(bot, "CBot", "m_isCrouching") = crouch;
        }
        catch { BotFieldsFailed(); }
    }

    private void GenericCounterStrafe(Snap me, IntPtr bot)
    {
        if (!Config.CounterStrafe || bot == IntPtr.Zero || !_botFieldsOk || !me.OnGround) return;
        try
        {
            bool vis = Schema.GetRef<bool>(bot, "CCSBot", "m_isEnemyVisible");
            bool aiming = Schema.GetRef<bool>(bot, "CCSBot", "m_isAimingAtEnemy");
            if (!vis || !aiming || !WeaponAllowsStop(me.Pawn)) return;
            _vel.X = me.VX * Config.CounterStrafeFactor;
            _vel.Y = me.VY * Config.CounterStrafeFactor;
            _vel.Z = me.VZ;
            _ang.X = me.Pitch; _ang.Y = me.Yaw; _ang.Z = 0f;   // 同上：角度必须给，否则 CS# 直接抛异常
            me.Pawn.Teleport(null, _ang, _vel);
        }
        catch { BotFieldsFailed(); }
    }

    private void EnterFire(Snap me, BotState st, float now) { st.FireStart = now; SetPhase(me, st, Phase.Fire, now); }

    /// <summary>
    /// 一次拉枪序列打完、转开火：消耗掉这次拉枪并进入冷却（PeekCooldown 秒），
    /// 同一波对枪里就不会再第二次 peek/蹲拉。
    /// </summary>
    private void EnterFireFromPeek(Snap me, BotState st, float now)
    {
        st.PeekCooldownUntil = now + Config.PeekCooldown;
        st.PeekPlanned = false;
        EnterFire(me, st, now);
    }

    private void EndEngage(Snap me, BotState st, IntPtr bot, float now)
    {
        if (st.Crouch && bot != IntPtr.Zero && _botFieldsOk)
        {
            try { Schema.GetRef<bool>(bot, "CBot", "m_isCrouching") = false; } catch { BotFieldsFailed(); }
        }
        st.Reset();
        st.Phase = Phase.Cooldown;
        st.CooldownUntil = now + Config.ReengageCooldown;
    }

    private void SetPhase(Snap me, BotState st, Phase p, float now)
    {
        Phase from = st.Phase;
        st.Phase = p; st.PhaseStart = now;
        if (p == Phase.M1Swing || p == Phase.M2Slide)
        {
            st.SwingX0 = me.X; st.SwingY0 = me.Y;   // 记下拉枪起点
            st.SwingSightAt = 0f;                    // 每次大拉重新等"看见对手"那一刻
            st.SwingPostMove = Lerp(Config.SwingPostMoveMin, Config.SwingPostMoveMax, (float)_rng.NextDouble());
        }
        if (_chatOn && p != Phase.Track) { try { Server.PrintToChatAll($" \x01[左手] {me.Ctrl.PlayerName}: {p}"); } catch { } }
        // 相位流水：每次切换落一行，事后能一眼看出序列断在哪一拍
        try
        {
            File.AppendAllText(Path.Combine(ModuleDirectory, "LeftHand-phase.log"),
                $"{DateTime.Now:HH:mm:ss.fff} slot={me.Slot} {from}->{p}\n");
        }
        catch { }
    }

    private void BotFieldsFailed()
    {        if (!_botFieldsOk) return;
        _botFieldsOk = false;
        Console.WriteLine("[LeftHand] 警告：CCSBot 字段读写失败（游戏更新？），已退化为仅 Teleport 控制模式");
    }

    private bool IsAttacker(int team) => team == 2 ? !_bombPlanted : _bombPlanted;

    private bool IsAimingAt(Snap from, Snap to)
    {
        float yawTo = MathF.Atan2(to.Y - from.Y, to.X - from.X) * R2D;
        return MathF.Abs(NormYaw(yawTo - from.Yaw)) < Config.EnemyAimConeDeg;
    }

    private static string WeaponName(CCSPlayerPawn p)
    {
        try { return p.WeaponServices?.ActiveWeapon.Value?.DesignerName ?? ""; } catch { return ""; }
    }

    private static float WeaponSpeed(CCSPlayerPawn p)
    {
        var n = WeaponName(p);
        if (n.Contains("knife") || n.Contains("bayonet")) return 250f;
        return WeaponSpeeds.TryGetValue(n, out var s) ? s : 230f;
    }

    private static bool WeaponAllowsStop(CCSPlayerPawn p)
    {
        var n = WeaponName(p);
        if (n.Length == 0) return false;
        return !(n.Contains("knife") || n.Contains("bayonet") || n.Contains("grenade") || n == "weapon_taser"
                 || n == "weapon_flashbang" || n == "weapon_molotov" || n == "weapon_decoy" || n == "weapon_c4");
    }

    private static bool IsSniper(CCSPlayerPawn p)
    {
        var n = WeaponName(p);
        return n == "weapon_awp" || n == "weapon_ssg08" || n == "weapon_scar20" || n == "weapon_g3sg1";
    }

    private static float NormYaw(float a) { while (a > 180f) a -= 360f; while (a < -180f) a += 360f; return a; }
    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
