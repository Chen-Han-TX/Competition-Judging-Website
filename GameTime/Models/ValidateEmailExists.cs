﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using GameTime.DAL;

namespace GameTime.Models
{
    public class ValidateJudgeEmailExists : ValidationAttribute
    {
        private JudgeDAL judgeContext = new JudgeDAL();
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string email = Convert.ToString(value);
            Judge judge = (Judge)validationContext.ObjectInstance;
            int judgeID = judge.JudgeID;

            if (judgeContext.isEmailExists(email, judgeID))
                // validation failed
                return new ValidationResult
                ("Email address already exists!");
            else
                // validation passed
                return ValidationResult.Success;
        }
    }
}
