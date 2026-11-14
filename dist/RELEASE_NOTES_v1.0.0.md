# LeftHand v1.0.0

拟人化 bot 走位 / Human-like bot movement for CS2 bots (CounterStrikeSharp).

## 中文

首个正式版本，包含两种接敌前的走位：

- **模式 1 — 骗枪四拍**：探出半个身位看一眼（不开火）→ 停 0.11s → 满速缩回掩体 → 墙后等 0.12~0.35s → 拉出一整个身位、急停 0.06s 后开火。
- **模式 2 — 蹲拉**：未照面时沿寻路方向满速接近，露头瞬间蹲下、借惯性滑 0.08s，然后蹲着开火。

细节：偷看相位强制按住 `m_isAimingAtEnemy`（不放枪），并强制身体朝向面对敌人；缩回全程满速保证真的退回掩体；目标中途死亡也会把剩余动作走完。

安装：解压 `LeftHand-v1.0.0.zip`，把 `addons/`、`configs/` 覆盖到 `game/csgo/`。

上游致谢：[ed0ard/CS2-Bot-Improver](https://github.com/ed0ard/CS2-Bot-Improver)。

## English

First public release. Two pre-engagement movement patterns:

- **Mode 1 — bait peek**: shoulder out half a body (no shooting) → hold 0.11s → full-speed retract behind cover → wait 0.12–0.35s → swing out a full body width, stop for 0.06s, then fire.
- **Mode 2 — crouch peek**: approach at run speed while unseen, duck the instant LOS is gained, slide 0.08s on residual momentum, shoot crouched.

Details: `m_isAimingAtEnemy` is held false during the peek beats (no early shots), body yaw is forced toward the enemy, retraction runs at full speed so the bot really returns behind cover, and a dying target no longer cancels the remaining beats.

Install: unzip `LeftHand-v1.0.0.zip`, merge `addons/` and `configs/` into `game/csgo/`.

Credits: [ed0ard/CS2-Bot-Improver](https://github.com/ed0ard/CS2-Bot-Improver).
