using OpenAC.Net.Core.Extensions;
using System;
using System.Text;
using System.Xml.Linq;

namespace OpenAC.Net.NFSe.Providers.Tinus
{
    internal class TinusServiceClient : NFSeSoapServiceClient, IServiceClient
    {
        public TinusServiceClient(ProviderTinus provider, TipoUrl tipoUrl) : base(provider, tipoUrl, SoapVersion.Soap11)
        {
        }

        public string CancelarNFSe(string cabec, string msg)
        {
            var message = new StringBuilder();
            message.Append("<CancelarNfse>");
            message.Append(msg);
            message.Append("</CancelarNfse>");

            var ns = string.Empty;
            switch (Provider.Configuracoes.WebServices.Ambiente)
            {
                case DFe.Core.Common.DFeTipoAmbiente.Producao:
                    {
                        ns = "xmlns:tin=\"http://www.tinus.com.br\"";
                        break;
                    }
                case DFe.Core.Common.DFeTipoAmbiente.Homologacao:
                    {
                        ns = "xmlns:tin=\"http://www.tinus2.com.br\"";
                        break;
                    }
            }

            return Execute("http://www.tinus.com.br/WSNFSE.CancelarNfse.CancelarNfse", message.ToString(), "", "CancelarNfseResponse", ns);
        }

        public string CancelarNFSeLote(string cabec, string msg)
        {
            throw new NotImplementedException();
        }

        public string ConsultarLoteRps(string cabec, string msg)
        {
            var message = new StringBuilder();
            message.Append("<ConsultarLoteRps>");
            message.Append(msg);
            message.Append("</ConsultarLoteRps>");

            var ns = string.Empty;
            switch (Provider.Configuracoes.WebServices.Ambiente)
            {
                case DFe.Core.Common.DFeTipoAmbiente.Producao:
                    {
                        ns = "xmlns:tin=\"http://www.tinus.com.br\"";
                        break;
                    }
                case DFe.Core.Common.DFeTipoAmbiente.Homologacao:
                    {
                        ns = "xmlns:tin=\"http://www.tinus2.com.br\"";
                        break;
                    }
            }

            return Execute("http://www.tinus.com.br/WSNFSE.ConsultarLoteRps.ConsultarLoteRps", message.ToString(), "", "ConsultarLoteRpsResponse", ns);
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
            message.Append("<RecepcionarLoteRps>");
            message.Append(msg);            
            message.Append("</RecepcionarLoteRps>");

            var ns = string.Empty;
            switch (Provider.Configuracoes.WebServices.Ambiente)
            {
                case DFe.Core.Common.DFeTipoAmbiente.Producao:
                    {
                        ns = "xmlns:tin=\"http://www.tinus.com.br\"";
                        break;
                    }
                case DFe.Core.Common.DFeTipoAmbiente.Homologacao:
                    {
                        ns = "xmlns:tin=\"http://www.tinus2.com.br\"";
                        break;
                    }
            }
             
            return Execute("http://www.tinus.com.br/WSNFSE.RecepcionarLoteRps.RecepcionarLoteRps", message.ToString(), "", "RecepcionarLoteRpsResponse", ns);
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
            return xmlDocument.ElementAnyNs(responseTag[0]).ToString();
        }
    }
}
