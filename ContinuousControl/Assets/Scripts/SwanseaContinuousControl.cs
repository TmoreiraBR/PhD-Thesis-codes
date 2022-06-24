using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class SwanseaContinuousControl : Agent
{

    // Script responsible for setting up the Unity ML-Agents Markov Decision Process (MDP) input states, actions and rewards and the simulated
    // training environment.
    // In our study, the training environment follows a 0D Tidal Range Structure (TRS) model. The TRS 0D model contains state-of-art functions for
    // simulating flow rate through turbines and sluices, power output and predicting lagoon water level variations. A modification to the state-of-art is
    // the inclusion of a "Momentum Ramp" function responsible for flow rate transitions when operating the TRS.

    // The fixed parameters (e.g. lagoon wetted area, number of turbines, sluice area) are for the case study of the Swansea Bay Tidal Lagoon,
    // but can be applied to any Tidal Range Structure.

    [SerializeField] private int nOfGroups;
    [SerializeField] private int nOfTurbines;
    private int To = 2; // Turbine Orientation (1-Flood or 2-Ebb oriented)
    // Turbine Group
    public Transform Ocean;
    public Transform Lagoon;
    public GameObject Barrage;
    public GameObject Sluices;
    public GameObject LagoonObj;

    static float MinScale = 60f; // (60s/m)
    private bool turbineIsReady = true;
    private bool sluiceIsReady = true;
    private float oceanElevation;
    private float lagoonElevation;
    public event Action OnReset;

    // Turbine&Pump variables
    private float g = 9.81f; // Gravity acceleration (m/s2)
    private float rho = 1024f; // Seawater density (kg/m3)
    private float f = 50f; // Grid frequency (Hz)
    private float Np = 95f; // Number of generator poles in the turbine (Athanasius Angeloudis)
    private float Dt = 7.35f; // Bulb turbine diameter (m)
    private float Ca = 20 * Mathf.Pow(10, 6); // (All 16 units = 20MW)
    private float CDt = 1.36f; // Turbine Discharge Coefficient when running in Idling mode
    private float Sp; // Calculated in Awake
    private float At; // Calculated in Awake
    // Barrage Losses ( Aggidis - Operational optimization of a tidal barrage across the Mersaey Estuary)
    float Eff_ge = .97f; // Generator Efficiency (Andre, 1976)
    float Eff_te = .995f; // Transformer Efficiency (Libaux et al, 2011)
    float Eff_we = .95f; // Water Friction (Baker and Leach, 2006)
    float Eff_gear_box_e = .972f; // Gear Box/Drive train (Taylor, 2008)
    float Eff_turbine_avail = .95f; // Turbine availlability in lifecycle (Baker and Leach, 2006)
    float CE; // Turbine local efficiency losses - Calculated in Awake

    float Q11; // Turbine Unit (Dimensionless) Flow-rate - Calculated in PowerGen
    float Eff; // Turbine Efficiency - Calculated in PowerGen
    float head; // Head Difference - Calculated in PowerGen - public shows variable changing value in Unity
    float n11; // Turbine Unit Speed
    float Qt; // (Temporary) Turbine Flow-rate - Calculated in PowerGen
    float QT; // (Total) Turbine Flow-rate - Calculated in PowerGen
    float Pow; // (Temporary) Power Output - Calculated in PowerGen
    float POW; // (Final) Power Output - Calculated in PowerGen
    float QTc = 0f; // Flow-rate used for calculating new Lagoon water level
    float QIdl = 0f;

    // Sluice gate
    float As = 800; // Value based on paper from Athanasius, where total sluice area = 800 m2 (80*10)
    float QS = 0;

    // Variable Lagoon parameters
    float AlVar;
    float lagoonInitialP;
    float lagoonCentered;
    float newLagoonElevation;
    float lagoonXPosition;
    float lagoonZPosition;

    // Reward (Energy)
    float En = 0; // Energy generated (Mega-Joules) per minute = (J/s)*(60s)*10^-6
    public float sluiceOpening = 0;
    public int nOTurbineOn = 0; 
    public int nOturbineIdling = 0; 

    // Use Amplitude Values for Normalize Ocean/Lagoon State inputs
    float A_M2 = 3.20f;
    float A_S2 = 1.14f;
    float A_N2 = 0.61f;
    float A_K1 = 0.08f;
    float Amp;
    float minOP;
    float maxOP;

    // Create timer for operating sluice and turbines every 15 min
    float timerTurbine = 15f; // each second is a minute because of MinScale
    float timerSluice = 15f;
    public float episodeEnd = 30 * 24 * 60; // each episode takes 30 days

    // Alternative to ramp function:
    float QTo; // Stores previous turbine flow-rate at time t
    float ETo; // Stores previous energy at time t
    float B = .4f; // "Momentum" factor approx equal to 15m lag
    public float QActualT = 0;
    public float EnActualT = 0; // Flow and energy estimated utilizing the momentum ramp function

    float QSo; // Stores previous sluice flow-rate at time t
    public float QActualS = 0;
    float SumRewards = 0;
    // private bool collectReward = true;
    //public float InitTime = 0;
    // Randomize phase difference between tidal constituents
    public float phaseM2 = 0;
    public float phaseS2 = 0;
    public float phaseN2 = 0;
    public float phaseK1 = 0;
    // Change Turbine color depending on chosen operation
    public Material TurbinePump;
    public Material TurbineIdling;
    public Material TurbineOff;
    public Material TurbinePowerGen;
    // Start is called before the first frame update
    float nOf;
    float[] LagoonWL;
    float[] LagoonKm2;

    public override void Initialize()
    {
        // RequestDecision();
        Ca = Ca / nOfGroups; // Group capacity
        nOf = nOfTurbines / nOfGroups; // Number of Turbines per group
        Sp = 2 * 60 * f / Np; // Turbine speed ?? (ref: Athanasius)
        At = Mathf.PI * Dt * Dt / 4;
        CE = Eff_ge * Eff_te * Eff_we * Eff_gear_box_e * Eff_turbine_avail;
        Amp = A_M2 + A_S2 + A_N2 + A_K1; // Max possible tidal range
        lagoonXPosition = Lagoon.transform.position.x;
        lagoonZPosition = Lagoon.transform.position.z;
        lagoonInitialP = Lagoon.transform.position.y;
        minOP = lagoonInitialP - Amp;
        maxOP = lagoonInitialP + Amp;
    }

    void Start()
    {
        LagoonWL = LagoonObj.GetComponent<ReadCSV>().LagoonWLVector;
        LagoonKm2 = LagoonObj.GetComponent<ReadCSV>().LagoonAreaVector;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (sluiceIsReady)
        {
            sluiceOpening = (actionBuffers.ContinuousActions[0] + 1f) / 2f; // Sets Sluice fully open or fully closed
            sluiceIsReady = false; // Pick action and wait 15 minutes
        }
        if (turbineIsReady)
        {
            nOTurbineOn = Mathf.RoundToInt(((actionBuffers.ContinuousActions[1] + 1f) / 2f) * nOfGroups) * nOfTurbines / nOfGroups; // Number of turbines that will generate energy
            int nOfTurbRem = nOfTurbines - nOTurbineOn; // Remaining Turbines for other Modes (Idling and Off)
            nOturbineIdling = Mathf.RoundToInt(((actionBuffers.ContinuousActions[2] + 1f) / 2f) * nOfGroups) * nOfTurbRem / nOfGroups;
            // Assign color to turbine and sluices of the TRS environment, according to action taken
            if (nOTurbineOn == 16)
            {
                Barrage.transform.GetChild(0).GetComponent<Renderer>().material = TurbinePowerGen;

            }
            else if (nOturbineIdling == 16)
            {
                Barrage.transform.GetChild(0).GetComponent<Renderer>().material = TurbineIdling;
            }
            else if (nOTurbineOn == 0 && nOturbineIdling == 0)
            {
                Barrage.transform.GetChild(0).GetComponent<Renderer>().material = TurbineOff;
            }

            if (sluiceOpening < .5f)
            {
                Sluices.transform.GetChild(0).GetComponent<Renderer>().material = TurbineOff;
                Sluices.transform.GetChild(1).GetComponent<Renderer>().material = TurbineOff;

            }
            else if (sluiceOpening > .5f)
            {
                Sluices.transform.GetChild(0).GetComponent<Renderer>().material = TurbineIdling;
                Sluices.transform.GetChild(1).GetComponent<Renderer>().material = TurbineIdling;
            }
            turbineIsReady = false; // Pick action and wait 15 minutes
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((Ocean.transform.position.y - minOP) / (maxOP - minOP)); // Normalized ocean water level
        sensor.AddObservation((Lagoon.transform.position.y - minOP) / (maxOP - minOP)); // Normalized lagoon water level
        sensor.AddObservation(sluiceOpening);
        sensor.AddObservation(nOTurbineOn);
        sensor.AddObservation(nOturbineIdling);
    }

    public override void OnEpisodeBegin()
    {
        Reset();
        // Random ocean phase inputs for each environment instace during training
        phaseM2 = 2 * Mathf.PI * UnityEngine.Random.Range(0.0f, 1.0f);
        phaseS2 = 2 * Mathf.PI * UnityEngine.Random.Range(0.0f, 1.0f);
        phaseN2 = 2 * Mathf.PI * UnityEngine.Random.Range(0.0f, 1.0f);
        phaseK1 = 2 * Mathf.PI * UnityEngine.Random.Range(0.0f, 1.0f);
    }

    private void Reset()
    {
        turbineIsReady = true;
        sluiceIsReady = true;
        sluiceOpening = 0f; // Sluice off
        nOTurbineOn = 0; // TurbineMode offline
        nOturbineIdling = 0;
        timerTurbine = 15f; // each second is a minute because of MinScale
        timerSluice = 15f; // each second is a minute because of MinScale
        episodeEnd = 30 * 24 * 60;
        QTc = 0; // QTc2 = 0; QTc3 = 0; QTc4 = 0; // First, Second, 3, 4 group flow-rates
        QIdl = 0;
        En = 0; // En2 = 0; En3 = 0; En4 = 0; // First, Second, 3, 4 group energies
        QActualT = 0; EnActualT = 0; QS = 0; QSo = 0; QActualS = 0;
        OnReset?.Invoke();
        Lagoon.transform.position = new Vector3(lagoonXPosition, lagoonInitialP, lagoonZPosition);
    }

    private void FixedUpdate()
    {
        if (turbineIsReady || sluiceIsReady) // turn on neural network only when asking for input
        {
            RequestDecision();
        }

        oceanElevation = Ocean.transform.position.y;
        lagoonElevation = Lagoon.transform.position.y;
        head = oceanElevation - lagoonElevation; // -> Sinal positivo = fluxo em direção à lagoa
        QTo = QActualT;
        QSo = QActualS;
        ETo = EnActualT;
        Sluice(head, out QS);
        Idling(head, nOturbineIdling, out QIdl);
        PowerGen(head, nOTurbineOn, out QTc, out En);

        // Momentum Ramp Function (instead of classic sinusoidal ramp for tidal range structures)
        QActualT = -B * (QTc + QIdl - QTo) + QTc + QIdl;
        QActualS = -B * (sluiceOpening * QS - QSo) + sluiceOpening * QS;
        EnActualT = -B * (En - ETo) + En;
        lagoonCentered = lagoonElevation - lagoonInitialP; // Centers lagoon around 0, so we can use AlVar equation

        Interp(lagoonCentered, out float AlVar);
        newLagoonElevation = (QActualT + QActualS) * (Time.fixedDeltaTime / (AlVar * Mathf.Pow(10, 6))) + lagoonElevation;
        AddReward(EnActualT); // EnActualT is the positive (or negative) energy accumulated in a minute for all turbines
        SumRewards += EnActualT; // Calculated Total reward
        Lagoon.transform.position = new Vector3(lagoonXPosition, newLagoonElevation, lagoonZPosition);
        // Timer for sluice and turbine operation (Keep Agent choice for 15 min)
        if (sluiceIsReady == false)
        {
            timerSluice -= Time.fixedDeltaTime;
            if (timerSluice < 0)
            {
                sluiceIsReady = true;
                timerSluice = 15f;
            }
        }
        if (turbineIsReady == false)
        {
            timerTurbine -= Time.fixedDeltaTime;
            if (timerTurbine < 0)
            {
                turbineIsReady = true;
                timerTurbine = 15f;
            }
        }
        episodeEnd -= Time.fixedDeltaTime;
        if (episodeEnd < 0)
        {
            Debug.Log("GetCumulativeReward" + GetCumulativeReward());
            Debug.Log("Energy (MJ) accumulated: " + SumRewards);
            SumRewards = 0; // So we reset the accumulated reward per episode
            EndEpisode();
        }
    }

    void ramp(float t, float tr, string command, out float r)
    {
        r = 0;
        if (command == "start")
        {
            if (t <= tr)
            {
                r = Mathf.Sin(Mathf.PI / 2 * t / tr);
            }
            else
            {
                r = 1;
            }
        }
        else if (command == "stop")
        {
            if (t <= tr)
            {
                r = Mathf.Cos(Mathf.PI / 2 * t / tr);
            }
            else
            {
                r = 0;
            }
        }
    }


    void Interp(float lagoonCentered, out float AlVar) // Returns Variable lake area in km2
    {
        bool Found = false;
        int ii = 0;
        float x0; float y0;
        float x1; float y1;
        AlVar = 0f;
        while (!Found)
        {
            x0 = LagoonWL[ii]; y0 = LagoonKm2[ii];
            x1 = LagoonWL[ii + 1]; y1 = LagoonKm2[ii + 1];
            if ((x0 <= lagoonCentered) && (x1 >= lagoonCentered))
            {
                Found = true;
                AlVar = y0 + (lagoonCentered - x0) * (y1 - y0) / (x1 - x0);
            }
            if (ii > 41)
            {
                Debug.Log("LagoonWL[ii]: " + LagoonWL[ii] + "lagoonCentered: " + lagoonCentered);
            }
            ii += 1;
        }
    }

    void Idling(float head, float nTurb, out float QTc) // Idling turbine function
    {
        QT = nTurb * Mathf.Sign(head) * CDt * At * Mathf.Sqrt(2 * g * Mathf.Abs(head));
        QTc = QT * MinScale;
    }

    void Sluice(float head, out float QS)
    {
        QS = Mathf.Sign(head) * As * Mathf.Sqrt(2 * g * Mathf.Abs(head)) * MinScale;
    }

    void PowerGen(float head, float nTurb, out float QTc, out float En)
    {
        if (Mathf.Abs(head) >= 1) // Minimum head of 1m for Power Gen.
        {
            n11 = Sp * Dt / Mathf.Sqrt(Mathf.Abs(head));
            if (n11 <= 255) // 
            {
                Q11 = 0.017f * n11 + 0.49f;
            }
            else if (n11 > 255)
            {
                Q11 = 4.75f;
            }

            float Eff = (-0.0019f * n11 + 1.2461f) * CE;
            if (Eff <= 0)
            {
                Eff = 0;
            }
            else if (Eff >= .95)
            {
                Eff = .95f;
            }
            if (To == 1) // Turbine is Flood Oriented (pointed towards Ocean)
            {
                if (head < 0) // Ebb Gen
                    Eff = 0.90f * Eff;
            }
            else if (To == 2) // Turbine is Ebb Oriented (pointed towards Coast - USUAL DESIGN)
            {
                if (head > 0) // Flood Gen
                    Eff = 0.90f * Eff;
            }
            Qt = nTurb * Mathf.Sign(head) * Q11 * Mathf.Pow(Dt, 2) * Mathf.Sqrt(Mathf.Abs(head)); // Andritz chart
            Pow = Mathf.Abs(rho * g * head * Qt) * Eff;

            if (Ca != 0 && Pow > nTurb * Ca) // Power Gen above turbine rated capacity
            {
                POW = nTurb * Ca; // Ca = turbine capacity (20MW for the Swansea Lagoon)
                QT = Mathf.Sign(head) * POW / (rho * g * Mathf.Abs(head) * Eff); // Dividing by Eff increases flow-rate above Ca (conservative)
            }
            else
            {
                POW = Pow;
                QT = Qt;
            }

            QTc = QT * MinScale;
            En = POW * MinScale * Mathf.Pow(10, -6);

        }
        else
        {
            QT = 0f;
            QTc = 0f;
            En = 0f;
        }
    }

}
