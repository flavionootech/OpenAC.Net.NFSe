using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core;
using OpenAC.Net.DFe.Core.Common;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace OpenAC.Net.NFSe.Providers.SigISS
{
    internal class SigISS204ServiceClient : NFSeSoapServiceClient, IServiceClient
    {
        #region Constructors

        public SigISS204ServiceClient(ProviderSigISS204 provider, TipoUrl tipoUrl, X509Certificate2 certificado) : base(provider, tipoUrl, certificado, SoapVersion.Soap12)
        {
        }

        #endregion Constructors

        #region Methods

        public string Enviar(string cabec, string msg)
        {
            var message = new StringBuilder();
            message.Append("<impl:RecepcionarLoteRpsRequest>");
            message.Append("<nfseCabecMsg>");
            message.AppendCData(cabec);
            message.Append("</nfseCabecMsg>");
            message.Append("<nfseDadosMsg>");
            message.AppendCData(msg);
            message.Append("</nfseDadosMsg>");
            message.Append("</impl:RecepcionarLoteRpsRequest>");

            return Execute("http://nfse.abrasf.org.br/RecepcionarLoteRps", message.ToString(), "RecepcionarLoteRpsResponse");
        }

        public string EnviarSincrono(string cabec, string msg)
        {
            var message = new StringBuilder();
            message.Append("<ns:RecepcionarLoteRpsSincronoRequest>");
            message.Append("<nfseCabecMsg>");
            message.AppendCData(cabec);
            message.Append("</nfseCabecMsg>");
            message.Append("<nfseDadosMsg>");
            message.AppendCData(msg);
            message.Append("</nfseDadosMsg>");
            message.Append("</ns:RecepcionarLoteRpsSincronoRequest>");

            return Execute("nfs#RecepcionarLoteRpsSincrono", message.ToString(), "RecepcionarLoteRpsSincronoResponse");
        }

        public string ConsultarSituacao(string cabec, string msg)
        {
            throw new System.NotImplementedException();
        }

        public string ConsultarLoteRps(string cabec, string msg)
        {
            var message = new StringBuilder();
            message.Append("<impl:ConsultarLoteRpsRequest>");
            message.Append("<nfseCabecMsg>");
            message.AppendCData(cabec);
            message.Append("</nfseCabecMsg>");
            message.Append("<nfseDadosMsg>");
            message.AppendCData(msg);
            message.Append("</nfseDadosMsg>");
            message.Append("</impl:ConsultarLoteRpsRequest>");

            return Execute("http://nfse.abrasf.org.br/ConsultarLoteRps", message.ToString(), "ConsultarLoteRpsResponse");
        }

        public string ConsultarSequencialRps(string cabec, string msg)
        {
            throw new System.NotImplementedException();
        }

        public string ConsultarNFSeRps(string cabec, string msg)
        {
            var message = new StringBuilder();
            message.Append("<e:consultarNfsePorRps>");
            message.Append("<xml>");
            message.AppendCData(msg);
            message.Append("</xml>");
            message.Append("</e:consultarNfsePorRps>");

            return Execute("http://nfse.abrasf.org.br/ConsultarNfsePorRps", message.ToString(), "consultarNfsePorRpsResponse");
        }

        public string ConsultarNFSe(string cabec, string msg)
        {
            var message = new StringBuilder();
            message.Append("<e:consultarNfseServicoPrestado>");
            message.Append("<xml>");
            message.AppendCData(msg);
            message.Append("</xml>");
            message.Append("</e:consultarNfseServicoPrestado>");

            return Execute("http://nfse.abrasf.org.br/ConsultarNfseServicoPrestado", message.ToString(), "consultarNfseServicoPrestadoResponse");
        }

        public string CancelarNFSe(string cabec, string msg)
        {
            var message = new StringBuilder();
            message.Append("<ns:CancelarNfseRequest>");
            message.Append("<nfseCabecMsg>");
            message.AppendCData(cabec);
            message.Append("</nfseCabecMsg>");
            message.Append("<nfseDadosMsg>");
            message.AppendCData(msg);
            message.Append("</nfseDadosMsg>");
            message.Append("</ns:CancelarNfseRequest>");

            return Execute("nfs#CancelarNfse", message.ToString(), "CancelarNfseResponse");
        }

        public string CancelarNFSeLote(string cabec, string msg)
        {
            throw new System.NotImplementedException();
        }

        public string SubstituirNFSe(string cabec, string msg)
        {
            var message = new StringBuilder();
            message.Append("<e:substituirNfse>");
            message.Append("<xml>");
            message.AppendCData(msg);
            message.Append("</xml>");
            message.Append("</e:substituirNfse>");

            return Execute("http://nfse.abrasf.org.br/SubstituirNfse", message.ToString(), "substituirNfseResponse");
        }

        private string Execute(string soapAction, string message, string responseTag)
        {
            return Execute(soapAction, message, "", responseTag, "xmlns:ns=\"https://abrasfnovaserrana.meumunicipio.online/ws/nfs\"");
        }

        protected override bool ValidarCertificadoServidor()
        {
            return Provider.Configuracoes.WebServices.Ambiente != DFeTipoAmbiente.Homologacao;
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

        #endregion Methods

    }
}
