using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;
using Sandbox.Game.GameSystems;
using System.Xml;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        class Implementation : ScriptBase
        {
            private int counter = 0;
            private static readonly int versionInfoDisplayTime = 2;
            PowerSystem powerSystem;
            public Implementation(MyGridProgram parent, MyIni custom, MyIni storage, MyIniParseResult customSectionResult) : base(custom, storage, customSectionResult)
            {
                powerSystem = new PowerSystem
                {
                    grid = parent.Me.CubeGrid
                };
                powerSystem.UpdatePowerStructure(parent);

                parent.Runtime.UpdateFrequency |= UpdateFrequency.Update100;
                parent.Runtime.UpdateFrequency |= UpdateFrequency.Update10;
                //parent.Runtime.UpdateFrequency |= UpdateFrequency.Update1;
            }

            public override void OnTerminal(MyGridProgram parent, string arguments)
            {
                //List<IMyPowerProducer> blocks = new List<IMyPowerProducer>();
                //parent.GridTerminalSystem.GetBlocksOfType<IMyPowerProducer>(blocks, block => !(block is IMySolarPanel) && !(block is IMyReactor) && !(block is IMyBatteryBlock));
                //parent.GridTerminalSystem.GetBlocksOfType<IMyPowerProducer>(blocks);
                //batteries = FindMethods.FindBatteries(blocks);
                //foreach (var block in reactors)
                //    parent.Echo("Name: " + block.GetType().Name + ": " + block.CustomName);
                parent.Runtime.UpdateFrequency ^= UpdateFrequency.Update10;
                parent.Runtime.UpdateFrequency |= UpdateFrequency.Once;
            }
            public override void OnCallOnce(MyGridProgram parent)
            {
                counter = versionInfoDisplayTime;
                parent.Echo("Power Script Running, Version: 0.0.3c");
            }
            public override void OnUpdate100(MyGridProgram parent)
            {
                powerSystem.UpdatePowerStructure(parent);

                if (counter != 0)
                    counter--;
                else
                   parent.Runtime.UpdateFrequency |= UpdateFrequency.Update10;
            }
            public override void OnUpdate10(MyGridProgram parent)
            {
                powerSystem.UpdateOutput();
                powerSystem.PrintPowerSystem(parent);
            }
            public override void OnUpdate1(MyGridProgram parent)
            {
            }
        }
    }
}
