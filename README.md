
# Bear and Deer Ecosystem Simulation

This project is a Unity-based simulation where a bear, controlled by Unity ML-Agents, chases a deer across a forest terrain. The bear learns optimal behavior through reinforcement learning, guided by a reward system.

## Features
- **Unity ML-Agents Integration**: Train the bear using reinforcement learning.
- **Dynamic Deer Behavior**: The deer moves in random directions, simulating natural behavior.
- **Terrain Adaptation**: The bear aligns itself with the terrain's slope.
- **Reward System**: The bear is rewarded for approaching the deer, penalized for moving away, and further penalized for taking too long.

## Installation
1. Clone this repository:
   ```bash
   git clone https://github.com/your-username/bear-deer-simulation.git
   ```
2. Open the project in Unity (recommended version: 2021.3 or later).
3. Install Unity ML-Agents package if not already installed:
   - Open Unity Package Manager (`Window > Package Manager`).
   - Search for "ML-Agents" and install the package.

## Usage
1. **Training the Bear**:
   - Open the `Bear_chase_deer` in Unity.
   - Set up the ML-Agents training configuration file (`config.yaml`) as needed.
   - Run the training using the following command:
     ```bash
     mlagents-learn ConfigName.yaml --run-id=BearTraining
     ```
2. **Inference**:
   - Add the generated bear behavior .onx file to the "Model" field of the Bear and switch "Behavior Type" to Inference.
   - You can also use the BearBehavior-124017.onx file placed in the Scripts folder to test the inference mode.
   - Press Play to observe the trained bear chasing the deer.

## Key Scripts
- **BearBehavior.cs**:
  - Handles the bear's movement, reward system, and interaction with the environment.
  - Implements the ML-Agents `Agent` class.
- **DeerBehavior.cs**:
  - Controls the deer's random movement across the terrain.

## Reward System
The bear is rewarded or penalized when:
- **Approaching the Deer**: A positive reward is given for reducing the distance to the deer.
- **Time Penalty**: A small penalty is applied for each second elapsed.
- **Final Reward**: A large reward is granted when the bear catches the deer.


## Future Enhancements
- Add more dynamic behaviors for the deer, such as fleeing in response to the bear's proximity.
- Create the systems to use food, water, life, and fitness bars.
- Add other foraging options like grass and fish.
- Implement multiple agents (e.g. multiple competing, fighting, and reproducing bears) for a more complex ecosystem.
- Improve the environment look.


