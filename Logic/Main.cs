using Godot;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Web3;
using Timer = Godot.Timer;

public class Main : Node2D
{

    private Label blockNumberLabel;

    private SimpleGoDotRpcClient _simpleRpcClient;
    public override async void _Ready()
    {
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        //This is a catch all certificate callback as currently GoDot does not properly support TLS 1.3 (i.e infura)
        //See: https://github.com/godotengine/godot-mono-builds/pull/47
        ServicePointManager.ServerCertificateValidationCallback +=
            (sender, certificate, chain, errors) =>
            {

                if ((errors & (SslPolicyErrors.None)) > 0)
                {
                    return true;
                }

                if (
                    (errors & (SslPolicyErrors.RemoteCertificateNameMismatch)) > 0 ||
                    (errors & (SslPolicyErrors.RemoteCertificateNotAvailable)) > 0
                )
                {
                    return false;
                }

                return true;
            };
        //using the custom http client of GoDot (this does not work  with https tsl1.3 ) so only valid for http
        //this enables GoDot web but currently having issues after timer
        //_simpleRpcClient = new SimpleGoDotRpcClient(new Uri("http://testchain.nethereum.com:8545"), this);

        GetNode<Timer>("BlockNumberTimer").Start();
        blockNumberLabel = GetNode<Label>("lblBlockNumber");
        
        await SetBlockNumber();
    }


    public async void OnBlockNumberTimerTimeout()
    {
        await SetBlockNumber();
    }

    public async Task SetBlockNumber()
    {
        //using the custom godot rpc client
        //var web3 = new Web3(_simpleRpcClient);

        //using infura
        //var web3 = new Web3("https://mainnet.infura.io/v3/"yourProjectId"");

        //Using localhost testnet
        var web3 = new Web3();
        var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
        blockNumberLabel.Text = "Block Number: " + blockNumber.Value;
    }
}