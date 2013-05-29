﻿using System.Collections.Generic;
using System.Text;

namespace Satel
{    
    public class Integra
    {
        public Dictionary<byte,Partition> partition { get; private set; }
        public Dictionary<byte, Zone> zone{ get; private set; }
        public Dictionary<byte, Output> output { get; private set; }

        static string hardwareModel(int code)
        {
            switch (code)
            {
                case 0:
                    return "24";                    
                case 1:
                    return "32";
                case 2:
                    return "64";
                case 3:
                    return "128";
                case 4:
                    return "128-WRL SIM300";
                case 132:
                    return "128-WRL LEON";
                case 66:
                    return "64 PLUS";
                case 67:
                    return "128 PLUS";
                default:
                    return "UNKNOWN";                    
            }

        }

               

        public Integra(string host, int port)
        {
            Communication.integraAddress = host;
            Communication.integraPort = port;
            partition=new Dictionary<byte,Partition>();
            zone = new Dictionary<byte, Zone>();
            output = new Dictionary<byte, Output>();
        }


        public string getVersion()
        {
            try
            {
                var resp = Communication.sendCommand(0x7E);

                var result = "INTEGRA " + hardwareModel(resp[0]);
                result += " " + (char)resp[1] + "." + (char)resp[2] + (char)resp[3] + " " + (char)resp[4] + (char)resp[5] + (char)resp[6] + (char)resp[7];
                result += "-" + (char)resp[8] + (char)resp[9] + "-" + (char)resp[10] + (char)resp[11];
                result += " LANG: " + (resp[12] == 1 ? "English" : "Other");
                result += " SETTINGS: " + (resp[13] == 0xFF ? "stored" : "NOT STORED") + " in flash";
                return result;
            }
            catch
            {
                return "Communication failiure.";
            }
        }

        public void readPartitions()
        {            
            for (byte i = 1; i < 33; i++)
            {
                var resp = Communication.sendCommand(0xEE, 0x0, i);
                if (resp[3]!=0xfe) 
                    partition.Add(i, new Partition(i, Encoding.UTF8.GetString(resp, 3, 16),resp[2]));
            }
            Communication.closeConnection();
        }


        public void readZones()
        {
            for (byte i = 1; i < 129; i++)
            {
                var resp = Communication.sendCommand(0xEE, 0x5, i);
                if (resp[3] != 0xfe)                    
                    zone.Add(i, new Zone(partition[resp[19]],i, Encoding.UTF8.GetString(resp, 3, 16),resp[2]));
            }
            Communication.closeConnection();
        }

        public void readOutputs()
        {
            for (byte i = 1; i < 129; i++)
            {
                var resp = Communication.sendCommand(0xEE, 0x4, i);
                if (resp[2] != 0)
                    output.Add(i, new Output(i, Encoding.UTF8.GetString(resp, 3, 16),resp[2]));
            }
            Communication.closeConnection();

        }

    }
}
