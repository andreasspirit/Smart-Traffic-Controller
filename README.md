🚦 Smart Traffic Controller
A Test-Driven Development (TDD) project built in C# for the CO2401 Software Development module at UCLan Cyprus.

📖 What does it do?
This project simulates a Smart Traffic Light System that controls vehicle and pedestrian signals at an intersection. It manages signal state transitions, detects faults, logs issues to a web service, and sends email alerts when logging fails.

⚙️ How it works
The TrafficController class manages two signals:

🚗 Vehicle signal — green, amber, red, redamber, oosv
🚶 Pedestrian signal — wait, walk, oosp

The system only allows valid state transitions based on a state diagram. For example:

amber/wait → red/walk
red/walk → redamber/walk
redamber/walk → green/wait
green/wait → amber/wait


🛠️ Technologies Used

C# — main programming language
NUnit 4 — unit testing framework
NSubstitute — mocking framework
Visual Studio 2022 — development environment


✅ Tests

50+ unit tests all passing
Parameterised tests using [TestCase]
NSubstitute stubs and mocks for all dependencies
Follows Arrange/Act/Assert pattern


👨‍💻 Author
Andreas Pnevmatikas — UCLan Cyprus 2025/2026
