using AnimalSerialization.Tests.Models;
using System;

namespace AnimalSerialization.Tests.Conversion
{
    public class FarmConversion
    {

        public FarmResponse ConvertFarm(string json)
        {
            FarmResponse response = new FarmResponse();

            Farm<string, string> animalStringString = null;
            Farm<Dog, string> animalDogString = null;
            Farm<string, Cat> animalStringCat = null;
            Farm<Dog, Cat> animalDogCat = null;
            try
            {
                animalStringString = DeserializeStringString(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                try
                {
                    animalDogString = DeserializeDogString(json);
                }
                catch
                {
                    try
                    {
                        animalStringCat = DeserializeStringCat(json);
                    }
                    catch
                    {
                        animalDogCat = DeserializeDogCat(json);
                    }
                    Console.WriteLine(ex.ToString());
                }
            }
            if (animalStringString != null)
            {
                response.AnimalNames.Add(animalStringString.Animal1);
                response.AnimalNames.Add(animalStringString.Animal2);
                response.AnimalLegCount = 0;
            }
            if (animalDogString != null)
            {
                response.AnimalNames.Add(animalDogString.Animal1.Name);
                response.AnimalLegCount += 4;
                response.AnimalNames.Add(animalDogString.Animal2);
            }
            if (animalStringCat != null)
            {
                response.AnimalNames.Add(animalStringCat.Animal1);
                response.AnimalNames.Add(animalStringCat.Animal2.Name);
                response.AnimalLegCount += 4;
            }
            if (animalDogCat != null)
            {
                response.AnimalNames.Add(animalDogCat.Animal1.Name);
                response.AnimalLegCount += 4;
                response.AnimalNames.Add(animalDogCat.Animal2.Name);
                response.AnimalLegCount += 4;
            }

            return response;
        }

        private Farm<string, string> DeserializeStringString(string json)
        {
            return (Farm<string, string>)FarmSerialization.Deserialize<Farm<string, string>>(json);
        }

        private Farm<Dog, string> DeserializeDogString(string json)
        {
            return FarmSerialization.Deserialize<Farm<Dog, string>>(json);
        }

        private Farm<string, Cat> DeserializeStringCat(string json)
        {
            return FarmSerialization.Deserialize<Farm<string, Cat>>(json);
        }

        private Farm<Dog, Cat> DeserializeDogCat(string json)
        {
            return FarmSerialization.Deserialize<Farm<Dog, Cat>>(json);
        }
    }
}
