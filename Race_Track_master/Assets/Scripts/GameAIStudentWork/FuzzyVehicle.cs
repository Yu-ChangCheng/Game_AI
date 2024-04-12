using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameAI;

// All the Fuzz
using Tochas.FuzzyLogic;
using Tochas.FuzzyLogic.MembershipFunctions;
using Tochas.FuzzyLogic.Evaluators;
using Tochas.FuzzyLogic.Mergers;
using Tochas.FuzzyLogic.Defuzzers;
using Tochas.FuzzyLogic.Expressions;

namespace GameAICourse
{

    public class FuzzyVehicle : AIVehicle
    {

        // TODO create some Fuzzy Set enumeration types, and member variables for:
        // Fuzzy Sets (input and output), one or more Fuzzy Value Sets, and Fuzzy
        // Rule Sets for each output.
        // Also, create some methods to instantiate each of the member variables

        // Here are some basic examples to get you started
        enum FzOutputThrottle {Brake, Coast, Accelerate }
        enum FzOutputWheel { TurnLeft, Straight, TurnRight }
        enum FzInputSpeed { Slow, Medium, Fast }
        enum FzInputVehicleDirection { Left, Straight, Right }


        FuzzySet<FzInputSpeed> fzSpeedSet;
        FuzzySet<FzInputVehicleDirection> fzInputVehicleDirectionSet;

        FuzzySet<FzOutputThrottle> fzThrottleSet;
        FuzzyRuleSet<FzOutputThrottle> fzThrottleRuleSet;

        FuzzySet<FzOutputWheel> fzWheelSet;
        FuzzyRuleSet<FzOutputWheel> fzWheelRuleSet;

        FuzzyValueSet fzInputValueSet = new FuzzyValueSet();

        // These are used for debugging (see ApplyFuzzyRules() call
        // in Update()
        FuzzyValueSet mergedThrottle = new FuzzyValueSet();
        FuzzyValueSet mergedWheel = new FuzzyValueSet();



        private FuzzySet<FzInputSpeed> GetSpeedSet()
        {
            FuzzySet<FzInputSpeed> set = new FuzzySet<FzInputSpeed>();

            // TODO: Add some membership functions for each state
            IMembershipFunction SlowFx = new ShoulderMembershipFunction(0f, new Coords(0f, 1f), new Coords(30f, 0f), 100f);
            IMembershipFunction MediumFx = new TriangularMembershipFunction(new Coords(30f, 0f), new Coords(65f, 1f), new Coords(100f, 0f));
            IMembershipFunction FastFx = new ShoulderMembershipFunction(0f, new Coords(66f, 0f), new Coords(100f, 1f), 100f);

            set.Set(FzInputSpeed.Slow, SlowFx);
            set.Set(FzInputSpeed.Medium, MediumFx);
            set.Set(FzInputSpeed.Fast, FastFx);

            return set;
        }

        private FuzzySet<FzOutputThrottle> GetThrottleSet()
        {

            FuzzySet<FzOutputThrottle> set = new FuzzySet<FzOutputThrottle>();

            // TODO: Add some membership functions for each state
            IMembershipFunction BrakeFx = new ShoulderMembershipFunction(-50f, new Coords(-50f, 1f), new Coords(-12.5f, 0f), 50f);
            IMembershipFunction CoastFx = new TriangularMembershipFunction(new Coords(-20f, 0f), new Coords(0f, 1f), new Coords(20f, 0f));
            IMembershipFunction AccelerateFx = new ShoulderMembershipFunction(-50f, new Coords(12.5f, 0f), new Coords(50f, 1f), 50.0f);

            set.Set(FzOutputThrottle.Accelerate, AccelerateFx);
            set.Set(FzOutputThrottle.Coast, CoastFx);
            set.Set(FzOutputThrottle.Brake, BrakeFx);
            
            return set;
        }

        private FuzzySet<FzOutputWheel> GetWheelSet()
        {
            
            FuzzySet<FzOutputWheel> set = new FuzzySet<FzOutputWheel>();

            // TODO: Add some membership functions for each state
            IMembershipFunction TurnLeftFx = new ShoulderMembershipFunction(-0.75f, new Coords(-0.75f, 1f), new Coords(-0.5f, 0f), 0.75f);
            IMembershipFunction StraightFx = new TriangularMembershipFunction(new Coords(-0.75f, 0f), new Coords(0f, 1f), new Coords(0.75f, 0f));
            IMembershipFunction TurnRightFx = new ShoulderMembershipFunction(-0.75f, new Coords(0.5f, 0f), new Coords(0.75f, 1f), 0.75f);

            set.Set(FzOutputWheel.TurnLeft, TurnLeftFx);
            set.Set(FzOutputWheel.Straight, StraightFx);
            set.Set(FzOutputWheel.TurnRight, TurnRightFx);
            

            return set;
        }

        private FuzzySet<FzInputVehicleDirection> GetVehicleDirectionSet()
        {
            FuzzySet<FzInputVehicleDirection> set = new FuzzySet<FzInputVehicleDirection>();

            // TODO: Add some membership functions for each state
            IMembershipFunction leftFx = new ShoulderMembershipFunction(50f, new Coords(50f, 1f), new Coords(10f, 0f), -50f);
            IMembershipFunction straightFx = new TriangularMembershipFunction(new Coords(12.5f, 0f), new Coords(0f, 1f), new Coords(-12.5f, 0f));
            IMembershipFunction rightFx = new ShoulderMembershipFunction(50f, new Coords(-10f, 0f), new Coords(-50f, 1f), -50f);

            set.Set(FzInputVehicleDirection.Left, leftFx);
            set.Set(FzInputVehicleDirection.Straight, straightFx);
            set.Set(FzInputVehicleDirection.Right, rightFx);
            return set;
        }



        private FuzzyRule<FzOutputThrottle>[] GetThrottleRules()
        {

            FuzzyRule<FzOutputThrottle>[] rules =
            {
                // TODO: Add some rules. Here is an example
                // (Note: these aren't necessarily good rules)
                If(And(FzInputSpeed.Slow, FzInputVehicleDirection.Straight)).Then(FzOutputThrottle.Accelerate),
                If(And(FzInputSpeed.Slow, FzInputVehicleDirection.Left)).Then(FzOutputThrottle.Accelerate),
                If(And(FzInputSpeed.Slow, FzInputVehicleDirection.Right)).Then(FzOutputThrottle.Accelerate),
                If(And(FzInputSpeed.Medium, FzInputVehicleDirection.Straight)).Then(FzOutputThrottle.Accelerate),
                If(And(FzInputSpeed.Medium, FzInputVehicleDirection.Left)).Then(FzOutputThrottle.Brake),
                If(And(FzInputSpeed.Medium, FzInputVehicleDirection.Right)).Then(FzOutputThrottle.Brake),
                If(And(FzInputSpeed.Fast, FzInputVehicleDirection.Straight)).Then(FzOutputThrottle.Coast),
                If(And(FzInputSpeed.Fast, FzInputVehicleDirection.Left)).Then(FzOutputThrottle.Brake),
                If(And(FzInputSpeed.Fast, FzInputVehicleDirection.Right)).Then(FzOutputThrottle.Brake),
                
                // More example syntax
                //If(And(FzInputSpeed.Fast, Not(FzFoo.Bar)).Then(FzOutputThrottle.Accelerate),
            };

            return rules;
        }

        private FuzzyRule<FzOutputWheel>[] GetWheelRules()
        {

            FuzzyRule<FzOutputWheel>[] rules =
            {
                If(FzInputVehicleDirection.Left).Then(FzOutputWheel.TurnLeft),
                If(FzInputVehicleDirection.Straight).Then(FzOutputWheel.Straight),
                If(FzInputVehicleDirection.Right).Then(FzOutputWheel.TurnRight),
            };

            return rules;
        }

        private FuzzyRuleSet<FzOutputThrottle> GetThrottleRuleSet(FuzzySet<FzOutputThrottle> throttle)
        {
            var rules = this.GetThrottleRules();
            return new FuzzyRuleSet<FzOutputThrottle>(throttle, rules);
        }

        private FuzzyRuleSet<FzOutputWheel> GetWheelRuleSet(FuzzySet<FzOutputWheel> wheel)
        {
            var rules = this.GetWheelRules();
            return new FuzzyRuleSet<FzOutputWheel>(wheel, rules);
        }


        protected override void Awake()
        {
            base.Awake();

            StudentName = "Yu-Chang Cheng";

            // Only the AI can control. No humans allowed!
            IsPlayer = false;

        }

        protected override void Start()
        {
            base.Start();

            // TODO: You can initialize a bunch of Fuzzy stuff here
            fzSpeedSet = this.GetSpeedSet();

            fzThrottleSet = this.GetThrottleSet();
            fzThrottleRuleSet = this.GetThrottleRuleSet(fzThrottleSet);

            fzWheelSet = this.GetWheelSet();
            fzWheelRuleSet = this.GetWheelRuleSet(fzWheelSet);

            fzSpeedSet = this.GetSpeedSet();
            fzInputVehicleDirectionSet = this.GetVehicleDirectionSet();
        }

        System.Text.StringBuilder strBldr = new System.Text.StringBuilder();

        override protected void Update()
        {

            // TODO Do all your Fuzzy stuff here and then
            // pass your fuzzy rule sets to ApplyFuzzyRules()

            // Remove these once you get your fuzzy rules working.
            // You can leave one hardcoded while you work on the other.
            // Both steering and throttle must be implemented with variable
            // control and not fixed/hardcoded!

            //HardCodeSteering(0f);
            //HardCodeThrottle(1f);
            Vector3 vehicleForwardDirection = transform.forward;
            float lookDistanceAhead = 10f;
            Vector3 pointAheadOnPath = pathTracker.pathCreator.path.GetPointAtDistance(pathTracker.distanceTravelled + lookDistanceAhead);
            Vector3 directionToPathAhead = pointAheadOnPath - transform.position;
            
            float vehicleDirection = Vector3.SignedAngle(directionToPathAhead, vehicleForwardDirection, Vector3.up);

            // Simple example of fuzzification of vehicle state
            // The Speed is fuzzified and stored in fzInputValueSet
            fzSpeedSet.Evaluate(Speed, fzInputValueSet);
            fzInputVehicleDirectionSet.Evaluate(vehicleDirection, fzInputValueSet);


            // ApplyFuzzyRules evaluates your rules and assigns Thottle and Steering accordingly
            // Also, some intermediate values are passed back for debugging purposes
            // Throttle = someValue; //[-1f, 1f] -1 is full brake, 0 is neutral, 1 is full throttle
            // Steering = someValue; // [-1f, 1f] -1 if full left, 0 is neutral, 1 is full right

            ApplyFuzzyRules<FzOutputThrottle, FzOutputWheel>(
                fzThrottleRuleSet,
                fzWheelRuleSet,
                fzInputValueSet,
                // access to intermediate state for debugging
                out var throttleRuleOutput,
                out var wheelRuleOutput,
                ref mergedThrottle,
                ref mergedWheel
                );

            
            // Use vizText for debugging output
            // You might also use Debug.DrawLine() to draw vectors on Scene view
            if (vizText != null)
            {
                strBldr.Clear();

                strBldr.AppendLine($"Demo Output");
                strBldr.AppendLine($"Comment out before submission");

                // You will probably want to selectively enable/disable printing
                // of certain fuzzy states or rules

                AIVehicle.DiagnosticPrintFuzzyValueSet<FzInputSpeed>(fzInputValueSet, strBldr);
  
                AIVehicle.DiagnosticPrintRuleSet<FzOutputThrottle>(fzThrottleRuleSet, throttleRuleOutput, strBldr);
                AIVehicle.DiagnosticPrintRuleSet<FzOutputWheel>(fzWheelRuleSet, wheelRuleOutput, strBldr);

                vizText.text = strBldr.ToString();
            }

            // recommend you keep the base Update call at the end, after all your FuzzyVehicle code so that
            // control inputs can be processed properly (e.g. Throttle, Steering)
            base.Update();
        }

    }
}
