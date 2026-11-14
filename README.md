# LeftHand

**Fork / 派生自 [ed0ard/CS2-Bot-Improver](https://github.com/ed0ard/CS2-Bot-Improver)** — 上游提供 bot 控制层（BotController / BotVision / RayTrace），本仓库只在其之上做“拟人化接敌走位”。
Upstream provides the bot-control layer; this repo only adds human-like pre-engagement movement on top of it.

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL%20v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)

**A CounterStrikeSharp plugin that makes CS2 bots peek, bait and crouch-peek like a human instead of standing still or running straight at you.**

[中文说明](#中文说明) · [English](#english)

> Upstream / 上游: **[ed0ard/CS2-Bot-Improver](https://github.com/ed0ard/CS2-Bot-Improver)**
> LeftHand does **not** replace that project — it adds movement choreography on top of it, and is released under the same
> **AGPL-3.0** license. Credits to the original authors. / 本插件在 `git remote upstream` 上绑定该仓库，采用相同的 AGPL-3.0 协议，致谢原作者。

---

## 中文说明

### 它做什么

原版 CS2 bot 在对枪时的移动非常机械：要么站着不动，要么直接朝你拉出来。LeftHand 把这段“接敌前后的走位”接管下来，让 bot 在真正交战前做一段像真人的动作：

**模式 1 — 骗枪四拍（默认 70% 概率）**

1. 从掩体后探出约半个身位（`ShoulderOutTime`），全程**不开火**，只露头看一眼
2. 停 `0.11s` 确认你的位置（`ConfirmHold`）
3. 满速缩回掩体后（`ShoulderBackTime`，比探出多走一点，确保真的退进去）
4. 墙后停 `0.12~0.35s`（`BaitPause`，等你空一枪）
5. 拉出**一整个身位**（`SwingMaxTime` + `SwingMinTravel`），急停 `0.06s` 后开火

**模式 2 — 蹲拉（默认 30% 概率）**

还没照面时沿寻路方向满速往外走；**露头那一刻立刻蹲下**，借残余惯性滑 `0.08s`（`CrouchSlideTime`），然后蹲着开火。

### 关键细节（这一版踩过的坑）

- **偷看那几拍不开火**：bot 的 AI 一有视线就会扣扳机，所以 `M1ShoulderOut / M1Confirm / M1ShoulderBack / M1Pause` 四个相位全程把 `CCSBot.m_isAimingAtEnemy` 按成 `false`（`SuppressAimDuringPeek`）。
- **偷看时身体正对你**：探身是侧步，如果不强制写 yaw，模型会侧着身子看你。`ForceFaceDuringPeek` 在以上相位把 pawn 的身体朝向算成“面朝对手的头”。
- **缩回不能减速**：带减速曲线会少走 30%，人露在掩体外，看起来像“没缩回去”。
- **拉枪中途目标死了**：不要把整套动作清掉，按时间把剩下的拍走完，否则观感就是“缩回掩体后没下文”。
- **`Pawn.Teleport(null, angles, vel)` 三个参数不能同时为 null**，否则抛 `ArgumentException` 被 catch 吞掉，速度完全写不进去（底层坑）。
- **`Utilities.GetPlayers()` 拿不到 bot**，必须 `GetAllEntities()` + `player_controller` 过滤；`m_bSpottedByMask` 判可见会隔墙假阳性。

### 安装

**方式 A：直接下整合包（最快）**

下载 [`dist/LeftHand-v1.0.0.zip`](dist/LeftHand-v1.0.0.zip)，解压后把 `addons/` 和 `configs/` 覆盖到服务器的 `game/csgo/` 目录即可：

```
addons/counterstrikesharp/plugins/LeftHand/LeftHand.dll
configs/plugins/LeftHand/LeftHand.json
```

**方式 B：手动部署**

1. 服务器装好 Metamod + CounterStrikeSharp（本版针对 CS# **1.0.380 / net10.0** 编译）
2. 把 `LeftHand.dll` 放到 `game/csgo/addons/counterstrikesharp/plugins/LeftHand/`
3. 首次加载后会生成 `configs/plugins/LeftHand/LeftHand.json`，可直接用本仓库 `config/LeftHand.json` 覆盖
4. 重启或热重载（`core.json` 里 `PluginHotReloadEnabled=true` 时改 dll 的修改时间即可）

### 从源码构建

```bash
dotnet build -c Release
# 产物: bin/Release/net10.0/LeftHand.dll
```

### 主要参数

| 键 | 默认 | 含义 |
|---|---|---|
| `Mode2Chance` | 0.30 | 用蹲拉的概率（其余为骗枪大拉） |
| `ShoulderSpeed` | 150 | 探身/缩回的水平速度 |
| `ShoulderOutTime` | 0.21 | 探出时长（150×0.21 ≈ 33 单位 ≈ 半个身位） |
| `ConfirmHoldMin/Max` | 0.11 | 探出后停住的时长 |
| `ShoulderBackTime` | 0.25 | 缩回时长（全程满速，确保真的退回掩体） |
| `BaitPauseMin/Max` | 0.12 / 0.35 | 墙后停顿 |
| `SwingSpeedScale` | 0.70 | 大拉速度系数（1.0 = 满速） |
| `SwingMaxTime` | 0.60 | 大拉最长时长 |
| `SwingMinTravel` | 20 | 至少拉出多少单位才算拉到位 |
| `StopSettleTime` | 0.06 | 急停稳枪时间 |
| `CrouchSlideTime` | 0.08 | 蹲拉露头后惯性滑行时长 |
| `CrouchSlideDecay` | 0.82 | 惯性衰减系数 |
| `SuppressAimDuringPeek` | true | 偷看相位禁止开火 |
| `ForceFaceDuringPeek` | true | 偷看/蹲拉时身体正对敌人 |
| `PeekBaitMinDistance` | 60 | 距离大于此值才做骗枪（近了直接开打） |
| `M1FireAfterSwing` | true | 拉满身位后由插件强制开火（false = 只骗枪，交还 AI） |

完整列表见 `config/LeftHand.json` 的注释与 `src/LeftHandPlugin.cs` 的 `LeftHandConfig`。

### 调试

- `plugins/LeftHand/LeftHand-phase.log`：相位流水，每次换相位一行 `时间 slot= 旧->新`，异常记 `ERR-in-<phase>`。**定位“动作为什么不完整”最快的入口。**
- `plugins/LeftHand/LeftHand-debug.log`：每秒一行汇总 + 每只 bot 的 `Phase / tgt / vis / sp / dz`。

### 免责声明

本项目用于**本地/listen server 的 bot 练习与服务器端 bot AI 研究**，与上游项目相同的边界：仅供离线 bot 对局、与朋友的自建私服、以及用于 bot 练习的私人专用服务器。请勿用于任何官方匹配、第三方平台（Faciet/5E 等）或影响其他真人玩家的竞技对局。

许可证：**AGPL-3.0**（与上游一致）。修改后若以网络服务形式提供，需按 AGPL 第 13 条提供对应源码。

---

## English

### What it does

Vanilla CS2 bots move mechanically when they duel: they either stand still or run straight at you. LeftHand takes over the pre-engagement movement so bots behave like a human player:

**Mode 1 — bait peek, four beats (70% of engagements)**

1. Shoulder out about half a body width (`ShoulderOutTime`) — **no shooting**, just a look
2. Hold `0.11s` to confirm your position
3. Retract at full speed (`ShoulderBackTime`, slightly longer than the peek so the bot is really back behind cover)
4. Wait `0.12–0.35s` behind cover (inviting your shot)
5. Swing out a **full body width** (`SwingMaxTime` / `SwingMinTravel`), stop dead for `0.06s`, then fire

**Mode 2 — crouch peek (30%)**

While still out of sight the bot pushes toward you at run speed; the instant it gains line of sight it **ducks**, carries the residual momentum for `0.08s` (`CrouchSlideTime`) and shoots from a crouch.

### Implementation notes (the traps we hit)

- **No shooting during the peek**: bot AI pulls the trigger the moment it has LOS, so `CCSBot.m_isAimingAtEnemy` is forced `false` through `M1ShoulderOut / M1Confirm / M1ShoulderBack / M1Pause` (`SuppressAimDuringPeek`).
- **Face the enemy while peeking**: a shoulder peek is a side-step, so without writing yaw the model turns sideways. `ForceFaceDuringPeek` writes the pawn body angles toward the enemy's head during those phases.
- **Retraction must not decelerate**: a decaying curve loses ~30% of the distance and leaves the bot exposed.
- **Target dying mid-swing**: never wipe the whole sequence; finish the remaining beats on their timers.
- **`Pawn.Teleport(null, angles, vel)` cannot take all-null parameters** (throws `ArgumentException`, silently swallowed → velocity never applies).
- **`Utilities.GetPlayers()` does not return bots**; use `GetAllEntities()` + `player_controller`. `m_bSpottedByMask` gives false positives through walls.

### Install

**A — one-click package (fastest)**

Download [`dist/LeftHand-v1.0.0.zip`](dist/LeftHand-v1.0.0.zip) and merge its `addons/` and `configs/` into the server's `game/csgo/`:

```
addons/counterstrikesharp/plugins/LeftHand/LeftHand.dll
configs/plugins/LeftHand/LeftHand.json
```

**B — manual**

1. Metamod + CounterStrikeSharp on the server (built for CS# **1.0.380 / net10.0**)
2. Drop `LeftHand.dll` into `game/csgo/addons/counterstrikesharp/plugins/LeftHand/`
3. A `configs/plugins/LeftHand/LeftHand.json` is generated on first load — you can replace it with `config/LeftHand.json`
4. Restart, or hot-reload by touching the dll (with `PluginHotReloadEnabled=true`)

### Build

```bash
dotnet build -c Release
# -> bin/Release/net10.0/LeftHand.dll
```

### Debug

- `plugins/LeftHand/LeftHand-phase.log` — phase transitions, one line per change (`time slot= old->new`), errors as `ERR-in-<phase>`.
- `plugins/LeftHand/LeftHand-debug.log` — per-second summary plus `Phase / tgt / vis / sp / dz` per bot.

### Disclaimer

For **local / listen-server bot practice and server-side bot AI research**, under the same boundaries as the upstream project: offline bot matches, private games with friends, and private dedicated servers used for bot play. Do not use it on official matchmaking or third-party platforms (Faceit / 5E), and do not use it against real players in competitive games.

License: **AGPL-3.0** (same as upstream). If you run a modified version as a network service, section 13 requires you to offer the corresponding source.

### Credits

- Upstream: **[ed0ard/CS2-Bot-Improver](https://github.com/ed0ard/CS2-Bot-Improver)** (BotController, BotVision, RayTrace) — this project builds on its bot-control approach and is bound to it via `git remote upstream`.
- CounterStrikeSharp: <https://github.com/rofl0r/counterstrikesharp>
- Aim / recoil behaviour is intentionally left to the bot AI & external aim plugins; LeftHand only choreographs movement.
