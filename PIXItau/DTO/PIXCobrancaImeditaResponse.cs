using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentInterfaces.DTO
{
    public class PIXCobrancaImeditaResponse
    {
        public Calendario calendario { get; set; }
        public string status { get; set; }
        public string txid { get; set; }
        public int revisao { get; set; }
        public Loc loc { get; set; }
        public string location { get; set; }
        public Valor valor { get; set; }
        public string chave { get; set; }
        public string pixCopiaECola { get; set; }
    }

    public class Calendario
    {
        public DateTime criacao { get; set; }
        public string expiracao { get; set; }
    }

    public class Loc
    {
        public string id { get; set; }
        public string location { get; set; }
        public string tipoCob { get; set; }
        public DateTime criacao { get; set; }
    }

    public class Valor
    {
        public string original { get; set; }
        public int modalidadeAlteracao { get; set; }
    }
}
