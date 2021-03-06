﻿using KSP.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FillItUp
{
    public class FuelModel
    {
        private bool Changed = false;
        private FuelTypes.StageResDef fuelTypes;     
        private Dictionary<Tuple<string, int>, float> model;
        Tuple<string, int> key;

        public FuelModel()
        {
            model = new Dictionary<Tuple<string, int>, float>();
            fuelTypes = new FuelTypes.StageResDef();
        }

        public void SetFuelTypes(FuelTypes.StageResDef setfuelTypes)
        {
            this.fuelTypes = setfuelTypes;
        }

        public void SetAll(int stage, float amount)
        {
            foreach(var fuelType in fuelTypes.resources)
            {
                bool ignored = FillItUp.Instance._config.RuntimeLockedResources.ContainsKey(StageRes.Key2(stage, fuelType.First));
                if (!ignored)
                    Set(stage, fuelType.Second, amount);
            }
        }

        public void Set(int stage, string type, float amount)
        {
            if (model == null)
            {
                model = new Dictionary<Tuple<string, int>, float>();
            }
            key = new Tuple<string, int>(type, stage);
            float f;
            bool b = true;
            if (model.TryGetValue(key, out f))
                b = (amount != f);
            //if (!model.ContainsKey(key) || amount != model[key])
            if (b)
            {
                Changed = true;               
                model[key] = amount; 
            }

        }

        public float Get(int stage, string type)
        {
            if (model == null) return 1;
            key = new Tuple<string, int>(type, stage);
            float f;
            if (!model.TryGetValue(key, out f))
                f = 1;
            return f;
        }


        public void Apply(ShipConstruct ship, SortedDictionary<int, FuelTypes.StageResDef> stages)
        {
            if (Changed)
            { 
                foreach (var s in stages)
                {
                    Apply(ship, s.Key, s.Value);
                }
            }
        }

        public void Apply(ShipConstruct ship, int stage, FuelTypes.StageResDef stages)
        {
            if (ship == null || stages == null)
                return;
            if (Changed)
            {
                foreach (var part in stages.parts)
                {
                    foreach (var resource in part.Resources)
                    {
                        foreach (var fuelType in stages.resources)
                        {
                            key = new Tuple<string, int>(fuelType.Second, stage);

                            if (resource.resourceName == fuelType.Second)
                            {
                                if (!FillItUp.Instance.ignoreLockedTanks || resource.flowState)
                                {
                                    float f;
                                    if (model.TryGetValue(key, out f))
                                    {
                                        resource.amount = resource.maxAmount * f;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }


                var resourceEditors = EditorLogic.FindObjectsOfType<UIPartActionResourceEditor>();

                foreach (var ed in resourceEditors)
                {
                    ed.resourceAmnt.text = ed.Resource.amount.ToString("F1");
                    ed.resourceMax.text = ed.Resource.maxAmount.ToString("F1");
                    ed.slider.value = (float)(ed.Resource.amount / ed.Resource.maxAmount);
                }

                GameEvents.onEditorShipModified.Fire(ship);

                Changed = false;
            }
            
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
