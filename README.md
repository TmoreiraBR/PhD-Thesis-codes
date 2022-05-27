# Túlio Moreira PhD Thesis
Codes for reproducing results from PhD Thesis: AUTOMATIC FLEXIBLE CONTROL OF TIDAL RANGE STRUCTURES

This repository contains codes for simulating and optimising the operation of Tidal Range Structures (TRS). All simulation models are 0D. 

**Examples of conventional (prediction dependent) control optimisation algorithims (in Python), for the Swansea Bay Tidal Lagoon case study:**

Two-Way Scheme: CH, CHV, EHT, EHTV, EHN, EHNV (EHN, EHNV -> Under revision)*

Two-Way with Pumping Scheme: EHTP, EHTVP (To be added)*

* *The presented flexible control algorithims (EHT, EHN) are our interpretation of the "head-control" strategy developed by Jingjing Xue, Reza Ahmadian and Roger A. Falconer, in the work: Optimising the Operation of Tidal Range Schemes https://doi.org/10.3390/en12152870 

**Examples of real-time prediction-free control optimisation solutions (C#/Unity3D/Unity ML-Agents):**

Continuous control, Proximal Policy Optimisation (PPO) solution (Added - Under revision) - Swansea Bay Tidal Lagoon, without pumping (To be added)

Discrete control, Proximal Policy Optimisation (PPO) solution (To be added) - La Rance Tidal Barrage, with pumping (To be added)

Hybrid control, Proximal Policy Optimisation (PPO) solution (To be added) - Swansea Bay Tidal Lagoon, with pumping (To be added)

Please, when using this repository for academic purposes cite the works of:

Moreira, Túlio Marcondes, et al. "Prediction-free, real-time flexible control of tidal lagoons through Proximal Policy Optimisation: A case study for the Swansea Lagoon." Ocean Engineering 247 (2022): 110657.

Marcondes Moreira, Túlio, et al. "Development and Validation of an AI-Driven Model for the La Rance Tidal Barrage: A Generalisable Case Study." arXiv e-prints (2022): arXiv-2202.

Optimising the Operation of Tidal Range Schemes https://doi.org/10.3390/en12152870
