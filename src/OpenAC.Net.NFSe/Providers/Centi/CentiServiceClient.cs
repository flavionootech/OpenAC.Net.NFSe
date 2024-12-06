using System;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

namespace OpenAC.Net.NFSe.Providers.Centi
{
    internal class CentiServiceClient(ProviderCenti provider, TipoUrl tipoUrl) : NFSeSoapServiceClient(provider, tipoUrl, SoapVersion.Soap11), IServiceClient
    {
        public string CancelarNFSe(string cabec, string msg)
        {
            var dados = new StringBuilder();
            dados.Append("{\"xml\": \"");
            dados.Append(msg.Replace("\"", "\\\""));
            dados.Append("\", \"usuario\": \"");
            dados.Append(Provider.Configuracoes.WebServices.Usuario);
            dados.Append("\", \"senha\": \"");
            dados.Append(Provider.Configuracoes.WebServices.Senha);
            dados.Append("\"}");

            Execute(new StringContent(dados.ToString(), CharSet, "application/json"), HttpMethod.Post);

            return EnvelopeRetorno;
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
            var dados = new StringBuilder();
            dados.Append("{\"xml\": \"");
            dados.Append(msg.Replace("\"", "\\\""));
            dados.Append("\", \"usuario\": \"");
            dados.Append(Provider.Configuracoes.WebServices.Usuario);
            dados.Append("\", \"senha\": \"");
            dados.Append(Provider.Configuracoes.WebServices.Senha);
            dados.Append("\"}");

            Execute(new StringContent(dados.ToString(), CharSet, "application/json"), HttpMethod.Post);

            return EnvelopeRetorno;
        }

        public string SubstituirNFSe(string cabec, string msg)
        {
            throw new NotImplementedException();
        }

        protected override string TratarRetorno(XElement xmlDocument, string[] responseTag)
        {
            throw new NotImplementedException();
        }
    }
}
