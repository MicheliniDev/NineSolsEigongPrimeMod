using UnityEngine;

namespace EigongPrime {
    internal class ColorChange {
        private GameObject eigongSpriteHolder = null!;
        private _2dxFX_ColorChange eigongCurrentHueValue = null!;

        private GameObject eigongCutsceneDummySpriteHolder = null!;
        private _2dxFX_ColorChange eigongCutsceneDummyCurrentHueValue = null!;

        private GameObject eigongSwordSpriteHolder = null!;
        private _2dxFX_ColorChange eigongSwordCurrentHueValue = null!;

        private GameObject eigongSwordCutsceneSpriteHolder = null!;
        private _2dxFX_ColorChange eigongSwordCutsceneCurrentHueValue = null!;

        private GameObject eigongFooSpriteHolder = null!;
        private _2dxFX_ColorChange eigongFooCurrentHueValue = null!;

        public int dontspamstuffwow2 = 0;
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

                eigongCutsceneDummySpriteHolder = GameObject.Find("GameLevel/Room/Prefab/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/[CutScene] 二進/YiGung_Dummy/View/YiGung/Body");
                eigongCutsceneDummySpriteHolder.AddComponent<_2dxFX_ColorChange>();
                eigongCutsceneDummyCurrentHueValue = eigongCutsceneDummySpriteHolder.GetComponent<_2dxFX_ColorChange>();

                eigongSwordCutsceneSpriteHolder = GameObject.Find("GameLevel/Room/Prefab/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/[CutScene] 二進/YiGung_Dummy/View/YiGung/Weapon/Sword/Sword Sprite");
                eigongSwordCutsceneSpriteHolder.AddComponent<_2dxFX_ColorChange>();
                eigongSwordCutsceneCurrentHueValue = eigongSwordCutsceneSpriteHolder.GetComponent<_2dxFX_ColorChange>();

                eigongCurrentHueValue._HueShift = 130;
                eigongCutsceneDummyCurrentHueValue._HueShift = 130;
                eigongSwordCurrentHueValue._HueShift = 250;
                eigongSwordCutsceneCurrentHueValue._HueShift = 250f;
                eigongFooCurrentHueValue._HueShift = 250;
                dontspamstuffwow2++;
            }
        }
    }
}
