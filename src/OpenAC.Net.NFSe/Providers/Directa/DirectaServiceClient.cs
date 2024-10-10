using OpenAC.Net.Core.Extensions;
using System;
using System.Text;
using System.Xml.Linq;

namespace OpenAC.Net.NFSe.Providers.Directa
{
    internal class DirectaServiceClient : NFSeSoapServiceClient, IServiceClient
    {
        public DirectaServiceClient(ProviderDirecta provider, TipoUrl tipoUrl) : base(provider, tipoUrl, SoapVersion.Soap11)
        {
        }

        public string CancelarNFSe(string cabec, string msg)
        {
            var message = new StringBuilder();
            message.Append("<wsn:CancelarNfseRequest>");
            message.Append($"<nfseCabecMsg>");
            message.AppendCData(cabec);
            message.Append($"</nfseCabecMsg>");
            message.Append("<nfseDadosMsg>");
            message.AppendCData(msg);
            message.Append("</nfseDadosMsg>");
            message.Append("</wsn:CancelarNfseRequest>");

            return Execute("https://wsnfsev1.natal.rn.gov.br:8444/axis2/services/CancelarNfse", message.ToString(), "", "CancelarNfseResponse", "xmlns:wsn=\"https://wsnfsev1.natal.rn.gov.br:8444\"");
        }

        public string CancelarNFSeLote(string cabec, string msg)
        {
            throw new NotImplementedException();
        }

        public string ConsultarLoteRps(string cabec, string msg)
        {
            var message = new StringBuilder();
            message.Append("<wsn:ConsultarLoteRpsRequest>");
            message.Append($"<nfseCabecMsg>");
            message.AppendCData(cabec);
            message.Append($"</nfseCabecMsg>");
            message.Append("<nfseDadosMsg>");
            message.AppendCData(msg);
            message.Append("</nfseDadosMsg>");
            message.Append("</wsn:ConsultarLoteRpsRequest>");

            return Execute("https://wsnfsev1.natal.rn.gov.br:8444/axis2/services/ConsultarLoteRps", message.ToString(), "", "ConsultarLoteRpsResponse", "xmlns:wsn=\"https://wsnfsev1.natal.rn.gov.br:8444\"");
        }

        public string ConsultarNFSe(string cabec, string msg)
        {
            throw new NotImplementedException();
        }

        public string ConsultarNFSeRps(string cabec, string msg)
        {
            throw new NotImplementedException();
        }

        public string ConsultarSequencialRps(string cabec, string msg)
        {
            throw new NotImplementedException();
        }

        public string ConsultarSituacao(string cabec, string msg)
        {
            throw new NotImplementedException();
        }

        public string Enviar(string cabec, string msg)
        {
            var message = new StringBuilder();
            message.Append("<wsn:RecepcionarLoteRpsRequest>");
            message.Append($"<nfseCabecMsg>");
            message.AppendCData(cabec);
            message.Append($"</nfseCabecMsg>");
            message.Append("<nfseDadosMsg>");
            message.AppendCData(msg);
            message.Append("</nfseDadosMsg>");
            message.Append("</wsn:RecepcionarLoteRpsRequest>");

            return Execute("https://wsnfsev1.natal.rn.gov.br:8444/axis2/services/RecepcionarLoteRps", message.ToString(), "", "RecepcionarLoteRpsResponse", "xmlns:wsn=\"https://wsnfsev1.natal.rn.gov.br:8444\"");
        }

        public string EnviarSincrono(string cabec, string msg)
        {
            throw new NotImplementedException();
        }

        public string SubstituirNFSe(string cabec, string msg)
        {
            throw new NotImplementedException();
        }

        protected override string TratarRetorno(XElement xmlDocument, string[] responseTag)
        {
            return xmlDocument.ElementAnyNs(responseTag[0]).ElementAnyNs("outputXML").Value;
        }
    }
}
