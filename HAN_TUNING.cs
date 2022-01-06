using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace Independant_Pufts
{
    public class HAN_TUNING
    {

        public static System.Action CreateDecorModifier(string id, Tag eggTag, float decorThreshold, float modifierPerSecond, bool alsoInvert)
        {
            return delegate ()
            {
                string name = HAN_STRINGS.FERTILITY_MODIFIERS.DECOR.NAME;
                Db.Get().CreateFertilityModifier(id, eggTag, name, "decor description test", (string src) => string.Format(HAN_STRINGS.FERTILITY_MODIFIERS.DECOR.DESC, decorThreshold),
                    delegate (FertilityMonitor.Instance inst, Tag eggType)
                    {
                        DecorVulnerable component = inst.master.GetComponent<DecorVulnerable>();
                        if (component != null)
                        {
                            component.InDecor += delegate (float dt)
                            {
                                float decor = Grid.Decor[Grid.PosToCell(inst.transform.position)];
                                if (decor >= decorThreshold)
                                {
                                    inst.AddBreedingChance(eggType, dt * modifierPerSecond);
                                }
                                else if (alsoInvert)
                                {
                                    inst.AddBreedingChance(eggType, dt * -modifierPerSecond);
                                }
                            };
                        }
                        else
                        {
                            //Debug.LogError("Trying to add decor modifier " + id + " to " + inst.master.name.ToString() + "but missing DecorVunerable component.");
                        }
                    });

            };
 
        }

    }

    public class DecorVulnerable : StateMachineComponent<TemperatureVulnerable.StatesInstance>, ISim1000ms
    {
        public event Action<float> InDecor;

        public void Sim1000ms(float dt)
        {
            int cell = Grid.PosToCell(gameObject);

            if (!Grid.IsValidCell(cell))
            {
                return;
            }

            InDecor?.Invoke(dt);
        }
    }
}
