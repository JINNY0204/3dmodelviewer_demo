
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    static Animation anim;

    public static void AddController(Transform target)
    {
        var controller = target.AddComponent<AnimationController>();
        anim = controller.GetComponent<Animation>();
        anim.playAutomatically = false;
        anim[anim.clip.name].time = anim[anim.clip.name].length;
        anim.Sample();
        anim.Stop();
    }

    private void OnDestroy()
    {
        anim = null;
    }
}