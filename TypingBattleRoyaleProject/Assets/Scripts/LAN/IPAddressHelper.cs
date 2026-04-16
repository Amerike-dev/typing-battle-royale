using UnityEngine;
using System.Collections.Generic;

public static class IPAddressHelper
{
    public static List<IPInfo> GetAllIPAddresses()
    {
        var ipList = new List<IPInfo>();

        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                var info = new IPInfo();
                info.Address = ip.ToString();

                switch (ip.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        info.Type = "IPv4";

                        if (ip.ToString() == "127.0.0.1")
                        {
                            info.Description = "Loopback (localhost)";
                            info.IsRecommended = false;
                        }
                        else if (ip.ToString().StartsWith("192.168.") ||
                                 ip.ToString().StartsWith("10.") ||
                                 ip.ToString().StartsWith("172.16.") ||
                                 ip.ToString().StartsWith("172.17.") ||
                                 ip.ToString().StartsWith("172.18.") ||
                                 ip.ToString().StartsWith("172.19.") ||
                                 ip.ToString().StartsWith("172.20.") ||
                                 ip.ToString().StartsWith("172.21.") ||
                                 ip.ToString().StartsWith("172.22.") ||
                                 ip.ToString().StartsWith("172.23.") ||
                                 ip.ToString().StartsWith("172.24.") ||
                                 ip.ToString().StartsWith("172.25.") ||
                                 ip.ToString().StartsWith("172.26.") ||
                                 ip.ToString().StartsWith("172.27.") ||
                                 ip.ToString().StartsWith("172.28.") ||
                                 ip.ToString().StartsWith("172.29.") ||
                                 ip.ToString().StartsWith("172.30.") ||
                                 ip.ToString().StartsWith("172.31."))
                        {
                            info.Description = "LAN (Red Local) - RECOMENDADA";
                            info.IsRecommended = true;
                        }
                        else
                        {
                            info.Description = "IPv4 (Otra red)";
                            info.IsRecommended = false;
                        }
                        break;

                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        info.Type = "IPv6";
                        info.Description = "Dirección IPv6";
                        info.IsRecommended = false;
                        break;

                    default:
                        info.Type = "Desconocido";
                        info.Description = "Tipo de dirección desconocido";
                        info.IsRecommended = false;
                        break;
                }
                ipList.Add(info);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al obtener direcciones IP: {e.Message}");
        }

        return ipList;
    }
    
    public static string GetRecommendedLANIP()
    {
        var ips = GetAllIPAddresses();

        foreach (var ip in ips)
        {
            if (ip.IsRecommended) return ip.Address;
        }
        
        foreach (var ip in ips)
        {
            if (ip.Type == "IPv4" && ip.Address != "127.0.0.1") return ip.Address;
        }

        return "127.0.0.1";
    }
    
    public static void PrintAllIPAddresses()
    {
        Debug.Log("<color=cyan>=== Direcciones IP Disponibles ===</color>");

        var ips = GetAllIPAddresses();
        bool hasRecommended = false;

        foreach (var ip in ips)
        {
            string prefix = ip.IsRecommended ? "<color=green>[RECOMENDADA]</color> " : "";
            Debug.Log($"{prefix}{ip.Type}: {ip.Address} - {ip.Description}");

            if (ip.IsRecommended) hasRecommended = true;
        }

        Debug.Log("<color=cyan>===================================</color>");

        if (hasRecommended)
            Debug.Log($"<color=green>Usa esta IP para conectar otros jugadores: {GetRecommendedLANIP()}</color>");
        else
            Debug.LogWarning("<color=yellow>No se encontró una dirección IP LAN típica (192.168.x.x o 10.x.x.x).</color>");
    }
}

[System.Serializable]
public struct IPInfo
{
    public string Address;
    public string Type;
    public string Description;
    public bool IsRecommended;
}
