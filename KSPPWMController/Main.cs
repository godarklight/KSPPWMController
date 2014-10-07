using System;
using System.Collections.Generic;
using UnityEngine;

namespace KSPPWMController
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class Main : MonoBehaviour
    {
        //Input controls
        public float engineThrottle = 0f;
        public int engineHz = 5;
        //Freq bounds
        private const int MAX_HZ = 20;
        private const int MIN_HZ = 1;
        //Display Tracking
        private float lastEngineThrottle = 0f;
        private ScreenMessage throttleDisplay = null;
        private int lastEngineHz = 0;
        private ScreenMessage hzDisplay = null;
        //HZ limiter
        private float lastHzSet = float.MinValue;
        //State tracking
        private bool engineState = false;
        private float lastSwitch = float.MinValue;
        private float offTime = .2f;
        private float onTime = 0f;

        private void FixedUpdate()
        {
            if (FlightGlobals.ready && !FlightGlobals.fetch.activeVessel.packed)
            {
                HandleInputs();
                DisplayChanges();
                SetThrottle();
            }
        }

        private void SetThrottle()
        {
            if (engineState)
            {
                if ((Time.realtimeSinceStartup > (lastSwitch + onTime)) && (offTime != 0f))
                {
                    engineState = false;
                    lastSwitch = Time.realtimeSinceStartup;
                    FlightInputHandler.state.mainThrottle = 0f;
                }
            }
            else
            {
                if ((Time.realtimeSinceStartup > (lastSwitch + offTime)) && (onTime != 0f))
                {
                    engineState = true;
                    lastSwitch = Time.realtimeSinceStartup;
                    FlightInputHandler.state.mainThrottle = 1f;
                }
            }

            if (engineState && FlightInputHandler.state.mainThrottle != 1f)
            {
                FlightInputHandler.state.mainThrottle = 1f;
            }
            if (!engineState && FlightInputHandler.state.mainThrottle != 0f)
            {
                FlightInputHandler.state.mainThrottle = 0f;
            }
        }

        private void DisplayChanges()
        {
            //Display throttle changes
            if (engineThrottle != lastEngineThrottle)
            {
                lastEngineThrottle = engineThrottle;
                if (throttleDisplay != null)
                {
                    throttleDisplay.duration = 0f;
                    throttleDisplay = null;
                }
                throttleDisplay = ScreenMessages.PostScreenMessage("New throttle setting: " + (int)(engineThrottle * 100) + "%", 3f, ScreenMessageStyle.UPPER_CENTER);
                CalculateOnOffTimes();
            }

            //Display freq changes
            if (engineHz != lastEngineHz)
            {
                lastEngineHz = engineHz;
                if (hzDisplay != null)
                {
                    hzDisplay.duration = 0f;
                    hzDisplay = null;
                }
                hzDisplay = ScreenMessages.PostScreenMessage("New HZ setting: " + engineHz + " hz", 3f, ScreenMessageStyle.UPPER_CENTER);
                CalculateOnOffTimes();
            }
        }

        private void CalculateOnOffTimes()
        {
            float totalTime = 1 / (float)engineHz;
            onTime = totalTime * engineThrottle;
            offTime = totalTime - onTime;
        }

        private void HandleInputs()
        {
            if (FlightGlobals.ready && !FlightGlobals.fetch.activeVessel.packed)
            {
                if (GameSettings.THROTTLE_CUTOFF.GetKey())
                {
                    engineThrottle = 0f;
                }

                if (GameSettings.MODIFIER_KEY.GetKey())
                {
                    if (Time.realtimeSinceStartup > (lastHzSet + 0.5f))
                    {
                        if (GameSettings.THROTTLE_UP.GetKey())
                        {
                            lastHzSet = UnityEngine.Time.realtimeSinceStartup;
                            engineHz++;
                            if (engineHz > MAX_HZ)
                            {
                                engineHz = MAX_HZ;
                            }
                        }
                        if (GameSettings.THROTTLE_DOWN.GetKey())
                        {
                            lastHzSet = UnityEngine.Time.realtimeSinceStartup;
                            engineHz--;
                            if (engineHz < MIN_HZ)
                            {
                                engineHz = MIN_HZ;
                            }
                        }
                    }
                }
                else
                {
                    //3 seconds to full throttle?
                    if (GameSettings.THROTTLE_UP.GetKey())
                    {
                        engineThrottle = engineThrottle + (0.333f * Time.deltaTime);
                        if (engineThrottle > 1f)
                        {
                            engineThrottle = 1f;
                        }
                    }

                    if (GameSettings.THROTTLE_DOWN.GetKey())
                    {
                        engineThrottle = engineThrottle - (0.333f * Time.deltaTime);
                        if (engineThrottle < 0f)
                        {
                            engineThrottle = 0f;
                        }
                    }
                }
            }
        }
    }
}

