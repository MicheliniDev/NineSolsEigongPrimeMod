using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Mono.Cecil;
using NineSolsAPI;
using RCGMaker.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EigongPrime;

[BepInDependency(NineSolsAPICore.PluginGUID)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class EigongPrime : BaseUnityPlugin {
    private Harmony harmony = null!;

    #region Sprite Recoloring
    private GameObject eigongSpriteHolder = null!;
    private _2dxFX_ColorChange eigongCurrentHueValue = null!;

    private GameObject eigongSwordSpriteHolder = null!;
    private _2dxFX_ColorChange eigongSwordCurrentHueValue = null!;

    private GameObject eigongFooSpriteHolder = null!;
    private _2dxFX_ColorChange eigongFooCurrentHueValue = null!;
    #endregion
    private int dontspamstuffwow2 = 0;
    private int dontspamstuffwow = 0;

    private string eigongAttackStatesPath = "";

    #region Attacks LinkStateWeight
    LinkNextMoveStateWeight StunLinkStateWeight = null!;
    LinkNextMoveStateWeight StunPhase2LinkStateWeight = null!;
    LinkNextMoveStateWeight StunPhase3LinkStateWeight = null!;

    LinkNextMoveStateWeight TeleportToTopLinkStateWeight = null!;
    LinkNextMoveStateWeight TeleportToTopPhase2LinkStateWeight = null!;
    LinkNextMoveStateWeight TeleportToTopPhase3LinkStateWeight = null!;

    LinkNextMoveStateWeight TeleportForwardLinkStateWeight = null!;
    LinkNextMoveStateWeight TeleportForwardPhase2LinkStateWeight = null!;
    LinkNextMoveStateWeight TeleportForwardPhase3LinkStateWeight = null!;

    LinkNextMoveStateWeight TeleportToBackLinkStateWeight = null!;
    LinkNextMoveStateWeight TeleportToBackPhase2LinkStateWeight = null!;
    LinkNextMoveStateWeight TeleportToBackPhase3LinkStateWeight = null!;

    //Phase 1 Attacks
    LinkNextMoveStateWeight SlowStartLinkStateWeight = null!;
    LinkNextMoveStateWeight PokeLinkStateWeight = null!;
    LinkNextMoveStateWeight CrossUpLinkStateWeight = null!;
    LinkNextMoveStateWeight UnsheatheLinkStateWeight = null!;
    LinkNextMoveStateWeight FooCharmLinkStateWeight = null!;
    LinkNextMoveStateWeight CrimsonSlamLinkStateWeight = null!;
    LinkNextMoveStateWeight DunkLinkStateWeight = null!;

    //Phase 2 Attacks
    LinkNextMoveStateWeight SlowStartPhase2LinkStateWeight = null!;
    LinkNextMoveStateWeight PokePhase2LinkStateWeight = null!;
    LinkNextMoveStateWeight CrossUpPhase2LinkStateWeight = null!;
    LinkNextMoveStateWeight CrimsonSlamPhase2LinkStateWeight = null!;
    LinkNextMoveStateWeight FooCharmPhase2LinkStateWeight = null!;
    LinkNextMoveStateWeight FooFollowUp0LinkStateWeight = null!;
    LinkNextMoveStateWeight FooFollowUp1LinkStateWeight = null!;
    LinkNextMoveStateWeight UnsheathePhase2LinkStateWeight = null!;
    LinkNextMoveStateWeight WindBladeLinkStateWeight = null!;

    //Phase 3 Attacks
    LinkNextMoveStateWeight FooCharmPhase3LinkStateWeight = null!;
    LinkNextMoveStateWeight WindBladePhase3LinkStateWeight = null!;
    LinkNextMoveStateWeight UnsheathePhase3LinkStateWeight = null!;
    LinkNextMoveStateWeight JudgementCutLinkStateWeight = null!;
    LinkNextMoveStateWeight RegularJudgementCutLinkStateWeight = null!;
    #endregion

    #region Attacks BossGeneralState
    BossGeneralState stunBossGeneralState = null!;
    //Teleports
    BossGeneralState TeleportToTopBossGeneralState = null!;
    BossGeneralState TeleportForwardBossGeneralState = null!;
    BossGeneralState TeleportToBackBossGeneralState = null!;

    //Phase 1 Attacks
    BossGeneralState SlowStartBossGeneralState = null!;
    BossGeneralState UnsheatheBossGeneralState = null!;
    BossGeneralState CrossUpBossGeneralState = null!;
    BossGeneralState triplePokeBossGeneralState = null!;
    BossGeneralState FooCharmBossGeneralState = null!;
    BossGeneralState QuickFooBossGeneralState = null!;
    BossGeneralState DunkBossGeneralState = null!;
    BossGeneralState CrimsonSlamBossGeneralState = null!;

    //Phase 2 Attacks
    BossGeneralState WindBladeBossGeneralState = null!;
    BossGeneralState FooFollowUp0BossGeneralState = null!;
    BossGeneralState FooFollowUp1BossGeneralState = null!;

    //Phase 3 Attacks
    BossGeneralState JudgementCutBossGeneralState = null!;
    BossGeneralState RegularJudgementCutBossGeneralState = null!;
    #endregion
    public void Awake() {
        Log.Init(Logger);
        RCGLifeCycle.DontDestroyForever(gameObject);
        harmony = Harmony.CreateAndPatchAll(typeof(EigongPrime).Assembly);
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    public void Update() {
        if (SceneManager.GetActiveScene().name == "A11_S0_Boss_YiGung") {
            var baseHealthRef = AccessTools.FieldRefAccess<MonsterStat, float>("BaseHealthValue");
            baseHealthRef(MonsterManager.Instance.ClosetMonster.monsterStat) = 8000f;
            if (dontspamstuffwow == 0 && MonsterManager.Instance.ClosetMonster != null) {
                MonsterManager.Instance.ClosetMonster.postureSystem.CurrentHealthValue = 8000f;
                dontspamstuffwow++;
            }
            MonsterManager.Instance.ClosetMonster.monsterStat.Phase2HealthRatio = 2;
            MonsterManager.Instance.ClosetMonster.monsterStat.Phase3HealthRatio = 1.5f;
            RecolorSprite();
            GetAttackGameObjects();
            AlterAttacks();
        }
    }

    public void AlterAttacks() {
        #region New Attack Weights
        //Teleports
        AttackWeight TeleportToTop = new AttackWeight();
        TeleportToTop.state = TeleportToTopBossGeneralState;
        TeleportToTop.weight = 1;

        AttackWeight TeleportToBack = new AttackWeight();
        TeleportToBack.state = TeleportToBackBossGeneralState;
        TeleportToBack.weight = 1;

        AttackWeight TeleportForward = new AttackWeight();
        TeleportForward.state = TeleportForwardBossGeneralState;
        TeleportForward.weight = 1;

        //Phase1
        AttackWeight SlowStart = new AttackWeight();
        SlowStart.state = SlowStartBossGeneralState;
        SlowStart.weight = 1;

        AttackWeight CrossUp = new AttackWeight();
        CrossUp.state = CrossUpBossGeneralState;
        CrossUp.weight = 1;

        AttackWeight Pokes = new AttackWeight();
        Pokes.state = triplePokeBossGeneralState;
        Pokes.weight = 1;

        AttackWeight CrimsonSlam = new AttackWeight();
        CrimsonSlam.state = CrimsonSlamBossGeneralState;
        CrimsonSlam.weight = 1;

        AttackWeight Unsheathe = new AttackWeight();
        Unsheathe.state = UnsheatheBossGeneralState;
        Unsheathe.weight = 1;

        AttackWeight Foo = new AttackWeight();
        Foo.state = FooCharmBossGeneralState;
        Foo.weight = 1;

        AttackWeight QuickFoo = new AttackWeight();
        QuickFoo.state = QuickFooBossGeneralState;
        QuickFoo.weight = 1;

        AttackWeight Dunk = new AttackWeight();
        Dunk.state = DunkBossGeneralState;
        Dunk.weight = 1;

        //Phase 2
        AttackWeight WindBlade = new AttackWeight();
        WindBlade.state = WindBladeBossGeneralState;
        WindBlade.weight = 1;

        AttackWeight FooGeyser = new AttackWeight();
        FooGeyser.state = FooFollowUp0BossGeneralState;
        FooGeyser.weight = 1;

        AttackWeight FooSpike = new AttackWeight();
        FooSpike.state = FooFollowUp1BossGeneralState;
        FooSpike.weight = 1;

        //Phase 3
        AttackWeight CrimsonJudgementCut = new AttackWeight();
        CrimsonJudgementCut.state = JudgementCutBossGeneralState;
        CrimsonJudgementCut.weight = 1;

        AttackWeight NormalJudgementCut = new AttackWeight();
        NormalJudgementCut.state = RegularJudgementCutBossGeneralState;
        NormalJudgementCut.weight = 1;
        #endregion

        #region Assigning attacks
        var phase1AttackAssignments = new Dictionary<LinkNextMoveStateWeight, AttackWeight[]> {
            //Phase 1
            { StunLinkStateWeight, new AttackWeight[] {TeleportToBack, TeleportToTop, Unsheathe} },
            { TeleportToBackLinkStateWeight, new AttackWeight[] { Unsheathe, SlowStart, CrossUp, Pokes, TeleportToTop} },
            { TeleportForwardLinkStateWeight, new AttackWeight[] { CrossUp, Pokes, TeleportToBack} },
            { SlowStartLinkStateWeight, new AttackWeight[] { Foo, Unsheathe, TeleportToTop} },
            { PokeLinkStateWeight, new AttackWeight[] { Foo, FooGeyser, FooSpike} },
            { CrossUpLinkStateWeight, new AttackWeight[] { Unsheathe, Foo, TeleportForward } },
            { CrimsonSlamLinkStateWeight, new AttackWeight[] { CrossUp, Pokes, Foo, TeleportToTop, Dunk, TeleportToBack, TeleportForward} },
            { FooCharmLinkStateWeight, new AttackWeight[] { Pokes} },
            { UnsheatheLinkStateWeight, new AttackWeight[] { Unsheathe, TeleportToTop, Pokes, CrossUp} },
            { DunkLinkStateWeight, new AttackWeight[] { TeleportToTop, Unsheathe} },
            //Phase 2
            { StunPhase2LinkStateWeight, new AttackWeight[] {QuickFoo, TeleportToTop, FooGeyser, FooSpike, WindBlade, TeleportForward, TeleportToBack} },
            { PokePhase2LinkStateWeight, new AttackWeight[] { Pokes, FooSpike, FooGeyser, Foo, SlowStart, TeleportToBack} },
            { TeleportToBackPhase2LinkStateWeight, new AttackWeight[] { Unsheathe, SlowStart, CrossUp, Pokes, TeleportToTop, WindBlade, FooGeyser, FooSpike } },
            { TeleportForwardPhase2LinkStateWeight, new AttackWeight[] { CrossUp, Pokes, SlowStart, FooGeyser, FooSpike} },
            { SlowStartPhase2LinkStateWeight, new AttackWeight[] { Foo, Unsheathe, TeleportToTop, FooGeyser, FooSpike } },
            { CrossUpPhase2LinkStateWeight, new AttackWeight[] { Unsheathe, Foo, TeleportToBack, TeleportForward, FooSpike } },
            { CrimsonSlamPhase2LinkStateWeight, new AttackWeight[] { CrossUp, Pokes, QuickFoo, TeleportToTop, Dunk, TeleportToBack, TeleportForward, FooGeyser, FooSpike, WindBlade} },
            { FooCharmPhase2LinkStateWeight, new AttackWeight[] { Pokes, CrossUp, SlowStart, TeleportForward} },
            { FooFollowUp0LinkStateWeight, new AttackWeight[] {  Dunk, TeleportToTop, Unsheathe} },
            { FooFollowUp1LinkStateWeight, new AttackWeight[] {TeleportToTop, TeleportToBack, Unsheathe} },
            { UnsheathePhase2LinkStateWeight, new AttackWeight[] { Unsheathe, TeleportToTop, Dunk, Pokes, CrossUp, FooGeyser, FooSpike } },
            { WindBladeLinkStateWeight, new AttackWeight[] { QuickFoo, TeleportToBack} },
            //Phase 3
            { FooCharmPhase3LinkStateWeight, new AttackWeight[] { Pokes, CrossUp, Unsheathe} },
            { StunPhase3LinkStateWeight, new AttackWeight[] { QuickFoo, TeleportToTop, FooGeyser, FooSpike } },
            { TeleportToBackPhase3LinkStateWeight, new AttackWeight[] { Unsheathe, SlowStart, CrossUp, Pokes, TeleportToTop } },
            { TeleportForwardPhase3LinkStateWeight, new AttackWeight[] { CrossUp, Pokes, SlowStart} },
            { WindBladePhase3LinkStateWeight, new AttackWeight[] { QuickFoo, TeleportToBack} },
            { UnsheathePhase3LinkStateWeight, new AttackWeight[] { Unsheathe, TeleportToTop, Dunk, Pokes, CrossUp, SlowStart } },
            { JudgementCutLinkStateWeight, new AttackWeight[] { CrimsonJudgementCut,TeleportToTop, CrossUp, Dunk, FooGeyser, FooSpike } },
            { RegularJudgementCutLinkStateWeight, new AttackWeight[] { CrimsonJudgementCut, TeleportToTop, CrossUp, Dunk, FooGeyser, FooSpike } }
        };

        foreach (var entry in phase1AttackAssignments) {
            LinkNextMoveStateWeight linkWeight = entry.Key;
            AttackWeight[] attacks = entry.Value;
            foreach (AttackWeight attack in attacks) {
                if (!linkWeight.stateWeightList.Contains(attack)) {
                    linkWeight.stateWeightList.Add(attack);
                    Logger.LogInfo($"Added {attack.state} to {linkWeight.transform.parent.gameObject.name}");
                }
            }
        }
        #endregion
    }

    public void GetAttackGameObjects() {
        eigongAttackStatesPath = "GameLevel/Room/Prefab/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/LogicRoot/---Boss---/Boss_Yi Gung/States/Attacks/";

        StunLinkStateWeight = GameObject.Find("GameLevel/Room/Prefab/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/LogicRoot/---Boss---/Boss_Yi Gung/States/AttackParrying/weight").GetComponent<LinkNextMoveStateWeight>();
        StunPhase2LinkStateWeight = GameObject.Find("GameLevel/Room/Prefab/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/LogicRoot/---Boss---/Boss_Yi Gung/States/AttackParrying/weight (1)").GetComponent<LinkNextMoveStateWeight>();
        StunPhase3LinkStateWeight = GameObject.Find("GameLevel/Room/Prefab/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/LogicRoot/---Boss---/Boss_Yi Gung/States/AttackParrying/weight (2)").GetComponent<LinkNextMoveStateWeight>();
        
        #region Attacks LinkStateWeight
        //Teleports
        TeleportToTopLinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[2] Teleport To Top/phase (0)").GetComponent<LinkNextMoveStateWeight>();
        TeleportToTopPhase2LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[2] Teleport To Top/phase (1)").GetComponent<LinkNextMoveStateWeight>();
        TeleportToTopPhase3LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[2] Teleport To Top/phase (2)").GetComponent<LinkNextMoveStateWeight>();
        
        TeleportForwardLinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[7] Teleport Dash Forward/weight").GetComponent<LinkNextMoveStateWeight>();
        TeleportForwardPhase2LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[7] Teleport Dash Forward/weight (1)").GetComponent<LinkNextMoveStateWeight>();
        TeleportForwardPhase3LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[7] Teleport Dash Forward/weight (2)").GetComponent<LinkNextMoveStateWeight>();

        TeleportToBackLinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[5] Teleport to Back/weight").GetComponent<LinkNextMoveStateWeight>();
        TeleportToBackPhase2LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[5] Teleport to Back/weight (1)").GetComponent<LinkNextMoveStateWeight>();
        TeleportToBackPhase3LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[5] Teleport to Back/weight (2)").GetComponent<LinkNextMoveStateWeight>();
        
        //Phase 1 Attacks
        SlowStartLinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[1] Starter  Slow Attack 慢刀前揮/phase (0)").GetComponent<LinkNextMoveStateWeight>();
        PokeLinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[13] Tripple Poke 三連/weight").GetComponent<LinkNextMoveStateWeight>();
        CrossUpLinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[6] Double Attack/LinkMoveGroupingNode2 StarterPose/phase (0)").GetComponent<LinkNextMoveStateWeight>();
        UnsheatheLinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[3] Thrust Delay 一閃/phase (0)").GetComponent<LinkNextMoveStateWeight>();
        FooCharmLinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[10] Danger Foo Grab/interrupt weight").GetComponent<LinkNextMoveStateWeight>();
        CrimsonSlamLinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[17] DownAttack Danger 空中下危/weight").GetComponent<LinkNextMoveStateWeight>();
        DunkLinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[14] FooExplode Smash 下砸紅球/phase0 (1)").GetComponent<LinkNextMoveStateWeight>();

        //Phase 2 Attacks
        SlowStartPhase2LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[1] Starter  Slow Attack 慢刀前揮/phase (1)").GetComponent<LinkNextMoveStateWeight>();
        PokePhase2LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[13] Tripple Poke 三連/weight (1)").GetComponent<LinkNextMoveStateWeight>();
        CrossUpPhase2LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[6] Double Attack/LinkMoveGroupingNode2 StarterPose/phase (1)").GetComponent<LinkNextMoveStateWeight>();
        UnsheathePhase2LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[3] Thrust Delay 一閃/phase (1)").GetComponent<LinkNextMoveStateWeight>();
        FooCharmPhase2LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[10] Danger Foo Grab/interrupt weight (1)").GetComponent<LinkNextMoveStateWeight>();
        FooFollowUp0LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[4] Slash Up 上撈下打 大反危/phase (1)").GetComponent<LinkNextMoveStateWeight>();
        FooFollowUp1LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[12] UpSlash Down Danger/weight (1)").GetComponent<LinkNextMoveStateWeight>();
        CrimsonSlamPhase2LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[17] DownAttack Danger 空中下危/weight (1)").GetComponent<LinkNextMoveStateWeight>();
        WindBladeLinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[11] GiantChargeWave 紅白白紅/weight (2)").GetComponent<LinkNextMoveStateWeight>();

        //Phase 3 Attacks
        FooCharmPhase3LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[10] Danger Foo Grab/interrupt weight (2)").GetComponent<LinkNextMoveStateWeight>();
        WindBladePhase3LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[11] GiantChargeWave 紅白白紅/weight (3)").GetComponent<LinkNextMoveStateWeight>();
        UnsheathePhase3LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[3] Thrust Delay 一閃/phase (2)").GetComponent<LinkNextMoveStateWeight>();
        JudgementCutLinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[19] Thrust Full Screen Slash/LinkMoveGroupingNode Timing 2 Ground/weight (2)").GetComponent<LinkNextMoveStateWeight>();
        RegularJudgementCutLinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[20] TeleportOut/weight (2)").GetComponent<LinkNextMoveStateWeight>();
        #endregion
        
        #region Attacks BossGeneralState
        stunBossGeneralState = GameObject.Find("GameLevel/Room/Prefab/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/LogicRoot/---Boss---/Boss_Yi Gung/States/AttackParrying").GetComponent<BossGeneralState>();
        //Teleports
        TeleportToTopBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[2] Teleport To Top").GetComponent<BossGeneralState>();
        TeleportForwardBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[7] Teleport Dash Forward").GetComponent<BossGeneralState>();
        TeleportToBackBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[5] Teleport to Back").GetComponent<BossGeneralState>();
        
        //Phase 1 Attacks
        SlowStartBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[1] Starter  Slow Attack 慢刀前揮").GetComponent<BossGeneralState>();
        UnsheatheBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[3] Thrust Delay 一閃").GetComponent<BossGeneralState>();
        CrossUpBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[6] Double Attack").GetComponent<BossGeneralState>();
        triplePokeBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[13] Tripple Poke 三連").GetComponent<BossGeneralState>();
        FooCharmBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[10] Danger Foo Grab").GetComponent<BossGeneralState>();
        QuickFooBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[16] QuickFoo").GetComponent<BossGeneralState>();
        DunkBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[14] FooExplode Smash 下砸紅球").GetComponent<BossGeneralState>();
        CrimsonSlamBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[17] DownAttack Danger 空中下危").GetComponent<BossGeneralState>();

        //Phase 2 Attacks
        WindBladeBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[8] Long Charge (2階才有").GetComponent<BossGeneralState>();
        FooFollowUp0BossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[4] Slash Up 上撈下打 大反危").GetComponent<BossGeneralState>();
        FooFollowUp1BossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[12] UpSlash Down Danger").GetComponent<BossGeneralState>();

        //Phase 3 Attacks
        JudgementCutBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[19] Thrust Full Screen Slash").GetComponent<BossGeneralState>();
        RegularJudgementCutBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[20] TeleportOut").GetComponent<BossGeneralState>();
        #endregion
    }

    public void RecolorSprite() {
        if (dontspamstuffwow2 == 0) {
            eigongSpriteHolder = GameObject.Find("GameLevel/Room/Prefab/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/LogicRoot/---Boss---/Boss_Yi Gung/MonsterCore/Animator(Proxy)/Animator/View/YiGung/Body");
            eigongSpriteHolder.AddComponent<_2dxFX_ColorChange>();
            eigongCurrentHueValue = eigongSpriteHolder.GetComponent<_2dxFX_ColorChange>();

            eigongSwordSpriteHolder = GameObject.Find("GameLevel/Room/Prefab/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/LogicRoot/---Boss---/Boss_Yi Gung/MonsterCore/Animator(Proxy)/Animator/View/YiGung/Weapon/Sword/Sword Sprite");
            eigongSwordSpriteHolder.AddComponent<_2dxFX_ColorChange>();
            eigongSwordCurrentHueValue = eigongSwordSpriteHolder.GetComponent<_2dxFX_ColorChange>();

            eigongFooSpriteHolder = GameObject.Find("GameLevel/Room/Prefab/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/LogicRoot/---Boss---/Boss_Yi Gung/MonsterCore/Animator(Proxy)/Animator/View/YiGung/Weapon/Foo/FooSprite");
            eigongFooSpriteHolder.AddComponent<_2dxFX_ColorChange>();
            eigongFooCurrentHueValue = eigongFooSpriteHolder.GetComponent<_2dxFX_ColorChange>();
            dontspamstuffwow2++;
        }
        eigongCurrentHueValue._HueShift = 130;
        eigongSwordCurrentHueValue._HueShift = 250;
        eigongFooCurrentHueValue._HueShift = 250;

        if (Player.i.health.CurrentHealthValue <= 0f) {
            dontspamstuffwow2 = 0;
        }
    }

    public void OnDestroy() {
        harmony.UnpatchSelf();
    }
}