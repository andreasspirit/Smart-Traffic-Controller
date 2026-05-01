using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Xml;
using NSubstitute;
using SmartTrafficController;

[TestFixture]
class TrafficControllerTests
{


    //===========Level One Tests================

    // L1R1, L1R2
    [TestCase("MAINSTREET", "mainstreet")]
    [TestCase("NORTH", "north")]
    [TestCase("TEST", "test")]
    [TestCase("SOUTH", "south")]
    public void Constructor_WithID_ConvertsToLowercase(string input, string expected)
    {
        //Arranne and Act
        var trafficcontroller = new TrafficController(input);

        //Assert
        Assert.That(trafficcontroller.GetIntersectionID(), Is.EqualTo(expected));
    }

    //L1R3
    [TestCase("MaiNStreeT","mainstreet")]
    [TestCase("SoUTH", "south")]
    [TestCase("NortH", "north")]
    [TestCase("TESt", "test")]
    public void SetIntersectionID_WithUppercase_ConverttoLower(string input,string expected)
    {
        //Arrange
        var trafficcontroller = new TrafficController("test");

        //Act
        trafficcontroller.SetIntersectionID(input);

        //Assert
        Assert.That(trafficcontroller.GetIntersectionID(),Is.EqualTo(expected));

    }



    //L1R4
    [Test]
    public void Constructor_InitialStates_ReturnAmber()
    {

        //Arrange and act
        var trafficcontroller = new TrafficController("south");

        //Assert
        Assert.That(trafficcontroller.GetCurrentVehicleSignalState(), Is.EqualTo("amber"));  //Vehicle state
    }

    //L1R4
    [Test]
    public void Constructor_InitialStates_ReturnWait()
    {

        //Arrange and act
        var trafficcontroller = new TrafficController("south");

        //Assert
        Assert.That(trafficcontroller.GetCurrentPedestrianSignalState(), Is.EqualTo("wait"));   //Pedestrian state
    }



    // L1R5 - valid states
    [TestCase("red", "walk", true)]
    [TestCase("green", "wait", true)]
    [TestCase("amber", "wait", true)]
    [TestCase("redamber", "walk", true)]
    public void SetStateDirect_ValidStates_ReturnsTrue(string vehicle, string pedestrian, bool expected)
    {
        // Arrange
        var trafficcontroller = new TrafficController("test");

        // Act
        bool result = trafficcontroller.SetStateDirect(vehicle, pedestrian);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }


    // L1R5 - invalid states
    [TestCase("red", "start")]
    [TestCase("blue", "wait")]

    public void SetStateDirect_InvalidStates_ReturnsFalse(string vehicle, string pedestrian)
    {
        // Arrange
        var trafficcontroller = new TrafficController("test");

        // Act
        bool result = trafficcontroller.SetStateDirect(vehicle, pedestrian);

        // Assert
        Assert.That(result, Is.False);
    }



    //L1R5 - Stores valid states in lowercase
    [Test]
    public void SetStateDirect_ValidStates_StoresInLowercase()
    {

        //Arrange
        var trafficcontroller = new TrafficController("test");

        //Act
        trafficcontroller.SetStateDirect("GREEN", "WAIT");

        //Assert
        Assert.That(trafficcontroller.GetCurrentVehicleSignalState(), Is.EqualTo("green"));    
        Assert.That(trafficcontroller.GetCurrentPedestrianSignalState(), Is.EqualTo("wait"));   
    }





    //===========Level Two Tests================



    // L2R1      Valid transition
    [TestCase("amber", "wait", "red", "walk")]
    [TestCase("red", "walk", "redamber", "walk")]

    public void SetCurrentState_ValidTransition_ReturnsTrue( string getVehicle, string getPedestrian,string toVehicle, string toPedestrian)
    {
        // Arrange
        var trafficcontroller = new TrafficController("test");
        trafficcontroller.SetStateDirect(getVehicle, getPedestrian);

        // Act
        bool result = trafficcontroller.SetCurrentState(toVehicle, toPedestrian);

        // Assert
        Assert.That(result, Is.True);
    }


    // L2R1   Invalid Transition
    [TestCase("amber", "wait", "green", "walk")]
    [TestCase("green", "walk", "redamber", "walk")]
    public void SetCurrentState_InValidTransition_ReturnsFalse(string getVehicle, string getPedestrian, string toVehicle, string toPedestrian)
    {
        // Arrange
        var trafficcontroller = new TrafficController("test");
        trafficcontroller.SetStateDirect(getVehicle, getPedestrian);

        // Act
        bool result = trafficcontroller.SetCurrentState(toVehicle, toPedestrian);

        // Assert
        Assert.That(result, Is.False);
    }


    // L2R2  Correct states
    [TestCase("test", "red", "walk", "test", "red", "walk")]
    [TestCase("TEST2", "GREEN", "WAIT", "test2", "green", "wait")]
    [TestCase("test3", "redamber", "wait", "test3", "redamber", "wait")]
    [TestCase("TEST4", "AMBER", "WAIT", "test4", "amber", "wait")]
    public void Constructor_ValidState_SetsStatesCorrectly(string id, string vehicle, string pedestrian,string expectedID, string expectedVehicle, string expectedPedestrian)
    {
        // Arrange and Act
        var trafficcontroller = new TrafficController(id, vehicle, pedestrian);

        // Assert
        Assert.That(trafficcontroller.GetIntersectionID(), Is.EqualTo(expectedID));
        Assert.That(trafficcontroller.GetCurrentVehicleSignalState(), Is.EqualTo(expectedVehicle));
        Assert.That(trafficcontroller.GetCurrentPedestrianSignalState(), Is.EqualTo(expectedPedestrian));
    }



    // L2R2  Invalid states and throws exception for them
    [TestCase("test", "red", "oosp")]
    [TestCase("test", "green", "oosp")]
    [TestCase("test", "oosv", "wait")]
    [TestCase("test", "redamber", "oosp")]
    public void Constructor_InvalidState_ThrowArgumentException(string id, string vehicle, string pedestrian)
    {
        // Arrange and Act
        var exception_trafficcontroller = Assert.Throws<ArgumentException>(() =>  new TrafficController(id, vehicle, pedestrian));

        // Assert
        Assert.That(exception_trafficcontroller.Message, Is.EqualTo("Argument Exception: TrafficController can only be initialised to the following states: 'green', 'amber', 'red', 'redamber' for the vehicle signals and 'wait' or 'walk' for the pedestrian signal"));
    }



    //L2R3   use of NSubstitute to get state amber and wait
    [Test]
    public void Constructor_SetInitialStates_ToAmberAndWait()
    {

        //Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();

        //Act
        var trafficcontroller = new TrafficController("test",fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);

        //Assert
        Assert.That(trafficcontroller.GetCurrentVehicleSignalState(), Is.EqualTo("amber"));
        Assert.That(trafficcontroller.GetCurrentPedestrianSignalState(), Is.EqualTo("wait"));
    }



    //L2R4   use of NSubstitute for status of vehicle,pedestrian and time to return their get status string output
    [Test]
    public void GetStatusReport_ReturnsConcatenatedStatus()
    {

        //Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();

        fake_vehicle.GetStatus().Returns("VehicleSignal,OK,OK,OK,");
        fake_pedestrian.GetStatus().Returns("PedestrianSignal,OK,OK,");
        fake_time.GetStatus().Returns("Time,OK,OK,");


        //Act
        var trafficcontroller = new TrafficController("test", fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);


        string result=trafficcontroller.GetStatusReport();

        //Assert
        Assert.That(result,Is.EqualTo("VehicleSignal,OK,OK,OK,PedestrianSignal,OK,OK,Time,OK,OK,"));
      
    }





    //===========Level Three Tests================





    //L3R1
    [Test]
    public void SetCurrentState_AmberToRed_DelayCorrectly_ReturnsTrue()
    {

        //Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();

        fake_time.Delay(3).Returns(true);
        fake_vehicle.SetAllRed(true).Returns(true);
        fake_pedestrian.SetWalk(true).Returns(true);
        fake_pedestrian.SetAudible(true).Returns(true);


        //Act
        var trafficcontroller = new TrafficController("test", fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);

        trafficcontroller.SetCurrentState("red", "walk");

        //Assert
        fake_time.Received(1).Delay(3);      //verify delay and call with 3 seconds

    }



    //L3R1  
    [Test]
    public void SetCurrentState_AmberToRed_DelayFails_ReturnsFalse()
    {

        //Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();

        //delay fails
        fake_time.Delay(3).Returns(false);



        //Act
        var trafficcontroller = new TrafficController("test", fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);

        bool result = trafficcontroller.SetCurrentState("red", "walk");

        //Assert
       Assert.That(result, Is.False);
       Assert.That(trafficcontroller.GetCurrentVehicleSignalState(), Is.EqualTo("amber"));
    }




    // L3R1 - verify SetWalk is called when transitioning amber to red
    [Test]
    public void SetCurrentState_AmberToRed_CallsSetWalk()
    {
        // Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();

        fake_time.Delay(3).Returns(true);
        fake_vehicle.SetAllRed(true).Returns(true);
        fake_pedestrian.SetWalk(true).Returns(true);
        fake_pedestrian.SetAudible(true).Returns(true);

        var trafficcontroller = new TrafficController("test", fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);

        // Act
        trafficcontroller.SetCurrentState("red", "walk");

        // Assert
        fake_pedestrian.Received(1).SetWalk(true);
    }


    // L3R1 - verify transition fails and state stays unchanged for each failing hardware call
    [TestCase(false, true, true, true)]   // Delay fails
    [TestCase(true, false, true, true)]   // SetAllRed fails
    [TestCase(true, true, false, true)]   // SetWalk fails
    [TestCase(true, true, true, false)]   // SetAudible fails
    public void SetCurrentState_AmberToRed_HardwareCallFails_ReturnsFalseAndStateUnchanged(bool delayFail, bool setAllRedFail, bool setWalkFail, bool setAudibleFail)
    {
        // Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();

        fake_time.Delay(3).Returns(delayFail);
        fake_vehicle.SetAllRed(true).Returns(setAllRedFail);
        fake_pedestrian.SetWalk(true).Returns(setWalkFail);
        fake_pedestrian.SetAudible(true).Returns(setAudibleFail);

        var trafficcontroller = new TrafficController("test",fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);

        // Act
        bool result = trafficcontroller.SetCurrentState("red", "walk");

        // Assert
        Assert.That(result, Is.False);
        Assert.That(trafficcontroller.GetCurrentVehicleSignalState(), Is.EqualTo("amber"));
        Assert.That(trafficcontroller.GetCurrentPedestrianSignalState(), Is.EqualTo("wait"));
    }


    // L3R2 - verify transition fails and state stays unchanged for each failing hardware call
    [TestCase(false, true, true, true,true)]   // Delay fails
    [TestCase(true, false, true, true,true)]   // SetWalk fails
    [TestCase(true, true, false, true,true)]   // Wait fails
    [TestCase(true, true, true, false,true)]   // SetAudible fails
    [TestCase(true, true, true, true, false)]   // SetAllgreen fails
    public void SetCurrentState_RedAmberToGreen_HardwareCallFails_ReturnsFalseAndStateUnchanged(bool delayFail, bool setAllGreenFail, bool setWalkFail,bool setWaitFail, bool setAudibleFail)
    {
        // Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();

        fake_time.Delay(3).Returns(delayFail);
        fake_vehicle.SetAllGreen(true).Returns(setAllGreenFail);
        fake_pedestrian.SetWalk(false).Returns(setWalkFail);
        fake_pedestrian.SetAudible(false).Returns(setAudibleFail);
        fake_pedestrian.SetWait(true).Returns(setWaitFail);

        var trafficcontroller = new TrafficController("test", fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);

        trafficcontroller.SetStateDirect("redamber", "walk");

        // Act
        bool result = trafficcontroller.SetCurrentState("green", "wait");

        // Assert
        Assert.That(result, Is.False);
        Assert.That(trafficcontroller.GetCurrentVehicleSignalState(), Is.EqualTo("redamber"));
        Assert.That(trafficcontroller.GetCurrentPedestrianSignalState(), Is.EqualTo("walk"));
    }


    //L3R2   Calls Delay of 3 sec from amber to green 
    [Test]
    public void SetCurrentState_RedAmberToGreen_CallsDelay_ReturnsTrue()
    {

        //Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();

        fake_time.Delay(3).Returns(true);
        fake_pedestrian.SetWalk(false).Returns(true);
        fake_pedestrian.SetWait(true).Returns(true);
        fake_pedestrian.SetAudible(false).Returns(true);
        fake_vehicle.SetAllGreen(true).Returns(true);



        //Act
        var trafficcontroller = new TrafficController("test", fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);

        trafficcontroller.SetStateDirect("redamber", "walk");

        trafficcontroller.SetCurrentState("green", "wait");

        //Assert
        fake_time.Received(1).Delay(3);      //verify delay and call with 3 seconds

    }



    //L3R2    Returns false and fails to set all green
    [Test]
    public void SetCurrentState_RedAmberToGreen_SetAllGreenFails_ReturnsFalse()
    {

        //Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();

        fake_time.Delay(3).Returns(true);
        fake_pedestrian.SetWalk(false).Returns(true);
        fake_pedestrian.SetWait(true).Returns(true);
        fake_pedestrian.SetAudible(false).Returns(true);
        fake_pedestrian.SetAudible(false).Returns(true);
        fake_vehicle.SetAllGreen(true).Returns(false);  //fails to set all green

        

        //Act
        var trafficcontroller = new TrafficController("test", fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);
        trafficcontroller.SetStateDirect("redamber", "walk");

        bool result = trafficcontroller.SetCurrentState("green", "wait");

        //Assert
        Assert.That(result, Is.False);
        Assert.That(trafficcontroller.GetCurrentVehicleSignalState(), Is.EqualTo("redamber"));
    }



    // L3R2 - Calls SetWait when  redamber to green
    [Test]
    public void SetCurrentState_RedAmberToGreen_SetWait_ReturnsTrue()
    {
        // Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();

        fake_time.Delay(3).Returns(true);
        fake_pedestrian.SetWalk(false).Returns(true);
        fake_pedestrian.SetWait(true).Returns(true);
        fake_pedestrian.SetAudible(false).Returns(true);
        fake_vehicle.SetAllGreen(true).Returns(true);

        var trafficcontroller = new TrafficController("test",fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);
        trafficcontroller.SetStateDirect("redamber", "walk");

        // Act
        trafficcontroller.SetCurrentState("green", "wait");

        // Assert
        fake_pedestrian.Received(1).SetWait(true);
    }



    //L3R3   Returns the fault detected when the state is out of service
    [Test]
    public void SetCurrentState_ToOutOfService_CallsFaultDetected_ReturnsTrue()
    {

        //Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();



        //Act
        var trafficcontroller = new TrafficController("test", fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);

        trafficcontroller.SetCurrentState("oosv", "oosp");

        //Assert
        fake_webservice.Received(1).FaultDetected(true);      

    }

    //L3R3  Returns the Log enginneer when out of service
    [Test]
    public void SetCurrentState_ToOutOfService_CallsLogEngineerRequired()
    {

        //Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();



        //Act
        var trafficcontroller = new TrafficController("test", fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);

        trafficcontroller.SetCurrentState("oosv", "oosp");

        //Assert
        fake_webservice.Received(1).LogEngineerRequired("out of service");

    }


    //L3R3    -Verifies that states have correct history
    [TestCase("amber", "wait")]
    [TestCase("green", "wait")]
    [TestCase("red", "walk")]
    [TestCase("redamber", "walk")]
    public void SetCurrentState_ReturnFromOutOfService_ReturnsToCorrectHistoryState(string fromVehicle,string fromPedestrian)
    {

        //Arrange
        
        var trafficcontroller = new TrafficController("test");
        trafficcontroller.SetStateDirect(fromVehicle, fromPedestrian);
        trafficcontroller.SetCurrentState("oosv", "oosp");

        //Act
        bool result = trafficcontroller.SetCurrentState(fromVehicle, fromPedestrian);

        //Assert
        Assert.That(result, Is.True);
        Assert.That(trafficcontroller.GetCurrentVehicleSignalState(), Is.EqualTo(fromVehicle));
        Assert.That(trafficcontroller.GetCurrentPedestrianSignalState(), Is.EqualTo(fromPedestrian));
    }


    // L3R3 - cannot return from Out of Service to a non-history state
    [TestCase("amber", "wait", "green", "wait")]
    [TestCase("amber", "wait", "red", "walk")]
    [TestCase("green", "wait", "amber", "wait")]
    [TestCase("red", "walk", "redamber", "walk")]
    public void SetCurrentState_ReturnFromOutOfService_ReturnsFalseNonHistoryState(string fromVehicle, string fromPedestrian, string toVehicle, string toPedestrian)
    {
        // Arrange
        var trafficcontroller = new TrafficController("test");
        trafficcontroller.SetStateDirect(fromVehicle, fromPedestrian);
        trafficcontroller.SetCurrentState("oosv", "oosp");

        // Act
        bool result = trafficcontroller.SetCurrentState(toVehicle, toPedestrian);

        // Assert
        Assert.That(result, Is.False);
    }



    //L3R4  Checks if devices has fault and all three faults are logged correctly
    [Test]
    public void GetStatusReport_FaultsDetection_FaultingDevices()
    {

        //Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();

        fake_vehicle.GetStatus().Returns("VehicleSignal,OK,FAULT,OK,");
        fake_pedestrian.GetStatus().Returns("PedestrianSignal,FAULT,OK,");
        fake_time.GetStatus().Returns("Timer,FAULT,OK,");

        //Act
        var trafficcontroller = new TrafficController("test", fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);

        trafficcontroller.GetStatusReport();

        //Assert
        fake_webservice.Received(1).LogEngineerRequired("VehicleSignal,PedestrianSignal,Timer,");

    }

    //L3R4  Gets the status report and does not call OOS 
    [Test]
    public void GetStatusReport_NoFaults_DoesNotCallsLogEngineerRequired()
    {

        //Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();

        fake_vehicle.GetStatus().Returns("VehicleSignal,OK,OK,");
        fake_pedestrian.GetStatus().Returns("PedestrianSignal,OK,OK,");
        fake_time.GetStatus().Returns("Timer,OK,OK,");

        //Act
        var trafficcontroller = new TrafficController("test", fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);

        trafficcontroller.GetStatusReport();

        //Assert
        fake_webservice.DidNotReceive().LogEngineerRequired("out of service");

    }




    //L3R5 Throws exception and sends email if a Fault is found
    [Test]
    public void GetStatusReport_LogEngineerRequired_ThrowsEmail()
    {

        //Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();

        fake_vehicle.GetStatus().Returns("VehicleSignal,OK,");
        fake_pedestrian.GetStatus().Returns("PedestrianSignal,FAULT,OK,");
        fake_time.GetStatus().Returns("Timer,OK,OK,");

        //Act
        fake_webservice
            .When(webservice => webservice.LogEngineerRequired("PedestrianSignal,"))
            .Throw(new Exception("log failed"));

        var trafficcontroller = new TrafficController("test", fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);

        trafficcontroller.GetStatusReport();

        //Assert
        fake_email.Received(1).SendMail("transportoffice@gmail.com","failed to log out of service","log failed");
    }



    //L3R5 Throws exception and sends email if a Fault is found 
//multiple faults
    [Test]
    public void GetStatusReport_LogEngineerRequired_VehicleAndPedestrianFault_ThrowsEmail()
    {

        //Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();

        fake_vehicle.GetStatus().Returns("VehicleSignal,FAULT,OK,");
        fake_pedestrian.GetStatus().Returns("PedestrianSignal,FAULT,OK,");
        fake_time.GetStatus().Returns("Timer,OK,OK,");

        //Act
        fake_webservice
            .When(webservice => webservice.LogEngineerRequired("VehicleSignal,PedestrianSignal,"))
            .Throw(new Exception("log failed"));

        var trafficcontroller = new TrafficController("test", fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);

        trafficcontroller.GetStatusReport();

        //Assert
        fake_email.Received(1).SendMail("transportoffice@gmail.com", "failed to log out of service", "log failed");
    }

    //L3R5 Throws exception and sends email if a Fault is found 
    //multiple faults...all fault
    [Test]
    public void GetStatusReport_LogEngineerRequired_AllFault_ThrowsEmail()
    {

        //Arrange
        var fake_vehicle = Substitute.For<IVehicleSignalManager>();
        var fake_pedestrian = Substitute.For<IPedestrianSignalManager>();
        var fake_time = Substitute.For<ITimeManager>();
        var fake_webservice = Substitute.For<IWebService>();
        var fake_email = Substitute.For<IEmailService>();

        fake_vehicle.GetStatus().Returns("VehicleSignal,FAULT,OK,");
        fake_pedestrian.GetStatus().Returns("PedestrianSignal,FAULT,OK,");
        fake_time.GetStatus().Returns("Timer,FAULT,OK,");

        //Act
        fake_webservice
            .When(webservice => webservice.LogEngineerRequired("VehicleSignal,PedestrianSignal,Timer,"))
            .Throw(new Exception("log failed"));

        var trafficcontroller = new TrafficController("test", fake_vehicle, fake_pedestrian, fake_time, fake_webservice, fake_email);

        trafficcontroller.GetStatusReport();

        //Assert
        fake_email.Received(1).SendMail("transportoffice@gmail.com", "failed to log out of service", "log failed");
    }

}