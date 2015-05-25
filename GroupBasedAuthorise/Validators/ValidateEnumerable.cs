using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GroupBasedAuthorise.Validators
{
    public class ValidateEnumerableAttribute : ValidationAttribute
    {
        private readonly int _minElements;
        private readonly int _maxElements;

        public ValidateEnumerableAttribute(int minElements, int maxElements)
        {
            _minElements = minElements;
            _maxElements = maxElements;
        }

        public override bool IsValid(object value)
        {
            var list = value as IList;
            if (list != null)
            {
                return list.Count >= _minElements && list.Count <= _maxElements;
            }
            return false;
        }
    }
}