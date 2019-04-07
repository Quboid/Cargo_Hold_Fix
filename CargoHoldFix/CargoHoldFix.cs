﻿using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.UI;
using Harmony;
using ICities;
using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

namespace CargoHoldFix
{
    public class CargoHoldFix : LoadingExtensionBase, IUserMod
    {
        public string Name => "Cargo Hold Fix";
        public string Description => "Cargo trains/ships/aircraft wait until they are more fully loaded";
        public const string settingsFileName = "CargoHoldFix";

        private static readonly string harmonyId = "quboid.csl_mods.cargo_hold_fix";
        private static HarmonyInstance harmonyInstance;
        private static readonly object padlock = new object();

        public static bool Enabled = true;

        private static UISlider m_sliderTrain, m_sliderPlane, m_sliderShip;
        //private static UICheckBox m_cbRoads, m_cbTrain, m_cbPlane, m_cbShip;

        public static SavedInt delayTrain = new SavedInt("delayTrain", settingsFileName, 5, true);
        public static SavedInt delayPlane = new SavedInt("delayPlane", settingsFileName, 5, true);
        public static SavedInt delayShip = new SavedInt("delayShip", settingsFileName, 5, true);
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
            Enabled = true;
            HarmonyInstance harmony = GetHarmonyInstance();
            harmony.PatchAll(Assembly.GetExecutingAssembly());

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
                else if (disableDummyRoads && connections[i].name == "Road Connection")
                {
                    connections[i].m_dummyTrafficFactor = 0;
                    continue;
                }
                else if (disableDummyRoads && connections[i].name == "Road Connection Small")
                {
                    connections[i].m_dummyTrafficFactor = 0;
                    continue;
                }
            }
        }

        public override void OnLevelUnloading()
        {
            Enabled = false;
            base.OnLevelUnloading();
        }

        public static HarmonyInstance GetHarmonyInstance()
        {
            lock (padlock)
            {
                if (harmonyInstance == null)
                {
                    harmonyInstance = HarmonyInstance.Create(harmonyId);
                }

                return harmonyInstance;
            }
        }


        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group;
            UICheckBox cb;

            group = helper.AddGroup("Delay Spawning");
            UIPanel panel = ((UIPanel)((UIHelper)group).self) as UIPanel;
            UILabel label = panel.AddUIComponent<UILabel>();
            label.text = "How long to delay vehicles to wait for cargo before spawning, compared to the default \ndelay. Takes effect after a game restart. Recommended: 3x";
            m_sliderTrain = (UISlider)group.AddSlider($"Trains:", 1f, 10f, 1f, delayTrain.value, ChangeSliderTrain);
            m_sliderTrain.tooltip = delayTrain.value.ToString() + "x";
            m_sliderPlane = (UISlider)group.AddSlider($"Planes:", 1f, 10f, 1f, delayPlane.value, ChangeSliderPlane);
            m_sliderPlane.tooltip = delayPlane.value.ToString() + "x";
            m_sliderShip = (UISlider)group.AddSlider($"Ships:", 1f, 10f, 1f, delayShip.value, ChangeSliderShip);
            m_sliderShip.tooltip = delayShip.value.ToString() + "x";
            group.AddSpace(10);

            group = helper.AddGroup("Disable Dummy Traffic");
            panel = ((UIPanel)((UIHelper)group).self) as UIPanel;
            label = panel.AddUIComponent<UILabel>();
            label.text = "Disable dummy vehicles that are just going from/to other cities (includes passenger \nvehicles). Takes effect after a game restart.";
            cb = (UICheckBox)group.AddCheckbox("Road Traffic", disableDummyRoads.value, (b) => { disableDummyRoads.value = b; });
            cb = (UICheckBox)group.AddCheckbox("Trains", disableDummyTrain.value, (b) => { disableDummyTrain.value = b; });
            cb = (UICheckBox)group.AddCheckbox("Planes", disableDummyPlane.value, (b) => { disableDummyPlane.value = b; });
            cb = (UICheckBox)group.AddCheckbox("Ships", disableDummyShip.value, (b) => { disableDummyShip.value = b; });
            group.AddSpace(2);
            label = panel.AddUIComponent<UILabel>();
            label.text = "Compatibility Note: If you are using any other mod that affects dummy traffic, leave these \nunchecked and use the other mod's options.";
            label.textScale = 0.88f;
            group.AddSpace(10);
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
}
