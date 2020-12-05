﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using Stexchange.Data.Models;

namespace Stexchange.Data.Validation
{
    public class PhFilterValidator : AbstractValidator<Filter>
    {
        private List<string> valueList = new List<string>(){"ph_null", "ph_kalkrijk", "ph_zuur", "ph_neutraal" };
        public PhFilterValidator()
        {
            RuleFor(x => valueList.Contains(x.Value));
        }
    }

    public class WaterFilterValidator : AbstractValidator<Filter>
    {
        private List<string> valueList = new List<string>() { "water_wisselnat", "water_nat", "water_normaal", "water_droog" };
        public WaterFilterValidator()
        {
            RuleFor(x => valueList.Contains(x.Value));
        }
    }

    public class NutrientsFilterValidator : AbstractValidator<Filter>
    {
        private List<string> valueList = new List<string>() { "nutrients_null", "nutrients_veel", "nutrients_gemiddeld", "nutrients_weinig", "nutrients_nooit" };
        public NutrientsFilterValidator()
        {
            RuleFor(x => valueList.Contains(x.Value));
        }
    }

    public class LightFilterValidator : AbstractValidator<Filter>
    {
        private List<string> valueList = new List<string>() { "light_vollezon", "light_halfschaduw", "light_schaduw" };
        public LightFilterValidator()
        {
            RuleFor(x => valueList.Contains(x.Value));
        }
    }

    public class IndigenousFilterValidator : AbstractValidator<Filter>
    {
        private List<string> valueList = new List<string>() { "indigenous_null", "indigenous_inheems", "indigenous_niet_inheems" };
        public IndigenousFilterValidator()
        {
            RuleFor(x => valueList.Contains(x.Value));
        }
    }

    public class WithPotFilterValidator : AbstractValidator<Filter>
    {
        private List<string> valueList = new List<string>() { "with_pot_ja", "with_pot_nee" };
        public WithPotFilterValidator()
        {
            RuleFor(x => valueList.Contains(x.Value));
        }
    }

    public class GiveAwayFilterValidator : AbstractValidator<Filter>
    {
        private List<string> valueList = new List<string>() { "give_away_ja", "give_away_nee" };
        public GiveAwayFilterValidator()
        {
            RuleFor(x => valueList.Contains(x.Value));
        }
    }

    public class PlantTypeFilterValidator : AbstractValidator<Filter>
    {
        private List<string> valueList = new List<string>() { "plant_type_plant", "plant_type_zaad", "plant_type_stek" };
        public PlantTypeFilterValidator()
        {
            RuleFor(x => valueList.Contains(x.Value));
        }
    }
}
