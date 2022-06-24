# Túlio Moreira PhD Thesis

This repository contains revised codes for reproducing results from the PhD Thesis: AUTOMATIC FLEXIBLE CONTROL OF TIDAL RANGE STRUCTURES. The codes are aimed at simulating and optimising the operation of Tidal Range Structures (TRS), utilising the Swansea Bay Tidal Lagoon and La Rance as case studies. Currently, all simulation models are 0D.

**Examples of tide-prediction dependent control optimisation algorithims (Jupyter Notebook), for the Swansea Bay Tidal Lagoon case study:**

Two-Way Scheme: CH, CHV, EHT, EHTV, EHN, EHNV, EHN, EHNV

Two-Way with Pumping Scheme: EHTP, EHTVP (Being added)

All prediction dependent methods are being added in **PredictionDependentMethods** folder.

For the latest stable implementation for optimisation routines without pumping, visit https://github.com/TmoreiraBR/CoBaseTRS, submitted to Software Impacts.

* *The presented flexible control algorithims (EHT, EHN) are our interpretation of the "head-control" strategy developed by Jingjing Xue, Reza Ahmadian and Roger A. Falconer, in the work: Optimising the Operation of Tidal Range Schemes https://doi.org/10.3390/en12152870 

* *For all codes "V" stands for variant TRS operation, where sluice gates operate independently from sluices.
* *For all codes "P" stands for pumping. The pumping model utilised has been developed in the work: Marcondes Moreira, Túlio, et al. "Development and Validation of an AI-Driven Model for the La Rance Tidal Barrage: A Generalisable Case Study." arXiv e-prints (2022): arXiv-2202.
* *For all codes, both upper bound (perfect forecast) and real-time scenarios (utilising tidal predictions) are showcased.

**Examples of real-time prediction-free control optimisation solutions (C#/Unity3D/Unity ML-Agents):**

Continuous control, Proximal Policy Optimisation (PPO) solution, for the case study of the Swansea Bay Tidal Lagoon, without pumping -> Added to **ContinuousControl** folder. Current model results were published in [1].

Discrete control, Proximal Policy Optimisation (PPO) solution, for the case study of the La Rance Tidal Barrage, with pumping (To be added)

Hybrid control (Discrete + Continuous), Proximal Policy Optimisation (PPO) solution, for the case study of the Swansea Bay Tidal Lagoon, with pumping (To be added)

**This study uses data from the National Tidal and Sea Level Facility, provided by the British Oceanographic Data Centre (BODC) and funded by the Environment Agency.**

More specifically, 26 months of pre-processed measured (and predicted) ocean data by BODC (at the location of Mumbles) are currently available. All data are numbered. Furthermore, measured data contains the suffix "r" (real), while prediction data contains the "p" suffix. For acquring the raw data, please visit the BODC website (https://www.bodc.ac.uk/).

**Please, when using this repository for academic purposes cite the works of:**

[1] Moreira, Túlio Marcondes, et al. "Prediction-free, real-time flexible control of tidal lagoons through Proximal Policy Optimisation: A case study for the Swansea Lagoon." Ocean Engineering 247 (2022): 110657.

Marcondes Moreira, Túlio, et al. "Development and Validation of an AI-Driven Model for the La Rance Tidal Barrage: A Generalisable Case Study." arXiv e-prints (2022): arXiv-2202.

**Consider also citing the following work, when referring to flexible TRS operation:**

Optimising the Operation of Tidal Range Schemes https://doi.org/10.3390/en12152870
