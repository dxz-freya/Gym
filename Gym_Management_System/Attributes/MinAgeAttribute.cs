using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Attributes
{
    public class MinAgeAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly int _minAge;
        public MinAgeAttribute(int minAge)
        {
            _minAge = minAge;
        }

        public override bool IsValid(object? value)
        {
            if (value is not DateTime dob)
                return true; // 留给 Required 去处理

            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;

            return age >= _minAge;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-minage", ErrorMessage ??
                $"You must be at least {_minAge} years old.");
            MergeAttribute(context.Attributes, "data-val-minage-years", _minAge.ToString());
        }

        private void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (!attributes.ContainsKey(key))
                attributes.Add(key, value);
        }
    }
}
