using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerManager.Domain.Entities
{
    public class Customer : Entity
    {
        public string CardHolderName { get; set; }
        public string Cpf { get; set; }
        public bool Status { get; set; }


        public Customer(string cardHolderName, string cpf, bool status)
        {
            CardHolderName = cardHolderName;
            Cpf = cpf;
        }

        public void Inativar() => Status = false;

        public void Atualizar(string cardHolderName, string cpf)
        {
            CardHolderName = cardHolderName;
            Cpf = cpf;
        }
    }
}
