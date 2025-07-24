using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class N1Scenario : MonoBehaviour
{
    private void Start()
    {
        AudioManager.Instance.PlayBGM("51StageBGM");
        ADVManager.Instance.StartScenario("NStory1_Start");
    }

    public void MiddleStart()
    {
        ADVManager.Instance.StartScenario("NStory1_Middle");
    }
}
