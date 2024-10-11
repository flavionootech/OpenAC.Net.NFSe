using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core;
using System;
using System.Text;
using System.Xml.Linq;

namespace OpenAC.Net.NFSe.Providers.ISSTaiobeiras
{
    internal class ISSTaiobeirasServiceClient(ProviderISSTaiobeiras provider, TipoUrl tipoUrl) : NFSeSoapServiceClient(provider, tipoUrl, SoapVersion.Soap11), IServiceClient
    {
        public string CancelarNFSe(string cabec, string msg)
        {
            var message = new StringBuilder();
            message.Append("<nfse:CancelarNfse>");
            message.Append("<nfse:CancelarNfseRequest>");
            message.Append($"<nfseCabecMsg>");
            message.AppendCData(cabec);
            message.Append($"</nfseCabecMsg>");
            message.Append("<nfseDadosMsg>");
            message.AppendCData(msg);
            message.Append("</nfseDadosMsg>");
            message.Append("</nfse:CancelarNfseRequest>");
            message.Append("</nfse:CancelarNfse>");

            return Execute("http://nfse.abrasf.org.br/CancelarNfse", message.ToString(), "", "CancelarNfseResponse", "xmlns:nfse=\"http://nfse.abrasf.org.br\"");
        }

        public string CancelarNFSeLote(string cabec, string msg)
        {
            throw new NotImplementedException();
        }

        public string ConsultarLoteRps(string cabec, string msg)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public string EnviarSincrono(string cabec, string msg)
        {
            var message = new StringBuilder();
            message.Append("<nfse:RecepcionarLoteRpsSincrono>");
            message.Append("<nfse:RecepcionarLoteRpsSincronoRequest>");
            message.Append($"<nfseCabecMsg>");
            message.AppendCData(cabec);
            message.Append($"</nfseCabecMsg>");
            message.Append("<nfseDadosMsg>");
            message.AppendCData(msg);
            message.Append("</nfseDadosMsg>");
            message.Append("</nfse:RecepcionarLoteRpsSincronoRequest>");
            message.Append("</nfse:RecepcionarLoteRpsSincrono>");


            return Execute("http://nfse.abrasf.org.br/RecepcionarLoteRpsSincrono", message.ToString(), "", "RecepcionarLoteRpsSincronoResponse", "xmlns:nfse=\"http://nfse.abrasf.org.br\"");
        }

        public string SubstituirNFSe(string cabec, string msg)
        {
            throw new NotImplementedException();
        }

        protected override string TratarRetorno(XElement xmlDocument, string[] responseTag)
        {
            var element = xmlDocument.ElementAnyNs("Fault");
            if (element != null)
            {
                var exMessage = $"{element.ElementAnyNs("faultcode").GetValue<string>()} - {element.ElementAnyNs("faultstring").GetValue<string>()}";
                throw new OpenDFeCommunicationException(exMessage);
            }

            return xmlDocument.ElementAnyNs(responseTag[0]).ElementAnyNs(responseTag[0]).ElementAnyNs("outputXML").Value;
        }
    }
}
