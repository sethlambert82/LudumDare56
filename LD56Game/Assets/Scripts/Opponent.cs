using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opponent : MonoBehaviour
{
    public Character[] characters;
    public Character activeCharacter;

    public Character[] targets;


    // Start is called before the first frame update
    void Start()
    {
        //targets = FindObjectsOfType<Character>();
    }

    // Update is called once per frame
    void Update()
    {
        bool hasLivingCharacters = false;
        foreach(Character c in characters)
        {
            if(!c.IsDead())hasLivingCharacters = true;
        }
        if (!hasLivingCharacters)
        {
            Game.CompleteLevel();
        }
    }

    public void TakeCharacterTurn(Character c)
    {
        if (c.IsDead())
        {
            c.EndTurn(); return;
        }
        activeCharacter = c;
        StartCoroutine(CharacterTurnCoroutine());
    }


    public void ShootTarget(int index)
    {
        Vector2? shootVec = FindBallisticTrajectory(activeCharacter.projectileEmitter.position, targets[index].gameObject, activeCharacter.maxFireForce, 0.05f);
        if(shootVec != null)
        {
            StartCoroutine(ShootAnimation((Vector2)shootVec));
        }
        else 
        { 
            Debug.Log("Target out of range");
            activeCharacter.WalkInDirection(Mathf.Sign(ToTarget(targets[index]).x));
        }
    }

    IEnumerator ShootAnimation(Vector2 shootVector)
    {
        activeCharacter.Shoot();
        activeCharacter.fireForce = shootVector;
        for(int i = 0; i < 75; i++)
        {
            Vector2 animVector = Vector2.zero;

            activeCharacter.ShowBallisticTrajectory(activeCharacter.projectileEmitter.position, Vector2.Lerp(animVector, shootVector, (float)(i) / 74f),25);

            yield return null;
        }
        activeCharacter.FireProjectile(shootVector);
    }


    Vector2? FindShootVector(Character target)
    {
        Vector2 calculatedVector;
        Vector2 toTarget = target.transform.position - activeCharacter.projectileEmitter.transform.position;

        Vector2 testAngle = Quaternion.Euler(0, 0, 45) * Vector2.right * target.maxFireForce;
        float testAngleHeight = testAngle.y * testAngle.y / (2f * 4.9f) -toTarget.y;

        bool tooFar = Mathf.Abs(toTarget.x) > Mathf.Abs(testAngle.x * (testAngle.y / 4.9f + Quadratic(-4.9f,0,-testAngleHeight)));

        Debug.DrawLine((Vector2)activeCharacter.projectileEmitter.transform.position, (Vector2)activeCharacter.projectileEmitter.transform.position+toTarget);

        if (tooFar) return null;

        float force = activeCharacter.maxFireForce;

        for (int j = 0; j < 50; j++)
        {
            float shootAngle = toTarget.x > 0 ? 0 : 180;
            float angleStep = 0.25f * Mathf.Sign(toTarget.x);
            for (int i = 0; i < 360; i++)
            {
                calculatedVector = Quaternion.Euler(0, 0, shootAngle) * Vector2.right * force;
                float timeToHit = Mathf.Abs(toTarget.x / calculatedVector.x);

                Vector2 estimatedHitPoint = (Vector2)activeCharacter.projectileEmitter.transform.position + new Vector2(calculatedVector.x*timeToHit, calculatedVector.y*timeToHit-4.9f*(timeToHit*timeToHit));
                Collider2D? hit = Physics2D.OverlapCircle(estimatedHitPoint, 0.05f);
                    
                if (hit && hit.gameObject.transform.IsChildOf(target.transform)) return calculatedVector;

                shootAngle += angleStep;
            }

            force -= activeCharacter.maxFireForce / 50f;
        }
        
        return null;
    }



    Vector2? FindBallisticTrajectory(Vector2 referencePosition, GameObject targetLocation, float maxForce, float hitSize = 0.5f, bool spiceAiming = true)
    {
        Vector2 calculatedVector;
        Vector2 toTarget = (Vector2)targetLocation.transform.position - referencePosition;
        //if (spiceAiming) toTarget = Game.SpiceOpponentAiming(toTarget);

        Vector2 testAngle = Quaternion.Euler(0, 0, 45) * Vector2.right * maxForce;
        float testAngleHeight = testAngle.y * testAngle.y / (2f * 4.9f) - toTarget.y;

        bool tooFar = Mathf.Abs(toTarget.x) > Mathf.Abs(testAngle.x * (testAngle.y / 4.9f + Quadratic(-4.9f, 0, -testAngleHeight)));
        if (tooFar) return null;

        float force = maxForce;
        for(int i = 0; i < 50; i++)
        {
            float launchAngle = 90;
            float angleStep = 0.5f * -Mathf.Sign(toTarget.x);

            for(int j = 0; j < 180; j++)
            {
                calculatedVector = Quaternion.Euler(0, 0, launchAngle) * Vector2.right * force;
                float timeToHit = Mathf.Abs(toTarget.x / calculatedVector.x);
                Vector2 estimatedHitPoint = referencePosition + new Vector2(calculatedVector.x * timeToHit, calculatedVector.y * timeToHit - 4.9f * (timeToHit * timeToHit));
                Collider2D? hit = Physics2D.OverlapCircle(estimatedHitPoint, hitSize);



                if (hit && hit.gameObject.transform.IsChildOf(targetLocation.transform))
                {
                    bool obstruction=false;
                    Vector2 lastPos = referencePosition;
                    for(int k = 1; k <= 24; k++)
                    {
                        float t = timeToHit * (float)k / 24f;
                        Vector2 pos = referencePosition + new Vector2(calculatedVector.x * t, calculatedVector.y * t - 4.9f * (t * t));

                        //Debug.DrawLine(lastPos, pos, Color.Lerp(Color.red, Color.green, t/timeToHit),2);

                        RaycastHit2D hit2 = Physics2D.CircleCast(lastPos, 0.2f, pos - lastPos, (pos-lastPos).magnitude, LayerMask.GetMask("Platform"));
                        if(hit2.collider != null)
                        {
                            if (hit2.collider.gameObject.layer == 3) { obstruction = true; break; }
                            else if (hit2.collider.transform.IsChildOf(targetLocation.transform)) break;
                        }
                        lastPos = pos;

                    }

                    if(!obstruction) return calculatedVector;
                }
                launchAngle += angleStep;
            }
            force = Mathf.Lerp(maxForce, 0, (float)i/50f);
        }
        return null;
    }


    IEnumerator CharacterTurnCoroutine()
    {
        float timeout = 10f;
        yield return new WaitForSeconds(1.5f);
        if(FindBallisticTrajectory(activeCharacter.projectileEmitter.position, targets[0].gameObject, activeCharacter.maxFireForce, 0.05f) == null)
        {
            bool obstacleAhead = ScanForObstacles();
            activeCharacter.WalkInDirection(Mathf.Sign(ToTarget(targets[0]).x));
            while (!activeCharacter.CantWalkFurther() && FindBallisticTrajectory(activeCharacter.projectileEmitter.position, targets[0].gameObject, activeCharacter.maxFireForce, 0.05f) == null && timeout > 0)
            {
                if (!activeCharacter.IsGrounded()) yield return null;
                timeout -= Time.deltaTime;
                yield return null;
            }
        }

        Vector2? shootVec = FindBallisticTrajectory(activeCharacter.projectileEmitter.position, targets[0].gameObject, activeCharacter.maxFireForce, 0.05f);
        if (shootVec != null)
        {
            yield return StartCoroutine(ShootAnimation((Vector2)shootVec));
        }
        
        activeCharacter.EndTurn();


    }


    bool ScanForObstacles()
    {
        return false;
    }


    float Quadratic(float A, float B, float C)
    {
        return Mathf.Max((-B + Mathf.Sqrt(B * B - 4 * A * C)) / 2f * A, (-B - Mathf.Sqrt(B * B - 4 * A * C)) / 2f * A);
    }


    Vector2 ToTarget(Character target)
    {
        return target.transform.position - this.transform.position;
    }
}



#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(Opponent))]
public class OpponentEditor : UnityEditor.Editor
{
    Opponent t;

    public override void OnInspectorGUI()
    {
        t ??= (Opponent)target;
        base.OnInspectorGUI();
        if(GUILayout.Button("Shoot Target"))
        {
            t.ShootTarget(0);
        }
    }
}

#endif
