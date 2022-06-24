using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

public class WriteLagoonOceanWl : MonoBehaviour
{
    public Transform Ocean;
    string path, path2, path3, path4, path5, path6, path7, path8, path9, path10, path11;
    public GameObject Turbine;

    // This script should be attached to the Lagoon object in the Unity editor, for obtaining .txt outputs for
    // Lagoon and Ocean water levels, Power Generation, Turbine Operation and Turbine and Sluice flow rates

    // Remove this script during training to avoid poor performance.

    void CreateTtx()
    {
        path = Application.dataPath + "/LagoonWL.txt";
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "Lagoon Water Level (m)" + "\n");
        }

        path2 = Application.dataPath + "/OceanWL.txt";
        if (!File.Exists(path2))
        {
            File.WriteAllText(path2, "Ocean Water Level (m)" + "\n");
        }

        path3 = Application.dataPath + "/PowerGen.txt";
        if (!File.Exists(path3))
        {
            File.WriteAllText(path3, "Power Gen (MW)" + "\n");
        }

        path4 = Application.dataPath + "/nOTurbineOn.txt";
        if (!File.Exists(path4))
        {
            File.WriteAllText(path4, "nOTurbineOn" + "\n");
        }

        path5 = Application.dataPath + "/nOturbineIdling.txt";
        if (!File.Exists(path5))
        {
            File.WriteAllText(path5, "nOturbineIdling" + "\n");
        }

        path6 = Application.dataPath + "/TurbineQ.txt";
        if (!File.Exists(path6))
        {
            File.WriteAllText(path6, "Turbine Flow-rate (m3/s)" + "\n");
        }

        path7 = Application.dataPath + "/SluiceQ.txt";
        if (!File.Exists(path7))
        {
            File.WriteAllText(path7, "Sluice Flow-rate (m3/s)" + "\n");
        }

        path8 = Application.dataPath + "/SluiceOpening.txt";
        if (!File.Exists(path8))
        {
            File.WriteAllText(path8, "Sluice Opening (%)" + "\n");
        }

        path9 = Application.dataPath + "/TurbineModes.txt";
        if (!File.Exists(path9))
        {
            File.WriteAllText(path9, "Toff = 0, Ton = 1, TIdl = 2, Tpump = 3" + "\n");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        CreateTtx();
    }

    // Update is called once per frame
    void FixedUpdate() // Turbine.GetComponent<BulbTurbineScript>; e.g. BulbTurbineScript <- SwanseaDC
    {
        // Fill file storing Lagoon Water Levels
        float Lwl = transform.position.y;
        string LwlNL = Lwl.ToString() + "\n";
        File.AppendAllText(path, LwlNL);
        // Fill file storing Ocean Water Levels
        float Owl = Ocean.transform.position.y;
        string OwlNL = Owl.ToString() + "\n";
        File.AppendAllText(path2, OwlNL);
        // Fill file storing Power Gen
        float Pow = Turbine.GetComponent<SwanseaContinuousControl>().EnActualT/60;
        string PowNL = Pow.ToString() + "\n";
        File.AppendAllText(path3, PowNL);
        // Fill file storing nOTurbineOn
        float nOTurbineOn = Turbine.GetComponent<SwanseaContinuousControl>().nOTurbineOn;
        string nOTurbineOnNL = nOTurbineOn.ToString() + "\n";
        File.AppendAllText(path4, nOTurbineOnNL);
        // Fill file storing nOturbineIdling
        float nOturbineIdling = Turbine.GetComponent<SwanseaContinuousControl>().nOturbineIdling;
        string nOturbineIdlingNL = nOturbineIdling.ToString() + "\n";
        File.AppendAllText(path5, nOturbineIdlingNL);
        // Fill file storing Turbine Flow-rate
        float TurbineQ = Turbine.GetComponent<SwanseaContinuousControl>().QActualT;
        string TurbineQNL = TurbineQ.ToString() + "\n";
        File.AppendAllText(path6, TurbineQNL);
        // Fill file storing Sluice Flow-rate
        float SluiceQ = Turbine.GetComponent<SwanseaContinuousControl>().QActualS;
        string SluiceQNL = SluiceQ.ToString() + "\n";
        File.AppendAllText(path7, SluiceQNL);
        // Fill file storing Sluice Opening
        float SluiceO = Turbine.GetComponent<SwanseaContinuousControl>().sluiceOpening;
        string SluiceONL = SluiceO.ToString() + "\n";
        File.AppendAllText(path8, SluiceONL);
        // Fill file storing TurbineModes
        float one = 1;
        float two = 2;
        float zero = 0;
        if (nOTurbineOn > 0)
        {
            string TModeNL = one.ToString() + "\n";
            File.AppendAllText(path9, TModeNL);
        }
        else if (nOturbineIdling > 0)
        {
            string TModeNL = two.ToString() + "\n";
            File.AppendAllText(path9, TModeNL);
        }
        else // turbine is off
        {
            string TModeNL = zero.ToString() + "\n";
            File.AppendAllText(path9, TModeNL);
        }
    }
}
