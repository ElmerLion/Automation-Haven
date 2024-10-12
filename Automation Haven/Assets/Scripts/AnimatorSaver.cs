using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimatorSaveData {
    public string currentState;
    public float currentStateTime;
    public Dictionary<string, float> floatParameters;
    public Dictionary<string, int> intParameters;
    public Dictionary<string, bool> boolParameters;
    public Dictionary<string, string> triggerParameters;
}

public class AnimatorSaver : MonoBehaviour {
    public Animator animator;
    private AnimatorSaveData animatorSaveData;

    private void Start() {
        SaveManager.OnGameSaved += SaveAnimatorState;
        SaveManager.OnGameLoaded += LoadAnimatorState;
    }

    public void SaveAnimatorState(string filePath) {
        animatorSaveData = new AnimatorSaveData();
        animatorSaveData.floatParameters = new Dictionary<string, float>();
        animatorSaveData.intParameters = new Dictionary<string, int>();
        animatorSaveData.boolParameters = new Dictionary<string, bool>();
        animatorSaveData.triggerParameters = new Dictionary<string, string>();

        // Save the current state and its time
        AnimatorStateInfo currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        animatorSaveData.currentState = currentStateInfo.fullPathHash.ToString();
        animatorSaveData.currentStateTime = currentStateInfo.normalizedTime;

        // Save all float parameters
        foreach (AnimatorControllerParameter parameter in animator.parameters) {
            switch (parameter.type) {
                case AnimatorControllerParameterType.Float:
                    animatorSaveData.floatParameters[parameter.name] = animator.GetFloat(parameter.name);
                    break;
                case AnimatorControllerParameterType.Int:
                    animatorSaveData.intParameters[parameter.name] = animator.GetInteger(parameter.name);
                    break;
                case AnimatorControllerParameterType.Bool:
                    animatorSaveData.boolParameters[parameter.name] = animator.GetBool(parameter.name);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    // Triggers cannot be read directly, so we just store that they were set
                    if (animator.GetBool(parameter.name)) // Assuming trigger is set if its bool representation is true
                    {
                        animatorSaveData.triggerParameters[parameter.name] = parameter.name;
                    }
                    break;
            }
        }

        // Save the animatorSaveData using Easy Save 3 or any other serialization method
        ES3.Save("animatorSaveData", animatorSaveData, filePath);
    }

    public void LoadAnimatorState(string filePath) {
        // Load the animatorSaveData using Easy Save 3 or any other deserialization method
        animatorSaveData = ES3.Load<AnimatorSaveData>("animatorSaveData", filePath);

        // Restore all float parameters
        foreach (KeyValuePair<string, float> parameter in animatorSaveData.floatParameters) {
            animator.SetFloat(parameter.Key, parameter.Value);
        }

        // Restore all int parameters
        foreach (KeyValuePair<string, int> parameter in animatorSaveData.intParameters) {
            animator.SetInteger(parameter.Key, parameter.Value);
        }

        // Restore all bool parameters
        foreach (KeyValuePair<string, bool> parameter in animatorSaveData.boolParameters) {
            animator.SetBool(parameter.Key, parameter.Value);
        }

        // Restore all trigger parameters
        foreach (KeyValuePair<string, string> parameter in animatorSaveData.triggerParameters) {
            animator.SetTrigger(parameter.Key);
        }

        // Restore the current state
        animator.Play(animatorSaveData.currentState, 0, animatorSaveData.currentStateTime);
    }
}
