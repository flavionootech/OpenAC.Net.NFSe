using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core;
using System;
using System.Text;
using System.Xml.Linq;

namespace OpenAC.Net.NFSe.Providers.Publica
{
    internal class PublicaServiceClient(ProviderPublica provider, TipoUrl tipoUrl) : NFSeSoapServiceClient(provider, tipoUrl, SoapVersion.Soap11), IServiceClient
    {

        #region Methods

        public string Enviar(string nfseCabecMsg, string nfseDadosMsg)
        {
            var message = new StringBuilder();
            message.Append("<ns2:RecepcionarLoteRps>");
            message.Append("<XML>");
            message.AppendCData(nfseDadosMsg);
            message.Append("</XML>");
            message.Append("</ns2:RecepcionarLoteRps>");

            return Execute("RecepcionarLoteRps", message.ToString(), "RecepcionarLoteRpsResponse");
        }

        public string EnviarSincrono(string cabec, string msg)
        {
            return string.Empty;
        }

        public string CancelarNFSe(string nfseCabecMsg, string nfseDadosMsg)
        {
            var message = new StringBuilder();
            message.Append("<ns2:CancelarNfse>");
            message.Append("<XML>");
            message.AppendCData(nfseDadosMsg);
            message.Append("</XML>");
            message.Append("</ns2:CancelarNfse>");

            return Execute("CancelarNfse", message.ToString(), "CancelarNfseResponse");
        }

        public string ConsultarLoteRps(string nfseCabecMsg, string nfseDadosMsg)
        {
            var message = new StringBuilder();
            message.Append("<ns2:ConsultarLoteRps>");
            message.Append("<XML>");
            message.AppendCData(nfseDadosMsg);
            message.Append("</XML>");
            message.Append("</ns2:ConsultarLoteRps>");

            return Execute("ConsultarLoteRps", message.ToString(), "ConsultarLoteRpsResponse");
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

        private string Execute(string action, string message, string responseTag) => Execute(action, message, "", responseTag, "xmlns:ns2=\"http://service.nfse.integracao.ws.publica/\"");



        protected override string TratarRetorno(XElement xmlDocument, string[] responseTag)
        {
            var element = xmlDocument.ElementAnyNs("Fault");
            if (element == null) return xmlDocument.ElementAnyNs(responseTag[0]).ElementAnyNs("return").Value;

            var exMessage = $"{element.ElementAnyNs("faultcode").GetValue<string>()} - {element.ElementAnyNs("faultstring").GetValue<string>()}";
            throw new OpenDFeCommunicationException(exMessage);
        }

        #endregion Methods
    }
}
