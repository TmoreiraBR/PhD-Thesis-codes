Go to Assets/Scripts for visualising the main scripts responsible for training/testing the DRL Agents. 

For training or testing, simply copy all folders in this repository > Open UnityHub -> Add **ContinuousControl** folder as new project and open it. A prefab with the 0D model environment is available and should be droped into the scene.

Lagoon variable wetted area and test ocean files are loaded in ReadCSV.cs and OceanMov.cs files. Make sure the correct path is assigned. 

The prefab already contains a trained policy neural network for the agent. When running the model (Play in Unity), WriteLagoonOceanWl.cs creates .txt files with ocean, lagoon water levels, power output and the strategy performed by the agent. 

For hyperparameters setting, use "old" trainer config file when using ml-agents 0.22.0. For newer versions (after 0.22.0 and up to 0.26.0) use "updated" trainer config.  
