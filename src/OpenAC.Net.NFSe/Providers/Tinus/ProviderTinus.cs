using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core;
using OpenAC.Net.DFe.Core.Serializer;
using OpenAC.Net.NFSe.Configuracao;
using OpenAC.Net.NFSe.Nota;
using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OpenAC.Net.NFSe.Providers.Tinus
{
    internal class ProviderTinus : ProviderABRASF
    {
        #region Constructors

        public ProviderTinus(ConfigNFSe config, OpenMunicipioNFSe municipio) : base(config, municipio)
        {
            Name = "Tinus";
        }

        #endregion Constructors

        protected override IServiceClient GetClient(TipoUrl tipo)
        {
            return new TinusServiceClient(this, tipo);
        }

        protected override string GerarCabecalho()
        {
            return $"<cabecalho versao=\"1\" xmlns=\"http://www.abrasf.org.br/ABRASF/arquivos/nfse.xsd\"><versaoDados>1</versaoDados></cabecalho>";
        }

        protected override string GetSchema(TipoUrl tipo)
        {
            return "nfsetinus.xsd";
        }

        protected override string GetNamespace()
        {
            return "";
            //return "xmlns=\"http://www.tinus.com.br\"";
        }

        protected override void PrepararEnviar(RetornoEnviar retornoWebservice, NotaServicoCollection notas)
        {
            if (retornoWebservice.Lote == 0) retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "Lote não informado." });
            if (notas.Count == 0) retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "RPS não informado." });
            if (retornoWebservice.Erros.Count > 0) return;

            var xmlLoteRps = new StringBuilder();

            foreach (var nota in notas)
            {
                var xmlRps = WriteXmlRps(nota, false, false);
                xmlLoteRps.Append(xmlRps);
                GravarRpsEmDisco(xmlRps, $"Rps-{nota.IdentificacaoRps.DataEmissao:yyyyMMdd}-{nota.IdentificacaoRps.Numero}.xml", nota.IdentificacaoRps.DataEmissao);
            }

            var xmlLote = new StringBuilder();
            xmlLote.Append($"<Arg>");
            xmlLote.Append($"<LoteRps id=\"L{retornoWebservice.Lote}\">");
            xmlLote.Append($"<NumeroLote>{retornoWebservice.Lote}</NumeroLote>");
            xmlLote.Append($"<Cnpj>{Configuracoes.PrestadorPadrao.CpfCnpj.ZeroFill(14)}</Cnpj>");
            xmlLote.Append($"<InscricaoMunicipal>{Configuracoes.PrestadorPadrao.InscricaoMunicipal}</InscricaoMunicipal>");
            xmlLote.Append($"<QuantidadeRps>{notas.Count}</QuantidadeRps>");
            xmlLote.Append("<ListaRps>");
            xmlLote.Append(xmlLoteRps);
            xmlLote.Append("</ListaRps>");
            xmlLote.Append("</LoteRps>");
            xmlLote.Append("</Arg>");

            retornoWebservice.XmlEnvio = xmlLote.ToString();
        }

        protected override XElement WriteInfoRPS(NotaServico nota)
        {
            var incentivadorCultural = nota.IncentivadorCultural == NFSeSimNao.Sim ? 1 : 2;

            string regimeEspecialTributacao;
            string optanteSimplesNacional;
            if (nota.RegimeEspecialTributacao == RegimeEspecialTributacao.SimplesNacional)
            {
                regimeEspecialTributacao = "6";
                optanteSimplesNacional = "1";
            }
            else
            {
                var regime = (int)nota.RegimeEspecialTributacao;
                regimeEspecialTributacao = regime == 0 ? string.Empty : regime.ToString();
                optanteSimplesNacional = "2";
            }

            var situacao = nota.Situacao == SituacaoNFSeRps.Normal ? "1" : "2";

            var infoRps = new XElement("InfRps", new XAttribute("id", $"R{nota.IdentificacaoRps.Numero}"));

            infoRps.Add(WriteIdentificacao(nota));
            infoRps.AddChild(AdicionarTag(TipoCampo.DatHor, "", "DataEmissao", 20, 20, Ocorrencia.Obrigatoria, nota.IdentificacaoRps.DataEmissao));
            infoRps.AddChild(AdicionarTag(TipoCampo.Int, "", "NaturezaOperacao", 1, 1, Ocorrencia.Obrigatoria, nota.NaturezaOperacao));
            infoRps.AddChild(AdicionarTag(TipoCampo.Int, "", "RegimeEspecialTributacao", 1, 1, Ocorrencia.NaoObrigatoria, regimeEspecialTributacao));
            infoRps.AddChild(AdicionarTag(TipoCampo.Int, "", "OptanteSimplesNacional", 1, 1, Ocorrencia.Obrigatoria, optanteSimplesNacional));
            infoRps.AddChild(AdicionarTag(TipoCampo.Int, "", "IncentivadorCultural", 1, 1, Ocorrencia.Obrigatoria, incentivadorCultural));
            infoRps.AddChild(AdicionarTag(TipoCampo.Int, "", "Status", 1, 1, Ocorrencia.Obrigatoria, situacao));

            return infoRps;
        }

        protected override void AssinarEnviar(RetornoEnviar retornoWebservice)
        {
            retornoWebservice.XmlEnvio = XmlSigning.AssinarXmlTodos(retornoWebservice.XmlEnvio, "Rps", "InfRps", "id", Certificado, false, false, true, SignDigest.SHA1);
            retornoWebservice.XmlEnvio = XmlSigning.AssinarXml(retornoWebservice.XmlEnvio, "Arg", "LoteRps", "id", Certificado, false, false, true, SignDigest.SHA1);
        }

        protected override void TratarRetornoEnviar(RetornoEnviar retornoWebservice, NotaServicoCollection notas)
        {
            // Analisa mensagem de retorno
            var xmlRet = XDocument.Parse(retornoWebservice.XmlRetorno);
            var xmlRecepcionarLoteRpsResult = xmlRet.ElementAnyNs("RecepcionarLoteRpsResponse").ElementAnyNs("RecepcionarLoteRpsResult");
            MensagemErro(retornoWebservice, xmlRecepcionarLoteRpsResult);
            if (retornoWebservice.Erros.Any()) return;

            retornoWebservice.Data = xmlRecepcionarLoteRpsResult?.ElementAnyNs("DataRecebimento")?.GetValue<DateTime>() ?? DateTime.MinValue;
            retornoWebservice.Protocolo = xmlRecepcionarLoteRpsResult?.ElementAnyNs("Protocolo")?.GetValue<string>() ?? string.Empty;
            retornoWebservice.Sucesso = !retornoWebservice.Protocolo.IsEmpty();

            if (!retornoWebservice.Sucesso) return;

            foreach (var nota in notas)
            {
                nota.NumeroLote = retornoWebservice.Lote;
            }
        }

        protected override void PrepararCancelarNFSe(RetornoCancelar retornoWebservice)
        {
            if (retornoWebservice.NumeroNFSe.IsEmpty() || retornoWebservice.CodigoCancelamento.IsEmpty())
            {
                retornoWebservice.Erros.Add(new Evento { Codigo = "AC0001", Descricao = "Número da NFSe/Codigo de cancelamento não informado para cancelamento." });
                return;
            }

            var loteBuilder = new StringBuilder();
            loteBuilder.Append($"<Arg>");
            loteBuilder.Append("<Pedido>");
            loteBuilder.Append($"<InfPedidoCancelamento Id=\"N{retornoWebservice.NumeroNFSe}\">");
            loteBuilder.Append("<IdentificacaoNfse>");
            loteBuilder.Append($"<Numero>{retornoWebservice.NumeroNFSe}</Numero>");
            loteBuilder.Append($"<Cnpj>{Configuracoes.PrestadorPadrao.CpfCnpj.ZeroFill(14)}</Cnpj>");
            loteBuilder.Append($"<InscricaoMunicipal>{Configuracoes.PrestadorPadrao.InscricaoMunicipal}</InscricaoMunicipal>");
            loteBuilder.Append($"<CodigoMunicipio>{Configuracoes.PrestadorPadrao.Endereco.CodigoMunicipio}</CodigoMunicipio>");
            loteBuilder.Append("</IdentificacaoNfse>");
            loteBuilder.Append($"<CodigoCancelamento>{retornoWebservice.CodigoCancelamento}</CodigoCancelamento>");
            loteBuilder.Append("</InfPedidoCancelamento>");
            loteBuilder.Append("</Pedido>");
            loteBuilder.Append("</Arg>");

            retornoWebservice.XmlEnvio = loteBuilder.ToString();
        }

        protected override void PrepararConsultarLoteRps(RetornoConsultarLoteRps retornoWebservice)
        {
            var loteBuilder = new StringBuilder();
            loteBuilder.Append($"<Arg>");
            loteBuilder.Append("<Prestador>");
            loteBuilder.Append($"<Cnpj>{Configuracoes.PrestadorPadrao.CpfCnpj.ZeroFill(14)}</Cnpj>");
            loteBuilder.Append($"<InscricaoMunicipal>{Configuracoes.PrestadorPadrao.InscricaoMunicipal}</InscricaoMunicipal>");
            loteBuilder.Append("</Prestador>");
            loteBuilder.Append($"<Protocolo>{retornoWebservice.Protocolo}</Protocolo>");
            loteBuilder.Append("</Arg>");
            retornoWebservice.XmlEnvio = loteBuilder.ToString();
        }

        protected override void TratarRetornoConsultarLoteRps(RetornoConsultarLoteRps retornoWebservice, NotaServicoCollection notas)
        {
            // Analisa mensagem de retorno
            var xmlRet = XDocument.Parse(retornoWebservice.XmlRetorno);
            var xmlConsultarLoteRpsResult = xmlRet.ElementAnyNs("ConsultarLoteRpsResponse").ElementAnyNs("ConsultarLoteRpsResult");
            MensagemErro(retornoWebservice, xmlConsultarLoteRpsResult);
            if (retornoWebservice.Erros.Any()) return;

            var retornoLote = xmlRet.ElementAnyNs("ConsultarLoteRpsResponse");
            var consultarLoteRpsResult = retornoLote?.ElementAnyNs("ConsultarLoteRpsResult");
            var listaNfse = consultarLoteRpsResult?.ElementAnyNs("ListaNfse");

            if (listaNfse == null)
            {
                retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "Lista de NFSe não encontrada! (ListaNfse)" });
                return;
            }

            retornoWebservice.Sucesso = true;

            foreach (var compNfse in listaNfse.ElementsAnyNs("CompNfse"))
            {
                var nfse = compNfse.ElementAnyNs("Nfse")?.ElementAnyNs("InfNfse");
                var numeroNFSe = nfse.ElementAnyNs("Numero")?.GetValue<string>() ?? string.Empty;
                var chaveNFSe = nfse.ElementAnyNs("CodigoVerificacao")?.GetValue<string>() ?? string.Empty;
                var dataEmissao = nfse.ElementAnyNs("DataEmissao")?.GetValue<DateTime>() ?? DateTime.Now;
                var numeroRps = nfse?.ElementAnyNs("IdentificacaoRps")?.ElementAnyNs("Numero")?.GetValue<string>() ?? string.Empty;
                GravarNFSeEmDisco(compNfse.AsString(true), $"NFSe-{numeroNFSe}-{chaveNFSe}-.xml", dataEmissao);

                var nota = notas.FirstOrDefault(x => x.IdentificacaoRps.Numero == numeroRps);
                if (nota == null)
                {
                    notas.Load(compNfse.ToString());
                }
                else
                {
                    nota.IdentificacaoNFSe.Numero = numeroNFSe;
                    nota.IdentificacaoNFSe.Chave = chaveNFSe;
                    nota.IdentificacaoNFSe.DataEmissao = dataEmissao;
                    nota.XmlOriginal = compNfse.AsString();
                }
            }
        }

        protected override void TratarRetornoCancelarNFSe(RetornoCancelar retornoWebservice, NotaServicoCollection notas)
        {
            // Analisa mensagem de retorno
            var xmlRet = XDocument.Parse(retornoWebservice.XmlRetorno);
            var xmlResult = xmlRet.Root.ElementAnyNs("CancelarNfseResult");
            MensagemErro(retornoWebservice, xmlResult);
            if (retornoWebservice.Erros.Any())
            {
                return;
            }

            var confirmacaoCancelamento = xmlResult?.ElementAnyNs("Cancelamento")?.ElementAnyNs("Confirmacao")?.ElementAnyNs("Pedido")?.ElementAnyNs("InfPedidoCancelamento");
            if (confirmacaoCancelamento == null)
            {
                retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "Confirmação do cancelamento não encontrada! (InfConfirmacaoCancelamento)" });
                return;
            }

            retornoWebservice.Sucesso = xmlResult?.ElementAnyNs("Cancelamento")?.ElementAnyNs("Confirmacao")?.ElementAnyNs("DataHoraCancelamento") != null;
            retornoWebservice.Data = xmlResult?.ElementAnyNs("Cancelamento")?.ElementAnyNs("Confirmacao")?.ElementAnyNs("DataHoraCancelamento")?.GetValue<DateTime>() ?? DateTime.MinValue;

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
