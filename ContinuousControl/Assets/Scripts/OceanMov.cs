using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

public class OceanMov : MonoBehaviour
{
    private Vector3 _startPosition;
    // Sol. senoidal para oceano
    static float MinScale = 60f; // (60s/m)
    static float A_M2 = 3.20f, T_M2 = (12.42f * 60 * 60) / MinScale;
    static float Omega_M2 = 2 * Mathf.PI / T_M2;
    static float A_S2 = 1.14f, T_S2 = 12 * 60 * 60 / MinScale;
    static float Omega_S2 = 2 * Mathf.PI / T_S2;
    static float A_N2 = 0.61f, T_N2 = (12.66f * 60 * 60) / MinScale;
    static float Omega_N2 = 2 * Mathf.PI / T_N2;
    static float A_K1 = 0.08f, T_K1 = (23.93f * 60 * 60) / MinScale;
    static float Omega_K1 = 2 * Mathf.PI / T_K1;
    float To;
    float pM2, pS2, pN2, pK1, EpRestart;
    int index; StreamReader strReader, strReader2;
    int dataPick;
    public GameObject Turbine;
    int jj = 0; // Minute Counter
    int kk = 0; // Every 15 min Counter
    int day = 0; // Day counter

    //TRAINING MODEL FOR OCEAN (Artificial Ocean Signal) -> Comment out if not training

    //void Start()
    //{
    //    _startPosition = transform.position;
    //}

    ////Update is called once per frame
    //void FixedUpdate()
    //{
    //    pM2 = Turbine.GetComponent<SwanseaContinuousControl>().phaseM2;
    //    pS2 = Turbine.GetComponent<SwanseaContinuousControl>().phaseS2;
    //    pN2 = Turbine.GetComponent<SwanseaContinuousControl>().phaseN2;
    //    pK1 = Turbine.GetComponent<SwanseaContinuousControl>().phaseK1;
    //    // Artificial ocean position equals the sum of main tidal components:
    //    transform.position = _startPosition + new Vector3(0.0f, A_M2 * Mathf.Sin(Omega_M2 * (Time.fixedTime) + pM2) + A_S2 * Mathf.Sin(Omega_S2 * (Time.fixedTime) + pS2) + A_N2 * Mathf.Sin(Omega_N2 * (Time.fixedTime) + pN2) + A_K1 * Mathf.Sin(Omega_K1 * (Time.fixedTime) + pK1), 0.0f);
    //}

    // TEST MODEL FOR OCEAN (REAL MEASURED DATA) -> Comment out if not testing

    float[] OceanWLVector = new float[2882]; // Create empty ocean vector

    // Start is called before the first frame update
    void Awake()
    {
        ReadCSVFileOcean();
    }

    void ReadCSVFileOcean()
    {
        // Read one month of test data from file location (change according to the folder where test data is in)
        // Swansea OceanTestData folder contains the 26 months of ocean measurements from BODC used for testing
         StreamReader strReader = new StreamReader("D:\\UnityMLAgentsProjects\\ml-agents-release_10\\ContinuousControl\\Assets\\Scripts\\SwanseaOceanTestData\\26r.csv"); // Add path for loading file

        int ii = 0;
        while (ii < 2881)

        {
            var data_string = strReader.ReadLine();
            var data_values = data_string.Split(","[0]);
            string data_wl = data_values[0];
            float OceanWL = float.Parse(data_wl, CultureInfo.InvariantCulture);
            OceanWLVector[ii] = OceanWL; // fill empty ocean vector with one month of measured data by BODC
            ii += 1;
        }
    }

    void Start()
    {
        jj = 0; // Minute Counter
        kk = 0; // Every 15 min Counter
        _startPosition = transform.position;
        transform.position = _startPosition + new Vector3(0.0f, Mathf.Lerp(OceanWLVector[0], OceanWLVector[1], jj / 15f), 0.0f); // Use when input data is for every 15 Min, else (for 1 Min res data):
        //transform.position = _startPosition + new Vector3(0.0f, OceanWLVector[0], 0.0f); \\
    }

    //Update is called once per frame
    void FixedUpdate()
    {
        if ((jj % 15 == 0) && (jj != 0))// 15 minutes passed
        {
            kk += 1;
            jj = 0;
        }
        if ((kk % 96 == 0) && (kk != 0) && (jj == 0))// 1 day passed
        {
            day += 1;
            Debug.Log("day: " + day);
        }
        jj += 1;
        transform.position = _startPosition + new Vector3(0.0f, Mathf.Lerp(OceanWLVector[kk], OceanWLVector[kk + 1], jj / 15f), 0.0f);
        //Debug.Log("OceanWLVector[kk]: " + OceanWLVector[kk] + "OceanWLVector[kk + 1]: " + OceanWLVector[kk + 1] + "Min: " + jj + "Wl: " + Mathf.Lerp(OceanWLVector[kk], OceanWLVector[kk + 1], jj / 15f));
    }

    //Use this FixedUpdate() method if ocean data is per 1 Min (not every 15 Min) \\ Used for La Rance real ocean (1975 data
    //void FixedUpdate()
    //{
    //    kk += 1;
    //    transform.position = _startPosition + new Vector3(0.0f, OceanWLVector[kk], 0.0f);
    //    //Debug.Log("OceanWLVector[kk]: " + OceanWLVector[kk] + "OceanWLVector[kk + 1]: " + OceanWLVector[kk + 1] + "Min: " + jj + "Wl: " + Mathf.Lerp(OceanWLVector[kk], OceanWLVector[kk + 1], jj / 15f));
    //}
}
