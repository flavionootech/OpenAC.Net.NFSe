using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core.Document;
using OpenAC.Net.NFSe.Configuracao;
using OpenAC.Net.NFSe.Nota;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OpenAC.Net.NFSe.Providers.ISSJoinville
{
    internal class ProviderISSJoinville : ProviderABRASF204
    {
        #region Constructors

        public ProviderISSJoinville(ConfigNFSe config, OpenMunicipioNFSe municipio) : base(config, municipio)
        {
            Name = "ISSJoinville";
        }

        #endregion Constructors

        #region Methods

        protected override bool PrecisaValidarSchema(TipoUrl tipo) => false;

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
            xmlLote.Append($"<EnviarLoteRpsEnvio {GetNamespace()}>");
            xmlLote.Append($"<LoteRps Id=\"L{retornoWebservice.Lote}\" versao=\"1.00\">");
            xmlLote.Append($"<NumeroLote>{retornoWebservice.Lote}</NumeroLote>");

            xmlLote.Append("<Prestador>");
            xmlLote.Append("<CpfCnpj>");

            switch (Configuracoes.PrestadorPadrao.CpfCnpj.Length)
            {
                case 11:
                    {
                        xmlLote.Append($"<Cpf>{Configuracoes.PrestadorPadrao.CpfCnpj}</Cpf>");
                        break;
                    }
                case 14:
                    {
                        xmlLote.Append($"<Cnpj>{Configuracoes.PrestadorPadrao.CpfCnpj}</Cnpj>");
                        break;
                    }
            }

            xmlLote.Append("</CpfCnpj>");
            xmlLote.Append("</Prestador>");

            //xmlLote.Append($"<Cnpj>{Configuracoes.PrestadorPadrao.CpfCnpj.ZeroFill(14)}</Cnpj>");
            //xmlLote.Append($"<InscricaoMunicipal>{Configuracoes.PrestadorPadrao.InscricaoMunicipal}</InscricaoMunicipal>");


            xmlLote.Append($"<QuantidadeRps>{notas.Count}</QuantidadeRps>");
            xmlLote.Append("<ListaRps>");
            xmlLote.Append(xmlLoteRps);
            xmlLote.Append("</ListaRps>");
            xmlLote.Append("</LoteRps>");
            xmlLote.Append("</EnviarLoteRpsEnvio>");
            retornoWebservice.XmlEnvio = xmlLote.ToString();
        }

        protected override IServiceClient GetClient(TipoUrl tipo)
        {
            return new ISSJoinvilleServiceClient(this, tipo);
        }

        protected override string GetNamespace()
        {
            return "xmlns=\"http://nfemws.joinville.sc.gov.br\"";
        }

        protected override void TratarRetornoEnviar(RetornoEnviar retornoWebservice, NotaServicoCollection notas)
        {
            var xmlRet = XDocument.Parse(retornoWebservice.XmlRetorno.HtmlDecode());

            var rootElement = xmlRet.ElementAnyNs("EnviarLoteRpsEnvioResponse");
            MensagemErro(retornoWebservice, rootElement, "EnviarLoteRpsResposta");
            if (retornoWebservice.Erros.Count > 0) return;

            var protocoloElement = rootElement?.ElementAnyNs("EnviarLoteRpsResposta");

            retornoWebservice.Lote = protocoloElement?.ElementAnyNs("NumeroLote")?.GetValue<int>() ?? 0;
            retornoWebservice.Data = protocoloElement?.ElementAnyNs("DataRecebimento")?.GetValue<DateTime>() ?? DateTime.MinValue;
            retornoWebservice.Protocolo = protocoloElement?.ElementAnyNs("Protocolo")?.GetValue<string>() ?? string.Empty;
            retornoWebservice.Sucesso = retornoWebservice.Lote > 0;

            if (!retornoWebservice.Sucesso) return;

            foreach (var nota in notas)
            {
                nota.NumeroLote = retornoWebservice.Lote;
            }
        }

        protected override void TratarRetornoConsultarLoteRps(RetornoConsultarLoteRps retornoWebservice, NotaServicoCollection notas)
        {
            // Analisa mensagem de retorno
            var xmlRet = XDocument.Parse(retornoWebservice.XmlRetorno);

          
            var retornoLote = xmlRet.ElementAnyNs("ConsultarLoteRpsEnvioResponse").ElementAnyNs("ConsultarLoteRpsResposta");
            var situacao = retornoLote?.ElementAnyNs("Situacao");
            if (situacao != null)
            {
                switch (situacao.GetValue<int>())
                {
                    case 2:
                        retornoWebservice.Situacao = "2 – Não Processado";
                        break;

                    case 3:
                        retornoWebservice.Situacao = "3 – Processado com Erro";
                        break;

                    case 4:
                        retornoWebservice.Situacao = "4 – Processado com Sucesso";
                        break;

                    default:
                        retornoWebservice.Situacao = "1 – Não Recebido";
                        break;
                }
            }

            MensagemErro(retornoWebservice, xmlRet, "ConsultarLoteRpsResposta");
            if (retornoWebservice.Erros.Any()) return;

            retornoWebservice.Sucesso = true;

            if (notas == null) return;

            var listaNfse = retornoLote?.ElementAnyNs("ListaNfse");

            if (listaNfse == null)
            {
                retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "Lista de NFSe não encontrada! (ListaNfse)" });
                return;
            }

            var notasFiscais = new List<NotaServico>();

            foreach (var compNfse in listaNfse.ElementsAnyNs("CompNfse"))
            {
                var nfse = compNfse.ElementAnyNs("Nfse").ElementAnyNs("InfNfse");
                var numeroNFSe = nfse.ElementAnyNs("Numero")?.GetValue<string>() ?? string.Empty;
                var chaveNFSe = nfse.ElementAnyNs("CodigoVerificacao")?.GetValue<string>() ?? string.Empty;
                var dataNFSe = nfse.ElementAnyNs("DataEmissao")?.GetValue<DateTime>() ?? DateTime.Now;
                var numeroRps = nfse.ElementAnyNs("DeclaracaoPrestacaoServico")?
                    .ElementAnyNs("InfDeclaracaoPrestacaoServico")?
                    .ElementAnyNs("Rps")?
                    .ElementAnyNs("IdentificacaoRps")?
                    .ElementAnyNs("Numero").GetValue<string>() ?? string.Empty;

                GravarNFSeEmDisco(compNfse.AsString(true), $"NFSe-{numeroNFSe}-{chaveNFSe}-.xml", dataNFSe);

                var nota = notas.FirstOrDefault(x => x.IdentificacaoRps.Numero == numeroRps);
                if (nota == null)
                {
                    nota = notas.Load(compNfse.ToString());
                }
                else
                {
                    nota.IdentificacaoNFSe.Numero = numeroNFSe;
                    nota.IdentificacaoNFSe.Chave = chaveNFSe;
                    nota.IdentificacaoNFSe.DataEmissao = dataNFSe;
                    nota.XmlOriginal = compNfse.ToString();
                }

                nota.Protocolo = retornoWebservice.Protocolo;
                notasFiscais.Add(nota);
            }

            retornoWebservice.Notas = [.. notasFiscais];
        }

        protected override void TratarRetornoCancelarNFSe(RetornoCancelar retornoWebservice, NotaServicoCollection notas)
        {
            // Analisa mensagem de retorno
            var xmlRet = XDocument.Parse(retornoWebservice.XmlRetorno);

            var xmlRoot = xmlRet.ElementAnyNs("CancelarNfseEnvioResponse");
            MensagemErro(retornoWebservice, xmlRoot, "CancelarNfseResposta");
            if (retornoWebservice.Erros.Any()) return;

            var confirmacaoCancelamento = xmlRoot.ElementAnyNs("CancelarNfseResposta")?.ElementAnyNs("RetCancelamento")?.ElementAnyNs("NfseCancelamento")?.ElementAnyNs("Confirmacao");
            if (confirmacaoCancelamento == null)
            {
                retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "Confirmação do cancelamento não encontrada!" });
                return;
            }

            // Se a nota fiscal cancelada existir na coleção de Notas Fiscais, atualiza seu status:
            var nota = notas.FirstOrDefault(x => x.IdentificacaoNFSe.Numero.Trim() == retornoWebservice.NumeroNFSe);
            if (nota == null) return;

            retornoWebservice.Data = confirmacaoCancelamento.ElementAnyNs("DataHora")?.GetValue<DateTime>() ?? DateTime.MinValue;
            retornoWebservice.Sucesso = retornoWebservice.Data != DateTime.MinValue;

            nota.Situacao = SituacaoNFSeRps.Cancelado;
            nota.Cancelamento.Pedido.CodigoCancelamento = retornoWebservice.CodigoCancelamento;
            nota.Cancelamento.DataHora = retornoWebservice.Data;
            nota.Cancelamento.MotivoCancelamento = retornoWebservice.Motivo;
            nota.Cancelamento.Signature = confirmacaoCancelamento.ElementAnyNs("Pedido").ElementAnyNs("Signature") != null ? DFeSignature.Load(confirmacaoCancelamento.ElementAnyNs("Pedido").ElementAnyNs("Signature")?.ToString()) : null;
        }

        #endregion Methods
    }
}