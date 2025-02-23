using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Dataset;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Enums;
using System.Linq;

namespace Content.Shared.Humanoid
{
    /// <summary>
    /// Figure out how to name a humanoid with these extensions.
    /// </summary>
    public sealed class NamingSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        public string GetName(string species, Gender? gender = null)
        {
            // if they have an old species or whatever just fall back to human I guess?
            // Some downstream is probably gonna have this eventually but then they can deal with fallbacks.
            if (!_prototypeManager.TryIndex(species, out SpeciesPrototype? speciesProto))
            {
                speciesProto = _prototypeManager.Index<SpeciesPrototype>("Human");
                Log.Warning($"Unable to find species {species} for name, falling back to Human");
            }

            switch (speciesProto.Naming)
            {
                case SpeciesNaming.First:
                    return Loc.GetString("namepreset-first",
                        ("first", GetFirstName(speciesProto, gender)));
                case SpeciesNaming.SkrellGenerator:
                    return GetSkrellName(speciesProto);
                case SpeciesNaming.TajaranGenerator:
                    return GetTajaranName(speciesProto, gender);
                // Start of Nyano - Summary: for Oni naming
                case SpeciesNaming.LastNoFirst:
                    return Loc.GetString("namepreset-lastnofirst",
                        ("first", GetFirstName(speciesProto, gender)), ("last", GetLastName(speciesProto)));
                // End of Nyano - Summary: for Oni naming
                case SpeciesNaming.TheFirstofLast:
                    return Loc.GetString("namepreset-thefirstoflast",
                        ("first", GetFirstName(speciesProto, gender)), ("last", GetLastName(speciesProto)));
                case SpeciesNaming.FirstDashFirst:
                    return Loc.GetString("namepreset-firstdashfirst",
                        ("first1", GetFirstName(speciesProto, gender)), ("first2", GetFirstName(speciesProto, gender)));
                case SpeciesNaming.LastFirst: // DeltaV: Rodentia name scheme
                    return Loc.GetString("namepreset-lastfirst",
                        ("last", GetLastName(speciesProto)), ("first", GetFirstName(speciesProto, gender)));
                case SpeciesNaming.FirstLast:
                default:
                    return Loc.GetString("namepreset-firstlast",
                        ("first", GetFirstName(speciesProto, gender)), ("last", GetLastName(speciesProto)));
            }
        }

        public string GetFirstName(SpeciesPrototype speciesProto, Gender? gender = null)
        {
            switch (gender)
            {
                case Gender.Male:
                    return _random.Pick(_prototypeManager.Index<DatasetPrototype>(speciesProto.MaleFirstNames).Values);
                case Gender.Female:
                    return _random.Pick(_prototypeManager.Index<DatasetPrototype>(speciesProto.FemaleFirstNames).Values);
                default:
                    if (_random.Prob(0.5f))
                        return _random.Pick(_prototypeManager.Index<DatasetPrototype>(speciesProto.MaleFirstNames).Values);
                    else
                        return _random.Pick(_prototypeManager.Index<DatasetPrototype>(speciesProto.FemaleFirstNames).Values);
            }
        }

        // Corvax-LastnameGender-Start: Added custom gender split logic
        public string GetLastName(SpeciesPrototype speciesProto, Gender? gender = null)
        {
            switch (gender)
            {
                case Gender.Male:
                    return _random.Pick(_prototypeManager.Index<DatasetPrototype>(speciesProto.MaleLastNames).Values);
                case Gender.Female:
                    return _random.Pick(_prototypeManager.Index<DatasetPrototype>(speciesProto.FemaleLastNames).Values);
                default:
                    if (_random.Prob(0.5f))
                        return _random.Pick(_prototypeManager.Index<DatasetPrototype>(speciesProto.MaleLastNames).Values);
                    else
                        return _random.Pick(_prototypeManager.Index<DatasetPrototype>(speciesProto.FemaleLastNames).Values);
            }
        }
        // Corvax-LastnameGender-End

    /// <summary>
    ///
    /// #TODOLIST add all species name generators
    /// </summary>
        public string GetTajaranName(SpeciesPrototype speciesProto, Gender? gender = null)
        {
            List<char> tajaranFemaleEndings = new List<char> { 'и', 'а', 'о', 'е', 'й', 'ь' };
            List<string> ruNamesSyllables = new List<string> { "кан", "тай", "кир", "раи", "кии", "мир", "кра", "тэк", "нал", "вар", "хар", "марр", "ран", "дарр",
	            "мирк", "ири", "дин", "манг", "рик", "зар", "раз", "кель", "шера", "тар", "кей", "ар", "но", "маи", "зир", "кер", "нир", "ра",
	            "ми", "рир", "сей", "эка", "гир", "ари", "нэй", "нре", "ак", "таир", "эрай", "жин", "мра", "зур", "рин", "сар", "кин", "рид", "эра", "ри", "эна" };
            string apostrophe =  "'";
            string newName = "";
            string fullName = "";

            for (int i = 0; i<2; i++)
            {
                for (int x = _random.Next(1,3); x>0; x--)
                {
                    newName += _random.PickAndTake(ruNamesSyllables);
                }

                newName += apostrophe;
                apostrophe = "";
            }

            fullName= string.Concat(char.ToUpper(newName[0]).ToString(), newName.Remove(0, 1));

            if ((gender == Gender.Female) && !(tajaranFemaleEndings.Any(x => fullName.EndsWith(x))))
            {
                fullName += "a";
            }

            if (_random.Prob(0.75f))
            {
                fullName += " " +  _random.Pick(new List<string> {"Хадии","Кайтам","Жан-Хазан","Нъярир’Ахан"});
            }
            else if (_random.Prob(0.8f))
            {
                fullName += " " +  _random.Pick(new List<string> {"Энай-Сэндай","Наварр-Сэндай","Року-Сэндай","Шенуар-Сэндай"});
            }

            return fullName;
        }

        public string GetSkrellName(SpeciesPrototype speciesProto)
        {
            List<string> ruFirstVarBeg = new List<string>
            {
                "заоо", "зао", "зикс", "зо", "йуо", "кью", "кьюм", "кси", "ксу", "квум", "кву",
			    "кви", "квей", "квиш", "куу", "кюан", "киэн", "ку", "кил", "лиа", "люик", "луи",
			    "рио", "сейу", "тсой", "уль", "улур", "урр", "ур", "цу", "эль", "эо", "эу"
            };

            List<string> ruSecondVarBeg = new List<string>
            {
                "заоо", "зао", "зо", "йуо", "лиа", "луи", "рио", "сейу", "эо"
            };

            List<string> ruFirstVarEnd = new List<string>
            {
                "аг", "вум", "вул", "вол", "гли", "зи", "заоо", "зао", "зикс", "зуо", "зук", "зуво", "уоо",
			    "икс", "ил", "ис", "йук", "кву", "квум", "куум", "куо", "куа", "куак", "кул", "квол", "уо",
			    "кью", "кьюа", "кэ", "кин", "кии", "кс", "ки", "киу", "кос", "лоа", "лак", "лум", "лик", "су",
			    "лии", "ллак", "мзикс", "мвол", "ори", "ору", "орр", "ррум", "ру", "руум", "руа", "рл",
			    "сэк", "сиа", "тейе", "тейку", "тсу", "туа", "туи", "ту", "тал", "уат", "уок", "урр", "уик",
			    "уии", "уэк", "эйкс", "эль", "эрр", "эй", "эйс", "о", "у", "а", "з", "э", "м" ,"к", "с", "р"
            };

            List<string> ruSecondVarEnd = new List<string>
            {
                "вум", "вул", "вол", "гли", "зи", "заоо", "зао", "зикс", "зуо", "зук", "зуво",
			    "йук", "кву", "квум", "куум", "куо", "куа", "куак", "кул", "квол", "кью", "кьюа",
			    "кэ", "кин", "кии", "кс", "ки", "киу", "кос", "лоа", "лак", "лум", "лик", "лии", "ллак",
			    "мзикс", "мвол", "ррум", "ру", "руум", "руа", "рл", "сэк", "су", "сиа", "тейе", "тейку",
			    "тсу", "туа", "туи", "ту", "тал", "з", "м", "к", "с", "р"
            };
            string newName = "";
            for (int i = 0; i<2; i++)
            {
                int variant = _random.Next(1,2);
                int leng = _random.Next(1,2);
                if (variant == 1)
                {
                    newName += _random.Pick(ruFirstVarBeg);
                }
                else
                {
                    newName += _random.Pick(ruSecondVarBeg);
                }
                for (int j = 0; j < leng; j++)
                {
                    if (_random.Prob(0.5f))
                    {
                        newName += "'";
                    }
                    if (variant == 1)
                    {
                        newName += _random.Pick(ruFirstVarEnd);
                    }
                    else
                    {
                        newName += _random.Pick(ruSecondVarEnd);
                    }
                }
                if (i == 0)
                {
                    newName += " ";
                }
            }
            return newName;
        }
    }
}
