using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTrafficController
{
    public class TrafficController
    {
        //object variables 
        private string intersectionID;  
        private string vehicleSignalState;
        private string pedestrianSignalState;

        // Dependency fields
        private IVehicleSignalManager vehicleSignalManager;
        private IPedestrianSignalManager pedestrianSignalManager;
        private ITimeManager timeManager;
        private IWebService webService;
        private IEmailService emailService;


        // History fields for OOS state (L3R3)
        private string previousVehicleState;
        private string previousPedestrianState;



        //L1R1,L1R2,L1R4  Parameter constructor that converts the ID to lowercase and sets initial states to amber/wait
        public TrafficController(string id)
        {
            intersectionID = id.ToLower();    //L1R2: store id in lower case

            //L1R4: initial states
            vehicleSignalState = "amber";
            pedestrianSignalState = "wait";
            previousVehicleState = vehicleSignalState;
            previousPedestrianState = pedestrianSignalState;

        }



        //Getter functions
        public string GetIntersectionID()
        {
            return intersectionID;  //returns the intersection id
        }

        public string GetCurrentVehicleSignalState()
        {
            return vehicleSignalState;   //returns the current vehicle signal state
        }

        public string GetCurrentPedestrianSignalState()
        {
            return pedestrianSignalState;  //returns the current pedestrian signal state
        }

        // L1R3  Which sets the intercection id and converts it to lowercase
        public void SetIntersectionID(string id)
        {
            intersectionID = id.ToLower();
        }


        //L1R5 Sets the vehicle and pedestrian states without a transition validation and it returns true
        //if both states are valid and false if the states are invalid
        public bool SetStateDirect(string vehicleSignalState,string pedestrianSignalState)
        {
            string vehicle_in=vehicleSignalState.ToLower();
            string pedestrian_in=pedestrianSignalState.ToLower();

            bool vehicleValid = vehicle_in == "red" || vehicle_in == "redamber" || vehicle_in == "green" || vehicle_in == "amber" || vehicle_in == "oosv";
            bool pedestrianValid = pedestrian_in == "wait" || pedestrian_in == "walk" || pedestrian_in == "oosp";

            if (!vehicleValid || !pedestrianValid)
            {
                return false;
            }


            //if Valid update the states and sore them to lowercased
           this.vehicleSignalState = vehicle_in;
           this.pedestrianSignalState = pedestrian_in;
            return true;
        }



        // L2R1, L3R1, L3R2, L3R3 
        public bool SetCurrentState(string vehicleSignal, string pedestrianSignal)
        {
            string vehicle_sig = vehicleSignal.ToLower();
            string pedestrian_sig = pedestrianSignal.ToLower();

            // L3R3 - log fault to out of service
            if (vehicle_sig == "oosv" && pedestrian_sig == "oosp")
            {
                //Save current state
                previousVehicleState = vehicleSignalState;
                previousPedestrianState = pedestrianSignalState;
                vehicleSignalState = vehicle_sig;
                pedestrianSignalState = pedestrian_sig;

                // L3R3 - log fault to web service
                if (webService != null)
                {
                    try
                    {
                        webService.FaultDetected(true);
                        webService.LogEngineerRequired("out of service");
                    }
                    //L3R5 Sends email if web service call fails
                    catch (Exception exception) {
                        emailService.SendMail("transportoffice@gmail.com", "failed to log out of service",exception.Message);
                    }
                }
                return true;
            }

            // L3R3 - return from out of service to history state
            if (vehicleSignalState == "oosv" && pedestrianSignalState == "oosp")
            {
                //Allows transition back to the previous state
                if (vehicle_sig == previousVehicleState && pedestrian_sig == previousPedestrianState)
                {
                    vehicleSignalState = vehicle_sig;
                    pedestrianSignalState = pedestrian_sig;
                    return true;
                }
                return false;
            }

            //Gets the state diagram and validates transition based on it
            bool validTransition = false;
            if (vehicleSignalState == "amber" && pedestrianSignalState == "wait")
            {
                validTransition = vehicle_sig == "red" && pedestrian_sig == "walk";
            }
            else if (vehicleSignalState == "red" && pedestrianSignalState == "walk")
            {
                validTransition = vehicle_sig == "redamber" && pedestrian_sig == "walk";
            }
            else if (vehicleSignalState == "redamber" && pedestrianSignalState == "walk")
            {
                validTransition = vehicle_sig == "green" && pedestrian_sig == "wait";
            }
            else if (vehicleSignalState == "green" && pedestrianSignalState == "wait")
            {
                validTransition = vehicle_sig == "amber" && pedestrian_sig == "wait";
            }

            //Checks if the transition is not in the state diagram so it returns false
            if (!validTransition)
            {

                return false;
            }

            // L3R1 - amber to red transition with delay of 3sec and then signals
            if (vehicleSignalState == "amber" && vehicle_sig == "red")
            {
                if (timeManager != null) 
                {

                    if (!timeManager.Delay(3))
                    {
                        return false;
                    }
                    if (!vehicleSignalManager.SetAllRed(true))
                    {

                        return false;
                    }

                    if (!pedestrianSignalManager.SetWalk(true))
                    {
                        return false;
                    }
                    if (!pedestrianSignalManager.SetAudible(true))
                    {
                        return false;
                    }
                }
            }

            // L3R2 - redamber to green transition with delay of 3sec and then signals
            if (vehicleSignalState == "redamber" && vehicle_sig == "green")
            {
                if (timeManager != null)
                {
                    if (!timeManager.Delay(3))
                    {
                        return false;
                    }
                    if (!pedestrianSignalManager.SetWalk(false))//false so it disables walk
                    {  
                        return false;
                    }
                    if (!pedestrianSignalManager.SetWait(true))
                    {
                        return false;
                    }
                    if (!pedestrianSignalManager.SetAudible(false))   //disabled
                    {  
                        return false;
                    }
                    if (!vehicleSignalManager.SetAllGreen(true))
                    {
                        return false;
                    }
                }
            }

            vehicleSignalState = vehicle_sig;
            pedestrianSignalState = pedestrian_sig;
            return true;
        }



        // L2R4,L3R4,L3R5  Returns status report for all three managers and log engineer is required if it detects a fault
        public string GetStatusReport()
        {
            string vehicleStatus = vehicleSignalManager.GetStatus();
            string pedestrianStatus = pedestrianSignalManager.GetStatus();
            string timeStatus = timeManager.GetStatus();


            //L3R4 Checks for fault status 
            string fault = "";

            if (vehicleStatus.Contains("FAULT"))
            {
                fault += "VehicleSignal,";
            }
            if (pedestrianStatus.Contains("FAULT"))
            {
                fault += "PedestrianSignal,";
            }
            if (timeStatus.Contains("FAULT"))
            {
                fault += "Timer,";
            }

            //require log engineer if any fault is found
            if (fault != "")
            {
                try
                {
                    webService.LogEngineerRequired(fault);
                }

                //L3R5 Send fallback email if the logging fails for web service
                catch(Exception exception)
                {
                    emailService.SendMail("transportoffice@gmail.com", "failed to log out of service", exception.Message);
                }
            }

            return vehicleStatus + pedestrianStatus + timeStatus;
        }

        //L2R2  The three parameter constructor which validates and sets recommended states and throws the ArgumentException if invalid
        public TrafficController(string id, string vehicleSignalState, string pedestrianSignalState)
        {
            string vehicle_in=vehicleSignalState.ToLower();
            string pedestrian_in = pedestrianSignalState.ToLower();
            bool vehicleValid = vehicle_in == "red" || vehicle_in == "redamber" || vehicle_in == "green" || vehicle_in == "amber";
            bool pedestrianValid = pedestrian_in == "wait" || pedestrian_in == "walk";

            //Throws the exception if state is invalid
            if(!vehicleValid || !pedestrianValid)
            {
                throw new ArgumentException("Argument Exception: TrafficController can only be initialised to the following states: 'green', 'amber', 'red', 'redamber' for the vehicle signals and 'wait' or 'walk' for the pedestrian signal");
            }
            intersectionID = id.ToLower();
            this.vehicleSignalState = vehicle_in;
            this.pedestrianSignalState = pedestrian_in;
            previousVehicleState = vehicleSignalState;
            previousPedestrianState = pedestrianSignalState;
        }

        // L2R3 This is the dependency injection constructor which allows the 5 required dependencies to be injected for unit testing
        public TrafficController(string id,IVehicleSignalManager iVehicleSignalManager,IPedestrianSignalManager iPedestrianSignalManager,ITimeManager iTimeManager, IWebService iWebService, IEmailService iEmailService)
        {
            intersectionID = id.ToLower();
            vehicleSignalState = "amber";
            pedestrianSignalState = "wait";

            previousVehicleState = vehicleSignalState;
            previousPedestrianState = pedestrianSignalState;

            //Assign the injected dependencies
            vehicleSignalManager = iVehicleSignalManager;
            pedestrianSignalManager = iPedestrianSignalManager;
            timeManager = iTimeManager;
            webService = iWebService;
            emailService = iEmailService;
        }

    }
}
