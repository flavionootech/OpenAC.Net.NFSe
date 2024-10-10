using OpenAC.Net.NFSe.Configuracao;
using System.Linq;
using System.Xml.Linq;
using System;
using OpenAC.Net.NFSe.Nota;
using OpenAC.Net.Core.Extensions;

namespace OpenAC.Net.NFSe.Providers.Directa
{
    internal class ProviderDirecta : ProviderABRASF
    {
        #region Constructors

        public ProviderDirecta(ConfigNFSe config, OpenMunicipioNFSe municipio) : base(config, municipio)
        {
            Name = "Directa";
        }

        #endregion Constructors

        protected override IServiceClient GetClient(TipoUrl tipo)
        {
            return new DirectaServiceClient(this, tipo);
        }

        protected override string GerarCabecalho()
        {
            return $"<cabecalho versao=\"1\" xmlns=\"http://www.abrasf.org.br/ABRASF/arquivos/nfse.xsd\"><versaoDados>1</versaoDados></cabecalho>";
        }

        protected override void TratarRetornoCancelarNFSe(RetornoCancelar retornoWebservice, NotaServicoCollection notas)
        {
            // Analisa mensagem de retorno
            var xmlRet = XDocument.Parse(retornoWebservice.XmlRetorno);
            MensagemErro(retornoWebservice, xmlRet, "CancelarNfseResposta");
            if (retornoWebservice.Erros.Any())
            {
                return;
            }

            var confirmacaoCancelamento = xmlRet.ElementAnyNs("CancelarNfseResposta")?.ElementAnyNs("Cancelamento")?.ElementAnyNs("Confirmacao")?.ElementAnyNs("Pedido")?.ElementAnyNs("InfPedidoCancelamento");
            if (confirmacaoCancelamento == null)
            {
                retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "Confirmação do cancelamento não encontrada! (InfConfirmacaoCancelamento)" });
                return;
            }

            retornoWebservice.Sucesso = xmlRet.ElementAnyNs("CancelarNfseResposta")?.ElementAnyNs("Cancelamento")?.ElementAnyNs("Confirmacao")?.ElementAnyNs("Datahora") != null;
            retornoWebservice.Data = xmlRet.ElementAnyNs("CancelarNfseResposta")?.ElementAnyNs("Cancelamento")?.ElementAnyNs("Confirmacao")?.ElementAnyNs("Datahora")?.GetValue<DateTime>() ?? DateTime.MinValue;

            // Se a nota fiscal cancelada existir na coleção de Notas Fiscais, atualiza seu status:
            var nota = notas.FirstOrDefault(x => x.IdentificacaoNFSe.Numero.Trim() == retornoWebservice.NumeroNFSe);
            if (nota != null)
            {
                nota.Situacao = SituacaoNFSeRps.Cancelado;
                nota.Cancelamento.Pedido.CodigoCancelamento = retornoWebservice.CodigoCancelamento;
                nota.Cancelamento.DataHora = retornoWebservice.Data;
                nota.Cancelamento.MotivoCancelamento = retornoWebservice.Motivo;
            }
        }
    }
}
