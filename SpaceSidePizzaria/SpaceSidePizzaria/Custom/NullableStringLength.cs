using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SpaceSidePizzaria.Custom
{
    public class NullableStringLength : ValidationAttribute
    {
        public int MinLength { get; set; }
        public int MaxLength { get; set; }

        public NullableStringLength(int minLength, int maxLength)
        {
            MinLength = minLength;
            MaxLength = maxLength;
        }

        public override bool IsValid(object value)
        {
            return value == null || 
                (value.ToString().Length > MinLength && value.ToString().Length < MaxLength);
        }
    }
}