using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndSequence : MonoBehaviour
{

    [SerializeField] Character A;
    [SerializeField] Character B;

    [SerializeField] HideableCG screen;


    // Start is called before the first frame update
    void Awake()
    {
        Game.overrideTurn = true;
        Game.continueTurnOrder = false;
        Debug.Log("GANK");
        A.WalkInDirection(1);
        B.WalkInDirection(-1);
    }

    // Update is called once per frame
    void Update()
    {

        if(!screen.isVisible)screen.Show();
        if (A.CantWalkFurther() && B.CantWalkFurther())
        {
            StartCoroutine(JumpAnimation(A,0f));
            StartCoroutine(JumpAnimation(B,0.75f));
        }
    }

    IEnumerator JumpAnimation(Character c, float delay)
    {
        yield return new WaitForSeconds(delay);
        float time = 120;

        while (time>0)
        {
            if (c.IsGrounded()) { c.Jump(Vector2.up * c.maxJumpStrength); }
            time -= Time.deltaTime;
            yield return null;
        }

    }
}
