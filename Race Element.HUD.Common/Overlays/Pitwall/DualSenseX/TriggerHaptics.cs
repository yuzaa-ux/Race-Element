using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static RaceElement.ACCSharedMemory;
using static RaceElement.Data.SetupConverter;
using static RaceElement.HUD.Common.Overlays.Pitwall.DualSenseX.DualSenseXConfiguration;
using static RaceElement.HUD.Common.Overlays.Pitwall.DualSenseX.DualSenseXResources;

namespace RaceElement.HUD.Common.Overlays.Pitwall.DualSenseX;

internal static class TriggerHaptics
{
    public static Packet HandleBrakePressure(SPageFilePhysics pagePhysics)
    {
        return null;
    }

    public static Packet HandleBraking(DualSenseXConfiguration config, Game gameWhenStarted)
    {
        Packet p = new();
        List<Instruction> instructions = [];
        int controllerIndex = 0;

        if (SimDataProvider.LocalCar.Electronics.AbsActivation > 0 && config.AbsHaptics.AbsEffect)
        {
            instructions.Add(new Instruction()
            {
                type = InstructionType.TriggerUpdate,
                /// Start: 0-9 Strength:0-8 Frequency:0-255
                //parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.AutomaticGun, 0, 6, 45 } // vibrate is not enough
                parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibrateResistanceB, config.AbsHaptics.AbsFrequency/*85*/, 1, 0, 0, 0, 0, 0]
            });

        }

        // TODO: add either an option to threshold it on brake input or based on some curve?
        if (gameWhenStarted == Game.AssettoCorsa1 && SimDataProvider.LocalCar.Inputs.Brake > config.BrakeSlip.BrakeTreshold / 100f)
        {
            float[] slipRatios = SimDataProvider.LocalCar.Tyres.SlipRatio;
            if (slipRatios.Length == 4)
            {
                float slipRatioFront = Math.Max(slipRatios[0], slipRatios[1]);
                float slipRatioRear = Math.Max(slipRatios[2], slipRatios[3]);

                // TODO: add option for front and rear ratio threshold.
                if (slipRatioFront > config.BrakeSlip.FrontSlipTreshold || slipRatioRear > config.BrakeSlip.RearSlipTreshold)
                {
                    float frontslipCoefecient = slipRatioFront * 4f;
                    frontslipCoefecient.ClipMax(20);

                    float rearSlipCoefecient = slipRatioFront * 2f;
                    rearSlipCoefecient.ClipMax(15);

                    float magicValue = frontslipCoefecient + rearSlipCoefecient;

                    instructions.Add(new Instruction()
                    {
                        type = InstructionType.TriggerUpdate,
                        /// Start: 0-9 Strength:0-8 Frequency:0-255
                        //parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.AutomaticGun, 0, 6, 45 } // vibrate is not enough
                        parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibrateResistanceB, config.BrakeSlip.Frequency /*85*/, magicValue, 0, 0, 0, 0, 0]
                    });
                }
            }
        }

        if (instructions.Count == 0)
        {
            instructions.Add(new Instruction()
            {
                type = InstructionType.TriggerUpdate,
                parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal]
            });
        }


        if (instructions.Count == 0) return null;
        p.instructions = instructions.ToArray();
        return p;
    }

    public static Packet HandleAcceleration(DualSenseXConfiguration config, Game gameWhenStarted)
    {
        Packet p = new();
        List<Instruction> instructions = [];
        int controllerIndex = 0;

        //float rearLeftSlip = pagePhysics.SlipRatio[(int)Wheel.RearLeft];
        //float rearRightSlip = pagePhysics.SlipRatio[(int)Wheel.RearRight];
        //float averageRearTyreSlip = rearLeftSlip + rearRightSlip / 2;

        //if (averageRearTyreSlip > 1)
        //{
        //    averageRearTyreSlip.ClipMax(8);
        //    instructions.Add(new Instruction()
        //    {
        //        type = InstructionType.TriggerUpdate,
        //        //parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibrateResistanceB, 200, 1, 0, 0, 0, 0, 0 }
        //        /// Start: 0-9 Strength:0-8 Frequency:0-255
        //        parameters = [controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, 0, averageRearTyreSlip / 4, 100 + averageRearTyreSlip * 4]
        //    });
        //}
        //else
        //{
        //    instructions.Add(new Instruction()
        //    {
        //        type = InstructionType.TriggerUpdate,
        //        parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal]
        //    });
        //}

        if (config.TcHaptics.TcEffect && SimDataProvider.LocalCar.Electronics.TractionControlActivation > 0)
        {
            instructions.Add(new Instruction()
            {
                type = InstructionType.TriggerUpdate,
                parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibrateResistanceB, config.TcHaptics.TcFrequency/*130*/, 10, 0, 0, 0, 0, 0]
                /// Start: 0-9 Strength:0-8 Frequency:0-255
                //parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, 0, 6, 65 }
            });
        }

        if (gameWhenStarted == Game.AssettoCorsa1 && SimDataProvider.LocalCar.Inputs.Throttle > config.ThrottleSlip.ThrottleTreshold / 100f)
        {
            float[] slipRatios = SimDataProvider.LocalCar.Tyres.SlipRatio;
            if (slipRatios.Length == 4)
            {
                float slipRatioFront = Math.Max(slipRatios[0], slipRatios[1]);
                float slipRatioRear = Math.Max(slipRatios[2], slipRatios[3]);

                if (slipRatioFront > config.ThrottleSlip.FrontSlipTreshold || slipRatioRear > config.ThrottleSlip.RearSlipTreshold)
                {

                    float frontslipCoefecient = slipRatioFront * 4;
                    frontslipCoefecient.ClipMax(20);

                    float rearSlipCoefecient = slipRatioFront * 6f;
                    rearSlipCoefecient.ClipMax(30);

                    float magicValue = frontslipCoefecient + rearSlipCoefecient;


                    instructions.Add(new Instruction()
                    {
                        type = InstructionType.TriggerUpdate,
                        parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibrateResistanceB, config.ThrottleSlip.Frequency/*130*/, magicValue, 0, 0, 0, 0, 0]
                        /// Start: 0-9 Strength:0-8 Frequency:0-255
                        //parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, 0, 6, 65 }
                    });


                    //instructions.Add(new Instruction()
                    //{
                    //    type = InstructionType.TriggerUpdate,
                    //    parameters = [controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, 0, magicValue, 45]
                    //});
                }
            }
        }

        if (instructions.Count == 0)
        {
            instructions.Add(new Instruction()
            {
                type = InstructionType.TriggerUpdate,
                parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal]
            });
        }


        if (instructions.Count == 0) return null;
        p.instructions = instructions.ToArray();
        return p;
    }
}
