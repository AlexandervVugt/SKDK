using System;
using System.Collections.Generic;
using FluentValidation;
using Stexchange.Data.Helpers;
using Stexchange.Data.Models;

namespace Stexchange.Data.Validation
{
    public class PhFilterValidator : AbstractValidator<Filter>
    {
        private readonly List<string> valueList = new List<string>(){"ph_null", "ph_kalkrijk", "ph_zuur", "ph_neutraal" };
        public PhFilterValidator()
        {
            RuleFor(x => valueList.Contains(x.Value)).Must(x => x == true).WithMessage(StandardMessages.InvalidOptionUsed("bodem"));
        }
    }

    public class WaterFilterValidator : AbstractValidator<Filter>
    {
        private readonly List<string> valueList = new List<string>() { "water_wisselnat", "water_nat", "water_normaal", "water_droog" };
        public WaterFilterValidator()
        {
            RuleFor(x => valueList.Contains(x.Value)).Must(x => x == true).WithMessage(StandardMessages.InvalidOptionUsed("vocht"));
        }
    }

    public class NutrientsFilterValidator : AbstractValidator<Filter>
    {
        private readonly List<string> valueList = new List<string>() { "nutrients_null", "nutrients_veel", "nutrients_gemiddeld", "nutrients_weinig", "nutrients_nooit" };
        public NutrientsFilterValidator()
        {
            RuleFor(x => valueList.Contains(x.Value)).Must(x => x == true).WithMessage(StandardMessages.InvalidOptionUsed("voeding"));
        }
    }

    public class LightFilterValidator : AbstractValidator<Filter>
    {
        private readonly List<string> valueList = new List<string>() { "light_volle_zon", "light_halfschaduw", "light_schaduw" };
        public LightFilterValidator()
        {
            RuleFor(x => valueList.Contains(x.Value)).Must(x => x == true).WithMessage(StandardMessages.InvalidOptionUsed("licht"));
        }
    }

    public class IndigenousFilterValidator : AbstractValidator<Filter>
    {
        private readonly List<string> valueList = new List<string>() { "indigenous_null", "indigenous_inheems", "indigenous_niet_inheems" };
        public IndigenousFilterValidator()
        {
            RuleFor(x => valueList.Contains(x.Value)).Must(x => x == true).WithMessage(StandardMessages.InvalidOptionUsed("inheems"));
        }
    }

    public class WithPotFilterValidator : AbstractValidator<Filter>
    {
        private readonly List<string> valueList = new List<string>() { "with_pot_ja", "with_pot_nee" };
        public WithPotFilterValidator()
        {
            RuleFor(x => valueList.Contains(x.Value)).Must(x => x == true).WithMessage(StandardMessages.InvalidOptionUsed("met pot"));
        }
    }

    public class GiveAwayFilterValidator : AbstractValidator<Filter>
    {
        private readonly List<string> valueList = new List<string>() { "give_away_ja", "give_away_nee" };
        public GiveAwayFilterValidator()
        {
            RuleFor(x => valueList.Contains(x.Value)).Must(x => x == true).WithMessage(StandardMessages.InvalidOptionUsed("gratis weg te geven"));
        }
    }

    public class PlantTypeFilterValidator : AbstractValidator<Filter>
    {
        private readonly List<string> valueList = new List<string>() { "plant_type_plant", "plant_type_zaad", "plant_type_stek" };
        public PlantTypeFilterValidator()
        {
            RuleFor(x => valueList.Contains(x.Value)).Must(x => x == true).WithMessage(StandardMessages.InvalidOptionUsed("plant soort"));
        }
    }
}
