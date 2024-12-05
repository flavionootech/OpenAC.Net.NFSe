using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core;
using OpenAC.Net.DFe.Core.Document;
using OpenAC.Net.DFe.Core.Serializer;
using OpenAC.Net.NFSe.Configuracao;
using OpenAC.Net.NFSe.Nota;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace OpenAC.Net.NFSe.Providers.Publica
{
    internal class ProviderPublica : ProviderABRASF204
    {
        #region Constructors

        public ProviderPublica(ConfigNFSe config, OpenMunicipioNFSe municipio) : base(config, municipio)
        {
            Name = "Publica";
        }

        #endregion Constructors

        #region Methods

        protected override bool PrecisaValidarSchema(TipoUrl tipo)
        {
            switch (tipo)
            {
                case TipoUrl.CancelarNFSe:
                    {

                        return false;
                    }
                default:
                    {
                        return true;
                    }
            }
        }

        protected override string GetSchema(TipoUrl tipo)
        {
            return "schema_nfse_v03.xsd";
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

            valores.AddChild(AdicionarTag(TipoCampo.Int, "", "IssRetido", 1, 1, Ocorrencia.Obrigatoria, nota.Servico.Valores.IssRetido == SituacaoTributaria.Retencao ? 1 : 2));

            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorIss", 1, 15, Ocorrencia.MaiorQueZero, nota.Servico.Valores.ValorIss));

            if (nota.Servico.Valores.IssRetido == SituacaoTributaria.Retencao && nota.Servico.Valores.ValorIssRetido > 0)
            {
                valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorIssRetido", 1, 15, Ocorrencia.MaiorQueZero, nota.Servico.Valores.ValorIssRetido));
            }

            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "OutrasRetencoes", 1, 15, Ocorrencia.MaiorQueZero, nota.Servico.Valores.OutrasRetencoes));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "Aliquota", 1, 5, Ocorrencia.MaiorQueZero, nota.Servico.Valores.Aliquota));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorLiquidoNfse", 1, 15, Ocorrencia.MaiorQueZero, nota.Servico.Valores.ValorLiquidoNfse));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "DescontoIncondicionado", 1, 15, Ocorrencia.MaiorQueZero, nota.Servico.Valores.DescontoIncondicionado));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "DescontoCondicionado", 1, 15, Ocorrencia.MaiorQueZero, nota.Servico.Valores.DescontoCondicionado));

            return valores;
        }

        protected override XElement WriteServicosRps(NotaServico nota)
        {
            var servico = new XElement("Servico");

            servico.Add(WriteValoresRps(nota));

            if (Municipio.Codigo != 3534708 || nota.Servico.ResponsavelRetencao != ResponsavelRetencao.Prestador)// ourinhos nao permite
            {
                if (nota.Servico.ResponsavelRetencao.HasValue)
                    servico.AddChild(AdicionarTag(TipoCampo.Int, "", "ResponsavelRetencao", 1, 1, Ocorrencia.NaoObrigatoria, (int)nota.Servico.ResponsavelRetencao + 1));
            }

            servico.AddChild(AdicionarTag(TipoCampo.Str, "", "ItemListaServico", 1, 5, Ocorrencia.Obrigatoria, nota.Servico.ItemListaServico));

            servico.AddChild(AdicionarTag(TipoCampo.Str, "", "Discriminacao", 1, 2000, Ocorrencia.Obrigatoria, nota.Servico.Discriminacao));

            servico.AddChild(AdicionarTag(TipoCampo.Str, "", "CodigoMunicipio", 1, 20, Ocorrencia.Obrigatoria, nota.Servico.CodigoMunicipio));
            servico.AddChild(AdicionarTag(TipoCampo.Int, "", "CodigoPais", 4, 4, Ocorrencia.MaiorQueZero, nota.Servico.CodigoPais));


            return servico;
        }

        protected override XElement WriteTomadorRps(NotaServico nota)
        {
            if (nota.Tomador.CpfCnpj.IsEmpty()) return null;

            var tomador = new XElement("Tomador");

            var idTomador = new XElement("IdentificacaoTomador");
            tomador.Add(idTomador);

            var cpfCnpjTomador = new XElement("CpfCnpj");
            idTomador.Add(cpfCnpjTomador);

            cpfCnpjTomador.AddChild(AdicionarTagCNPJCPF("", "Cpf", "Cnpj", nota.Tomador.CpfCnpj));

            idTomador.AddChild(AdicionarTag(TipoCampo.Str, "", "InscricaoMunicipal", 1, 150, Ocorrencia.NaoObrigatoria, nota.Tomador.InscricaoMunicipal));

            tomador.AddChild(AdicionarTag(TipoCampo.Str, "", "NifTomador", 1, 150, Ocorrencia.NaoObrigatoria, nota.Tomador.DocTomadorEstrangeiro));
            tomador.AddChild(AdicionarTag(TipoCampo.Str, "", "RazaoSocial", 1, 150, Ocorrencia.Obrigatoria, nota.Tomador.RazaoSocial));

            if (nota.Tomador.EnderecoExterior.CodigoPais > 0)
            {
                var enderecoExt = new XElement("EnderecoExterior");
                tomador.Add(enderecoExt);

                enderecoExt.AddChild(AdicionarTag(TipoCampo.Int, "", "CodigoPais", 8, 8, Ocorrencia.Obrigatoria, nota.Tomador.EnderecoExterior.CodigoPais));
                enderecoExt.AddChild(AdicionarTag(TipoCampo.Str, "", "EnderecoCompletoExterior", 8, 8, Ocorrencia.Obrigatoria, nota.Tomador.EnderecoExterior.EnderecoCompleto));
            }
            else
            {
                var endereco = new XElement("Endereco");
                tomador.Add(endereco);

                endereco.AddChild(AdicionarTag(TipoCampo.Str, "", "Endereco", 1, 125, Ocorrencia.Obrigatoria, nota.Tomador.Endereco.Logradouro));

                if (nota.Tomador.Endereco.Numero.IsNumeric())
                {
                    endereco.AddChild(AdicionarTag(TipoCampo.Str, "", "Numero", 1, 10, Ocorrencia.Obrigatoria, nota.Tomador.Endereco.Numero));
                }
                else
                {
                    endereco.AddChild(AdicionarTag(TipoCampo.Str, "", "Numero", 1, 10, Ocorrencia.Obrigatoria, "00"));
                }

                endereco.AddChild(AdicionarTag(TipoCampo.Str, "", "Complemento", 1, 60, Ocorrencia.NaoObrigatoria, nota.Tomador.Endereco.Complemento));
                endereco.AddChild(AdicionarTag(TipoCampo.Str, "", "Bairro", 1, 60, Ocorrencia.Obrigatoria, nota.Tomador.Endereco.Bairro));
                endereco.AddChild(AdicionarTag(TipoCampo.Int, "", "CodigoMunicipio", 7, 7, Ocorrencia.Obrigatoria, nota.Tomador.Endereco.CodigoMunicipio));
                endereco.AddChild(AdicionarTag(TipoCampo.Str, "", "Uf", 2, 2, Ocorrencia.Obrigatoria, nota.Tomador.Endereco.Uf));
                endereco.AddChild(AdicionarTag(TipoCampo.StrNumber, "", "Cep", 8, 8, Ocorrencia.Obrigatoria, nota.Tomador.Endereco.Cep));
            }

            if (nota.Tomador.DadosContato.Email.IsEmpty() && nota.Tomador.DadosContato.Telefone.IsEmpty()) return tomador;

            var contato = new XElement("Contato");
            tomador.Add(contato);

            contato.AddChild(AdicionarTag(TipoCampo.Str, "", "Telefone", 8, 8, Ocorrencia.NaoObrigatoria, nota.Tomador.DadosContato.Telefone));
            contato.AddChild(AdicionarTag(TipoCampo.Str, "", "Email", 8, 8, Ocorrencia.NaoObrigatoria, nota.Tomador.DadosContato.Email));

            return tomador;
        }

        protected override XElement WriteRps(NotaServico nota)
        {
            var rootRps = new XElement("Rps");

            var infServico = new XElement("InfRps", new XAttribute("id", $"R{nota.IdentificacaoRps.Numero.OnlyNumbers()}"));
            //var infServico = new XElement("InfRps");
            rootRps.Add(infServico);

            infServico.Add(WriteIdentificacaoRps(nota));

            infServico.AddChild(AdicionarTag(TipoCampo.DatHorTz, "", "DataEmissao", 10, 10, Ocorrencia.Obrigatoria, nota.IdentificacaoRps.DataEmissao));

            string regimeEspecialTributacao;
            string optanteSimplesNacional;
            if (nota.OptanteSimplesNacional == NFSeSimNao.Sim)
            {
                optanteSimplesNacional = "1";
            }
            else
            {
                optanteSimplesNacional = "2";
            }

            var naturezaOperacao = 0;
            switch (nota.NaturezaOperacao)
            {
                case 1:
                    {
                        naturezaOperacao = NaturezaOperacao.Publica.TributacaoNoMunicipio;
                        if (nota.OptanteSimplesNacional == NFSeSimNao.Sim)
                        {
                            naturezaOperacao = NaturezaOperacao.Publica.TributacaoNoMunicipioSimplesNacional;
                        }

                        break;
                    }
                case 2:
                    {
                        naturezaOperacao = NaturezaOperacao.Publica.TributacaoForaMunicipio;
                        if (nota.OptanteSimplesNacional == NFSeSimNao.Sim)
                        {
                            naturezaOperacao = NaturezaOperacao.Publica.TributacaoForaMunicipioSimplesNacional;
                        }

                        break;
                    }
            }

            infServico.AddChild(AdicionarTag(TipoCampo.Int, "", "NaturezaOperacao", 1, 1, Ocorrencia.Obrigatoria, naturezaOperacao));



            infServico.AddChild(AdicionarTag(TipoCampo.Int, "", "OptanteSimplesNacional", 1, 1, Ocorrencia.Obrigatoria, optanteSimplesNacional));
            infServico.AddChild(AdicionarTag(TipoCampo.Int, "", "IncentivadorCultural", 1, 1, Ocorrencia.Obrigatoria, nota.IncentivadorCultural == NFSeSimNao.Sim ? 1 : 2));
            infServico.AddChild(AdicionarTag(TipoCampo.Int, "", "Status", 1, 1, Ocorrencia.Obrigatoria, (int)nota.Situacao + 1));

            infServico.AddChild(WriteServicosRps(nota));
            infServico.AddChild(WritePrestadorRps(nota));
            infServico.AddChild(WriteTomadorRps(nota));

            return rootRps;
        }

        protected override XElement WritePrestadorRps(NotaServico nota)
        {
            if (nota.Prestador.CpfCnpj.IsEmpty() && nota.Prestador.InscricaoMunicipal.IsEmpty()) return null;

            var prestador = new XElement("Prestador");

            prestador.AddChild(AdicionarTagCNPJCPF("", "CpfCnpj", "Cnpj", nota.Prestador.CpfCnpj));

            prestador.AddChild(AdicionarTag(TipoCampo.Str, "", "InscricaoMunicipal", 1, 15, Ocorrencia.NaoObrigatoria, nota.Prestador.InscricaoMunicipal));
            return prestador;
        }

        protected override void AssinarCancelarNFSe(RetornoCancelar retornoWebservice)
        {
            retornoWebservice.XmlEnvio = XmlSigning.AssinarXmlTodos(retornoWebservice.XmlEnvio, "Pedido", "InfPedidoCancelamento", "id", Certificado, false, false, true, SignDigest.SHA1);
        }

        protected override void AssinarEnviar(RetornoEnviar retornoWebservice)
        {
            retornoWebservice.XmlEnvio = XmlSigning.AssinarXmlTodos(retornoWebservice.XmlEnvio, "Rps", "InfRps", "id", Certificado, false, false, true, SignDigest.SHA1);
        }

        protected override void AssinarConsultarLoteRps(RetornoConsultarLoteRps retornoWebservice)
        {
        }

        protected override void PrepararConsultarLoteRps(RetornoConsultarLoteRps retornoWebservice)
        {
            var loteBuilder = new StringBuilder();
            loteBuilder.Append($"<ConsultarLoteRpsEnvio {GetNamespace()}>");
            loteBuilder.Append($"<Prestador id=\"L{retornoWebservice.Lote}\">");

            loteBuilder.Append(Configuracoes.PrestadorPadrao.CpfCnpj.IsCNPJ()
                ? $"<Cnpj>{Configuracoes.PrestadorPadrao.CpfCnpj.ZeroFill(14)}</Cnpj>"
                : $"<Cpf>{Configuracoes.PrestadorPadrao.CpfCnpj.ZeroFill(11)}</Cpf>");

            if (!Configuracoes.PrestadorPadrao.InscricaoMunicipal.IsEmpty()) loteBuilder.Append($"<InscricaoMunicipal>{Configuracoes.PrestadorPadrao.InscricaoMunicipal}</InscricaoMunicipal>");
            loteBuilder.Append("</Prestador>");
            loteBuilder.Append("</ConsultarLoteRpsEnvio>");


            retornoWebservice.XmlEnvio = XmlSigning.AssinarXmlTodos(loteBuilder.ToString(), "ConsultarLoteRpsEnvio", "Prestador", "id", Certificado, false, false, true, SignDigest.SHA1);

            // Insere o <Protocolo> após a <Signature>
            var xmlDoc = XDocument.Parse(retornoWebservice.XmlEnvio);

            var signatureNode = xmlDoc.Root.ElementAnyNs("Signature");
            if (signatureNode != null)
            {
                signatureNode.AddAfterSelf(new XElement("Protocolo", retornoWebservice.Protocolo));
            }

            // Atualiza o XML final no retorno
            retornoWebservice.XmlEnvio = xmlDoc.ToString(SaveOptions.DisableFormatting).Replace("xmlns=\"\"", "");
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
            xmlLote.Append($"<EnviarLoteRpsEnvio {GetNamespace()} xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.publica.inf.br schema_nfse_v03.xsd\">");
            xmlLote.Append($"<LoteRps id=\"L{retornoWebservice.Lote}\" versao=\"1.00\">");
            xmlLote.Append($"<NumeroLote>{retornoWebservice.Lote}</NumeroLote>");

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

            xmlLote.Append($"<InscricaoMunicipal>{Configuracoes.PrestadorPadrao.InscricaoMunicipal}</InscricaoMunicipal>");


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
            return new PublicaServiceClient(this, tipo);
        }

        protected override string GetNamespace()
        {
            return "xmlns=\"http://www.publica.inf.br\"";
        }

        protected override void TratarRetornoEnviar(RetornoEnviar retornoWebservice, NotaServicoCollection notas)
        {
            var xmlRet = XDocument.Parse(retornoWebservice.XmlRetorno.HtmlDecode());

            //var rootElement = xmlRet.ElementAnyNs("EnviarLoteRpsEnvioResponse");
            MensagemErro(retornoWebservice, xmlRet, "EnviarLoteRpsResposta");
            if (retornoWebservice.Erros.Count > 0) return;

            var protocoloElement = xmlRet?.ElementAnyNs("EnviarLoteRpsResposta");

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

        protected override void PrepararCancelarNFSe(RetornoCancelar retornoWebservice)
        {
            if (retornoWebservice.NumeroNFSe.IsEmpty() || retornoWebservice.CodigoCancelamento.IsEmpty())
            {
                retornoWebservice.Erros.Add(new Evento { Codigo = "AC0001", Descricao = "Número da NFSe/Codigo de cancelamento não informado para cancelamento." });
                return;
            }

            var loteBuilder = new StringBuilder();

            loteBuilder.Append($"<CancelarNfseEnvio {GetNamespace()}>");
            loteBuilder.Append("<Pedido>");
            loteBuilder.Append($"<InfPedidoCancelamento id=\"N{retornoWebservice.NumeroNFSe}\">");
            loteBuilder.Append("<IdentificacaoNfse>");
            loteBuilder.Append($"<Numero>{retornoWebservice.NumeroNFSe}</Numero>");
            loteBuilder.Append(Configuracoes.PrestadorPadrao.CpfCnpj.IsCNPJ()
                ? $"<Cnpj>{Configuracoes.PrestadorPadrao.CpfCnpj.ZeroFill(14)}</Cnpj>"
                : $"<Cpf>{Configuracoes.PrestadorPadrao.CpfCnpj.ZeroFill(11)}</Cpf>");
            if (!Configuracoes.PrestadorPadrao.InscricaoMunicipal.IsEmpty()) loteBuilder.Append($"<InscricaoMunicipal>{Configuracoes.PrestadorPadrao.InscricaoMunicipal}</InscricaoMunicipal>");
            loteBuilder.Append($"<CodigoMunicipio>{Configuracoes.PrestadorPadrao.Endereco.CodigoMunicipio}</CodigoMunicipio>");
            loteBuilder.Append($"<CodigoVerificacao>{retornoWebservice.CodigoVerificacao}</CodigoVerificacao>");
            loteBuilder.Append("</IdentificacaoNfse>");
            loteBuilder.Append($"<CodigoCancelamento>{retornoWebservice.CodigoCancelamento}</CodigoCancelamento>");

            if (!string.IsNullOrEmpty(retornoWebservice.Motivo))
            {
                string motivo = retornoWebservice.Motivo;
                string resultado = motivo.Length > 255 ? motivo.Substring(0, 255) : motivo;

                loteBuilder.Append($"<MotivoCancelamento>{resultado}</MotivoCancelamento>");
            }

            //loteBuilder.Append($"<CodigoVerificacao>{retornoWebservice.CodigoVerificacao}</CodigoVerificacao>");
            loteBuilder.Append("</InfPedidoCancelamento>");
            loteBuilder.Append("</Pedido>");
            loteBuilder.Append("</CancelarNfseEnvio>");
            retornoWebservice.XmlEnvio = loteBuilder.ToString();
        }

        protected override void TratarRetornoConsultarLoteRps(RetornoConsultarLoteRps retornoWebservice, NotaServicoCollection notas)
        {
            // Analisa mensagem de retorno
            var xmlRet = XDocument.Parse(retornoWebservice.XmlRetorno);


            var retornoLote = xmlRet.ElementAnyNs("ConsultarLoteRpsResposta");
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
                var numeroRps = nfse.ElementAnyNs("IdentificacaoRps")?.ElementAnyNs("Numero")?.GetValue<string>() ?? string.Empty;

                GravarNFSeEmDisco(compNfse.AsString(true), $"NFSe-{numeroNFSe}-{chaveNFSe}-.xml", dataNFSe);

                var nota = notas.FirstOrDefault(x => x.IdentificacaoRps.Numero == numeroRps);
                if (nota == null)
                {
                    nota = notas.Load(compNfse.ToString());
                }
                else
                {
                    nota.XmlOriginal = compNfse.ToString();
                }

                nota.IdentificacaoRps.Numero = numeroRps;

                nota.IdentificacaoNFSe.Numero = numeroNFSe;
                nota.IdentificacaoNFSe.Chave = chaveNFSe;
                nota.IdentificacaoNFSe.DataEmissao = dataNFSe;

                nota.Protocolo = retornoWebservice.Protocolo;
                notasFiscais.Add(nota);
            }

            retornoWebservice.Notas = [.. notasFiscais];
        }

        protected override void TratarRetornoCancelarNFSe(RetornoCancelar retornoWebservice, NotaServicoCollection notas)
        {
            // Analisa mensagem de retorno
            var xmlRet = XDocument.Parse(retornoWebservice.XmlRetorno);

            MensagemErro(retornoWebservice, xmlRet, "CancelarNfseResposta");
            if (retornoWebservice.Erros.Any()) return;

            var confirmacaoCancelamento = xmlRet.ElementAnyNs("CancelarNfseResposta")?.ElementAnyNs("Cancelamento")?.ElementAnyNs("Confirmacao");
            if (confirmacaoCancelamento == null)
            {
                retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "Confirmação do cancelamento não encontrada!" });
                return;
            }

            // Se a nota fiscal cancelada existir na coleção de Notas Fiscais, atualiza seu status:
            var nota = notas.FirstOrDefault(x => x.IdentificacaoNFSe.Numero.Trim() == retornoWebservice.NumeroNFSe);
            if (nota == null) return;

            retornoWebservice.Data = confirmacaoCancelamento.ElementAnyNs("DataHoraCancelamento")?.GetValue<DateTime>() ?? DateTime.MinValue;
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