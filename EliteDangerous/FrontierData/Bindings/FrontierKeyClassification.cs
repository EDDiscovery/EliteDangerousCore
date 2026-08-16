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
    static public class FrontierKeyClassification
    {
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

        public enum Mode
        {
            Ship, UIPanel, GalaxyMap, Camera, OnFoot, SRV, All, FSS, SAA, HoloMe, Store, Colonisation
        }

        // given a action name, what section in frontiers menu is it in? Higly breakable this.

        public static Tuple<Classification,Mode> GetClass(string ActionName)
        {
            if (discrete.TryGetValue(ActionName, out Tuple<Classification,Mode> classification))
                return classification;

            // page 1

            if (ActionName.StartsWith("UI") || (ActionName.StartsWith("Cycle") && (ActionName.EndsWith("Panel") || ActionName.EndsWith("Page"))))
                return new Tuple<Classification, Mode>(Classification.InterfaceMode, Mode.UIPanel);
            else if (ActionName.StartsWith("Cam"))
                return new Tuple<Classification, Mode>(Classification.GalaxyMap, Mode.Camera);
            else if (ActionName.StartsWith("PhotoCameraToggle") || ActionName.StartsWith("VanityCamera"))
                return new Tuple<Classification, Mode>(Classification.CameraSuite, Mode.Camera);
            else if (ActionName.Contains("FreeCam") || ActionName.StartsWith("PitchCamera") || ActionName.StartsWith("YawCamera") || ActionName.StartsWith("RollCamera")
                        || ActionName == "ToggleRotationLock" || ActionName.StartsWith("FixCamera") || ActionName == "QuitCamera" || ActionName == "ToggleAdvanceMode"
                        || ActionName.StartsWith("FStop")
                )
                return new Tuple<Classification, Mode>(Classification.FreeCam, Mode.Camera);
            else if (ActionName.StartsWith("CommanderCreat"))
                return new Tuple<Classification, Mode>(Classification.HoloMe, Mode.HoloMe);
            else if (ActionName.StartsWith("GalnetAudio"))
                return new Tuple<Classification, Mode>(Classification.PlayList, Mode.All);
            else if (ActionName.StartsWith("Store"))
                return new Tuple<Classification, Mode>(Classification.StoreCamera, Mode.Store);
            else if (ActionName.Contains("Settlement") || ActionName.Contains("Construction") || ActionName.Contains("Placement"))
                return new Tuple<Classification, Mode>(Classification.SystemColonisation, Mode.Colonisation);

            // page 2

            else if (ActionName.Contains("Alternate"))      // must be in front
                return new Tuple<Classification, Mode>(Classification.AlternateFlightControls, Mode.Ship);
            else if (ActionName.Contains("Landing"))        // must be in front
                return new Tuple<Classification, Mode>(Classification.FlightLandingOverrides, Mode.Ship);
            else if (ActionName.StartsWith("Yaw") || ActionName.StartsWith("Roll") || ActionName.StartsWith("Pitch"))
                return new Tuple<Classification, Mode>(Classification.FlightRotation, Mode.Ship);
            else if (ActionName.Contains("Thrust"))
                return new Tuple<Classification, Mode>(Classification.FlightThrust, Mode.Ship);
            else if (ActionName.Contains("Throttle") || ActionName == "ForwardKey" || ActionName == "BackwardKey" || ActionName.StartsWith("SetSpeed"))
                return new Tuple<Classification, Mode>(Classification.FlightThrottle, Mode.Ship);
            else if (ActionName.Contains("Target") || ActionName.Contains("Threat") || ActionName.Contains("Hostile") || ActionName == "WingNavLock" || ActionName.Contains("Subsystem"))
                return new Tuple<Classification, Mode>(Classification.Targetting, Mode.Ship);
            else if (ActionName == "PrimaryFire" || ActionName == "SecondaryFire" || ActionName.StartsWith("CycleFireGroup") || ActionName == "DeployHardpointToggle")
                return new Tuple<Classification, Mode>(Classification.Weapons, Mode.Ship);
            else if (ActionName.StartsWith("Radar") || (ActionName.StartsWith("Increase") && ActionName.EndsWith("Power")))
                return new Tuple<Classification, Mode>(Classification.ShipMiscellaneous, Mode.Ship);        // see also above
            else if ((ActionName.StartsWith("Focus") && ActionName.EndsWith("Panel")) || ActionName.EndsWith("MapOpen"))
                return new Tuple<Classification, Mode>(Classification.ModeSwitches, Mode.Ship);
            else if (ActionName.StartsWith("HeadLook"))
                return new Tuple<Classification, Mode>(Classification.HeadlookMode, Mode.All);
            else if (ActionName.StartsWith("MultiCrew"))
                return new Tuple<Classification, Mode>(Classification.MultiCrew, Mode.Ship);
            else if (ActionName.StartsWith("Order"))
                return new Tuple<Classification, Mode>(Classification.FightersOrders, Mode.Ship);
            else if (ActionName.StartsWith("SAA") || ActionName.StartsWith("ExplorationSAA"))
                return new Tuple<Classification, Mode>(Classification.SAA, Mode.SAA);
            else if (ActionName.StartsWith("ExplorationFSS"))
                return new Tuple<Classification, Mode>(Classification.FSS, Mode.FSS);

            // page 3

            else if (ActionName.StartsWith("BuggyTurret"))
                return new Tuple<Classification, Mode>(Classification.DrivingTurretControls, Mode.SRV);
            else if (ActionName.StartsWith("IncreaseSpeed") || ActionName.StartsWith("DecreaseSpeed"))
                return new Tuple<Classification, Mode>(Classification.DriveThrottle, Mode.SRV);
            else if (ActionName.StartsWith("Steer") || ActionName.StartsWith("Buggy"))
                return new Tuple<Classification, Mode>(Classification.Driving, Mode.SRV);
            else if (ActionName.EndsWith("Panel_Buggy") || ActionName.EndsWith("MapOpen_Buggy"))
                return new Tuple<Classification, Mode>(Classification.DrivingModeSwitches, Mode.SRV);

            // page 4

            else if (ActionName.EndsWith("Open_Humanoid") || ActionName.EndsWith("Panel_Humanoid"))
                return new Tuple<Classification, Mode>(Classification.OnFootModeSwitches, Mode.OnFoot);
            else if (ActionName.StartsWith("HumanoidEmote"))
                return new Tuple<Classification, Mode>(Classification.OnFootEmotes, Mode.OnFoot);
            else if (ActionName.StartsWith("Humanoid"))     // rest are OnFoot
                return new Tuple<Classification, Mode>(Classification.OnFoot, Mode.OnFoot);

            System.Diagnostics.Debug.WriteLine($"Frontier Key new Tuple<Classification,Mode>(Classification Unknown ID {ActionName}");
            return new Tuple<Classification,Mode>(Classification.Misc, Mode.All);
        }

        static Dictionary<string, Tuple<Classification,Mode>> discrete = new Dictionary<string, Tuple<Classification,Mode>>
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

            { "HumanoidOpenAccessPanelButton", new Tuple<Classification,Mode>(Classification.OnFootModeSwitches , Mode.OnFoot) },
            {"HumanoidConflictContextualUIButton", new Tuple<Classification,Mode>(Classification.OnFootModeSwitches , Mode.OnFoot) },
            {"MouseReset", new Tuple<Classification,Mode>(Classification.MouseControls, Mode.All) },
            {"BlockMouseDecay", new Tuple<Classification,Mode>(Classification.MouseControls, Mode.All) },
            {"OpenOrders", new Tuple<Classification,Mode>(Classification.FightersOrders, Mode.Ship) },
        };

    }

}
