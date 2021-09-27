using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MyPlayerController : PlayerController
{
    bool _moveKeyPressed = false;

    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                GetDirInput();
                break;

            case CreatureState.Moving:
                GetDirInput();
                break;
        }

        base.UpdateController();
    }

    void LateUpdate()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    void GetDirInput()
    {
        _moveKeyPressed = true;

        if (Input.GetKey(KeyCode.W))
        {
            Dir = MoveDir.Up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Dir = MoveDir.Down;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Dir = MoveDir.Left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Dir = MoveDir.Right;
        }
        else
        {
            _moveKeyPressed = false;
        }
    }

    protected override void UpdateIdle()
    {
        // Moving State로 전이할 것인가를 결정.
        if (_moveKeyPressed)
        {
            State = CreatureState.Moving;
            return;
        }

        // Skill State로 전이할 것인가를 결정.
        if (_coInputCooltime == null && Input.GetKey(KeyCode.Space))
        {
            Debug.Log("Skill !!");

            C_Skill skill = new C_Skill() { SkillInfo = new SkillInfo() };
            skill.SkillInfo.SkillId = 2;
            Managers.Network.Send(skill);

            _coInputCooltime = StartCoroutine("CoInputCoolTime", 0.2f);
        }
    }

    Coroutine _coInputCooltime;
    IEnumerator CoInputCoolTime(float time)
    {
        yield return new WaitForSeconds(time);

        _coInputCooltime = null;
    }


    protected override void MoveToNextPosition()
    {
        if (_moveKeyPressed == false)
        {
            State = CreatureState.Idle;
            CheckUpdatedFlag();

            return;
        }

        Vector3Int destination = CellPosition;
        switch (Dir)
        {
            case MoveDir.Up:
                destination += Vector3Int.up;
                break;

            case MoveDir.Down:
                destination += Vector3Int.down;
                break;

            case MoveDir.Left:
                destination += Vector3Int.left;
                break;

            case MoveDir.Right:
                destination += Vector3Int.right;
                break;
        }

        if (Managers.Map.CanMove(destination))
        {
            if (Managers.Object.FindCreature(destination) == null)
            {
                CellPosition = destination;
            }
        }

        CheckUpdatedFlag();
    }

    protected override void CheckUpdatedFlag()
    {
        if (_updated)
        {
            C_Move movePacket = new C_Move();
            movePacket.PositionInfo = PositionInfo;
            Managers.Network.Send(movePacket);

            _updated = false;
        }
    }
}
