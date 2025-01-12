using Lockstep.Collision2D;
using Lockstep.Game;
using Lockstep.Math;
using UnityEngine;
using Debug = Lockstep.Logging.Debug;

namespace Lockstep.Game
{
    public class InputMono : UnityEngine.MonoBehaviour
    {
        // �Ƿ�Ϊ�ط�ģʽ��ͨ��Launcher.Instance�жϣ����Ϊnull�����Ƶģʽ����Ĭ��Ϊfalse
        private static bool IsReplay => Launcher.Instance?.IsVideoMode ?? false;

        // �ذ����ֲ�
        [HideInInspector] public int floorMask;
        // ���߳���
        public float camRayLength = 100;

        // �������״̬
        public bool hasHitFloor; // �Ƿ������ذ�
        public LVector2 mousePos; // ��������������λ��
        public LVector2 inputUV; // ����ķ�������
        public bool isInputFire; // �Ƿ���л�������
        public int skillId; // ����ID
        public bool isSpeedUp; // �Ƿ����

        void Start()
        {
            floorMask = LayerMask.GetMask("Floor"); // ��ȡ�ذ����ֲ�
        }

        public void Update()
        {
            if (World.Instance != null && !IsReplay)
            {
                // ��ȡˮƽ�ʹ�ֱ����
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                inputUV = new LVector2(h.ToLFloat(), v.ToLFloat());

                // ��ȡ���������������ذ���Ϣ
                isInputFire = Input.GetButton("Fire1");
                hasHitFloor = Input.GetMouseButtonDown(1);

                // ���������ذ壬��ȡ��������������λ��
                if (hasHitFloor)
                {
                    Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit floorHit;
                    if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask))
                    {
                        mousePos = floorHit.point.ToLVector2XZ();
                    }
                }

                // ��ȡ��������
                skillId = 0;
                for (int i = 0; i < 6; i++)
                {
                    if (Input.GetKey(KeyCode.Keypad1 + i))
                    {
                        skillId = i + 1;
                    }
                }

                // ��ȡ��������
                isSpeedUp = Input.GetKeyDown(KeyCode.Space);

                // ����ǰ����״̬�洢��GameInputService�ľ�̬������
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
