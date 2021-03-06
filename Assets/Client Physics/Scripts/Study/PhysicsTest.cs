﻿using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

/// <summary>
/// Handles the logic/loop of the Physics User Study
/// </summary>
public class PhysicsTest : MonoBehaviour {
    public string testFolder;
	public bool righthanded = true;
	public int participantID = 0;
	public LatencyHandler latencyHandler;
	public PIDTuning.TestRunner pidTestRunner;
	public float testDurationInS;
	public int noLatency = 0;
	public int mediumLatency = 125;
	public int highLatency = 350;
	public int chosenLatency = 0;
	public Transform phaseHand;
	public Transform phaseFoot;
    public FootPhaseResetPlate footReset;

    [Header("UI")]
    public List<Text> handUI = new List<Text>();
    public Text countdownDisplay;
    public Text remainingTimeDisplay;
    public Text completionsDisplay;

    List<int> handNumbers = new List<int>();

    PHASE phase = PHASE.HAND;
	float timeUntilCompletion, phaseRunTime;
	bool run, saved;

	List<SuccessfullTask> handCompletions = new List<SuccessfullTask>();
	List<SuccessfullTask> footCompletions = new List<SuccessfullTask>();

    [Header("Bounds")]
    public CheckBound[] handBounds;
	public CheckBound[] footBounds;
    [Header("Finish")]
    public GameObject finishHand;
	public GameObject finishFoot;

    Vector3 handFinishAtStart;

    public enum PHASE
    {
        HAND,
        FOOT
    }

    // Use this for initialization
    void Start() {

	}

    void ClearMeasuredTime()
    {
        timeUntilCompletion = 0;
        foreach (CheckBound bound in handBounds)
        {
            bound.RemoveBodyMeasurements();
            bound.timeSpent = 0f;
            bound.contacts = new List<Collider>();
            bound.timesPerBodyParts = new Dictionary<string, float>();
        }
        foreach (CheckBound bound in footBounds)
        {
            bound.RemoveBodyMeasurements();
            bound.timeSpent = 0f;
            bound.contacts = new List<Collider>();
            bound.timesPerBodyParts = new Dictionary<string, float>();
        }

    }

    void StartTest(int latency)
    {
        ClearMeasuredTime();
        chosenLatency = latency;
        latencyHandler.latency_ms = chosenLatency;
        phase = PHASE.HAND;
        saved = false;
        StartCoroutine(StartPhaseInS(15f));
    }


	void OnEnable()
	{
		//SetActiveBounds(handBounds, false);
		//SetActiveBounds(footBounds, false);
		if (righthanded)
		{
			PrepareRightHanded(phaseHand);
			//PrepareRightHanded(phaseFoot);
		}
        handFinishAtStart = new Vector3(finishHand.transform.position.x, finishHand.transform.position.y, finishHand.transform.position.z);
		participantID = Random.Range(0, 100000);

        for(int i = 1; i <= 4; i++)
        {
            handNumbers.Add(i);
        }
	}

    /// <summary>
    /// Enables CheckBounds to measure time, activates colliders
    /// </summary>
    /// <param name="bounds"></param>
    /// <param name="active"></param>
	void SetActiveBounds(CheckBound[] bounds, bool active)
	{
		foreach(CheckBound testBound in bounds)
		{
			testBound.measure = active;
            testBound.contacts = new List<Collider>();
           
            foreach(Transform child in testBound.transform)
            {
                if (child.name.Equals("Target"))
                {
                    child.GetComponent<CheckFinish>().trigger = active;

                }
                child.GetComponent<BoxCollider>().enabled = active;
            }
		}
	}

    /// <summary>
    /// Flips environment.
    /// </summary>
    /// <param name="parent"></param>
	void PrepareRightHanded(Transform parent)
	{
		foreach (Transform child in parent)
		{
			child.position = new Vector3(-child.position.x, child.position.y, child.position.z);
		}
	}

	void Update()
	{
        if (Input.GetKeyDown(KeyCode.F))
        {
            phaseFoot.gameObject.SetActive(true);
            phase = PHASE.FOOT;
            StartCoroutine(StartPhaseInS(15f));
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
		{
            StartTest(noLatency);
		}

		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
            StartTest(mediumLatency);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
		{
            StartTest(highLatency);
        }

    }

    /// <summary>
    /// Prepares new task.
    /// </summary>
	void ResetTask()
	{
        ClearMeasuredTime();
        if (phase.Equals(PHASE.HAND))
        {
            handNumbers.Reverse();
            int i = 0;
            foreach(Text number in handUI)
            {
                number.text = "" + handNumbers[i];
                i++;
            }
            finishHand.GetComponent<CheckFinish>().trigger = false;
            finishHand.transform.position = new Vector3(-finishHand.transform.position.x, finishHand.transform.position.y, finishHand.transform.position.z);
            finishHand.GetComponent<CheckFinish>().trigger = true;
        }
        else
        {
            finishFoot.GetComponent<CheckFinish>().trigger = false;
            SetActiveBounds(footBounds, false);
            phaseFoot.gameObject.SetActive(false);
            run = false;
            footReset.gameObject.SetActive(true);
        }
	}

    /// <summary>
    /// Continues the foot phase after task completion
    /// </summary>
    public void OnFootReset()
    {
        phaseFoot.gameObject.SetActive(true);
        foreach (CheckBound bound in footBounds)
        {
            bound.gameObject.GetComponent<BoxCollider>().enabled = true;
        }
        SetActiveBounds(footBounds, true);
        run = true;
        finishFoot.GetComponent<CheckFinish>().trigger = true;
    }


    IEnumerator StartPhaseInS(float timeLeft)
    {
        run = false;
        countdownDisplay.enabled = true;
        completionsDisplay.text = "" + 0;

        if (phase.Equals(PHASE.HAND))
        {
            phaseFoot.gameObject.SetActive(false);
        }

        while (timeLeft != 0)
        {
            countdownDisplay.text = "" + timeLeft;
            yield return new WaitForSeconds(1.0f);
            timeLeft--;
        }

        countdownDisplay.enabled = false;

        pidTestRunner.gameObject.SetActive(true);
        pidTestRunner.ResetTestRunner();
        pidTestRunner.StartManualRecord();
        run = true;

        ClearMeasuredTime();
        if (phase.Equals(PHASE.HAND))
        {
            handNumbers.Sort();
            int i = 0;
            foreach(Text text in handUI)
            {
                text.text = "" + handNumbers[i];
                i++;
            }

            finishFoot.GetComponent<CheckFinish>().trigger = false;
            SetActiveBounds(footBounds, false);
            phaseFoot.gameObject.SetActive(false);

            finishHand.transform.position = handFinishAtStart;
            phaseHand.gameObject.SetActive(true);
            SetActiveBounds(handBounds, true);
            finishHand.GetComponent<CheckFinish>().trigger = true;
        }
        else
        {
            finishHand.GetComponent<CheckFinish>().trigger = false;
            SetActiveBounds(handBounds, false);
            phaseHand.gameObject.SetActive(false);

            phaseFoot.gameObject.SetActive(true);
            SetActiveBounds(footBounds, true);
            finishFoot.GetComponent<CheckFinish>().trigger = true;
        }

        StartCoroutine(DisplayRemainingTime());

    }

    /// <summary>
    /// Stores successful Completions
    /// </summary>
    /// <param name="phase"></param>
    public void OnFinishLineReached(PHASE phase)
	{
		Dictionary<string, CheckBoundValueStorage> successFullBounds = new Dictionary<string, CheckBoundValueStorage>();
		CheckBound[] testBounds;
		List<SuccessfullTask> completions;
		if (phase.Equals(PHASE.HAND))
		{
            finishHand.GetComponent<BoxCollider>().enabled = false;
			testBounds = handBounds;
			completions = handCompletions;
		}
		else
		{
            foreach (CheckBound bound in footBounds)
            {
                bound.gameObject.GetComponent<BoxCollider>().enabled = false;
            }

            finishFoot.GetComponent<BoxCollider>().enabled = false;
            testBounds = footBounds;
			completions = footCompletions;
		}
		foreach(CheckBound bound in testBounds)
		{
			successFullBounds.Add(bound.gameObject.name, new CheckBoundValueStorage(bound.timeSpent, bound.timesPerBodyParts));
		}

		SuccessfullTask task = new SuccessfullTask(successFullBounds, timeUntilCompletion);

        if (timeUntilCompletion > 1)
        {
            completions.Add(task);
            completionsDisplay.text = "" + completions.Count();
        }

        finishHand.GetComponent<BoxCollider>().enabled = true;
        finishFoot.GetComponent<BoxCollider>().enabled = true;

        ResetTask();
	}

    /// <summary>
    /// UI: Countdown
    /// </summary>
    /// <returns></returns>
    IEnumerator DisplayRemainingTime()
    {
        remainingTimeDisplay.enabled = true;
        remainingTimeDisplay.text = "" + testDurationInS;
        while (phaseRunTime < testDurationInS)
        {
            int time = (int)Mathf.Round(testDurationInS - phaseRunTime);
            remainingTimeDisplay.text = "" + time;
            yield return null;
        }
        remainingTimeDisplay.enabled = false;
    }

	// Update is called once per frame
	void FixedUpdate()
	{
		if (run)
		{
			phaseRunTime += Time.deltaTime;
            timeUntilCompletion += Time.deltaTime;
			if (phaseRunTime > testDurationInS)
			{
                phaseRunTime = 0;
                run = false;

                StopCoroutine("DisplayRemainingTime");
                StopCoroutine("StartPhaseInS");

				if (phase.Equals(PHASE.HAND))
				{
                    //Start foot test
					phaseHand.gameObject.SetActive(false);
                    //SetActiveBounds(footBounds, false);
                    Debug.Log("hand phase done");
                    SaveResults();
					//StartCoroutine(StartPhaseInS(15f));
				}
				else
                {
                    if (!saved)
                    {
                        Debug.Log("foot phase done");
                        saved = true;
                        //End Test & Save
                        TestWrapUp();
                    }
                }
                remainingTimeDisplay.enabled = false;
            }
		}
	}

	public void TestWrapUp()
	{
        finishFoot.GetComponent<CheckFinish>().trigger = false;
        SetActiveBounds(footBounds, false);
        phaseFoot.gameObject.SetActive(false);

        SaveResults();
        footCompletions = new List<SuccessfullTask>();
        handCompletions = new List<SuccessfullTask>();
	}

	void SaveResults()
	{
		string id = chosenLatency + "-" + participantID;
        char directorySeparator = Path.DirectorySeparatorChar;

        string dataPath = testFolder.Replace('/', directorySeparator);
		string folder = Path.Combine(dataPath, "PhysicsTest" + directorySeparator + participantID + directorySeparator + chosenLatency + directorySeparator + phase);

		Directory.CreateDirectory(folder);

		File.WriteAllText(Path.Combine(folder, "physics_test" + "_" + chosenLatency + ".json"), ToJson().ToString());

		pidTestRunner.StopManualRecord();
		pidTestRunner.SaveTestData(true, folder);

        handCompletions = new List<SuccessfullTask>();
        footCompletions = new List<SuccessfullTask>();
	}

	JObject ToJson()
	{
		JObject json = new JObject();

		json["ID"] = participantID;
		json["latency"] = chosenLatency;
        
		if(handCompletions.Count > 0)
		{
			Dictionary<string, float> averageTimeInSections = GetAverageTimeInSections(handCompletions);
			Dictionary<string, float>.KeyCollection keys = averageTimeInSections.Keys;
			foreach (string section in keys)
			{
				json[section] = averageTimeInSections[section];
			}
		}

		if (footCompletions.Count > 0)
		{
			Dictionary<string, float> averageTimeInSections = GetAverageTimeInSections(footCompletions);
			Dictionary<string, float>.KeyCollection keys = averageTimeInSections.Keys;
			foreach (string section in keys)
			{
				json[section] = averageTimeInSections[section];
			}
		}

		for (int i = 0; i < handCompletions.Count; i++)
		{
			json["hand_" + i] = handCompletions[i].ToJson();
		}

		for (int i = 0; i < footCompletions.Count; i++)
		{
			json["foot_" + i] = footCompletions[i].ToJson();
		}

		return json;
	}
	class SuccessfullTask
    {
        float completionTime;
        Dictionary<string, CheckBoundValueStorage> bounds;

        public SuccessfullTask(Dictionary<string, CheckBoundValueStorage> bounds, float completionTime)
        {
            this.bounds = bounds;
            this.completionTime = completionTime;
        }

        public JObject ToJson()
		{
			JObject json = new JObject();
			json["timeUntilCompletion"] = completionTime;

            foreach (string currentBound in bounds.Keys)
			{
				json[currentBound] = bounds[currentBound].timeSpent;
                foreach (string touchedObj in bounds[currentBound].timesPerBodyParts.Keys)
                {
                    json[touchedObj + " involved in " + currentBound] = new BodyPartEvaluation(bounds[currentBound], currentBound, touchedObj).ToJson();
                }
			}

			return json;
		}

        class BodyPartEvaluation
        {
            CheckBoundValueStorage storage;
            string bodyPart;
            string currentBound;
            public BodyPartEvaluation(CheckBoundValueStorage storage, string currentBound, string bodyPart)
            {
                this.storage = storage;
                this.bodyPart = bodyPart;
                this.currentBound = currentBound;
            }

            public JObject ToJson()
            {
                JObject json = new JObject();
                if (storage.timeSpent > 0f && storage.timesPerBodyParts[bodyPart] > 0f)
                {
                    json[currentBound + "_" + bodyPart] = storage.timesPerBodyParts[bodyPart];
                    json[currentBound + "_" + bodyPart + " in %"] = (storage.timesPerBodyParts[bodyPart] / storage.timeSpent) * 100f + "%";
                }
                return json;
            }
        }

		public Dictionary<string, CheckBoundValueStorage> GetBounds()
		{
			return bounds;
		}
    }

    class CheckBoundValueStorage
    {
        public float timeSpent;
        public Dictionary<string, float> timesPerBodyParts = new Dictionary<string, float>();

        public CheckBoundValueStorage(float timeSpent, Dictionary<string, float> timesPerBodyParts)
        {
            this.timeSpent = timeSpent;
            this.timesPerBodyParts = timesPerBodyParts;
        }
    }

    Dictionary<string, float> GetAverageTimeInSections(List<SuccessfullTask> tasks)
	{
		Dictionary<string, float> averageTimeInSections = new Dictionary<string, float>();
        List<string> keys = new List<string>();
		foreach(string sectionName in tasks[0].GetBounds().Keys)
		{
			keys.Add(sectionName);
		}
		
		foreach (string section in keys) {
			float totalTimeInSection = 0;
			foreach (SuccessfullTask success in tasks)
			{
                totalTimeInSection += success.GetBounds()[section].timeSpent;
			}
			averageTimeInSections.Add(section, totalTimeInSection / tasks.Count);
		}

		return averageTimeInSections;
	}
}
