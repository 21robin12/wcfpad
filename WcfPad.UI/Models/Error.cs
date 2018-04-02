using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfPad.UI.Models
{
    public class Error
    {
        public string Message { get; set; }
        public bool IsError = true;

        public Error(Exception e)
        {
            Message = AddInnerExceptionMessages(e.Message, e);
        }

        public Error(string message)
        {
            Message = message;
        }

        private string AddInnerExceptionMessages(string message, Exception e)
        {
            if (e.InnerException != null)
            {
                if (!message.EndsWith(" "))
                {
                    message += " ";
                }

                message += $"Inner Exception: {e.InnerException.Message}";
                return AddInnerExceptionMessages(message, e.InnerException);
            }

            return message;
        }
    }
}
