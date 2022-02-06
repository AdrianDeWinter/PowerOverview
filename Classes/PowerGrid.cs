using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        struct PowerGrid
        {
            public IMyTerminalBlock refBlock;

            public List<IMyCubeGrid> attachedGrids;
            public IMyCubeGrid grid;

            public List<IMyPowerProducer> powerBlocks;
            public float totalMaxOutput;
            public float totalCurOutput;

            public List<IMyPowerProducer> windTurbines;
            public float windMaxOutput;
            public float windCurOutput;

            public List<IMyPowerProducer> h2Engines;
            public float hydroMaxOutput;
            public float hydroCurOutput;

            public List<IMyBatteryBlock> batteries;
            public float batteryMaxOutput;
            public float batteryCurOutput;
            public float batteryStored;
            public float batteryCapacity;
            public float batteryCurChargeRate;
            public float batteryMaxChargeRate;

            public List<IMySolarPanel> solarPanels;
            public float solarMaxOutput;
            public float solarCurOutput;

            public List<IMyReactor> reactors;
            public float reactorMaxOutput;
            public float reactorCurOutput;

            public List<IMyPowerProducer> others;
            public float othersMaxOutput;
            public float othersCurOutput;
            /// <summary>
            /// copies all properties of a into the PowerGrid instance
            /// </summary>
            /// <param name="a">
            /// the PowerGrid to be cloned
            /// </param>
            public PowerGrid(PowerGrid a) {
                refBlock = a.refBlock;
                attachedGrids = a.attachedGrids;
                grid  = a.grid;

                powerBlocks  = a.powerBlocks;
                totalMaxOutput  = a.totalMaxOutput;
                totalCurOutput  = a.totalCurOutput;

                windTurbines = a.windTurbines;
                windMaxOutput = a.windMaxOutput;
                windCurOutput = a.windCurOutput;

                h2Engines = a.h2Engines;
                hydroMaxOutput = a.hydroMaxOutput;
                hydroCurOutput = a.hydroCurOutput;

                batteries = a.batteries;
                batteryMaxOutput = a.batteryMaxOutput;
                batteryCurOutput = a.batteryCurOutput;
                batteryStored = a.batteryStored;
                batteryCapacity = a.batteryCapacity;
                batteryCurChargeRate = a.batteryCurChargeRate;
                batteryMaxChargeRate = a.batteryMaxChargeRate;

                solarPanels = a.solarPanels;
                solarMaxOutput = a.solarMaxOutput;
                solarCurOutput = a.solarCurOutput;

                reactors = a.reactors;
                reactorMaxOutput = a.reactorMaxOutput;
                reactorCurOutput = a.reactorCurOutput;

                others = a.others;
                othersMaxOutput = a.othersMaxOutput;
                othersCurOutput = a.othersCurOutput;
            }
            /// <summary>
            /// prints all blocks in this PowerGrid in a formatted fashion
            /// </summary>
            /// <param name="parent">
            /// reference to the parent MyGridProgram
            /// </param>
            public void PrintAllPowerBlocks(MyGridProgram target)
            {
                foreach (var pp in powerBlocks)
                    target.Echo(
                        pp.CustomName
                        + ",\nProducing: "
                        + FormattedPower(pp.CurrentOutput)
                        );
                target.Echo("\n");
            }

            /// <summary>
            /// adds all properties of b to a, except unique ones (grid and refBlock)
            /// </summary>
            /// <param name="a">
            /// PowerGrid to add to, this also the PowerGrid whose refBlock and grid properties will be copied to the resulting PowerGrid
            /// </param>
            /// <param name="b">
            /// PowerGrid to be added, this has its refBlock and grid properties ignored
            /// </param>
            public static PowerGrid operator +(PowerGrid a, PowerGrid b)
            {
                PowerGrid c = new PowerGrid(a);

                //add all grids in b not already registered in a to c
                foreach (IMyCubeGrid g in b.attachedGrids)
                    if (!a.attachedGrids.Contains(g))
                        c.attachedGrids.Add(g);

                c.attachedGrids.AddRange(b.attachedGrids);
                
                c.powerBlocks.AddRange(b.powerBlocks);
                c.totalMaxOutput += b.totalMaxOutput;
                c.totalCurOutput += b.totalCurOutput;
                
                c.windTurbines.AddRange(b.windTurbines);
                c.windMaxOutput += b.windMaxOutput;
                c.windCurOutput += b.windCurOutput;
                
                c.h2Engines.AddRange(b.h2Engines);
                c.hydroMaxOutput += b.hydroMaxOutput;
                c.hydroCurOutput += b.hydroCurOutput;
                
                c.batteries.AddRange(b.batteries);
                c.batteryMaxOutput += b.batteryMaxOutput;
                c.batteryCurOutput += b.batteryCurOutput;
                c.batteryStored += b.batteryStored;
                c.batteryCapacity += b.batteryCapacity;
                c.batteryCurChargeRate += b.batteryCurChargeRate;
                c.batteryMaxChargeRate += b.batteryMaxChargeRate;
                
                c.solarPanels.AddRange(b.solarPanels);
                c.solarMaxOutput += b.solarMaxOutput;
                c.solarCurOutput += b.solarCurOutput;
                
                c.reactors.AddRange(b.reactors);
                c.reactorMaxOutput += b.reactorMaxOutput;
                c.reactorCurOutput += b.reactorCurOutput;
                
                c.others.AddRange(b.others);
                c.othersMaxOutput += b.othersMaxOutput;
                c.othersCurOutput += b.othersCurOutput;
                return c;
            }
            /// <summary>
            /// prints this grids current power stats in a nicely formatted fashion
            /// </summary>
            /// <param name="parent">
            /// reference to the parent MyGridProgram
            /// </param>
            public void PrintPowerStats(MyGridProgram parent)
            {
                if (windTurbines.Count != 0)
                    parent.Echo("Wind Turbine Output:\n" + FormattedPower(windCurOutput));
                if (h2Engines.Count != 0)
                    parent.Echo("Hydrogen Engines Output:\n" + FormattedPower(hydroCurOutput));
                if (batteries.Count != 0)
                {
                    parent.Echo("Batteries Output:\n" + FormattedPower(batteryCurOutput));
                    parent.Echo("Batteries Charge Rate:\n" + FormattedPower(batteryCurChargeRate));
                    parent.Echo("Batteries Charge:\n" + FormattedPower(batteryStored) + " (" + Math.Round(batteryStored/batteryCapacity * 100) + "%)");
                }
                if (reactors.Count != 0)
                    parent.Echo("Reactors Output:\n" + FormattedPower(reactorCurOutput));
                if (solarPanels.Count != 0)
                    parent.Echo("Solarpanels Output:\n" + FormattedPower(solarCurOutput));
                if (others.Count != 0)
                    parent.Echo("Other Power Blocks Output:\n" + FormattedPower(othersCurOutput));
                if (powerBlocks.Count != 0)
                    parent.Echo("Total Output:\n" + FormattedPower(totalCurOutput));
                parent.Echo("\n");
            }
            /// <summary>
            ///      prints the formatted grid name
            /// </summary>
            /// <param name="parent">
            /// reference to the parent MyGridProgram
            /// </param>
            public void PrintGridName(MyGridProgram parent)
            {
                parent.Echo(grid.CustomName + ":");
            }

            /// <summary>
            /// reparses the entire construct and sorts all power blocks into their repective categories
            /// </summary>
            /// <param name="parent">
            /// reference to the parent MyGridProgram
            /// </param>
            public void UpdatePowerBlocks(MyGridProgram parent)
            {
                powerBlocks = new List<IMyPowerProducer>();
                IMyTerminalBlock refBlock = this.refBlock;
                parent.GridTerminalSystem.GetBlocksOfType<IMyPowerProducer>(powerBlocks, block => block.IsSameConstructAs(refBlock));
                var temp = new List<IMyPowerProducer>(powerBlocks);
                
                batteries = FindMethods.FindBatteries(powerBlocks);
                temp = temp.Except(batteries).ToList();

                reactors = FindMethods.FindReactors(powerBlocks);
                temp = temp.Except(reactors).ToList();

                solarPanels = FindMethods.FindSolarPanels(powerBlocks);
                temp = temp.Except(solarPanels).ToList();

                windTurbines = FindMethods.FindWindTurbines(powerBlocks);
                temp = temp.Except(windTurbines).ToList();

                h2Engines = FindMethods.FindHydrogenEngines(powerBlocks);
                temp = temp.Except(h2Engines).ToList();

                others = temp;
            }

            /// <summary>
            /// updates the current and max output of all powerblocks
            /// </summary>
            public void UpdateProduction()
            {
                FindMethods.SumPowerCapacities(powerBlocks, ref totalMaxOutput, ref totalCurOutput, true);
             
                FindMethods.SumPowerCapacities(reactors.Cast<IMyPowerProducer>().ToList(), ref reactorMaxOutput, ref reactorCurOutput);
                
                FindMethods.SumPowerCapacities(solarPanels.Cast<IMyPowerProducer>().ToList(), ref solarMaxOutput, ref solarCurOutput);
                
                FindMethods.SumPowerCapacities(windTurbines.Cast<IMyPowerProducer>().ToList(), ref windMaxOutput, ref windCurOutput);
                
                FindMethods.SumPowerCapacities(h2Engines.Cast<IMyPowerProducer>().ToList(), ref hydroMaxOutput, ref hydroCurOutput);
                
                FindMethods.SumPowerCapacities(others.Cast<IMyPowerProducer>().ToList(), ref othersMaxOutput, ref othersCurOutput);

                FindMethods.SumPowerCapacities(batteries.Cast<IMyPowerProducer>().ToList(), ref batteryMaxOutput, ref batteryCurOutput);

                batteryStored = 0;
                batteryCapacity = 0;
                batteryCurChargeRate = 0;
                batteryMaxChargeRate = 0;
                foreach (IMyBatteryBlock bat in batteries)
                {
                    batteryStored += bat.CurrentStoredPower;
                    batteryCapacity += bat.MaxStoredPower;
                    batteryCurChargeRate += bat.CurrentInput;
                    batteryMaxChargeRate += bat.MaxInput;
                }
            }
        }
    }
}
