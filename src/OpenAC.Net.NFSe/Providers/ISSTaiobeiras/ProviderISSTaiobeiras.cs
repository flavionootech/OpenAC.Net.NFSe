using OpenAC.Net.Core.Extensions;
using OpenAC.Net.NFSe.Configuracao;
using OpenAC.Net.NFSe.Nota;
using System.Linq;
using System.Text;

namespace OpenAC.Net.NFSe.Providers.ISSTaiobeiras
{
    internal class ProviderISSTaiobeiras : ProviderABRASF204
    {
        #region Constructors

        public ProviderISSTaiobeiras(ConfigNFSe config, OpenMunicipioNFSe municipio) : base(config, municipio)
        {
            Name = "ISSTaiobeiras";
        }

        #endregion Constructors

        protected override IServiceClient GetClient(TipoUrl tipo)
        {
            return new ISSTaiobeirasServiceClient(this, tipo);
        }

        protected override void PrepararEnviarSincrono(RetornoEnviar retornoWebservice, NotaServicoCollection notas)
        {
            if (retornoWebservice.Lote == 0) retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "Lote não informado." });
            if (notas.Count == 0) retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "RPS não informado." });
            if (retornoWebservice.Erros.Any()) return;

            var xmlLoteRps = new StringBuilder();

            foreach (var nota in notas)
            {
                var xmlRps = WriteXmlRps(nota, false, false);
                GravarRpsEmDisco(xmlRps, $"Rps-{nota.IdentificacaoRps.DataEmissao:yyyyMMdd}-{nota.IdentificacaoRps.Numero}.xml", nota.IdentificacaoRps.DataEmissao);
                xmlLoteRps.Append(xmlRps);
            }

            var xmlLote = new StringBuilder();
            xmlLote.Append($"<EnviarLoteRpsSincronoEnvio xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" {GetNamespace()}>");
            xmlLote.Append($"<LoteRps Id=\"L{retornoWebservice.Lote}\" {GetVersao()}>");
            xmlLote.Append($"<NumeroLote>{retornoWebservice.Lote}</NumeroLote>");
            if (UsaPrestadorEnvio) xmlLote.Append("<Prestador>");
            xmlLote.Append("<CpfCnpj>");
            xmlLote.Append(Configuracoes.PrestadorPadrao.CpfCnpj.IsCNPJ()
                ? $"<Cnpj>{Configuracoes.PrestadorPadrao.CpfCnpj.ZeroFill(14)}</Cnpj>"
                : $"<Cpf>{Configuracoes.PrestadorPadrao.CpfCnpj.ZeroFill(11)}</Cpf>");
            xmlLote.Append("</CpfCnpj>");
            if (!Configuracoes.PrestadorPadrao.InscricaoMunicipal.IsEmpty()) xmlLote.Append($"<InscricaoMunicipal>{Configuracoes.PrestadorPadrao.InscricaoMunicipal}</InscricaoMunicipal>");
            if (UsaPrestadorEnvio) xmlLote.Append("</Prestador>");
            xmlLote.Append($"<QuantidadeRps>{notas.Count}</QuantidadeRps>");
            xmlLote.Append("<ListaRps>");
            xmlLote.Append(xmlLoteRps);
            xmlLote.Append("</ListaRps>");
            xmlLote.Append("</LoteRps>");
            xmlLote.Append("</EnviarLoteRpsSincronoEnvio>");

            retornoWebservice.XmlEnvio = xmlLote.ToString();
        }
    }
}
