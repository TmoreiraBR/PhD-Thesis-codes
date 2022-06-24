using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

public class ReadCSV : MonoBehaviour
{
    // Script responsible for reading cvs file with lagoon elevation and area
    //public float[] LagoonWLVector = new float[44];
    //public float[] LagoonAreaVector = new float[44];
    // New Lagoon Area:
    public float[] LagoonWLVector = new float[46];
    public float[] LagoonAreaVector = new float[46];

    // Start is called before the first frame update
    void Awake()
    {
        ReadCSVFile();
    }

    void ReadCSVFile()
    {
        // Read Swansea wetted area (km2 as a function of lake water level) - Digitised from Xue, J., Ahmadian, R., Falconer, R.A., 2019. Optimising the operation
        // of tidal range schemes.Energies 12(15), 2870.

        StreamReader strReader = new StreamReader("D:\\UnityMLAgentsProjects\\ml-agents-release_10\\SwanseaProject\\SwanseaMLAgents\\Assets\\Scripts\\_wansea2.csv");
        bool endOfFile = false;
        int ii = 0;
        while (!endOfFile)
        {
            var data_string = strReader.ReadLine ();
            if (data_string == null)
            {
                endOfFile = true;
                break;
            }
            // Storing to variable

            var data_values = data_string.Split(","[0]);
            string data_wl = data_values[0];
            float LagoonWL = float.Parse(data_wl, CultureInfo.InvariantCulture);
            string data_area = data_values[1];
            float LagoonArea = float.Parse(data_area, CultureInfo.InvariantCulture);
            LagoonWLVector[ii] = LagoonWL;
            LagoonAreaVector[ii] = LagoonArea;
            //Debug.Log(LagoonWLVector[ii]);
            //Debug.Log(LagoonAreaVector[ii]);
            ii += 1;
        }
    }
}
