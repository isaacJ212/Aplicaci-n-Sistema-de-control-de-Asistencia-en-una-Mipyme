using System.Net;

namespace MipymeAsistencia.Application.Common.Helpers;

public static class IpAddressValidator
{
    public static bool EsIpPermitida(string? ipCliente, string? ipsPermitidasConfig)
    {
        if (string.IsNullOrWhiteSpace(ipsPermitidasConfig) || ipsPermitidasConfig.Trim() == "*")
            return true; // Acceso global permitido si está configurado como '*' o vacío

        if (string.IsNullOrWhiteSpace(ipCliente))
            return false;

        // Normalizar IP del cliente
        ipCliente = ipCliente.Trim();
        if (ipCliente == "::ffff:127.0.0.1") ipCliente = "127.0.0.1";

        if (!IPAddress.TryParse(ipCliente, out var clientAddress))
            return false;

        var tokens = ipsPermitidasConfig.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var rawToken in tokens)
        {
            var token = rawToken.Trim();
            if (token == "*") return true;

            // Manejo de localhost estándar
            if (token.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                if (IPAddress.IsLoopback(clientAddress)) return true;
                continue;
            }

            // Manejo de rango CIDR (ej. 192.168.1.0/24)
            if (token.Contains('/'))
            {
                if (PerteneceACidr(clientAddress, token))
                    return true;
            }
            else
            {
                // Comparación directa de IP
                if (IPAddress.TryParse(token, out var allowedAddress))
                {
                    if (allowedAddress.Equals(clientAddress))
                        return true;

                    // Comparación loopback IPv4/IPv6
                    if (IPAddress.IsLoopback(clientAddress) && IPAddress.IsLoopback(allowedAddress))
                        return true;
                }
            }
        }

        return false;
    }

    private static bool PerteneceACidr(IPAddress address, string cidr)
    {
        try
        {
            var parts = cidr.Split('/');
            if (parts.Length != 2) return false;

            var baseIp = IPAddress.Parse(parts[0]);
            var prefixLength = int.Parse(parts[1]);

            if (baseIp.AddressFamily != address.AddressFamily)
                return false;

            var baseBytes = baseIp.GetAddressBytes();
            var addressBytes = address.GetAddressBytes();

            int fullBytes = prefixLength / 8;
            int remainingBits = prefixLength % 8;

            for (int i = 0; i < fullBytes; i++)
            {
                if (baseBytes[i] != addressBytes[i])
                    return false;
            }

            if (remainingBits > 0)
            {
                byte mask = (byte)(0xFF << (8 - remainingBits));
                if ((baseBytes[fullBytes] & mask) != (addressBytes[fullBytes] & mask))
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
