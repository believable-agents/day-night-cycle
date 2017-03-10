using UnityEngine;
using System.Collections;
using DayNight;
using System;

public class DayNightCycle : MonoBehaviour {

	public const string GameObjectName = "DayNight";

	public delegate IEnumerator HourChangedDelegate(DayNightCycle sender, int newHour, int newMinutes);
	public delegate IEnumerator SunRiseDelegate(DayNightCycle sender, float currentTime);
	public delegate IEnumerator SunSetDelegate(DayNightCycle sender, float currentTime);

	public event Action<DayNightCycle, int, int> HourChanged;
	public event SunRiseDelegate SunRise;
	public event SunSetDelegate SunSet;

	public float DayInMinutes; //{ get; set; }
	public float NightInMinutes; //{ get; set; }

	public int Hour { get { return (int) SunTime; } }
	public int Minute { get { return (int) (SunTime % 1 * 60); } }
	public TimeData CurrentTime { get { return new TimeData (SunTime); } }

	GameObject Sun;
	GameObject Moon;

	public float[] SunCycle = new float[4];		// Stores times for start of sunrise, end of sunrise, start of sunset, and end of sunset
	public float[] MoonCycle = new float[4];	// Same as SunCycle, but for the moon
	public Material DayNightSkybox;
	
	public Color SunColour;			// Regular colour of the sun
	public Color SunRiseColour;		// Colour the sun fades from/to when rising/setting
	public Color MoonColour;		// Regular colour of the moon
	public Color MoonRiseColour;	// Colour the moon fades from/to when rising/setting
	
	public float SunTime = 0f;
	float MoonTime;
	float DaySpeed = 1f;
	float NightSpeed = 10f;
	float CurrentSpeed;

	[System.NonSerialized]
	private int PreviousHour = 0;
	[System.NonSerialized]
	private int PreviousMinutes = 0;
	[System.NonSerialized]
	public bool Pause = false;
	[System.NonSerialized]
	public float TransitionTime = 0f;
	//We use this to slow time back down just BEFORE the
	//sun rises, to keep the whole day at regular time.
	[System.NonSerialized]
	public float SunriseOffset;
	public float Multiplier = 1f;

	public bool MoveSun = true;
	public bool ShowTime;
	public void SetTime(float time) {
		SunTime = time;
		Start();
	}

	// Use this for initialization
	void Start () {
		Multiplier = 1f;
		Sun = transform.FindChild("sun").gameObject;
		Moon = transform.FindChild("moon").gameObject;
		MoonTime = SunTime + 12f;
		DaySpeed = ((SunCycle[3] - SunCycle[0]) / DayInMinutes) * 60f;
		NightSpeed = ((SunCycle[0] + (24f - SunCycle[3])) / NightInMinutes) * 60f;
		CurrentSpeed = DaySpeed;
        SunriseOffset = ((DaySpeed + NightSpeed) / 2) / 3600;
		
		if (DayInMinutes == 0 || !MoveSun) {
			return;
		}
		
		Sun.GetComponent<Light>().color = SunColour;
		Sun.GetComponent<Light>().intensity = 0.6f;
		Sun.GetComponent<Light>().shadowStrength = 1;
		Moon.GetComponent<Light>().color = MoonColour;
		Moon.GetComponent<Light>().intensity = 0.1f;
		Moon.GetComponent<Light>().shadowStrength = 0.2f;

		if(SunTime < SunCycle[0] || SunTime > SunCycle[3]) {
			DayNightSkybox.SetFloat("_BlendNight", 1);
			DayNightSkybox.SetFloat("_BlendDusk", 0);
		}
		else if(SunTime < SunCycle[2] || SunTime > SunCycle[1]) {
			DayNightSkybox.SetFloat("_BlendNight", 0);
			DayNightSkybox.SetFloat("_BlendDusk", 0);
		}

		
	}
	
	// Update is called once per frame
	void Update () {
		if(Pause) {
			return;
		}

		//Time is stored as hours with decimals rather than seconds. At a speed of 1, a full cycle will take 24 hours.
		//Sun and Moon have different timers
		SunTime += (Time.deltaTime / 3600) * CurrentSpeed;
		while(SunTime > 24f) {
			SunTime -= 24f;
		}
		MoonTime += (Time.deltaTime / 3600) * CurrentSpeed;
		while(MoonTime > 24f) {
			MoonTime -= 24f;
		}

        if (PreviousHour < Mathf.Floor(SunTime) ||
           PreviousHour > Mathf.Floor(SunTime) ||
           (PreviousHour == Mathf.Floor(SunTime) && Minute == PreviousMinutes + 15))
        {

            if (PreviousHour != Mathf.Floor(SunTime))
            {
                PreviousMinutes = 0;
            }
            else {
                PreviousMinutes = Minute;
            }
            PreviousHour = (int)SunTime;

            //			Debug.Log("Announcing: " + PreviousHour + ":" + PreviousMinutes);

            if (HourChanged != null)
            {
                HourChanged(this, Hour, Minute);
            }
        }
		
		if (ShowTime) {
			DebugConsole.Clear();
			DebugConsole.Log(string.Format("{0:00}:{1:00}", Hour, Minute));
		}
		
		if (DayInMinutes == 0 || !MoveSun) {
			return;
		}

        //--- Sun Cycle ---//
        // If the time is before sunrise, rotate the sun between straight down, and the east horizon
        if (SunTime < SunCycle[0] && this.MoveSun) {
			Sun.transform.rotation = Quaternion.Euler( 90 * (SunTime - SunCycle[0]) / SunCycle[0], 0, 0);
		}
		// If the time is after sunset, rotate the sun between the west horizon, and straight down
		if(SunTime > SunCycle[3] && this.MoveSun) {
			Sun.transform.rotation = Quaternion.Euler(180 + 90 * (SunTime - SunCycle[3]) / (24 - SunCycle[3]), 0, 0);
			// TransitionTime is used to smoothly change the speed of time at sunrise and sunset, over the course of one realtime second
			// Check if the day-to-night transition has already completed, and move it along if it hasn't
			if( TransitionTime <= 1f) {
				TransitionTime += Time.deltaTime;
				if(TransitionTime > 1f) {
					TransitionTime = 1f;
				}
			}
		}
		// If the sun is about to rise, then we start shifting the speed of time back to the speed during daylight hours
		else if(SunTime > SunCycle[0] - SunriseOffset && TransitionTime > 0f) {
			TransitionTime -= Time.deltaTime;
			if(TransitionTime < 0f) {
				TransitionTime = 0f;
			}
		}

		// Calculate the speed of time, based on what time of day it is, and what part of the transition we're in
		CurrentSpeed = (DaySpeed + ((NightSpeed - DaySpeed) * TransitionTime)) * Multiplier;

        // Check if we are between sunrise and sunset. The last bit is so we have one update after sunset, to turn everything off

        if (SunTime > SunCycle[0] && SunTime < SunCycle[3] + ((Time.deltaTime / 3600) * CurrentSpeed)) {

			// If the sun isn't turned on, turn it on
			if(!Sun.GetComponent<Light>().enabled)
			{
				Sun.GetComponent<Light>().enabled = true;
				// If anyone is listening, trigger the "SunRise" event
				if(SunRise != null) {
					StartCoroutine(SunRise(this, SunTime));
				}
				DebugConsole.Log("Sun is rising");
			}

			// Rotate the sun from one horizon to the other over the daytime period
			Sun.transform.rotation = Quaternion.Euler(180 * (SunTime - SunCycle[0]) / (SunCycle[3] - SunCycle[0]), 0, 0);
			
			// Check if the sun is rising 
			if(SunTime < SunCycle[1] + ((Time.deltaTime / 3600) * CurrentSpeed)) {
				//Slowly phase the sun in, and change its colour
				Sun.GetComponent<Light>().intensity = 0.5f * ((SunTime - SunCycle[0]) / (SunCycle[1] - SunCycle[0]));
				Sun.GetComponent<Light>().shadowStrength = 1f * ((SunTime - SunCycle[0]) / (SunCycle[1] - SunCycle[0]));
				Sun.GetComponent<Light>().color = Color.Lerp(SunRiseColour, SunColour, ((SunTime - SunCycle[0]) / (SunCycle[1] - SunCycle[0])));
				
				float time = ((SunTime - SunCycle[0]) / (SunCycle[1] - SunCycle[0]));
				float dusk = 2 * (0.5f - Math.Abs(0.5f - time));
				
				DayNightSkybox.SetFloat("_BlendDusk", dusk);
				DayNightSkybox.SetFloat("_BlendNight", Mathf.Clamp(1 - (2 * time), 0f, 1f));
			}
			
			// Otherwise, check if the sun is setting
			else if(SunTime > SunCycle [2] + ((Time.deltaTime / 3600) * CurrentSpeed)) {
				// Slowly phase the sun out, and change its colour
				Sun.GetComponent<Light>().intensity = 0.5f - 0.5f * ((SunTime - SunCycle[2]) / (SunCycle[3] - SunCycle[2]));
				Sun.GetComponent<Light>().shadowStrength = Mathf.Clamp(1f - 1f * ((SunTime - SunCycle[2]) / (SunCycle[3] - SunCycle[2])), 0, 1);
				Sun.GetComponent<Light>().color = Color.Lerp(SunColour, SunRiseColour, ((SunTime - SunCycle[2]) / (SunCycle[3] - SunCycle[2])));
				
				float time = ((SunTime - SunCycle[2]) / (SunCycle[3] - SunCycle[2]));
				float dusk = 2 * (0.5f - Math.Abs(0.5f - time));
				
				DayNightSkybox.SetFloat("_BlendDusk", dusk);
				DayNightSkybox.SetFloat("_BlendNight", Mathf.Clamp(2 * (time - 0.5f), 0f, 1f));
			}
		}
		// If the sun isn't used, turn off the light
		else {
			if(Sun.GetComponent<Light>().enabled)
			{
				Sun.GetComponent<Light>().enabled = false;
				// If anyone is listening, trigger the "SunSet" event
				if(SunSet != null) {
					StartCoroutine(SunSet(this, SunTime));
				}
				DebugConsole.Log("Sun is Setting");
			}
		}
		
		//--- Moon Cycle ---//
		// If the time is before moonrise, rotate the moon between straight down, and the east horizon
		if(MoonTime < MoonCycle[0] && this.MoveSun) {
			Moon.transform.rotation = Quaternion.Euler(90 * (MoonTime - MoonCycle[0]) / MoonCycle[0], 0, 0);
		}

		// If the time is after moonset, rotate the moon between the west horizon, and straight down
		if(MoonTime > MoonCycle[3] && this.MoveSun) {
			Moon.transform.rotation = Quaternion.Euler(180 + 90 * (MoonTime - MoonCycle[3]) / (24 - MoonCycle[3]), 0, 0);
		}

		// Check if we are between moonrise and moonset. The last bit is so we have one update after moonset, to turn everything off
		if(MoonTime > MoonCycle[0] && MoonTime < MoonCycle[3] + ((Time.deltaTime / 3600) * CurrentSpeed)) {
			Moon.GetComponent<Light>().enabled = true;

			//Rotate the moon from one horizon to the other over the nighttime period
		    if (this.MoveSun)
		    {
		        Moon.transform.rotation = Quaternion.Euler(180*(MoonTime - MoonCycle[0])/(MoonCycle[3] - MoonCycle[0]), 0, 0);
		    }

		    //Check if the moon is rising
			if(MoonTime < MoonCycle[1] + ((Time.deltaTime / 3600) * CurrentSpeed)) {
				//Slowly phase the moon in, and change its colour
				Moon.GetComponent<Light>().intensity = 0.1f * ((MoonTime - MoonCycle[0]) / (MoonCycle[1] - MoonCycle[0]));
				Moon.GetComponent<Light>().shadowStrength = 0.2f * ((MoonTime - MoonCycle[0]) / (MoonCycle[1] - MoonCycle[0]));
				Moon.GetComponent<Light>().color = Color.Lerp(MoonRiseColour, MoonColour, ((MoonTime - MoonCycle[0]) / (MoonCycle[1] - MoonCycle[0])));
				
			}
			
			//Otherwise, check if the moon is setting
			else if(MoonTime > MoonCycle [2] + ((Time.deltaTime / 3600) * CurrentSpeed)) {
				//Slowly phase the moon out, and change its colour
				Moon.GetComponent<Light>().intensity = 0.1f - 0.1f * ((MoonTime - MoonCycle[2]) / (MoonCycle[3] - MoonCycle[2]));
				Moon.GetComponent<Light>().shadowStrength = 0.2f - 0.2f * ((MoonTime - MoonCycle[2]) / (MoonCycle[3] - MoonCycle[2]));
				Moon.GetComponent<Light>().color = Color.Lerp(MoonColour, MoonRiseColour, ((MoonTime - MoonCycle[2]) / (MoonCycle[3] - MoonCycle[2])));
			}
		}
		//If the moon isn't used, turn off the light
		else {
			Moon.GetComponent<Light>().enabled = false;
		}

		// If we have reached a new hour, or quarter of an hour, and anyone is listening, trigger the HourChanged event
//		Debug.Log (minutes);

		

	}
}
