using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core;
using OpenAC.Net.NFSe.Configuracao;
using OpenAC.Net.NFSe.Nota;
using System.Linq;
using System;
using System.Text;
using System.Xml.Linq;
using OpenAC.Net.DFe.Core.Serializer;
using System.Xml;
using OpenAC.Net.DFe.Core.Document;

namespace OpenAC.Net.NFSe.Providers.Centi
{
    internal class ProviderCenti : ProviderABRASF200
    {
        public ProviderCenti(ConfigNFSe config, OpenMunicipioNFSe municipio) : base(config, municipio)
        {
            Name = "Centi";
        }

        protected override XElement WriteRpsRps(NotaServico nota)
        {
            var rps = new XElement("Rps", new XAttribute("Id", nota.IdentificacaoRps.Numero));

            rps.Add(WriteIdentificacaoRps(nota));

            rps.AddChild(AdicionarTag(TipoCampo.DatHor, "", "DataEmissao", 10, 10, Ocorrencia.Obrigatoria, nota.IdentificacaoRps.DataEmissao));
            rps.AddChild(AdicionarTag(TipoCampo.Int, "", "Status", 1, 1, Ocorrencia.Obrigatoria, (int)nota.Situacao + 1));

            rps.AddChild(WriteSubstituidoRps(nota));

            return rps;
        }

        protected override void PrepararEnviarSincrono(RetornoEnviar retornoWebservice, NotaServicoCollection notas)
        {
            if (notas.Count == 0) retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "RPS não informado." });
            if (notas.Count > 3) retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "Apenas 3 RPS podem ser enviados em modo Sincrono." });
            if (retornoWebservice.Erros.Count > 0) return;

            var xmlLoteRps = new StringBuilder();

            foreach (var nota in notas)
            {
                var xmlRps = WriteXmlRps(nota, false, false);
                xmlLoteRps.Append(xmlRps);
                GravarRpsEmDisco(xmlRps, $"Rps-{nota.IdentificacaoRps.DataEmissao:yyyyMMdd}-{nota.IdentificacaoRps.Numero}.xml", nota.IdentificacaoRps.DataEmissao);
            }

            var xmlLote = new StringBuilder();
            xmlLote.Append($"<GerarNfseEnvio {GetNamespace()}>");
            xmlLote.Append(xmlLoteRps);
            xmlLote.Append("</GerarNfseEnvio>");

            retornoWebservice.XmlEnvio = xmlLote.ToString();
        }

        protected override XElement WriteServicosRps(NotaServico nota)
        {
            var servico = new XElement("Servico");

            servico.Add(WriteValoresRps(nota));

            servico.AddChild(AdicionarTag(TipoCampo.Int, "", "IssRetido", 1, 1, Ocorrencia.Obrigatoria, nota.Servico.Valores.IssRetido == SituacaoTributaria.Retencao ? 1 : 2));

            if (nota.Servico.ResponsavelRetencao.HasValue)
                servico.AddChild(AdicionarTag(TipoCampo.Int, "", "ResponsavelRetencao", 1, 1, Ocorrencia.NaoObrigatoria, (int)nota.Servico.ResponsavelRetencao + 1));

            //servico.AddChild(AdicionarTag(TipoCampo.Str, "", "ItemListaServico", 1, 5, Ocorrencia.Obrigatoria, nota.Servico.ItemListaServico.Replace(".", "")));
            servico.AddChild(AdicionarTag(TipoCampo.Str, "", "ItemListaServico", 1, 5, Ocorrencia.Obrigatoria, nota.Servico.CodigoTributacaoMunicipio));
            servico.AddChild(AdicionarTag(TipoCampo.Str, "", "CodigoCnae", 1, 7, Ocorrencia.NaoObrigatoria, nota.Servico.CodigoCnae));
            //servico.AddChild(AdicionarTag(TipoCampo.Str, "", "CodigoTributacaoMunicipio", 1, 20, Ocorrencia.NaoObrigatoria, nota.Servico.CodigoTributacaoMunicipio));
            servico.AddChild(AdicionarTag(TipoCampo.Str, "", "Discriminacao", 1, 2000, Ocorrencia.Obrigatoria, nota.Servico.Discriminacao));
            servico.AddChild(AdicionarTag(TipoCampo.Str, "", "CodigoMunicipio", 1, 20, Ocorrencia.Obrigatoria, nota.Servico.CodigoMunicipio));
            servico.AddChild(AdicionarTag(TipoCampo.Int, "", "CodigoPais", 4, 4, Ocorrencia.MaiorQueZero, nota.Servico.CodigoPais));
            servico.AddChild(AdicionarTag(TipoCampo.Int, "", "ExigibilidadeISS", 1, 1, Ocorrencia.Obrigatoria, (int)nota.Servico.ExigibilidadeIss + 1));
            servico.AddChild(AdicionarTag(TipoCampo.Int, "", "MunicipioIncidencia", 7, 7, Ocorrencia.MaiorQueZero, nota.Servico.MunicipioIncidencia));
            servico.AddChild(AdicionarTag(TipoCampo.Str, "", "NumeroProcesso", 1, 30, Ocorrencia.NaoObrigatoria, nota.Servico.NumeroProcesso));

            return servico;
        }

        protected override XElement WriteValoresRps(NotaServico nota)
        {
            var valores = new XElement("Valores");

            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorServicos", 1, 15, Ocorrencia.Obrigatoria, nota.Servico.Valores.ValorServicos));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorDeducoes", 1, 15, Ocorrencia.MaiorQueZero, nota.Servico.Valores.ValorDeducoes));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorPis", 1, 15, Ocorrencia.MaiorQueZero, nota.Servico.Valores.ValorPis));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorCofins", 1, 15, Ocorrencia.MaiorQueZero, nota.Servico.Valores.ValorCofins));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorInss", 1, 15, Ocorrencia.MaiorQueZero, nota.Servico.Valores.ValorInss));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorIr", 1, 15, Ocorrencia.MaiorQueZero, nota.Servico.Valores.ValorIr));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorCsll", 1, 15, Ocorrencia.MaiorQueZero, nota.Servico.Valores.ValorCsll));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "OutrasRetencoes", 1, 15, Ocorrencia.MaiorQueZero, nota.Servico.Valores.OutrasRetencoes));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorIss", 1, 15, Ocorrencia.MaiorQueZero, nota.Servico.Valores.ValorIss));
            valores.AddChild(AdicionarTag(TipoCampo.De4, "", "Aliquota", 1, 6, Ocorrencia.Obrigatoria, nota.Servico.Valores.Aliquota));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "DescontoIncondicionado", 1, 15, Ocorrencia.MaiorQueZero, nota.Servico.Valores.DescontoIncondicionado));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "DescontoCondicionado", 1, 15, Ocorrencia.MaiorQueZero, nota.Servico.Valores.DescontoCondicionado));

            return valores;
        }

        protected override void PrepararCancelarNFSe(RetornoCancelar retornoWebservice)
        {
            var xmlCancelamento = new StringBuilder();
            xmlCancelamento.Append($"<CancelarNfseEnvio {GetNamespace()}>");
            xmlCancelamento.Append($"<Pedido>");
            xmlCancelamento.Append($"<InfPedidoCancelamento>");
            xmlCancelamento.Append($"<IdentificacaoNfse>");            
            xmlCancelamento.Append($"<Numero>{retornoWebservice.NumeroNFSe}</Numero>");
            xmlCancelamento.Append($"<CpfCnpj>");

            switch (Configuracoes.PrestadorPadrao.CpfCnpj.Length)
            {
                case 11:
                    {
                        xmlCancelamento.Append($"<Cpf>{Configuracoes.PrestadorPadrao.CpfCnpj}</Cpf>");
                        break;
                    }
                case 14:
                    {
                        xmlCancelamento.Append($"<Cnpj>{Configuracoes.PrestadorPadrao.CpfCnpj}</Cnpj>");
                        break;
                    }
            }

            xmlCancelamento.Append($"</CpfCnpj>");
            xmlCancelamento.Append($"<InscricaoMunicipal>{Configuracoes.PrestadorPadrao.InscricaoMunicipal}</InscricaoMunicipal>");
            xmlCancelamento.Append($"<CodigoMunicipio>{Configuracoes.PrestadorPadrao.Endereco.CodigoMunicipio}</CodigoMunicipio>");
            xmlCancelamento.Append($"<CodigoVerificacao>{retornoWebservice.CodigoVerificacao}</CodigoVerificacao>");

            if (!string.IsNullOrEmpty(retornoWebservice.Motivo))
            {
                var motivo = retornoWebservice.Motivo;
                if (motivo?.Length > 9)
                {
                    motivo = motivo.Substring(0, 9).Trim();
                }

                xmlCancelamento.Append($"<DescricaoCancelamento>{motivo}</DescricaoCancelamento>");
            }

            xmlCancelamento.Append($"<Id>{retornoWebservice.Protocolo}</Id>");
            xmlCancelamento.Append($"</IdentificacaoNfse>");
            xmlCancelamento.Append($"<CodigoCancelamento>{retornoWebservice.CodigoCancelamento}</CodigoCancelamento>");
            xmlCancelamento.Append($"</InfPedidoCancelamento>");
            xmlCancelamento.Append($"</Pedido>");
            xmlCancelamento.Append($"</CancelarNfseEnvio>");

            retornoWebservice.XmlEnvio = xmlCancelamento.ToString();
        }

        protected override void AssinarCancelarNFSe(RetornoCancelar retornoWebservice)
        {
            retornoWebservice.XmlEnvio = XmlSigning.AssinarXml(retornoWebservice.XmlEnvio, "Pedido", "", Certificado);
        }

        protected override void TratarRetornoCancelarNFSe(RetornoCancelar retornoWebservice, NotaServicoCollection notas)
        {
            // Analisa mensagem de retorno
            var xmlRet = XDocument.Parse(retornoWebservice.XmlRetorno);
            MensagemErro(retornoWebservice, xmlRet, "GerarNfseResposta");
            if (retornoWebservice.Erros.Any()) return;

            var confirmacaoCancelamento = xmlRet.ElementAnyNs("GerarNfseResposta")?.ElementAnyNs("ListaNfse")?.ElementAnyNs("CompNfse")?.ElementAnyNs("Nfse")?.ElementAnyNs("InfNfse")?.ElementAnyNs("DescricaoCancelamento");
            if (confirmacaoCancelamento is null || string.IsNullOrEmpty(confirmacaoCancelamento.Value))
            {
                retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "Confirmação do cancelamento não encontrada!" });
                return;
            }

            // Se a nota fiscal cancelada existir na coleção de Notas Fiscais, atualiza seu status:
            var nota = notas.FirstOrDefault(x => x.IdentificacaoNFSe.Numero.Trim() == retornoWebservice.NumeroNFSe);
            if (nota == null) return;

            retornoWebservice.Data = DateTime.Now;
            retornoWebservice.Sucesso = true;

            nota.Situacao = SituacaoNFSeRps.Cancelado;
            nota.Cancelamento.Pedido.CodigoCancelamento = retornoWebservice.CodigoCancelamento;
            nota.Cancelamento.DataHora = retornoWebservice.Data;
            nota.Cancelamento.MotivoCancelamento = retornoWebservice.Motivo;
            nota.Cancelamento.Signature = null;
        }

        protected override void AssinarEnviarSincrono(RetornoEnviar retornoWebservice)
        {
            //retornoWebservice.XmlEnvio = XmlSigning.AssinarXmlTodos(retornoWebservice.XmlEnvio, "Rps", "InfRps", "id", Certificado, false, false, true, SignDigest.SHA1);
            retornoWebservice.XmlEnvio = XmlSigning.AssinarXml(retornoWebservice.XmlEnvio, "Rps", "", Certificado);
        }

        protected override void TratarRetornoEnviarSincrono(RetornoEnviar retornoWebservice, NotaServicoCollection notas)
        {
            // Analisa mensagem de retorno
            var xmlRet = XDocument.Parse(retornoWebservice.XmlRetorno);
            MensagemErro(retornoWebservice, xmlRet, "GerarNfseResposta");
            if (retornoWebservice.Erros.Any()) return;

            var nfse = xmlRet.ElementAnyNs("GerarNfseResposta")?.ElementAnyNs("ListaNfse")?.ElementAnyNs("CompNfse")?.ElementAnyNs("Nfse")?.ElementAnyNs("InfNfse");
            var numeroNFSe = nfse.ElementAnyNs("Numero")?.GetValue<string>() ?? string.Empty;
            var chaveNFSe = nfse.ElementAnyNs("CodigoVerificacao")?.GetValue<string>().Replace("*", "") ?? string.Empty;
            var dataNFSe = nfse.ElementAnyNs("DataEmissao")?.GetValue<DateTime>() ?? DateTime.Now;
            var numeroRps = nfse.ElementAnyNs("IdentificacaoRps")?.GetValue<string>() ?? string.Empty;

            GravarNFSeEmDisco(nfse.AsString(true), $"NFSe-{numeroNFSe}-{chaveNFSe}-.xml", dataNFSe);

            var nota = notas.FirstOrDefault(x => x.IdentificacaoRps.Numero == numeroRps);
            if (nota != null)
            {
                nota.IdentificacaoNFSe.Numero = numeroNFSe;
                nota.IdentificacaoNFSe.Chave = chaveNFSe;
                nota.IdentificacaoNFSe.DataEmissao = dataNFSe;

                retornoWebservice.Protocolo = nfse.Attribute("Id").Value;
            }

            retornoWebservice.Sucesso = true;
        }

        protected override string GetNamespace()
        {
            return "xmlns=\"http://www.centi.com.br/files/nfse.xsd\"";
        }

        protected override IServiceClient GetClient(TipoUrl tipo)
        {
            return new CentiServiceClient(this, tipo);
        }
    }
}
