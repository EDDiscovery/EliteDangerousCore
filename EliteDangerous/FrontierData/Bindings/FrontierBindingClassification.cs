/*
 * Copyright 2026 - 2026 EDDiscovery development team
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under
 * the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
 * ANY KIND, either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EliteDangerousCore
{
    static public class FrontierBindingClassification
    {
        // matches frontier options screen
        public enum Classification
        {
            // page 1
            InterfaceMode, GalaxyMap, CameraSuite, FreeCam, HoloMe, PlayList, StoreCamera, SystemColonisation,

            // page 2 ship
            MouseControls, FlightRotation, FlightThrust, AlternateFlightControls, FlightThrottle, FlightLandingOverrides, FlightMiscellaneous,     
            Targetting, Weapons, Cooling, ShipMiscellaneous, ModeSwitches, HeadlookMode,
            MultiCrew, FightersOrders, FSS, SAA,

            // page 3 srv
            Driving, DrivingTargeting, DrivingTurretControls, DriveThrottle, DrivingMiscellaneous, DrivingModeSwitches, 

            // page 4 onfoot
            OnFoot, OnFootModeSwitches, OnFootEmotes,

            Misc,
        };

        // modal state
        public enum Mode
        {
            Ship, Landing, UIPanel, GalaxyMap, Camera, FreeCamera, OnFoot, OnFootWheel, 
            SRV, SRVTurret, All, FSS, SAA, HoloMe, Store, Colonisation, ColonisationSuite, MultiCrew
        }

        // get class and mode of an action
        public static Tuple<Classification, Mode> GetKeyClass(string actionName)
        {
            if (discrete.TryGetValue(actionName, out Tuple<Classification,Mode> classification))        // direct for ones without any sort of pattern
                return classification;

            // page 1

            if (actionName.StartsWith("UI") || (actionName.StartsWith("Cycle") && (actionName.EndsWith("Panel") || actionName.EndsWith("Page"))))
                return new Tuple<Classification, Mode>(Classification.InterfaceMode, Mode.UIPanel);
            else if (actionName.StartsWith("Cam"))
                return new Tuple<Classification, Mode>(Classification.GalaxyMap, Mode.GalaxyMap);
            else if (actionName.StartsWith("VanityCamera"))
                return new Tuple<Classification, Mode>(Classification.CameraSuite, Mode.Camera);
            else if (actionName.Contains("FreeCam") || actionName.StartsWith("PitchCamera") || actionName.StartsWith("YawCamera") || actionName.StartsWith("RollCamera")
                        || actionName == "ToggleRotationLock" || actionName.StartsWith("FixCamera") || actionName == "QuitCamera" || actionName == "ToggleAdvanceMode"
                        || actionName.StartsWith("FStop")
                )
                return new Tuple<Classification, Mode>(Classification.FreeCam, Mode.FreeCamera);
            else if (actionName.StartsWith("CommanderCreat"))
                return new Tuple<Classification, Mode>(Classification.HoloMe, Mode.HoloMe);
            else if (actionName.StartsWith("GalnetAudio"))
                return new Tuple<Classification, Mode>(Classification.PlayList, Mode.All);
            else if (actionName.StartsWith("Store"))
                return new Tuple<Classification, Mode>(Classification.StoreCamera, Mode.Store);
            else if (actionName.Contains("Settlement") || actionName.Contains("Construction") || actionName.Contains("Placement"))
                return new Tuple<Classification, Mode>(Classification.SystemColonisation, Mode.Colonisation);

            // page 2

            else if (actionName.Contains("Alternate"))      // must be in front
                return new Tuple<Classification, Mode>(Classification.AlternateFlightControls, Mode.Ship);
            else if (actionName.Contains("Landing"))        // must be in front
                return new Tuple<Classification, Mode>(Classification.FlightLandingOverrides, Mode.Landing);
            else if (actionName.StartsWith("Yaw") || actionName.StartsWith("Roll") || actionName.StartsWith("Pitch"))
                return new Tuple<Classification, Mode>(Classification.FlightRotation, Mode.Ship);
            else if (actionName.Contains("Thrust"))
                return new Tuple<Classification, Mode>(Classification.FlightThrust, Mode.Ship);
            else if (actionName.Contains("Throttle") || actionName == "ForwardKey" || actionName == "BackwardKey" || actionName.StartsWith("SetSpeed"))
                return new Tuple<Classification, Mode>(Classification.FlightThrottle, Mode.Ship);
            else if (actionName.Contains("Target") || actionName.Contains("Threat") || actionName.Contains("Hostile") || actionName == "WingNavLock" || actionName.Contains("Subsystem"))
                return new Tuple<Classification, Mode>(Classification.Targetting, Mode.Ship);
            else if (actionName == "PrimaryFire" || actionName == "SecondaryFire" || actionName.StartsWith("CycleFireGroup") || actionName == "DeployHardpointToggle")
                return new Tuple<Classification, Mode>(Classification.Weapons, Mode.Ship);
            else if (actionName.StartsWith("Radar") || (actionName.StartsWith("Increase") && actionName.EndsWith("Power")))
                return new Tuple<Classification, Mode>(Classification.ShipMiscellaneous, Mode.Ship);        // see also above
            else if ((actionName.StartsWith("Focus") && actionName.EndsWith("Panel")) || actionName.EndsWith("MapOpen"))
                return new Tuple<Classification, Mode>(Classification.ModeSwitches, Mode.Ship);
            else if (actionName.StartsWith("HeadLook"))
                return new Tuple<Classification, Mode>(Classification.HeadlookMode, Mode.All);
            else if (actionName.StartsWith("MultiCrew"))
                return new Tuple<Classification, Mode>(Classification.MultiCrew, Mode.MultiCrew);
            else if (actionName.StartsWith("Order"))
                return new Tuple<Classification, Mode>(Classification.FightersOrders, Mode.Ship);
            else if (actionName.StartsWith("SAA") || actionName.StartsWith("ExplorationSAA"))
                return new Tuple<Classification, Mode>(Classification.SAA, Mode.SAA);
            else if (actionName.StartsWith("ExplorationFSS"))
                return new Tuple<Classification, Mode>(Classification.FSS, Mode.FSS);

            // page 3

            else if (actionName.StartsWith("BuggyTurret"))
                return new Tuple<Classification, Mode>(Classification.DrivingTurretControls, Mode.SRVTurret);
            else if (actionName.StartsWith("IncreaseSpeed") || actionName.StartsWith("DecreaseSpeed"))
                return new Tuple<Classification, Mode>(Classification.DriveThrottle, Mode.SRV);
            else if (actionName.StartsWith("Steer") || actionName.StartsWith("Buggy"))
                return new Tuple<Classification, Mode>(Classification.Driving, Mode.SRV);
            else if (actionName.EndsWith("Panel_Buggy") || actionName.EndsWith("MapOpen_Buggy"))
                return new Tuple<Classification, Mode>(Classification.DrivingModeSwitches, Mode.SRV);

            // page 4

            else if (actionName.EndsWith("Open_Humanoid") || actionName.EndsWith("Panel_Humanoid"))
                return new Tuple<Classification, Mode>(Classification.OnFootModeSwitches, Mode.OnFoot);
            else if (actionName.StartsWith("HumanoidEmote"))
                return new Tuple<Classification, Mode>(Classification.OnFootEmotes, Mode.OnFoot);
            else if (actionName.StartsWith("Humanoid"))     // rest are Onfoot
            {
                if ( actionName.Contains("WheelButton"))
                    return new Tuple<Classification, Mode>(Classification.OnFoot, Mode.OnFootWheel);
                else
                    return new Tuple<Classification, Mode>(Classification.OnFoot, Mode.OnFoot);
            }

            System.Diagnostics.Debug.WriteLine($"Frontier Key Classification Unknown ID {actionName}");
            return new Tuple<Classification, Mode>(Classification.Misc, Mode.All);
        }


        // given a value name, give it the classification of where it is in the menu system
        public static Tuple<Classification, Mode> GetValueClass(string ActionName)
        {
            if (ActionName.StartsWith("MouseTurret") || ActionName.StartsWith("BuggyTurret"))
                return Tuple.Create(Classification.Driving, Mode.SRVTurret);
            if (ActionName.StartsWith("MouseBuggy") || ActionName.StartsWith("Buggy") || ActionName == "DriveAssistDefault" || ActionName == "EnableMenuGroupsSRV")
                return Tuple.Create(Classification.Driving, Mode.SRV);
            if (ActionName.StartsWith("MouseHumanoid") || ActionName.StartsWith("Humanoid") || ActionName.Contains("OnFoot"))
                return Tuple.Create(Classification.OnFoot, Mode.OnFoot);
            if (ActionName.StartsWith("FSS"))
                return Tuple.Create(Classification.FSS, Mode.FSS);
            if (ActionName.StartsWith("SAA"))
                return Tuple.Create(Classification.SAA, Mode.SAA);
            if (ActionName.StartsWith("FreeCam") || ActionName == "ThrottleRangeFreeCam")
                return Tuple.Create(Classification.CameraSuite, Mode.FreeCamera);
            if (ActionName.Contains("Camera"))
                return Tuple.Create(Classification.CameraSuite, Mode.Camera);
            if (ActionName == "YawToRollMode_Landing")
                return new Tuple<Classification, Mode>(Classification.FlightLandingOverrides, Mode.Landing);
            if (ActionName == "DeployHardpointsOnFire")
                return new Tuple<Classification, Mode>(Classification.Weapons, Mode.Ship);
            if (ActionName == "UIFocusMode" || ActionName == "EnableMenuGroups")
                return new Tuple<Classification, Mode>(Classification.ModeSwitches, Mode.Ship);
            if (ActionName.Contains("MuteButton"))
                return Tuple.Create(Classification.ShipMiscellaneous, Mode.Ship);
            if (ActionName.StartsWith("Throttle"))
                return Tuple.Create(Classification.FlightThrottle, Mode.Ship);
            if (ActionName.StartsWith("YawTo"))
                return Tuple.Create(Classification.FlightRotation, Mode.Ship);
            if (ActionName.StartsWith("Mouse"))
                return Tuple.Create(Classification.MouseControls, Mode.Ship);
            if (ActionName.StartsWith("Placement"))
                return Tuple.Create(Classification.SystemColonisation, Mode.Colonisation);
            if (ActionName.StartsWith("Headlook") || ActionName == "MotionHeadlook" || ActionName == "yawRotateHeadlook")
                return Tuple.Create(Classification.HeadlookMode, Mode.All);
            if (ActionName.Contains("PanelFocus") || ActionName == "FocusOnTextEntryField")
                return Tuple.Create(Classification.InterfaceMode, Mode.UIPanel);
            if (ActionName.Contains("MultiCrew"))
                return Tuple.Create(Classification.MultiCrew, Mode.Ship);

            System.Diagnostics.Debug.WriteLine($"Frontier Key Classification Unknown Value {ActionName}");
            return new Tuple<Classification, Mode>(Classification.Misc, Mode.All);
        }

        // given a value name and its value, is there a defined set of values?
        // return is null or a string list compatible with the VariablesForm ComboBoxO
        public static string[] GetValueOptions(string name, string current)
        {
            if (current == "1" || current == "0")
                return new string[] { "0", "Off", "1", "On" };

            else if (name == "YawToRollMode")
                return new string[] { "Bindings_YawIntoRollNone", "Off", "Bindings_YawIntoRollTime", "On Initial Roll", "Bindings_YawIntoRollLowRoll", "On Low Roll" };
            else if (name == "YawToRollMode_FAOff")
                return new string[] { "", "Default to Standard Controls", "Bindings_YawIntoRollNone", "Off", "Bindings_YawIntoRollTime", "On Initial Roll", "Bindings_YawIntoRollLowRoll", "On Low Roll" };
            else if (name == "YawToRollMode_Landing")
                return new string[] { "", "Default to Standard Controls", "Bindings_YawIntoRollNone", "Off", "Bindings_YawIntoRollTime", "On Initial Roll", "Bindings_YawIntoRollLowRoll", "On Low Roll" };
            else if (name.Equals("YawCameraMouse"))
                return new string[] { "", "Off", "Bindings_MouseYaw", "Yaw", "Bindings_MouseRoll", "Roll" };

            else if (name == "CqcMuteButtonMode" || name == "MuteButtonMode")
                return new string[] { "mute_toggle", "Toggle", "mute_pushToTalk", "Push to Talk", "mute_pushToMute", "Push to Mute" };

            else if (name == "UIFocusMode")
                return new string[] { "Bindings_FocusModeToggle", "Cycle", "Bindings_FocusModeHold", "Direction" };
            else if (name.Contains("PanelFocusOptions"))
                return new string[] { "", "Focuses the Panel", "FocusOption_Nothing", "Does Nothing", "FocusOption_Show", "Shows the Panel" };
            else if (name.Contains("HeadlookMode"))
                return new string[] { "Bindings_HeadlookModeAccumulate", "Accumulate", "Bindings_HeadlookModeDirect", "Direct" };
            else if (name.Equals("BuggyThrottleRange"))
                return new string[] { "", "Full Range", "Bindings_BuggyThrottleForewardOnly", "Forward Only" };
            else if (name.Equals("ThrottleRangeFreeCam"))
                return new string[] { "", "Full Range", "Bindings_ThrottleForewardOnlyFreeCam", "Forward Only" };

            else if (name == "MouseXMode")
                return new string[] { "", "Off", "Bindings_MouseRoll", "Roll", "Bindings_MouseYaw", "Yaw" };
            else if (name.Equals("MouseBuggyRollingXMode"))
                return new string[] { "", "Off", "Bindings_MouseRoll", "Roll" };
            else if (name.Equals("MouseHumanoidXMode"))
                return new string[] { "", "Off", "Bindings_MouseYaw", "Rotate" };

            else if (name.Equals("MultiCrewThirdPersonMouseXMode") || name.Equals("FSSMouseXMode") || name.Equals("SAAThirdPersonMouseXMode") || name.Equals("MouseBuggySteeringXMode") || name.Equals("MouseTurretXMode"))
            {
                return new string[] { "", "Off", "Bindings_MouseYaw", "Yaw" };
            }

            else if (name == "MouseYMode" || name.Equals("FSSMouseYMode") || name.Equals("SAAThirdPersonMouseYMode") || name.Equals("MouseHumanoidYMode") || name.Equals("MultiCrewThirdPersonMouseYMode")
                            || name.Equals("MouseBuggyYMode") || name.Equals("PitchCameraMouse") || name.Equals("MouseTurretYMode"))
            {
                return new string[] { "", "Off", "Bindings_MousePitch", "Pitch", "Bindings_MousePitchInverted", "Inverted" };
            }
            return null;
        }

        // is the button a Hold button, which stops it clashing with other buttons assigned to it
        public static bool HoldButton(string ActionName)
        {
            return ActionName == "HumanoidEmoteWheelButton" || ActionName == "HumanoidItemWheelButton" ||
                ActionName == "HumanoidConflictContextualUIButton" || ActionName == "HumanoidOpenAccessPanelButton";
        }


        // overrides
        private static Dictionary<string, Tuple<Classification, Mode>> discrete = new Dictionary<string, Tuple<Classification, Mode>>
        {
            { "GalaxyMapHome", new Tuple<Classification,Mode>(Classification.GalaxyMap , Mode.GalaxyMap) },
            { "ToggleFreeCam", new Tuple<Classification,Mode>(Classification.CameraSuite, Mode.Camera) },
            { "ToggleButtonUpInput", new Tuple<Classification,Mode>(Classification.Cooling, Mode.Ship) },
            { "DeployHeatSink", new Tuple<Classification,Mode>(Classification.Cooling, Mode.Ship) },
            {"ToggleFlightAssist", new Tuple<Classification,Mode>(Classification.FlightMiscellaneous, Mode.Ship) },
            {"UseBoostJuice", new Tuple<Classification,Mode>(Classification.FlightMiscellaneous, Mode.Ship) },
            {"HyperSuperCombination", new Tuple<Classification,Mode>(Classification.FlightMiscellaneous, Mode.Ship) },
            {"Supercruise", new Tuple<Classification,Mode>(Classification.FlightMiscellaneous, Mode.Ship) },
            {"Hyperspace", new Tuple<Classification,Mode>(Classification.FlightMiscellaneous, Mode.Ship) },
            {"DisableRotationCorrectToggle", new Tuple<Classification,Mode>(Classification.FlightMiscellaneous, Mode.Ship) },
            {"OrbitLinesToggle", new Tuple<Classification,Mode>(Classification.FlightMiscellaneous, Mode.Ship) },

            {"ShipSpotLightToggle", new Tuple<Classification,Mode>(Classification.ShipMiscellaneous, Mode.Ship) },
            {"ResetPowerDistribution", new Tuple<Classification,Mode>(Classification.ShipMiscellaneous, Mode.Ship) },
            {"HMDReset", new Tuple<Classification,Mode>(Classification.ShipMiscellaneous, Mode.All) },
            {"ToggleCargoScoop", new Tuple<Classification,Mode>(Classification.ShipMiscellaneous, Mode.Ship) },
            {"EjectAllCargo", new Tuple<Classification,Mode>(Classification.ShipMiscellaneous, Mode.Ship) },
            {"LangingGearToggle", new Tuple<Classification,Mode>(Classification.ShipMiscellaneous, Mode.Ship) },
            {"MicrophoneMute", new Tuple<Classification,Mode>(Classification.ShipMiscellaneous, Mode.All) },
            {"UseShieldCell", new Tuple<Classification,Mode>(Classification.ShipMiscellaneous, Mode.Ship) },
            {"FireChaffLauncher", new Tuple<Classification,Mode>(Classification.ShipMiscellaneous, Mode.Ship) },
            {"TriggerFieldNeutraliser", new Tuple<Classification,Mode>(Classification.ShipMiscellaneous, Mode.Ship) },
            {"ChargeECM", new Tuple<Classification,Mode>(Classification.ShipMiscellaneous, Mode.Ship) },
            {"WeaponColourToggle", new Tuple<Classification,Mode>(Classification.ShipMiscellaneous, Mode.Ship) },
            {"EngineColourToggle", new Tuple<Classification,Mode>(Classification.ShipMiscellaneous, Mode.Ship) },
            {"NightVisionToggle", new Tuple<Classification,Mode>(Classification.ShipMiscellaneous, Mode.Ship) },
            {"TriggerColonisationModule", new Tuple<Classification,Mode>(Classification.ShipMiscellaneous, Mode.Ship) },
            {"UIFocus", new Tuple<Classification,Mode>(Classification.ModeSwitches, Mode.Ship) },
            {"QuickCommsPanel", new Tuple<Classification,Mode>(Classification.ModeSwitches, Mode.Ship) },
            {"Pause", new Tuple<Classification,Mode>(Classification.ModeSwitches, Mode.Ship) },
            {"HeadLookToggle", new Tuple<Classification,Mode>(Classification.ModeSwitches, Mode.Ship) },
            {"FriendsMenu", new Tuple<Classification,Mode>(Classification.ModeSwitches, Mode.Ship) },
            {"OpenCodexGoToDiscovery", new Tuple<Classification,Mode>(Classification.ModeSwitches, Mode.Ship) },
            {"PlayerHUDModeToggle", new Tuple<Classification,Mode>(Classification.ModeSwitches, Mode.Ship) },
            {"ExplorationFSSEnter", new Tuple<Classification,Mode>(Classification.ModeSwitches, Mode.Ship) },
            {"ShowPGScoreSummaryInput" , new Tuple<Classification,Mode>(Classification.ModeSwitches, Mode.Ship) },
            {"OpenOrders", new Tuple<Classification,Mode>(Classification.FightersOrders, Mode.Ship) },

            {"ToggleDriveAssist" , new Tuple<Classification,Mode>(Classification.Driving, Mode.SRV) },
            {"VerticalThrustersButton" , new Tuple<Classification,Mode>(Classification.Driving, Mode.SRV) },
            {"AutoBreakBuggyButton" , new Tuple<Classification,Mode>(Classification.Driving, Mode.SRV) },
            {"HeadlightsBuggyButton" , new Tuple<Classification,Mode>(Classification.Driving, Mode.SRV) },
            {"ToggleBuggyTurretButton" , new Tuple<Classification,Mode>(Classification.Driving, Mode.SRV) },
            {"SelectTarget_Buggy" , new Tuple<Classification,Mode>(Classification.DrivingTargeting, Mode.SRV) },
            {"DriveSpeedAxis" , new Tuple<Classification,Mode>(Classification.DriveThrottle, Mode.SRV) },
            {"BuggyToggleReverseThrottleInput" , new Tuple<Classification,Mode>(Classification.DriveThrottle, Mode.SRV) },
            {"IncreaseEnginesPower_Buggy" , new Tuple<Classification,Mode>(Classification.DrivingMiscellaneous, Mode.SRV) },
            {"IncreaseWeaponsPower_Buggy" , new Tuple<Classification,Mode>(Classification.DrivingMiscellaneous, Mode.SRV) },
            {"IncreaseSystemsPower_Buggy" , new Tuple<Classification,Mode>(Classification.DrivingMiscellaneous, Mode.SRV) },
            {"ResetPowerDistribution_Buggy" , new Tuple<Classification,Mode>(Classification.DrivingMiscellaneous, Mode.SRV) },
            {"ToggleCargoScoop_Buggy" , new Tuple<Classification,Mode>(Classification.DrivingMiscellaneous, Mode.SRV) },
            {"EjectAllCargo_Buggy" , new Tuple<Classification,Mode>(Classification.DrivingMiscellaneous, Mode.SRV) },
            {"RecallDismissShip" , new Tuple<Classification,Mode>(Classification.DrivingMiscellaneous, Mode.SRV) },
            {"OpenCodexGoToDiscovery_Buggy" , new Tuple<Classification,Mode>(Classification.DrivingModeSwitches , Mode.SRV) },
            {"PlayerHUDModeToggle_Buggy", new Tuple<Classification,Mode>(Classification.DrivingModeSwitches , Mode.SRV) },
            {"HeadLookToggle_Buggy" , new Tuple<Classification,Mode>(Classification.DrivingModeSwitches , Mode.SRV) },

            {"ExplorationFSSTarget" , new Tuple<Classification,Mode>(Classification.FSS, Mode.FSS) },

            {"HumanoidOpenAccessPanelButton", new Tuple<Classification,Mode>(Classification.OnFootModeSwitches , Mode.OnFoot) },
            {"HumanoidConflictContextualUIButton", new Tuple<Classification,Mode>(Classification.OnFootModeSwitches , Mode.OnFoot) },

            {"MouseReset", new Tuple<Classification,Mode>(Classification.MouseControls, Mode.All) },
            {"BlockMouseDecay", new Tuple<Classification,Mode>(Classification.MouseControls, Mode.All) },

            {"PhotoCameraToggle", new Tuple<Classification,Mode>(Classification.CameraSuite, Mode.Ship) },
            {"PhotoCameraToggle_Buggy", new Tuple<Classification,Mode>(Classification.CameraSuite, Mode.SRV) },
            {"PhotoCameraToggle_Humanoid", new Tuple<Classification,Mode>(Classification.CameraSuite, Mode.OnFoot) },

            {"HumanoidItemWheelButton", new Tuple<Classification,Mode>(Classification.OnFoot, Mode.OnFoot) },       // here to intercept WheenButton others
            {"HumanoidEmoteWheelButton", new Tuple<Classification,Mode>(Classification.OnFoot, Mode.OnFoot) },

            {"HumanoidUtilityWheelCycleMode", new Tuple<Classification,Mode>(Classification.OnFoot, Mode.OnFootWheel) },
            {"PlaceSettlement", new Tuple<Classification,Mode>(Classification.SystemColonisation, Mode.ColonisationSuite) },        // presuming its only valid when colonisation suite is fired
        };
    }
}
