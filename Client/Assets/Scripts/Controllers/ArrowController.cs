using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ArrowController : CreatureController
{
    protected override void Init()
    {
        switch (_lastDir)
        {
            case MoveDir.Up:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;

            case MoveDir.Down:
                transform.rotation = Quaternion.Euler(0, 0, -180);
                break;

            case MoveDir.Left:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;

            case MoveDir.Right:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
        }

        State = CreatureState.Moving;
        _speed = 15.0f;

        base.Init();
    }

    protected override void UpdateAnimation()
    {
        // Animation 필요 없음.
    }

    protected override void MoveToNextPosition()
    {

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

        if (Managers.Map.CanMove(destination))
        {
            GameObject maybeCreature = Managers.Object.FindCreature(destination);
            if (maybeCreature == null)
            {
                CellPosition = destination;
            }
            else
            {
                //TEMP
                CreatureController creatureController = maybeCreature.GetComponent<CreatureController>();
                if (creatureController != null)
                {
                    creatureController.OnDamaged();
                }

                Managers.Resource.Destroy(gameObject);

            }
        }
        else
        {
            Managers.Resource.Destroy(gameObject);
        }
    }
}
