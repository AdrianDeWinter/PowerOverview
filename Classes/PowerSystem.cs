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
        struct PowerSystem
        {
            public List<PowerGrid> powerStructure;
            public IMyCubeGrid grid;

            public double totalProduction;
            public double potentialProduction;

            public double totalWindProduction;
            public double potentialWindProduction;

            public double totalSolarProduction;
            public double potentialSolarProduction;

            public double totalNuclearProduction;
            public double potentialNuclearProduction;

            public double totalHydrogenProduction;
            public double potentialHydrogenProduction;

            public double totalBatteryProduction;
            public double potentialBatteryProduction;
            public double totalBatteryConsumption;
            public double potentialBatteryConsumption;
            public double totalBatteryCharge;
            public double potentialBatteryCharge;

            public double totalOtherProduction;
            public double potentialOtherProduction;

            public void UpdatePowerStructure(MyGridProgram parent)
            {
                //parent.Echo("Updating...\n");
                //parse power hirarchy
                List<IMyShipConnector> knownConnectors = new List<IMyShipConnector>();
                powerStructure = ParseNetwork(ref knownConnectors, parent.Me, parent);
            }

            /// <summary>
            /// parses the entire grid construct by construct and stores references to all PowerBlocks
            /// </summary>
            /// <param name="block">
            /// </param>
            /// <param name="parent">
            /// reference to the parent MyGridProgram
            /// </param>
            private List<PowerGrid> ParseNetwork(ref List<IMyShipConnector> knownConnectors, IMyTerminalBlock block, MyGridProgram parent)
            {
                List<PowerGrid> returnValue = new List<PowerGrid>();
                List<IMyCubeGrid> attachedGrids = new List<IMyCubeGrid>();
                List<IMyShipConnector> connectorsOnAssembly = FindMethods.FindConnectors(block, parent);
                knownConnectors.AddRange(connectorsOnAssembly);

                foreach (IMyShipConnector connector in connectorsOnAssembly)
                    if (connector.Status == MyShipConnectorStatus.Connected)
                    {
                        attachedGrids.Add(connector.OtherConnector.CubeGrid);
                        if (!knownConnectors.Contains(connector.OtherConnector))
                            returnValue.AddRange(ParseNetwork(ref knownConnectors, connector.OtherConnector, parent));
                    }

                PowerGrid thisGrid = new PowerGrid
                {
                    grid = block.CubeGrid,
                    refBlock = block,
                    attachedGrids = new List<IMyCubeGrid>(attachedGrids)
                };
                thisGrid.UpdatePowerBlocks(parent);
                thisGrid.UpdateProduction();
                returnValue.Add(thisGrid);

                return returnValue;
            }
            /// <summary>
            /// updates the current and max output of all powerblocks in all constructs on this grid
            /// </summary>
            public void UpdateOutput()
            {
                //reset totals
                totalProduction = 0;
                potentialProduction = 0;

                totalWindProduction = 0;
                potentialWindProduction = 0;

                totalSolarProduction = 0;
                potentialSolarProduction = 0;

                totalNuclearProduction = 0;
                potentialNuclearProduction = 0;

                totalHydrogenProduction = 0;
                potentialHydrogenProduction = 0;

                totalBatteryProduction = 0;
                potentialBatteryProduction = 0;
                totalBatteryConsumption = 0;
                potentialBatteryConsumption = 0;
                totalBatteryCharge = 0;
                potentialBatteryCharge = 0;

                totalOtherProduction = 0;
                potentialOtherProduction = 0;

                //add up all categories grid by grid
                foreach (var grid in powerStructure)
                {
                    //update the grids totals
                    grid.UpdateProduction();

                    //only add a grids output total to production if it is producing
                    if(grid.totalCurOutput >= 0)
                        totalProduction += grid.totalCurOutput;

                    potentialProduction += grid.totalMaxOutput;

                    potentialBatteryProduction += grid.batteryMaxOutput;
                    totalBatteryProduction += grid.batteryCurOutput;
                    totalBatteryConsumption += grid.batteryCurChargeRate;
                    potentialBatteryConsumption += grid.batteryMaxChargeRate;
                    totalBatteryCharge += grid.batteryStored;
                    potentialBatteryCharge += grid.batteryCapacity;

                    potentialSolarProduction += grid.solarMaxOutput;
                    totalSolarProduction += grid.solarCurOutput;

                    potentialNuclearProduction += grid.reactorMaxOutput;
                    totalNuclearProduction += grid.reactorCurOutput;

                    potentialHydrogenProduction += grid.hydroMaxOutput;
                    totalHydrogenProduction += grid.hydroCurOutput;

                    potentialWindProduction += grid.windMaxOutput;
                    totalWindProduction += grid.windCurOutput;

                    potentialOtherProduction += grid.othersMaxOutput;
                    totalOtherProduction += grid.othersCurOutput;
                }   
            }

            /// <summary>
            /// prints the entire power systems current and max outpust, and each PowerGrid's details
            /// </summary>
            /// <param name="parent">
            /// reference to the parent MyGridProgram
            /// </param>
            public void PrintPowerSystem(MyGridProgram target)
            {
                target.Echo("Potential Output: "
                    + FormattedPower(potentialProduction)
                    );

                if(potentialWindProduction != 0)
                    target.Echo("   Wind: "
                        + FormattedPower(potentialWindProduction)
                    + " ("
                    + Math.Round(potentialWindProduction / potentialProduction * 100, 2)
                    + "%)");
                if (potentialSolarProduction != 0)
                    target.Echo("   Solar: "
                        + FormattedPower(potentialSolarProduction)
                    + " ("
                    + Math.Round(potentialSolarProduction / potentialProduction * 100, 2)
                    + "%)");
                if (potentialBatteryProduction != 0)
                    target.Echo("   Batteries: "
                     + FormattedPower(potentialBatteryProduction)
                    + " ("
                    + Math.Round(potentialBatteryProduction / potentialProduction * 100, 2)
                    + "%)");
                if (potentialNuclearProduction != 0)
                    target.Echo("   Reactor: "
                        + FormattedPower(potentialNuclearProduction)
                    + " ("
                    + Math.Round(potentialNuclearProduction / potentialProduction * 100, 2)
                    + "%)");
                if (potentialHydrogenProduction != 0)
                    target.Echo("   Hydrogen: "
                        + FormattedPower(potentialHydrogenProduction)
                    + " ("
                    + Math.Round(potentialHydrogenProduction / potentialProduction * 100, 2)
                    + "%)");
                if (potentialOtherProduction != 0)
                    target.Echo("   Other: "
                        + FormattedPower(potentialOtherProduction)
                    + " ("
                    + Math.Round(potentialOtherProduction / potentialProduction * 100, 2)
                    + "%)");
                target.Echo("\n");

                target.Echo(
                    "Current Consumption: "
                    + FormattedPower(totalProduction)
                    + " ("
                    + Math.Round(totalProduction / potentialProduction * 100, 2)
                    + "%)");

                if (potentialBatteryConsumption != 0)
                    target.Echo("   Batteries: "
                     + FormattedPower(totalBatteryConsumption)
                    + " ("
                    + Math.Round(totalBatteryConsumption / potentialProduction * 100, 2)
                    + "% of production capacity)");

                target.Echo("\n");

                foreach (var grid in powerStructure)
                {
                    grid.PrintGridName(target);
                    grid.PrintPowerStats(target);
                }
            }
        }
    }
}
