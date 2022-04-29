// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;

var cert = X509Certificate2
    .CreateFromCertFile("../../../infura2.cer");

    var hash = cert.GetCertHashString();

Debug.WriteLine(hash);
