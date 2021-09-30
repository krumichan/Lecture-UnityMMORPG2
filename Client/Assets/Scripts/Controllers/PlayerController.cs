using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
    protected Coroutine _coSkill;
    protected bool _rangeSkill = false;

    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateAnimation()
    {
        if (_animator == null || _sprite == null)
        {
            return;
        }

        if (PositionInfo.State == CreatureState.Idle)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    PlayWithFlip("IDLE_BACK");
                    break;

                case MoveDir.Down:
                    PlayWithFlip("IDLE_FRONT");
                    break;

                case MoveDir.Left:
                    PlayWithFlip("IDLE_RIGHT", true);
                    break;

                case MoveDir.Right:
                default:
                    PlayWithFlip("IDLE_RIGHT");
                    break;
            }
        }
        else if (PositionInfo.State == CreatureState.Moving)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    PlayWithFlip("WALK_BACK");
                    break;

                case MoveDir.Down:
                    PlayWithFlip("WALK_FRONT");
                    break;

                case MoveDir.Left:
                    PlayWithFlip("WALK_RIGHT", true);
                    break;

                case MoveDir.Right:
                    PlayWithFlip("WALK_RIGHT");
                    break;
            }
        }
        else if (PositionInfo.State == CreatureState.Skill)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    PlayWithFlip(_rangeSkill ? "ATTACK_WEAPON_BACK" : "ATTACK_BACK");
                    break;

                case MoveDir.Down:
                    PlayWithFlip(_rangeSkill ? "ATTACK_WEAPON_FRONT" : "ATTACK_FRONT");
                    break;

                case MoveDir.Left:
                    PlayWithFlip(_rangeSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT", true);
                    break;

                case MoveDir.Right:
                    PlayWithFlip(_rangeSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
                    break;
            }
        }
        else
        {

        }
    }

    protected override void UpdateController()
    {
        base.UpdateController();
    }

    public void UseSkill(int skillId)
    {
        switch (skillId)
        {
            case 1:
                _coSkill = StartCoroutine("CoStartPunch");
                break;

            case 2:
                _coSkill = StartCoroutine("CoStartShootArrow");
                break;
        }
    }

    protected virtual void CheckUpdatedFlag()
    {

    }

    IEnumerator CoStartPunch()
    {
        // ��� �ð�.
        _rangeSkill = false;

        State = CreatureState.Skill;

        // Client ������ Delay�� �־��ִ� ����.
        // Server ������ Delay�� �����Ͽ� Filtering �ϱ� ������,
        // Client ������ Skill ��Ÿ�� ���� ������ ���� ���ǹ���
        // Request Packet�� Server ���� ������ ���� �����ϱ� �����̴�.
        yield return new WaitForSeconds(0.5f);

        State = CreatureState.Idle;
        _coSkill = null;

        CheckUpdatedFlag();
    }

    IEnumerator CoStartShootArrow()
    {
        // ��� �ð�.
        _rangeSkill = true;

        State = CreatureState.Skill;

        yield return new WaitForSeconds(0.3f);
        
        State = CreatureState.Idle;
        _coSkill = null;

        CheckUpdatedFlag();
    }

    public override void OnDamaged()
    {
        Debug.Log("Player HIT !");
    }
}
