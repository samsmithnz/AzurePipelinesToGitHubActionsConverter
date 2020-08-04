using AnimalSerialization.Tests.Models;
using System;

namespace AnimalSerialization.Tests.Conversion
{
    public class FarmConversionSam
    {

        public FarmResponse ConvertFarm(string json)
        {
            FarmResponse response = new FarmResponse();

            Farm<string, string> animalStringString = null;
            Farm<Dog, string> animalDogString = null;
            Farm<string, Barn> animalStringBarn = null;
            Farm<Dog, Barn> animalDogBarn = null;
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
                        animalStringBarn = DeserializeStringBarn(json);
                    }
                    catch
                    {
                        animalDogBarn = DeserializeDogBarn(json);
                    }
                    Console.WriteLine(ex.ToString());
                }
            }
            if (animalStringString != null)
            {
                response.Items.Add(animalStringString.FarmItem1);
                response.AnimalLegCount += 0;
                response.Items.Add(animalStringString.FarmItem2);
                response.BuildingCount += 0;
            }
            if (animalDogString != null)
            {
                response.Items.Add(animalDogString.FarmItem1.Name);
                response.AnimalLegCount += 4;

                response.Items.Add(animalDogString.FarmItem2);
                response.BuildingCount += 0;
            }
            if (animalStringBarn != null)
            {
                response.Items.Add(animalStringBarn.FarmItem1);
                response.AnimalLegCount += 0;

                response.Items.Add(animalStringBarn.FarmItem2.BarnType);
                response.BuildingCount += 1;
            }
            if (animalDogBarn != null)
            {
                response.Items.Add(animalDogBarn.FarmItem1.Name);
                response.AnimalLegCount += 4;

                response.Items.Add(animalDogBarn.FarmItem2.BarnType);
                response.BuildingCount += 1;
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

        private Farm<string, Barn> DeserializeStringBarn(string json)
        {
            return FarmSerialization.Deserialize<Farm<string, Barn>>(json);
        }

        private Farm<Dog, Barn> DeserializeDogBarn(string json)
        {
            return FarmSerialization.Deserialize<Farm<Dog, Barn>>(json);
        }
    }
}
