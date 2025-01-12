using Lockstep.Collision2D;
using Lockstep.Game;
using Lockstep.Math;
using UnityEngine;
using Debug = Lockstep.Logging.Debug;

namespace Lockstep.Game
{
    public class InputMono : UnityEngine.MonoBehaviour
    {
        // 是否为回放模式，通过Launcher.Instance判断，如果为null或非视频模式，则默认为false
        private static bool IsReplay => Launcher.Instance?.IsVideoMode ?? false;

        // 地板遮罩层
        [HideInInspector] public int floorMask;
        // 射线长度
        public float camRayLength = 100;

        // 输入相关状态
        public bool hasHitFloor; // 是否点击到地板
        public LVector2 mousePos; // 鼠标在世界坐标的位置
        public LVector2 inputUV; // 输入的方向向量
        public bool isInputFire; // 是否进行火力输入
        public int skillId; // 技能ID
        public bool isSpeedUp; // 是否加速

        void Start()
        {
            floorMask = LayerMask.GetMask("Floor"); // 获取地板遮罩层
        }

        public void Update()
        {
            if (World.Instance != null && !IsReplay)
            {
                // 获取水平和垂直输入
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                inputUV = new LVector2(h.ToLFloat(), v.ToLFloat());

                // 获取火力输入和鼠标点击地板信息
                isInputFire = Input.GetButton("Fire1");
                hasHitFloor = Input.GetMouseButtonDown(1);

                // 如果点击到地板，获取鼠标在世界坐标的位置
                if (hasHitFloor)
                {
                    Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit floorHit;
                    if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask))
                    {
                        mousePos = floorHit.point.ToLVector2XZ();
                    }
                }

                // 获取技能输入
                skillId = 0;
                for (int i = 0; i < 6; i++)
                {
                    if (Input.GetKey(KeyCode.Keypad1 + i))
                    {
                        skillId = i + 1;
                    }
                }

                // 获取加速输入
                isSpeedUp = Input.GetKeyDown(KeyCode.Space);

                // 将当前输入状态存储在GameInputService的静态变量中
                GameInputService.CurGameInput = new PlayerInput()
                {
                    mousePos = mousePos,
                    inputUV = inputUV,
                    isInputFire = isInputFire,
                    skillId = skillId,
                    isSpeedUp = isSpeedUp,
                };
            }
        }
    }
}
