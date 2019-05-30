using System;
using System.Collections.Generic;
using System.Text;

namespace DDPA.Commons.Results
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }

        public ValidationResult()
        {
            IsValid = false;
            Message = "";
        }
    }
}
