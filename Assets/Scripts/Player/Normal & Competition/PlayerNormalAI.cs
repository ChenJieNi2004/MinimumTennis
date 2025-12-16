using System.Collections;
using UnityEngine;
using System;

public class PlayerNormalAI : MonoBehaviour
{
    // Editable Parameters
    private float delay = 0.30f;
    private float distance = 1.0f;

    // �ΐ푊��̃v���C��
    private GameObject opponentPlayer;

    // �{�[��
    private GameObject ballObject;
    private NormalBallController ball;

    // �ړ����ɗp����p�����[�^
    private bool isArrivedX = true;
    private bool isArrivedZ = true;
    private Vector3 targetPosition;

    // �ԋ����ɗp����p�����[�^
    private bool hitBall;
    private string lastShooter;

    // �T�[�u���ɗp����p�����[�^
    private float autoServeCount;

    // ���g��1P��2P�����i�[
    [NonSerialized]
    public Players player;

    // ��������p�p�����[�^
    [NonSerialized]
    public bool autoMove;
    [NonSerialized]
    public bool autoShot;

    [NonSerialized]
    public float x;
    [NonSerialized]
    public float z;

    [NonSerialized]
    public bool shot;
    [NonSerialized]
    public Vector3 shotPower;

    [NonSerialized]
    public bool toss;

    [NonSerialized]
    public bool serve;

    // �e�C�N�o�b�N���ɗp����p�����[�^
    [NonSerialized]
    public float autoMoveLateralDirection;
    [NonSerialized]
    public bool takebackBack;
    [NonSerialized]
    public bool takebackFore;

    // Start is called before the first frame update
    void Start()
    {
        delay = Parameters.reactionDelay[(int)player];
        distance = Parameters.distance[(int)player];

        if (player == Players.p1) { opponentPlayer = CharacterGenerator.GetCharacter((int)Players.p2); }
        else if (player == Players.p2) { opponentPlayer = CharacterGenerator.GetCharacter((int)Players.p1); }

        targetPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        delay = Parameters.reactionDelay[(int)player];
        distance = Parameters.distance[(int)player];

        // �ړ��݂̂�����������Ă���Ƃ�
        if (autoMove && !autoShot)
        {
            // ����\�łȂ��Ƃ��A�e�C�N�o�b�N��Ԃ���������
            if (!GameData.controllable) { ResetTakeback(); }
        }
        else
        {
            // �ړ�������������Ă��Ȃ��ꍇ�́A�e�C�N�o�b�N��Ԃ���������
            ResetTakeback();
        }
    }

    void LateUpdate()
    {
        DecideNextPosition();
        lastShooter = GameData.lastShooter;
    }

    private void FixedUpdate()
    {
        if (autoMove || autoShot)
        {
            // �{�[����T���A�ϐ��Ɋi�[����
            if (CountObjectAmount("Ball") == 1 && !ballObject)
            {
                ballObject = GameObject.FindWithTag("Ball");
                ball = ballObject.GetComponent<NormalBallController>();
            }
            else if (CountObjectAmount("Ball") == 0)
            {
                ballObject = null;
                ball = null;
            }
        }

        if (autoMove)
        {
            // �����ňړ�����
            if (GameData.controllable) { AutoMove(targetPosition); }
            else { targetPosition = transform.position; }
        }

        if (autoShot)
        {
            // �����Ńg�X�E�T�[�u������
            if (GameData.controllable) { AutoTossAndServe(); }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.CompareTag("Ball")) { return; }

        if (autoShot)
        {
            // �����ŃV���b�g��ł�
            AutoShot();
        }
    }

    private void AutoMove(Vector3 ballArrivalPoint)
    {
        float arrivalRange = 4.0f;
        Vector3 arrivalPointInPlayerLocal = transform.InverseTransformPoint(ballArrivalPoint);

        if (!isArrivedX && !(ballArrivalPoint.x - arrivalRange <= transform.position.x
            && transform.position.x <= ballArrivalPoint.x + arrivalRange))
        {
            // �E�����ւ̈ړ�
            if (arrivalPointInPlayerLocal.x > 0) { x = 1.0f; }
            // �������ւ̈ړ�
            else { x = -1.0f; }
        }
        else
        {
            isArrivedX = true;
            x = 0.0f;
        }

        if (!isArrivedZ && !(ballArrivalPoint.z - arrivalRange <= transform.position.z
            && transform.position.z <= ballArrivalPoint.z + arrivalRange))
        {
            // �O�����ւ̈ړ�
            if (arrivalPointInPlayerLocal.z > 0) { z = 1.0f; }
            // ������ւ̈ړ�
            else { z = -1.0f; }
        }
        else
        {
            isArrivedZ = true;
            z = 0.0f;
        }
    }

    private void AutoTossAndServe()
    {
        if (GameData.server == name 
            && GameData.ballAmount == 0 && !GameData.isServeIn)
        {
            autoServeCount += Time.deltaTime;

            if (autoServeCount > 2.0f)
            {
                // トスを上げる
                GameData.isToss = true;
                toss = true;

                autoServeCount = 0.0f;
            }
        }
        else { toss = false; }

        if (GameData.server == name 
            && !GameData.isServeIn && GameData.isToss && GameData.lastShooter == null
            && ball != null)
        {
            // サーブを打つ
            if (ball.transform.position.y >= 14.0f)
            {
                serve = true;

                // 打つ方向をランダムに変える
                int random = UnityEngine.Random.Range(-1, 2);
                x = random;
            }
        }
        else { serve = false; }
    }

    private void AutoShot()
    {
        shot = hitBall;

        if (hitBall)
        {
            shotPower = DecideBallSpeed();
            hitBall = false;
        }
    }

    private void DecideNextPosition()
    {
        if (!opponentPlayer || !ball) { return; }

        Vector3 nextTargetPosition;

        // ����v���C�����{�[����ł��Ԃ����u��
        if (lastShooter != opponentPlayer.name && GameData.lastShooter == opponentPlayer.name)
        {
            Vector3 ballArrivalPoint = CalculateBallArrivalPoint();

            if (GameData.playersCourtArea[(int)player].CheckInside(ballArrivalPoint.x, ballArrivalPoint.z, 5.0f))
            {
                hitBall = true;
                nextTargetPosition = ballArrivalPoint + CalculateExtraDistance();
                UpdateTargetPosition(nextTargetPosition, delay);
            }
            else { hitBall = false; }

            // �ړ��݂̂�����������Ă���ꍇ�A�e�C�N�o�b�N����
            if (autoMove && !autoShot && hitBall) { DecideTakeback(ballArrivalPoint, delay); }
        }
        // �������{�[����ł��Ԃ����u��
        else if (lastShooter != name && GameData.lastShooter == name)
        {
            nextTargetPosition = OptimizePosition();
            UpdateTargetPosition(nextTargetPosition, delay * 1.50f);

            // �ړ��݂̂�����������Ă���ꍇ�A�e�C�N�o�b�N��Ԃ���������
            if (autoMove && !autoShot) { ResetTakeback(); }
        }
    }

    private Vector3 CalculateBallArrivalPoint()
    {
        Vector3 ballPosition = ball.transform.position;
        float ballRadius = ball.GetComponent<SphereCollider>().radius;
        float speedY = ball.SpeedY;
        float gravity = ball.Gravity;
        float arrivalTime = (speedY + Mathf.Sqrt(speedY * speedY + 2.0f * gravity * (ballPosition.y - ballRadius))) / gravity;

        float arrivalPointX = ball.SpeedX * arrivalTime + ballPosition.x;
        float arrivalPointZ = ball.SpeedZ * arrivalTime + ballPosition.z;
        Vector3 arrivalPoint = new Vector3(arrivalPointX, 0.0f, arrivalPointZ);

        return arrivalPoint;
    }

    private Vector3 CalculateExtraDistance()
    {
        float extraDistanceX = 0.30f * ball.SpeedX;
        float extraDistanceZ = 0.30f * ball.SpeedZ;
        Vector3 extraDistance = new Vector3(extraDistanceX, 0.0f, extraDistanceZ);

        return extraDistance;
    }

    private Vector3 OptimizePosition()
    {
        Vector3 predictedBallArrivalPoint = CalculateBallArrivalPoint();
        Vector3 optimizedPosition;

        if (predictedBallArrivalPoint.x > 0.0f) { optimizedPosition = new Vector3(-4.0f, 0.0f, -52.0f * transform.forward.z); }
        else { optimizedPosition = new Vector3(4.0f, 0.0f, -52.0f * transform.forward.z); }

        return optimizedPosition;
    }

    private Vector3 DecideBallSpeed()
    {
        Vector3 ballSpeed;
        Vector3 playerPosition = opponentPlayer.transform.position;
        Vector3 defaltTargetPoint;

        // ����v���C���[���O�q�|�W�V�����̂Ƃ�
        if (GameData.courtArea.zNegativeLimit / 2.0f < opponentPlayer.transform.position.z 
            && opponentPlayer.transform.position.z < GameData.courtArea.zPositiveLimit / 2.0f)
        {
            if (playerPosition.x < 0.0f) { defaltTargetPoint = new Vector3(12.0f, 0.0f, 35.0f * transform.forward.z); }
            else { defaltTargetPoint = new Vector3(-12.0f, 0.0f, 35.0f * transform.forward.z); }
        }
        // ����v���C���[����q�|�W�V�����̂Ƃ�
        else
        {
            if (playerPosition.x < 0.0f) { defaltTargetPoint = new Vector3(12.0f, 0.0f, 40.0f * transform.forward.z); }
            else { defaltTargetPoint = new Vector3(-12.0f, 0.0f, 40.0f * transform.forward.z); }
        }

        Vector3 noise = GenerateNoise();
        ballSpeed.y = DecideUpwardSpeed();
        ballSpeed.x = DecideLateralSpeed(ballSpeed.y, defaltTargetPoint, noise.x);
        ballSpeed.z = DecideDepthSpeed(ballSpeed.y, defaltTargetPoint, noise.z);

        return ballSpeed;
    }

    private float DecideUpwardSpeed()
    {
        if (!isArrivedX && !isArrivedZ) { return UnityEngine.Random.Range(30.0f, 40.0f); }
        else if (isArrivedX && isArrivedZ)
        {
            // �v���C���[���O�q�|�W�V�����̂Ƃ�
            if (GameData.courtArea.zNegativeLimit / 2.0f < opponentPlayer.transform.position.z 
            && opponentPlayer.transform.position.z < GameData.courtArea.zPositiveLimit / 2.0f) { return UnityEngine.Random.Range(45.0f, 50.0f); }
            // �v���C���[����q�|�W�V�����̂Ƃ�
            else { return UnityEngine.Random.Range(20.0f, 30.0f); }
        }
        else
        {
            // �v���C���[���O�q�|�W�V�����̂Ƃ�
            if (GameData.courtArea.zNegativeLimit / 2.0f < opponentPlayer.transform.position.z
            && opponentPlayer.transform.position.z < GameData.courtArea.zPositiveLimit / 2.0f) { return UnityEngine.Random.Range(35.0f, 45.0f); }
            // �v���C���[����q�|�W�V�����̂Ƃ�
            else {  return UnityEngine.Random.Range(25.0f, 30.0f); }
        }
    }

    private float DecideLateralSpeed(float ballSpeedY, Vector3 targetPosition, float noise)
    {
        int random = UnityEngine.Random.Range(-1, 3);
        int randomTarget = random < 0 ? -1 : 1;

        Vector3 ballPosition = ball.transform.position;
        float gravity = ball.Gravity;
        float arrivalTime = (ballSpeedY + Mathf.Sqrt(ballSpeedY * ballSpeedY + 2.0f * gravity * ballPosition.y)) / gravity;

        float disX = targetPosition.x * randomTarget * distance - ball.transform.position.x;

        return (disX / arrivalTime) * noise;
    }

    private float DecideDepthSpeed(float ballSpeedY, Vector3 targetPosition, float noise)
    {
        Vector3 ballPosition = ball.transform.position;
        float gravity = ball.Gravity;
        float arrivalTime = (ballSpeedY + Mathf.Sqrt(ballSpeedY * ballSpeedY + 2.0f * gravity * ballPosition.y)) / gravity;

        float disZ = targetPosition.z - ball.transform.position.z;

        return (disZ / arrivalTime) * noise;
    }

    private Vector3 GenerateNoise()
    {
        Vector3 noise = new Vector3(1.0f, 1.0f, 1.0f);

        if (!isArrivedX) { noise.x = UnityEngine.Random.Range(-0.50f, 1.0f); }
        if (!isArrivedZ) { noise.z = UnityEngine.Random.Range(0.80f, 1.0f); }

        float randomNoise = UnityEngine.Random.Range(0.0f, 1.0f);
        if (randomNoise < 0.20f && (isArrivedX || isArrivedZ))
        {
            noise.x = UnityEngine.Random.Range(-0.50f, 1.0f);
            noise.z = UnityEngine.Random.Range(0.90f, 1.0f);
        }

        return noise;
    }

    private void UpdateTargetPosition(Vector3 nextTargetPosition, float delay)
    {
        StartCoroutine(DelayMethod(delay, () =>
        {
            isArrivedX = false;
            isArrivedZ = false;
            targetPosition = nextTargetPosition;
        }));
    }

    private IEnumerator DelayMethod(float delaySeconds, Action action)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < delaySeconds)
        {
            if (!GameData.pause) { elapsedTime += Time.deltaTime; }
            if (!GameData.controllable) { yield break; }
            yield return null;
        }

        action();
    }

    private int CountObjectAmount(string tagName)
    {
        GameObject[] tagObjects = GameObject.FindGameObjectsWithTag(tagName);
        return tagObjects.Length;
    }

    private void DecideTakeback(Vector3 ballArrivalPoint, float delay)
    {
        StartCoroutine(DelayMethod(delay, () =>
        {
            Vector3 objectPositionInPlayerLocal = transform.InverseTransformPoint(ballArrivalPoint);

            // �E���őł��Ԃ�
            if (objectPositionInPlayerLocal.x > 0.0f)
            {
                autoMoveLateralDirection = -1;
                takebackFore = Parameters.charactersDominantHand[0] == DominantHand.right;
                takebackBack = Parameters.charactersDominantHand[0] != DominantHand.right;
            }
            // �����őł��Ԃ�
            else
            {
                autoMoveLateralDirection = 1;
                takebackFore = Parameters.charactersDominantHand[0] != DominantHand.right;
                takebackBack = Parameters.charactersDominantHand[0] == DominantHand.right;
            }
        }));
    }

    private void ResetTakeback()
    {
        autoMoveLateralDirection = 0.0f;
        takebackBack = false;
        takebackFore = false;
    }
}