using OpenAC.Net.Core.Extensions;
using System;
using System.Xml.Linq;

namespace OpenAC.Net.NFSe.Providers.ISSJoinville
{
    internal class ISSJoinvilleServiceClient(ProviderISSJoinville provider, TipoUrl tipoUrl) : NFSeSoapServiceClient(provider, tipoUrl, SoapVersion.Soap11), IServiceClient
    {

        #region Methods

        public string Enviar(string nfseCabecMsg, string nfseDadosMsg)
        {
            return Execute("https://nfemws.joinville.sc.gov.br/EnviarLoteRpsEnvio", nfseDadosMsg, "EnviarLoteRpsEnvioResponse");
        }

        public string EnviarSincrono(string cabec, string msg)
        {
            return string.Empty;
        }

        public string CancelarNFSe(string nfseCabecMsg, string nfseDadosMsg)
        {
            return Execute("https://nfemws.joinville.sc.gov.br/CancelarNfseEnvio", nfseDadosMsg, "CancelarNfseEnvioResponse");
        }

        public string ConsultarLoteRps(string nfseCabecMsg, string nfseDadosMsg)
        {
            return Execute("https://nfemws.joinville.sc.gov.br/ConsultarLoteRpsEnvio", nfseDadosMsg, "ConsultarLoteRpsEnvioResponse");
        }

        public string ConsultarNFSe(string nfseCabecMsg, string nfseDadosMsg)
        {
            return string.Empty;
        }

        public string ConsultarNFSeRps(string nfseCabecMsg, string nfseDadosMsg)
        {
            return string.Empty;
        }

        public string ConsultarSituacao(string nfseCabecMsg, string nfseDadosMsg)
        {
            return string.Empty;
        }

        public string ConsultarSequencialRps(string nfseCabecMsg, string nfseDadosMsg) => throw new NotImplementedException();

        public string CancelarNFSeLote(string nfseCabecMsg, string nfseDadosMsg) => throw new NotImplementedException();

        public string SubstituirNFSe(string nfseCabecMsg, string nfseDadosMsg) => throw new NotImplementedException();

        private string Execute(string action, string message, string responseTag) => Execute(action, message, "", responseTag, "xmlns:ws=\"nfemws.joinville.sc.gov.br\"");


        protected override string TratarRetorno(XElement xmlDocument, string[] responseTag)
        {
            return xmlDocument.ElementAnyNs(responseTag[0]).ToString();
        }

        #endregion Methods
    }
}
