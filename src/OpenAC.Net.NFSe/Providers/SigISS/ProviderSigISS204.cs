using OpenAC.Net.Core.Extensions;
using OpenAC.Net.NFSe.Configuracao;
using OpenAC.Net.NFSe.Nota;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenAC.Net.NFSe.Providers.SigISS
{
    internal class ProviderSigISS204 : ProviderABRASF204
    {
        #region Constructors

        public ProviderSigISS204(ConfigNFSe config, OpenMunicipioNFSe municipio) : base(config, municipio)
        {
            Name = "SigISS";
        }

        #endregion Constructors

        #region Methods

        #region Protected Methods

        protected override IServiceClient GetClient(TipoUrl tipo)
        {
            return new SigISS204ServiceClient(this, tipo, Certificado);
        }

        protected override void LoadRps(NotaServico nota, XElement rpsRoot)
        {
            base.LoadRps(nota, rpsRoot);

            var rps = rpsRoot.ElementAnyNs("Rps");
            if (rps != null)
            {
                var situacao = rps.ElementAnyNs("Status")?.GetValue<string>();

                //SmarAPD, a situação para cancelamento é 2
                if (situacao == "1")
                    nota.Situacao = SituacaoNFSeRps.Normal;
                else
                    nota.Situacao = SituacaoNFSeRps.Cancelado;
            }
        }

        #endregion Protected Methods

        #endregion Methods
    }
}
