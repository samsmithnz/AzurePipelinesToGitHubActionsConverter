using AnimalSerialization.Tests.Models;
using System;

namespace AnimalSerialization.Tests.Conversion
{
    public class FarmConversionAlex
    {

        public FarmResponse ConvertFarm(string yaml)
        {
            FarmResponse response = new FarmResponse();

            bool success = false;
            if (!success)
            {
                var animalStringString = DeserializeStringString(yaml); 
                if (animalStringString != null)
                {
                    success = true;
                    response.Items.Add(animalStringString.FarmItem1);
                    response.AnimalLegCount += 0;
                    response.Items.Add(animalStringString.FarmItem2);
                    response.BuildingCount += 0;
                }
            }

            if (!success)
            {
                var animalDogString = DeserializeObjectString(yaml); 
                if (animalDogString != null)
                {
                    success = true;
                    response.Items.Add(animalDogString.FarmItem1.Name);
                    response.AnimalLegCount += 4;

                    response.Items.Add(animalDogString.FarmItem2);
                    response.BuildingCount += 0;
                }
            }

            if (!success)
            {
                var animalStringBarn = DeserializeStringObject(yaml); 
                if (animalStringBarn != null)
                {
                    success = true;
                    response.Items.Add(animalStringBarn.FarmItem1);
                    response.AnimalLegCount += 0;

                    response.Items.Add(animalStringBarn.FarmItem2.BarnType);
                    response.BuildingCount += 1;
                    response.BarnTools.AddRange(animalStringBarn.FarmItem2.Tools);
                }
            }

            if (!success)
            {
                var animalDogBarn = DeserializeObjectObject(yaml); 
                if (animalDogBarn != null)
                {
                    success = true;
                    response.Items.Add(animalDogBarn.FarmItem1.Name);
                    response.AnimalLegCount += 4;

                    response.Items.Add(animalDogBarn.FarmItem2.BarnType);
                    response.BuildingCount += 1;
                    response.BarnTools.AddRange(animalDogBarn.FarmItem2.Tools);
                }
            }

            if (!success)
            {
                response = null;
            }

            return response;
        }

        //Note that the exception capture is here, so that the serialization class can be kept completely generic between the two approaches
        private Farm<string, string> DeserializeStringString(string json)
        {
            try
            {
                return (Farm<string, string>)FarmSerialization.Deserialize<Farm<string, string>>(json);
            }
            catch
            {
                //Do nothing with this exception
                return null;
            }
        }

        private Farm<Dog, string> DeserializeObjectString(string json)
        {
            try
            {
                return FarmSerialization.Deserialize<Farm<Dog, string>>(json);
            }
            catch
            {
                //Do nothing with this exception
                return null;
            }
        }

        private Farm<string, Barn> DeserializeStringObject(string json)
        {
            try
            {
                return FarmSerialization.Deserialize<Farm<string, Barn>>(json);
            }
            catch
            {
                //Do nothing with this exception
                return null;
            }
        }

        private Farm<Dog, Barn> DeserializeObjectObject(string json)
        {
            try
            {
                return FarmSerialization.Deserialize<Farm<Dog, Barn>>(json);
            }
            catch
            {
                //Do nothing with this exception
                return null;
            }
        }
    }
}
