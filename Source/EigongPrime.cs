using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using NineSolsAPI;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx.Configuration;
using UnityEngine.Pool;
using RCGMaker.Test;
using System.Linq;
using System;

namespace EigongPrime;

[BepInDependency(NineSolsAPICore.PluginGUID)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class EigongPrime : BaseUnityPlugin {
    private Harmony harmony = null!;

    private ColorChange colorChange = null!;

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
    LinkNextMoveStateWeight WindBladePhase1LinkStateWeight = null!;

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
    BossGeneralState ActualWindBladeBossGeneralState = null!;
    BossGeneralState FooFollowUp0BossGeneralState = null!;
    BossGeneralState FooFollowUp1BossGeneralState = null!;

    //Phase 3 Attacks
    BossGeneralState JudgementCutBossGeneralState = null!;
    BossGeneralState RegularJudgementCutBossGeneralState = null!;

    private GameObject fireTrail = null!;
    #endregion

    private int dontspamstuffwow = 0;

    private ConfigEntry<float> EigongAnimatorSpeed = null!;
    private ConfigEntry<float> EigongHPScale = null!;
    private ConfigEntry<bool> IsRandom = null!;
    private ConfigEntry<int> IsRandomMinimumAttackAmount = null!;
    private ConfigEntry<int> IsRandomMaximumAttackAmount = null!;

    private bool hasRandomLoopRan = false;
    public void Awake() {
        Log.Init(Logger);
        RCGLifeCycle.DontDestroyForever(gameObject);
        harmony = Harmony.CreateAndPatchAll(typeof(EigongPrime).Assembly);
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        colorChange = new ColorChange();

        EigongAnimatorSpeed = Config.Bind("General", "EigongSpeed", 1f, "The speed at which Eigong's attacks occur");
        EigongHPScale = Config.Bind("General", "EigongHPScale", 1f, "The scale for Eigong's HP, 1 is regular HP amount");
        IsRandom = Config.Bind("General", "IsRandom", false, "If true, randomizes Eigong Prime's moveset every time you enter her arena");
        IsRandomMinimumAttackAmount = Config.Bind("General", "IsRandomMinimumAttackAmount", 1, new ConfigDescription("The minimum amount of follow-ups that can be given to a move when randomized", new AcceptableValueRange<int>(1, 12)));
        IsRandomMaximumAttackAmount = Config.Bind("General", "IsRandomMaximumAttackAmount", 6, new ConfigDescription("The maximum amount of follow-ups that can be given to a move when randomized", new AcceptableValueRange<int>(1, 12)));
    }

    public void Update() {
        if (SceneManager.GetActiveScene().name == "A11_S0_Boss_YiGung" || SceneManager.GetActiveScene().name == "A11_S0_Boss_YiGung_回蓬萊") {
            colorChange.RecolorSprite();
            EigongHPChange();
            GetAttackGameObjects();
            AlterAttacks();

            MonsterManager.Instance.ClosetMonster.monsterCore.AnimationSpeed = EigongAnimatorSpeed.Value;

            if (MonsterManager.Instance.ClosetMonster.currentMonsterState == TeleportForwardBossGeneralState && MonsterManager.Instance.ClosetMonster.PhaseIndex != 0) {
                fireTrail.SetActive(false);
            } else {
                fireTrail.SetActive(true);
            }

            Player.i.PlayerDeadState.OnReviveEvent.AddListener(ResetFlags);
        }
    }

    public void ResetFlags() {
        colorChange.dontspamstuffwow2 = 0;
        hasRandomLoopRan = false;
    }

    private void EigongHPChange() {
        var baseHealthRef = AccessTools.FieldRefAccess<MonsterStat, float>("BaseHealthValue");
        baseHealthRef(MonsterManager.Instance.ClosetMonster.monsterStat) = 7000f * EigongHPScale.Value;
        if (dontspamstuffwow == 0 && MonsterManager.Instance.ClosetMonster != null) {
            if (ApplicationCore.IsInBossMemoryMode) MonsterManager.Instance.ClosetMonster.postureSystem.CurrentHealthValue = 8000f * EigongHPScale.Value * MonsterManager.Instance.ClosetMonster.monsterCore.monsterBase.monsterStat.BossMemoryHealthScale;
            else MonsterManager.Instance.ClosetMonster.postureSystem.CurrentHealthValue = 8000f * EigongHPScale.Value; 
            
            if (EigongHPScale.Value != 1f) ToastManager.Toast($"Eigong's base HP changed to {7000f * EigongHPScale.Value}");
            dontspamstuffwow++;
        }
        MonsterManager.Instance.ClosetMonster.monsterStat.Phase2HealthRatio = 2;
        MonsterManager.Instance.ClosetMonster.monsterStat.Phase3HealthRatio = 1.5f;
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

        AttackWeight ActualWindBlade = new AttackWeight();
        ActualWindBlade.state = ActualWindBladeBossGeneralState;
        ActualWindBlade.weight = 1;

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
        
        AttackWeight[] allAttackWeights = [TeleportToTop, TeleportToBack, TeleportForward, SlowStart, CrossUp, Pokes, Unsheathe, Foo, Dunk, WindBlade, FooGeyser, FooSpike];
        LinkNextMoveStateWeight[] allLinkWeights = [
                StunLinkStateWeight,
                TeleportToBackLinkStateWeight,
                TeleportForwardLinkStateWeight, 
                SlowStartLinkStateWeight, 
                PokeLinkStateWeight, 
                CrossUpLinkStateWeight, 
                CrimsonSlamLinkStateWeight, 
                UnsheatheLinkStateWeight, 
                DunkLinkStateWeight,
                WindBladePhase1LinkStateWeight,
                StunPhase2LinkStateWeight, 
                PokePhase2LinkStateWeight, 
                TeleportToBackPhase2LinkStateWeight,
                TeleportForwardPhase2LinkStateWeight, 
                SlowStartPhase2LinkStateWeight,
                CrossUpPhase2LinkStateWeight, 
                CrimsonSlamPhase2LinkStateWeight,
                FooFollowUp1LinkStateWeight, 
                UnsheathePhase2LinkStateWeight,
                WindBladeLinkStateWeight, 
                StunPhase3LinkStateWeight, 
                TeleportToBackPhase3LinkStateWeight, 
                TeleportForwardPhase3LinkStateWeight,
                WindBladePhase3LinkStateWeight,
                UnsheathePhase3LinkStateWeight,
                JudgementCutLinkStateWeight,
                RegularJudgementCutLinkStateWeight,
            ];

        if (IsRandom.Value == true && !hasRandomLoopRan) {
            System.Random random = new System.Random();

            int attackMin = IsRandomMinimumAttackAmount.Value;
            int attackMax = IsRandomMaximumAttackAmount.Value;

            hasRandomLoopRan = true;
            foreach (LinkNextMoveStateWeight linkWeight in allLinkWeights) {
                linkWeight.stateWeightList.Clear();

                int numberOfGivenAttacks = random.Next(attackMin, attackMax + 1);
                for (int i = 0; i < numberOfGivenAttacks; i++) {

                    int randomIndex = random.Next(0, allAttackWeights.Length);
                    AttackWeight chosenAttack = allAttackWeights[randomIndex];

                    if (!linkWeight.stateWeightList.Contains(chosenAttack)) {
                        linkWeight.stateWeightList.Add(chosenAttack);
                    }
                }
            }
        } 
        else {
            var AttackAssignments = new Dictionary<LinkNextMoveStateWeight, AttackWeight[]> {
                //Phase 1
                { StunLinkStateWeight, new AttackWeight[] {TeleportToBack, TeleportToTop, Unsheathe} },
                { TeleportToBackLinkStateWeight, new AttackWeight[] { Unsheathe, SlowStart, CrossUp, Pokes, TeleportToTop, TeleportToBack} },
                { TeleportForwardLinkStateWeight, new AttackWeight[] { CrossUp, Pokes, TeleportToBack} },
                { SlowStartLinkStateWeight, new AttackWeight[] { Foo, Unsheathe, TeleportToTop} },
                { PokeLinkStateWeight, new AttackWeight[] { Foo, FooGeyser, FooSpike, Dunk} },
                { CrossUpLinkStateWeight, new AttackWeight[] { Unsheathe, Foo, TeleportForward } },
                { CrimsonSlamLinkStateWeight, new AttackWeight[] { CrossUp, Pokes, Foo, TeleportToTop, Dunk, TeleportToBack, TeleportForward} },
                { UnsheatheLinkStateWeight, new AttackWeight[] { Unsheathe, TeleportToTop, Pokes, CrossUp, Dunk} },
                { DunkLinkStateWeight, new AttackWeight[] { TeleportToTop, Unsheathe} },
                //Phase 2
                { StunPhase2LinkStateWeight, new AttackWeight[] {QuickFoo, TeleportToTop, FooGeyser, FooSpike, TeleportToBack} },
                { PokePhase2LinkStateWeight, new AttackWeight[] { Pokes, FooSpike, FooGeyser, Foo, SlowStart, TeleportToBack} },
                { TeleportToBackPhase2LinkStateWeight, new AttackWeight[] { Unsheathe, SlowStart, CrossUp, Pokes, TeleportToTop, WindBlade, FooGeyser, FooSpike } },
                { TeleportForwardPhase2LinkStateWeight, new AttackWeight[] { CrossUp, Pokes, SlowStart, FooGeyser, FooSpike} },
                { SlowStartPhase2LinkStateWeight, new AttackWeight[] { Foo, Unsheathe, TeleportToTop, FooGeyser, FooSpike } },
                { CrossUpPhase2LinkStateWeight, new AttackWeight[] { Unsheathe, Foo, TeleportToBack, TeleportForward, FooSpike } },
                { CrimsonSlamPhase2LinkStateWeight, new AttackWeight[] { CrossUp, Pokes, TeleportToTop, Dunk, TeleportToBack, TeleportForward, FooGeyser, FooSpike, WindBlade} },
                { FooFollowUp1LinkStateWeight, new AttackWeight[] { TeleportToTop, Unsheathe, QuickFoo, FooGeyser, FooSpike} },
                { UnsheathePhase2LinkStateWeight, new AttackWeight[] { Unsheathe, TeleportToTop, Dunk, Pokes, CrossUp, FooGeyser, FooSpike } },
                { WindBladeLinkStateWeight, new AttackWeight[] { QuickFoo, TeleportToBack} },
                //Phase 3
                { StunPhase3LinkStateWeight, new AttackWeight[] { QuickFoo, TeleportToTop, FooGeyser, FooSpike, WindBlade, TeleportForward, TeleportToBack } },
                { TeleportToBackPhase3LinkStateWeight, new AttackWeight[] { Unsheathe, SlowStart, CrossUp, Pokes, TeleportToTop } },
                { TeleportForwardPhase3LinkStateWeight, new AttackWeight[] { CrossUp, Pokes, SlowStart} },
                { WindBladePhase3LinkStateWeight, new AttackWeight[] { QuickFoo, TeleportToBack, ActualWindBlade} },
                { UnsheathePhase3LinkStateWeight, new AttackWeight[] { Unsheathe, TeleportToTop, Dunk, Pokes, CrossUp, SlowStart } },
                { JudgementCutLinkStateWeight, new AttackWeight[] { CrimsonJudgementCut,TeleportToTop, CrossUp, Dunk, FooGeyser, FooSpike } },
                { RegularJudgementCutLinkStateWeight, new AttackWeight[] { CrimsonJudgementCut, TeleportToTop, CrossUp, Dunk, FooGeyser, FooSpike } }
            };

            foreach (var entry in AttackAssignments) {
                LinkNextMoveStateWeight linkWeight = entry.Key;
                AttackWeight[] attacks = entry.Value;
                foreach (AttackWeight attack in attacks) {
                    if (!linkWeight.stateWeightList.Contains(attack)) {
                        linkWeight.stateWeightList.Add(attack);
                    }
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
        WindBladePhase1LinkStateWeight = GameObject.Find($"{eigongAttackStatesPath}[11] GiantChargeWave 紅白白紅/weight (1)").GetComponent<LinkNextMoveStateWeight>();

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
        ActualWindBladeBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[11] GiantChargeWave 紅白白紅").GetComponent<BossGeneralState>();
        FooFollowUp0BossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[4] Slash Up 上撈下打 大反危").GetComponent<BossGeneralState>();
        FooFollowUp1BossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[12] UpSlash Down Danger").GetComponent<BossGeneralState>();

        //Phase 3 Attacks
        JudgementCutBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[19] Thrust Full Screen Slash").GetComponent<BossGeneralState>();
        RegularJudgementCutBossGeneralState = GameObject.Find($"{eigongAttackStatesPath}[20] TeleportOut").GetComponent<BossGeneralState>();

        //Adding component to chain attacks
        if (DunkBossGeneralState.gameObject.GetComponent<LinkMoveExtendProvider>() == null) {
            DunkBossGeneralState.gameObject.AddComponent<LinkMoveExtendProvider>();
        }

        //Fire Trail
        fireTrail = GameObject.Find("GameLevel/Room/Prefab/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/LogicRoot/---Boss---/Boss_Yi Gung/MonsterCore/Animator(Proxy)/Animator/LogicRoot/Phase1 Activator/FireFX _ Fxplayer");
    }
    #endregion

    public void OnDestroy() {
        harmony.UnpatchSelf();
    }
}