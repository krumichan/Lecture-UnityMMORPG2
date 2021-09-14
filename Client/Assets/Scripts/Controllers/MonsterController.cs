using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    Coroutine _coPatrol;
    Vector3Int _destination;

    public override CreatureState State
    {
        get { return _state; }
        set
        {
            if (_state == value)
            {
                return;
            }

            base.State = value;
            if (_coPatrol != null)
            {
                StopCoroutine(_coPatrol);
                _coPatrol = null;
            }
        }
    }

    protected override void Init()
    {
        base.Init();

        // Animator, SpriteRenderer를
        // 준비해야 하므로, Init() 뒤에 수행.
        State = CreatureState.Idle;
        Dir = MoveDir.None;
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        if (_coPatrol == null)
        {
            _coPatrol = StartCoroutine("CoPatrol");
        }
    }

    protected override void MoveToNextPosition()
    {
        // TODO: Astar
        Vector3Int moveCellDirection = _destination - CellPosition;
        if (moveCellDirection.x > 0)
        {
            Dir = MoveDir.Right;
        }
        else if (moveCellDirection.x < 0)
        {
            Dir = MoveDir.Left;
        }
        else if (moveCellDirection.y > 0)
        {
            Dir = MoveDir.Up;
        }
        else if (moveCellDirection.y < 0)
        {
            Dir = MoveDir.Down;
        }
        else
        {
            Dir = MoveDir.None;
        }

        Vector3Int destination = CellPosition;
        switch (_dir)
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

        if (Managers.Map.CanMove(destination) && Managers.Object.FindCreature(destination) == null)
        {
            CellPosition = destination;
        }
        else
        {
            State = CreatureState.Idle;
        }
    }

    public override void OnDamaged()
    {
        GameObject effect = Managers.Resource.Instantiate("Effect/DieEffect");
        effect.transform.position = transform.position;
        effect.GetComponent<Animator>().Play("START");
        GameObject.Destroy(effect, 0.5f);

        Managers.Object.Remove(gameObject);
        Managers.Resource.Destroy(gameObject);
    }

    IEnumerator CoPatrol()
    {
        int waitSeconds = Random.Range(1, 4);
        yield return new WaitForSeconds(waitSeconds);

        for (int i = 0; i < 10; ++i)
        {
            int xRange = Random.Range(-5, 6);
            int yRange = Random.Range(-5, 6);
            Vector3Int destination = CellPosition + new Vector3Int(xRange, yRange, 0);

            if (Managers.Map.CanMove(destination) && Managers.Object.FindCreature(destination) == null)
            {
                _destination = destination;
                State = CreatureState.Moving;
                yield break;
            }
        }

        State = CreatureState.Idle;
    }
}
