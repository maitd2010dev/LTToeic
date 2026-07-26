using CoreLTToeic.Application.Common.Constants;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLTToeic.Application.Models.EditModels
{
    public class LoginEditModel
    {
        [Required(ErrorMessage = MessageConstants.FIELD_REQUIRED)]
        public string UserName { get; set; }

        [Required(ErrorMessage = MessageConstants.FIELD_REQUIRED)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
