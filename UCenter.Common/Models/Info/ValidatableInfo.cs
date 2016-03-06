﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCenter.Common.Models
{
    public abstract class ValidatableInfo
    {
        public ICollection<ValidationResult> Errors { get; private set; } = new List<ValidationResult>();

        public virtual bool Validate()
        {
            var context = new ValidationContext(this, serviceProvider: null, items: null);
            this.Errors.Clear();
            return Validator.TryValidateObject(this, context, this.Errors, validateAllProperties: true);
        }
    }
}
