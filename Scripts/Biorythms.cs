//// ------------------------------------------------------------------------------
////  <autogenerated>
////      This code was generated by a tool.
////      Mono Runtime Version: 4.0.30319.1
//// 
////      Changes to this file may cause incorrect behavior and will be lost if 
////      the code is regenerated.
////  </autogenerated>
//// ------------------------------------------------------------------------------
//using System;
//using UnityEngine;
//using System.Collections;
//
//namespace AssemblyCSharp
//{
//	public class Biorythms : MonoBehaviour
//	{
//		Animator _animationControl;
//
//		void Start() {
//			// get the sun object
//			var sun = GameObject.Find("DayNight");
//
//			// get the cycle control behaviour
//			var cycle = sun.GetComponent<DayNightCycle>();
//
//			// attach to the event
//			cycle.HourChanged += HandleHourChanged;
//
//			// get the reference to the animator (not 3)
//			_animationControl.SetInteger("ActionID", 3);
//		}
//
//		IEnumerator HandleHourChanged (DayNightCycle sender, int newHour, int newMinutes)
//		{
//			return HandleHourChanged(sender, newHour, newMinutes);
//		}
//	}
//}
//