using System.Numerics;
using UnityEngine;

public class PlayerNormalController : MonoBehaviour
{
    // �قȂ���̓f�o�C�X���狤�ʂ̓��͐M�����󂯎�邽�߂̎󂯌�
    private IInputDevice inputDevice;

    // �������쎞�̈ړ��E�V���b�g�̃A���S���Y�����Ǘ�����AI
    private PlayerNormalAI ai;

    // ���g��1P��2P�����i�[
    [System.NonSerialized]
    public Players player;

    // �ړ����͗p�p�����[�^
    private float x;
    private float z;

    // �V���b�g���͗p�p�����[�^
    private bool normalShot;
    private bool lobShot;
    private bool fastShot;
    private bool dropShot;
    private bool cvShot;
    private bool aiShot;

    // �������쎞��p�̕ϐ�
    private UnityEngine.Vector3 aiShotPower;

    // �T�[�u���͗p�p�����[�^
    private bool toss;
    private bool serve;

    // �|�[�Y��ʋN���p
    private bool escape;

    // �ړ�����p�p�����[�^
    private Move move;

    // �V���b�g����p�p�����[�^
    private Shot shot;
    private bool isHit;
    private string lastShooter;

    private bool previousToss;
    private readonly float coolTime = 0.10f;
    private float coolTimeCount = 0.0f;

    private Animator animator;

    private string mlCommand = "";
    private bool hasAutoServed = false;
    private float autoServeTimer = 0f;
    private const float AUTO_SERVE_DELAY = 0.7f; 


    // Start is called before the first frame update
    void Start()
    {
        inputDevice = GetComponent<IInputDevice>();

        ai = GetComponent<PlayerNormalAI>();

        move = GetComponent<Move>();
        move.movableArea = GameData.movableArea[(int)player];

        shot = GetComponent<Shot>();
        if (player == Players.p1) { shot.opponentCourtArea = GameData.playersCourtArea[(int)Players.p2]; }
        else if (player == Players.p2) { shot.opponentCourtArea = GameData.playersCourtArea[(int)Players.p1]; }
        
        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // ���g���T�[�o�[�ł͂Ȃ��A�T�[�u�������Ă��Ȃ���Ԃ��A�܂��N���{�[����ł��Ă��Ȃ���Ԃ̏ꍇ
        // ���V�[�o�[��p�̑ҋ@���[�V�����ɐ؂�ւ���
        animator.SetBool("Receiver", GameData.server != name && !GameData.isServeIn && GameData.lastShooter == null);

        // Shot�N���X�����e�C�N�o�b�N�p�̕ϐ��ɁAAI�̏������ʂ���������
        shot.Takeback(ai.takebackFore, ai.takebackBack, ai.autoMoveLateralDirection);

        // �v���C�����R���g���[���\��Ԃɂ���Ƃ�
        if (GameData.controllable)
        {
            // JEN : ADDED TO AUTO SERVE
            if (GameData.server == name &&
                GameData.ballAmount == 0 &&
                !GameData.isServeIn &&
                !GameData.isToss &&
                !hasAutoServed)
            {
                toss = true;
                hasAutoServed = true;
                autoServeTimer = 0f;
            }

            if (hasAutoServed && GameData.isToss)
            {
                autoServeTimer += Time.deltaTime;

                if (autoServeTimer >= AUTO_SERVE_DELAY)
                {
                    serve = true;
                    Debug.Log("Served");
                }
            }
            
            if (GameData.isServeIn)
            {
                hasAutoServed = false;
                serve = false;
                autoServeTimer = 0f;
            }



            // �|�[�Y���͂���������A�|�[�Y��ʂ��J��or����
            if (escape) { GameData.pause = !GameData.pause; }

            // ���g���T�[�o�[�łȂ��ꍇ
            if (GameData.server != name)
            {
                // animator�̈ړ��A�j���[�V�����̏d�݂�1.0f�ɌŒ�
                animator.SetLayerWeight(1, 1.0f);
            }

            // �ړ����鏈��
            Move();

            // �g�X���グ�鏈��
            Toss();
        }
        else
        {
            // �ړ����~�߂�
            move.StopPlayer();

            // �{�[����ł��Ă��Ȃ���Ԃɂ���
            isHit = false;

            // �g�X���グ�Ă���A�T�[�u��łĂ�܂ł̎��Ԃ����Z�b�g����
            coolTimeCount = coolTime;
        }

        // ���v���C���Đ����Ă����
        if (GameData.gameState == GameState.Replay && inputDevice != null)
        {
            // ��������{�^������͂��邱�ƂŁA���v���C�̍Đ����L�����Z�����邽�߂̐M���𑗂�
            GameData.replayCancel = GameData.replayCancel 
                || normalShot || lobShot || fastShot || dropShot 
                || toss || serve || escape;
        }
    
    }

    void LateUpdate()
    {
        // �N�������_�����ꍇ�A�������́A�v���C�����R���g���[���\��ԂɂȂ��Ƃ�
        if (GameData.foul != FoulState.NoFoul || !GameData.controllable)
        {
            lastShooter = null;
            return;
        }

        // GameData������lastShooter�ƁA���g������lastShooter���قȂ�Ƃ�
        if (GameData.lastShooter != name && lastShooter == name)
        {
            // GameData����lastShooter�Ɏ��g������lastShooter���㏑�����čX�V����
            GameData.lastShooter = lastShooter;
            lastShooter = null;
        }
        else { lastShooter = null; }
    }

    void FixedUpdate()
    {
        // �����ňړ����������ꍇ
        if (ai.autoMove)
        {
            x = ai.x;
            z = ai.z;
        }
        // ���͂ňړ����������ꍇ
        else
        {
            x = inputDevice.GetMoveInput(player).x;
            z = inputDevice.GetMoveInput(player).y;
        }

        // �����ŃV���b�g���������ꍇ
        if (ai.autoShot)
        {
            aiShot = ai.shot;
            aiShotPower = ai.shotPower;

            toss = ai.toss;
            serve = ai.serve;
        }
        // ���͂ŃV���b�g���������ꍇ
        else
        {
            normalShot = inputDevice.GetNormalShotInput(player);
            lobShot = inputDevice.GetLobShotInput(player);
            fastShot = inputDevice.GetFastShotInput(player);
            dropShot = inputDevice.GetDropShotInput(player);
            cvShot = inputDevice.GetCVShotInput(player);

            if (!hasAutoServed)
            {
                toss = inputDevice.GetTossInput(player);
                serve = inputDevice.GetServeInput(player);
            }

        }

        if (inputDevice != null) { escape = inputDevice.GetEscapeInput(player); }
        else if (player == Players.p1) { escape = Input.GetKeyDown(KeyCode.Escape); }
    }

    void OnTriggerStay(Collider other)
    {
        // �Փ˂���Object��Ball�łȂ��ꍇ�A�������́A���łɃ{�[����ł��Ă����ꍇ
        if (!GameData.controllable && !other.gameObject.CompareTag("Ball") || isHit) { return; }

        GameObject ballObject = other.gameObject;

        // �V���b�g��ł���
        Shot(ballObject);
    }

    void OnTriggerExit(Collider other)
    {
        // Collider����o�Ă�����Object��Ball�̏ꍇ�A�{�[����ł��Ă��Ȃ���Ԃɂ���
        if (other.gameObject.CompareTag("Ball")) { isHit = false; }
    }

    public void ApplySwing(UnityEngine.Vector3 power)
    {
        if (!GameData.controllable) return;

        GameObject ballObject = FindClosestBallInRange();
        if (ballObject == null) return;

        GameData.lastShooter = gameObject.name;
        GameData.foul = FoulState.NoFoul;
        GameData.ballBoundCount = 0;

        shot.AIShot(
            ballObject,
            power,
            Parameters.charactersDominantHand[(int)player]
        );
    }
    private GameObject FindClosestBallInRange()
    {
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        if (balls == null || balls.Length == 0) return null;

        GameObject closest = null;
        float minDistSq = float.MaxValue;
        UnityEngine.Vector3 myPos = transform.position;

        foreach (GameObject ball in balls)
        {
            float d2 = (ball.transform.position - myPos).sqrMagnitude;
            if (d2 < minDistSq)
            {
                minDistSq = d2;
                closest = ball;
            }
        }
        
        float maxAllowedDistance = 10.0f; 
        if (minDistSq > maxAllowedDistance * maxAllowedDistance) return null;

        return closest;
    }
    private void Move()
    {
        // ���g���T�[�o�[���g�X��ԈȊO�Ń{�[�������݂���ꍇ�A�������́A���g���T�[�o�[�ł͂Ȃ��ꍇ�́C�v���C�����ړ�������
        if ((!GameData.isToss && GameData.ballAmount != 0) || GameData.server != name) { move.MovePlayer(x, z); }
    }

    private void Toss()
    {
        // �R���g���[���\�ɂȂ�����A�g�X���グ�Ă���T�[�u��łĂ�܂ł̎��Ԃ����Z���Ă���
        if (coolTimeCount < coolTime) { coolTimeCount += Time.deltaTime; }

        // ���g���T�[�o�[�ŁA�{�[�������݂����A�T�[�u�������Ă��Ȃ���Ԃ̏ꍇ
        if (GameData.server == name && GameData.ballAmount == 0 && !GameData.isServeIn)
        {
            // �g�X���グ�Ă���T�[�u���łĂ�悤�ɂȂ�܂ł̃N�[���^�C�����I����Ă���A�g�X�̓��͂��������ꍇ
            if (toss)
            {
                GameData.isToss = true;
                coolTimeCount = 0.0f;

                shot.Toss();
            }

            if (!previousToss && GameData.isToss) { animator.SetLayerWeight(1, 0.0f); }
            else if (previousToss && !GameData.isToss && GameData.ballAmount == 0)
            {
                animator.SetTrigger("Idle");
                animator.SetLayerWeight(1, 1.0f);
            }
        }
        previousToss = GameData.isToss;
    }

    private void Shot(GameObject ballObject)
    {
        if (!GameData.isToss && GameData.isServeIn && GameData.lastShooter != name)
        {
            float ballHight = ballObject.transform.position.y;

            // �����ŃV���b�g���s���ꍇ
            if (aiShot) { shot.AIShot(ballObject, aiShotPower, Parameters.charactersDominantHand[(int)player]); }
            // �v���C�����O�q�|�W�V�����ɂ���Ƃ�
            else if (GameData.courtArea.zNegativeLimit / 2.0f < transform.position.z && transform.position.z < GameData.courtArea.zPositiveLimit / 2.0f)
            {
                if (ballHight > 10.0f)
                {
                    // �X�}�b�V����ł�
                    if (normalShot || lobShot || fastShot || dropShot) { shot.Smash(ballObject); }
                }
                else if (ballHight > 2.50f)
                {
                    // �{���[��ł�
                    if (normalShot || lobShot || fastShot || dropShot) { shot.Volley(ballObject, Parameters.charactersDominantHand[(int)player]); }
                }
            }
            else
            {
                // ���͂ɉ����ĈقȂ�V���b�g��ł�
                if (lobShot) { shot.LobShot(ballObject, Parameters.charactersDominantHand[(int)player]); }
                else if (fastShot) { shot.FastShot(ballObject, Parameters.charactersDominantHand[(int)player]); }
                else if (dropShot) { shot.DropShot(ballObject, Parameters.charactersDominantHand[(int)player]); }
                else if (normalShot) { shot.NormalShot(ballObject, Parameters.charactersDominantHand[(int)player]); }
            }

            // �S�V���b�g���ʂ̏���
            if (normalShot || lobShot || fastShot || dropShot || aiShot)
            {
                isHit = true;

                GameData.ballBoundCount = 0;
                GameData.rallyCount++;

                if (GameData.foul == FoulState.NoFoul) { lastShooter = name; }
            }
        }
        else if (GameData.isToss)
        {
            // �g�X���グ�Ă���̃N�[���^�C���o�ߌ�A�T�[�u��ł�
            if (coolTimeCount >= coolTime && serve)
            {
                shot.Serve(ballObject, GameData.servePosition, x);

                isHit = true;

                GameData.isToss = false;

                if (GameData.foul == FoulState.NoFoul) { lastShooter = name; }
            }
        }
    }
}
