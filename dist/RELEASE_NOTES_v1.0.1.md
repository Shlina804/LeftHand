# LeftHand v1.0.1

聚焦"bot 怎么从掩体后面出来跟人对枪"这一件事：拉出来的距离、停住的速度、探头的节奏，以及一堆会让人一眼看穿是机器人的小毛病。

---

## 1. 新增 donk 式急停大拉：身位拉得特别多，停得特别快

这次把所有武器的"出掩体大拉"统一成一套动作，不再受武器跑速影响（以前 AWP 因为跑速低，拉出来明显比 M4 慢半拍，看着很别扭）：

- **拉出速度固定**：`SwingBaseSpeed 240 × SwingSpeedScale 0.86 ≈ 206 u/s`，AWP / 步枪 / 微冲全部一样快。
- **拉出的身位给足**：位移门槛 `SwingFullTravel 120`（约 1.7 个身位）、最低 `SwingMinTravel 90`，配合"看见对手满 `SwingPostMove 0.15s` 才允许进急停"，所以是一口气拉出很远，而不是探头试探。
- **急停又快又准**：拉到位置立刻进入 `M1Stop`，`StopSettleTime 0.05s`（3 帧左右）内把速度压到 0，配合 `CounterStrafe` 反向抵消惯性，落地就是"站死"的状态，第一枪就能压在爆头线。
- **动作顺序**：墙后站定对头 → 探 1/3 身位（不开枪，只骗一枪）→ 缩回 → 墙后停顿 → 整身大拉（边拉边打）→ 急停 → 开火。
- **拉出时正面朝向玩家**：大拉与急停阶段只写一次朝向（`FacedSwing` 每轮只掰一次），既保证"看着我拉出来"，又不会因为每帧和 AI 抢写而让模型抽动。

## 2. 修复一系列影响观感的 bug

- **模型畸形 / 像毒液一样抽动**：根因是每帧反复写 `bodyAngles`、`m_lookYaw`、`m_targetSpot` 与 bot 自己的 AI 抢字段。现在改为"只在必要时写一次"，并且蹲姿只写 `true`、不再每帧强写 `false`。
- **蹲着走跟站着走一样快**：蹲速统一由 `crouchSpeed = 跑速 × M2CrouchSpeedScale(0.40)` 约束，蹲拉惯性滑步也加了 `×1.6` 上限，不会再出现"蹲着却按跑速滑"。
- **"蹲下了但模型没蹲"**：以前只写 AI 的 `CBot.m_isCrouching`，AI 一覆盖模型就站着蹲走。现在同时写 pawn 的 duck 状态（`m_bDucking` / `m_bDucked`），身形是真的矮下去。
- **蹲拉刚蹲下就开枪**：模式2 改为「蹲下 → 蹲着往外走 0.26s（位移 ≥35）→ 急停 0.10s → 开火」，动作看得清，不再原地抖一下。
- **不开枪 / 没掩体不会打**：`FightWhenBotSees` 让 bot 一旦有真实视线就直接进 `Fire`；偷看阶段仍然只骗枪不还手（`SuppressAimDuringPeek`）。
- **选人抖动导致 peek 永远开不了头**：加 `TargetGraceTime 0.4`，短暂丢失目标时原地保持，相位不再在 `None/Track` 之间刷屏。
- **"看得见我才 peek"**：`HoldPositionWhileHidden` 在目标已锁定、还没开始动作时把水平速度写 0，先把位置定死，等墙后对好头再动。

## 3. 探头更快，节奏更密

- 准备时间 `PeekSetupTime 0.14`、探出 `ShoulderOutTime 0.17`、缩回 `ShoulderBackTime 0.26`，整个假动作压到半秒内完成，对手很难在缝隙里抓住开枪时机。
- 确认停顿 `ConfirmHoldMin/Max 0.08`、诱枪停顿 `BaitPauseMin/Max 0.08/0.15`、重交战冷却 `ReengageCooldown 0.15`、盲探延迟 `BlindPeekDelay 0.08`。
- `NoCoverPeekChance 1.0`：即使 bot 已有视线，也 100% 先做一个假动作再打，节奏更像人；配合 `PeekBaitMinDistance 30`，近距离也照样探头。

---

## 升级方式

用 `dist/LeftHand-v1.0.1.zip` 覆盖到服务器：

```
addons/counterstrikesharp/plugins/LeftHand/LeftHand.dll
configs/plugins/LeftHand/LeftHand.json
```

旧的 `LeftHand.json` 建议先备份，再套用新配置（新增了 `Mode2OnlyWhenHidden`、`HoldPositionWhileHidden`、`FightWhenBotSees`、`M2CrouchSpeedScale` 等键）。

## 兼容性

- CounterStrikeSharp 1.0.380 / .NET 10
- 与 `BotController`、`BotHider`、`BotVision`、`dtr-controller`、`RayTrace` 并存无冲突
- 许可：AGPL-3.0（fork 自 `ed0ard/CS2-Bot-Improver`）
