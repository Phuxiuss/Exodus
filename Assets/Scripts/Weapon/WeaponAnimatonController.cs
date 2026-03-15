using System.Collections;
using UnityEditor;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class WeaponAnimatonController : MonoBehaviour
{
    private Animator weaponAnimator;
    private string currentAnimation = "";
    private void Awake()
    {
        weaponAnimator =  GetComponent<Animator>();
        
    }

    public void ChangeAnimation(string animation, float crossfade = 0.2f, float delay = 0f)
    {
        if (delay > 0f)
        {
            StartCoroutine(Wait());
        }
        else
        {
            Validate();
        }
        
        void Validate()
        {
            weaponAnimator.CrossFade(animation, crossfade);
        }
        
        IEnumerator Wait()
        {
            yield return new WaitForSeconds(delay);
            Validate();
        }
    }

    public void ForcePlay(string animation, float delay = 0f)
    {
        if (delay > 0f)
        {
            StartCoroutine(Wait());
        }
        else
        {
            Validate();
        }
        
        void Validate()
        {
            weaponAnimator.Play(animation, 0, 0f);
        }
        
        IEnumerator Wait()
        {
            yield return new WaitForSeconds(delay);
            Validate();
        }
    }

    public bool IsPlaying()
    {
        if (weaponAnimator == null) return false;
        
        return true;
    }
}
