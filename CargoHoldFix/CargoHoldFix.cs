﻿using ColossalFramework;
using ColossalFramework.UI;
using CitiesHarmony.API;
using HarmonyLib;
using ICities;
using System;
using System.Reflection;
using UnityEngine;

namespace CargoHoldFix
{
    public class CargoHoldFix : LoadingExtensionBase, IUserMod
    {
        public string Name => "Optimised Outside Connections";
        public string Description => "Large vehicles spawn less frequently with more cargo/passengers at a time.";
        public const string settingsFileName = "OptimisedOutsideConnections";

        private static UISlider m_sliderPassengers, m_sliderTrain, m_sliderPlane, m_sliderShip;

        public static SavedInt delayPassengers = new SavedInt("delayPassengers", settingsFileName, 6, true);
        public static SavedInt delayTrain = new SavedInt("delayTrain", settingsFileName, 4, true);
        public static SavedInt delayPlane = new SavedInt("delayPlane", settingsFileName, 4, true);
        public static SavedInt delayShip = new SavedInt("delayShip", settingsFileName, 4, true);
        public static SavedBool disableDummyRoads = new SavedBool("disableDummyRoads", settingsFileName, false, true);
        public static SavedBool disableDummyTrain = new SavedBool("disableDummyTrain", settingsFileName, false, true);
        public static SavedBool disableDummyPlane = new SavedBool("disableDummyPlane", settingsFileName, false, true);
        public static SavedBool disableDummyShip = new SavedBool("disableDummyShip", settingsFileName, false, true);

        public CargoHoldFix()
        {
            try
            {
                if (GameSettings.FindSettingsFileByName(settingsFileName) == null)
                {
                    GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = settingsFileName } });
                }
            }
            catch (Exception)
            {
                Debug.Log("Could not load/create the setting file.");
            }
        }

        public override void OnLevelLoaded(LoadMode loadMode)
        {
            if (!(loadMode == LoadMode.LoadGame || loadMode == LoadMode.NewGame || loadMode == LoadMode.LoadScenario || loadMode == LoadMode.NewGameFromScenario || loadMode == LoadMode.NewScenarioFromGame || loadMode == LoadMode.NewScenarioFromMap))
            {
                return;
            }

            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());

            OutsideConnectionAI[] connections = Resources.FindObjectsOfTypeAll<OutsideConnectionAI>();
            for (int i = 0; i < connections.Length; i++)
            {
                if (disableDummyTrain && connections[i].name == "Train Connection")
                {
                    connections[i].m_dummyTrafficFactor = 0;
                    continue;
                }
                else if (disableDummyPlane && connections[i].name == "Airplane Connection")
                {
                    connections[i].m_dummyTrafficFactor = 0;
                    continue;
                }
                else if (disableDummyShip && connections[i].name == "Ship Connection")
                {
                    connections[i].m_dummyTrafficFactor = 0;
                    continue;
                }
                else if (disableDummyRoads && connections[i].name == "Road Connection Small")
                {
                    connections[i].m_dummyTrafficFactor = 0;
                    continue;
                }
                else if (disableDummyRoads && connections[i].name == "Road Connection")
                {
                    connections[i].m_dummyTrafficFactor = 0;
                    continue;
                }
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group;
            UICheckBox cb;

            group = helper.AddGroup("Optimised Outside Connections");
            UIPanel panel = ((UIPanel)((UIHelper)group).self);
            UILabel label;

            label = panel.AddUIComponent<UILabel>();
            label.text = "How long to delay vehicles to wait for passengers before spawning compared to the default. \ndelay. Takes effect after a few minutes. Recommended: 3x";
            m_sliderPassengers = (UISlider)group.AddSlider($"Big Passenger Vehicles:", 1f, 10f, 1f, delayPassengers.value, ChangeSliderPassengers);
            m_sliderPassengers.width = 400f;
            m_sliderPassengers.tooltip = (delayPassengers.value / 2).ToString() + "x";

            group = helper.AddGroup("Delay Cargo Spawning");
            panel = ((UIPanel)((UIHelper)group).self);
            label = panel.AddUIComponent<UILabel>();
            label.text = "How long to delay part-full cargo vehicles compared to the default. \ndelay. Takes effect after a game restart. Recommended: 4x";
            m_sliderTrain = (UISlider)group.AddSlider($"Cargo Trains:", 1f, 10f, 1f, delayTrain.value, ChangeSliderTrain);
            m_sliderTrain.width = 400f;
            m_sliderTrain.tooltip = delayTrain.value.ToString() + "x";
            m_sliderPlane = (UISlider)group.AddSlider($"Cargo Planes:", 1f, 10f, 1f, delayPlane.value, ChangeSliderPlane);
            m_sliderPlane.width = 400f;
            m_sliderPlane.tooltip = delayPlane.value.ToString() + "x";
            m_sliderShip = (UISlider)group.AddSlider($"Cargo Ships:", 1f, 10f, 1f, delayShip.value, ChangeSliderShip);
            m_sliderShip.width = 400f;
            m_sliderShip.tooltip = delayShip.value.ToString() + "x";
            group.AddSpace(10);

            group = helper.AddGroup("Disable Dummy Traffic");
            panel = ((UIPanel)((UIHelper)group).self) as UIPanel;
            label = panel.AddUIComponent<UILabel>();
            label.text = "Disable dummy vehicles that are just going from/to other cities (includes small \nvehicles like cars). Takes effect after a game restart.";
            cb = (UICheckBox)group.AddCheckbox("Road Traffic", disableDummyRoads.value, (b) => { disableDummyRoads.value = b; });
            cb = (UICheckBox)group.AddCheckbox("Trains", disableDummyTrain.value, (b) => { disableDummyTrain.value = b; });
            cb = (UICheckBox)group.AddCheckbox("Planes", disableDummyPlane.value, (b) => { disableDummyPlane.value = b; });
            cb = (UICheckBox)group.AddCheckbox("Ships", disableDummyShip.value, (b) => { disableDummyShip.value = b; });
            group.AddSpace(2);
            label = panel.AddUIComponent<UILabel>();
            label.text = "Compatibility Note: If you are using any other mod that affects dummy traffic, leave these \nunchecked and use the other mod's options.";
            label.textScale = 0.85f;
            label.textColor = new Color32(255, 255, 255, 100);
            group.AddSpace(10);
        }

        private static void ChangeSliderPassengers(float v)
        {
            delayPassengers.value = Convert.ToInt32(v);
            m_sliderPassengers.tooltip = (v / 2).ToString() + "x";
            m_sliderPassengers.RefreshTooltip();
        }

        private static void ChangeSliderTrain(float v)
        {
            delayTrain.value = Convert.ToInt32(v);
            m_sliderTrain.tooltip = v.ToString() + "x";
            m_sliderTrain.RefreshTooltip();
        }

        private static void ChangeSliderPlane(float v)
        {
            delayPlane.value = Convert.ToInt32(v);
            m_sliderPlane.tooltip = v.ToString() + "x";
            m_sliderPlane.RefreshTooltip();
        }

        private static void ChangeSliderShip(float v)
        {
            delayShip.value = Convert.ToInt32(v);
            m_sliderShip.tooltip = v.ToString() + "x";
            m_sliderShip.RefreshTooltip();
        }
    }

    public static class Patcher
    {
        private const string HarmonyId = "quboid.csl_mods.optimisedoutsideconnections";
        private static bool patched = false;

        public static void PatchAll()
        {
            if (patched) return;

            patched = true;
            var harmony = new Harmony(HarmonyId);
            harmony.PatchAll();
        }

        public static void UnpatchAll()
        {
            if (!patched) return;

            var harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId);
            patched = false;
        }
    }
}
