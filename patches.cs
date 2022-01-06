using TUNING;
using UnityEngine;
using HarmonyLib;
using System;
using System.Linq;

namespace Independant_Pufts
{
    public class Independant_Pufts
    {

        public static void OnLoad()
        {
            Debug.Log("IP Mod Onload here");

            //thanks to Pholith for demonstrating egg modifier insertion
            
            Type[] neighborParams = new Type[] { typeof(string), typeof(Tag), typeof(Tag), typeof(float), typeof(bool) };
            Type[] dietParams = new Type[] { typeof(string), typeof(Tag), typeof(Tag), typeof(float) };
            
            //object[] squeakyModifier = new object[] { "PuftAlphaNearbyBleachstone", "PuftBleachStoneEgg".ToTag(), "PuftAlpha".ToTag(), -0.00025f, false};

            object[] denseEggs = new object[] { "O2ForDense", "PuftOxyliteEgg".ToTag(), SimHashes.Oxygen.CreateTag(), 0.15f / PuftTuning.STANDARD_CALORIES_PER_CYCLE };

            object[] puftEggs = new object[] { "PO2ForNormal", "PuftEgg".ToTag(), SimHashes.ContaminatedOxygen.CreateTag(), 0.15f / PuftTuning.STANDARD_CALORIES_PER_CYCLE };

            object[] squeakyEggs = new object[] { "ChlorineForSqueak", "PuftBleachstoneEgg".ToTag(), SimHashes.ChlorineGas.CreateTag(), 0.15f / PuftTuning.STANDARD_CALORIES_PER_CYCLE };

            CREATURES.EGG_CHANCE_MODIFIERS.MODIFIER_CREATORS.Add(
                Traverse.Create<CREATURES.EGG_CHANCE_MODIFIERS>().Method("CreateDietaryModifier", dietParams).GetValue<System.Action>(denseEggs)
            );
            CREATURES.EGG_CHANCE_MODIFIERS.MODIFIER_CREATORS.Add(
                Traverse.Create<CREATURES.EGG_CHANCE_MODIFIERS>().Method("CreateDietaryModifier", dietParams).GetValue<System.Action>(puftEggs)
            );
            CREATURES.EGG_CHANCE_MODIFIERS.MODIFIER_CREATORS.Add(
                Traverse.Create<CREATURES.EGG_CHANCE_MODIFIERS>().Method("CreateDietaryModifier", dietParams).GetValue<System.Action>(squeakyEggs)
            );
            CREATURES.EGG_CHANCE_MODIFIERS.MODIFIER_CREATORS.Add(
                HAN_TUNING.CreateDecorModifier("DecorForPrinces", "PuftAlphaEgg".ToTag(), 100f, 7.0E-4f, true)
            );

        }

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch()]

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        class PuftTuningPatch
        {
            public static void Postfix()
            {
                Debug.Log("Postfix here");
                // Remove special Puft eggs from the normal Puft
                int index = 0;
                if ((index = PuftTuning.EGG_CHANCES_BASE.FindIndex(x => x.egg == "PuftOxyliteEgg".ToTag())) != -1)
                {
                    PuftTuning.EGG_CHANCES_BASE.RemoveAt(index);
                }

                if ((index = PuftTuning.EGG_CHANCES_BASE.FindIndex(x => x.egg == "PuftBleachstoneEgg".ToTag())) != -1)
                {
                    PuftTuning.EGG_CHANCES_BASE.RemoveAt(index);
                }

                // Remove the normal puft eggs from the special pufts
                if ((index = PuftTuning.EGG_CHANCES_OXYLITE.FindIndex(x => x.egg == "PuftEgg".ToTag())) != -1)
                {
                    PuftTuning.EGG_CHANCES_OXYLITE.RemoveAt(index);
                }
                
                if ((index = PuftTuning.EGG_CHANCES_BLEACHSTONE.FindIndex(x => x.egg == "PuftEgg".ToTag())) != -1)
                {
                    PuftTuning.EGG_CHANCES_BLEACHSTONE.RemoveAt(index);
                }

                // Add the Dense and Squeaky Puft eggs to the Prince
                PuftTuning.EGG_CHANCES_ALPHA.Add(new FertilityMonitor.BreedingChance()
                {
                    egg = "PuftOxyliteEgg".ToTag(),
                    weight = 0.10f
                }
                );

                PuftTuning.EGG_CHANCES_ALPHA.Add(new FertilityMonitor.BreedingChance()
                {
                    egg = "PuftBleachstoneEgg".ToTag(),
                    weight = 0.10f
                }
                );

                // Softly tune the Prince to be a bit more self-centered, but open to family reunions
                if ((index = PuftTuning.EGG_CHANCES_ALPHA.FindIndex(x => x.egg == "PuftEgg".ToTag())) != -1)
                {
                    PuftTuning.EGG_CHANCES_ALPHA.ElementAt(index).weight = 0.10f;
                }
                if ((index = PuftTuning.EGG_CHANCES_ALPHA.FindIndex(x => x.egg == "PuftAlphaEgg".ToTag())) != -1)
                {
                    PuftTuning.EGG_CHANCES_ALPHA.ElementAt(index).weight = 0.70f;
                }

                // Attempts to delete the original fertility modifiers for the old NearbyCreature mechanics
                if((index = Db.Get().FertilityModifiers.resources.FindIndex(x => x.Id == "PuftAlphaBalance")) != -1) {
                    Db.Get().FertilityModifiers.resources.RemoveAt(index);
                }
                if ((index = Db.Get().FertilityModifiers.resources.FindIndex(x => x.Id == "PuftAlphaNearbyOxylite")) != -1)
                {
                    Db.Get().FertilityModifiers.resources.RemoveAt(index);
                }
                if ((index = Db.Get().FertilityModifiers.resources.FindIndex(x => x.Id == "PuftAlphaNearbyBleachstone")) != -1)
                {
                    Db.Get().FertilityModifiers.resources.RemoveAt(index);
                }
            }
        }


        // Adds the decor vulnerability component to the pufts

        [HarmonyPatch(typeof(PuftAlphaConfig))]
        [HarmonyPatch("CreatePuftAlpha")]
        public class AddDecorVulnerabilityToPrincePatch
        {
            public static void Postfix(GameObject __result)
            {
                __result.AddOrGet<DecorVulnerable>();
                __result.AddOrGet<DecorProvider>().baseDecor = 80f;
                __result.AddOrGet<DecorProvider>().baseRadius = 7f;
            }
        }

        [HarmonyPatch(typeof(PuftConfig))]
        [HarmonyPatch("CreatePuft")]
        public class AddDecorVulnerabilityToPuftPatch
        {
            public static void Postfix(GameObject __result)
            {
                __result.AddOrGet<DecorVulnerable>();
            }
        }

        [HarmonyPatch(typeof(PuftBleachstoneConfig))]
        [HarmonyPatch("CreatePuftBleachstone")]
        public class AddDecorVulnerabilityToSqueakyPatch
        {
            public static void Postfix(GameObject __result)
            {
                __result.AddOrGet<DecorVulnerable>();
            }
        }

        [HarmonyPatch(typeof(PuftOxyliteConfig))]
        [HarmonyPatch("CreatePuftOxylite")]
        public class AddDecorVulnerabilityPatch
        {
            public static void Postfix(GameObject __result)
            {
                __result.AddOrGet<DecorVulnerable>();
            }
        }
    }
}
