using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Stupify.Data.CustomCommandBuiltIns.HarryPotterApiCommands
{
    class CharacterCommand : BuiltInCommand
    {
        public CharacterCommand(IServiceProvider provider) : base(provider)
        {
            Tag = "$hpCharacter";

            Func<bool, string> are = b => b ? "are" : "aren't";

            Execute = async args =>
            {
                var characters = await provider.GetService<HarryPotterApiClient>().CharacterAsync(args[0])
                    .ConfigureAwait(false);

                if (characters.Length != 1)
                {
                    return "This character couldn't be found";
                }

                var character = characters.Single();

                return $"```Name: {character.Name}" + Environment.NewLine +
                       $"House: {character.House}" + Environment.NewLine +
                       $"They are a {character.BloodStatus}" + Environment.NewLine +
                       $"They are a {character.Role}" + Environment.NewLine +
                       $"They went to {character.School}" + Environment.NewLine +
                       $"They are a {character.Species}" + Environment.NewLine +
                       $"They {are(character.DeathEater)} a Death Eater" + Environment.NewLine +
                       $"They {are(character.DumbledoresArmy)} part of Dumbledores Army" + Environment.NewLine +
                       $"They {are(character.MinistryOfMagic)} part of the Ministry Of Magic" + Environment.NewLine +
                       $"They {are(character.OrderOfThePhoenix)} part of the Order Of The Phoenix```";

            };
        }
    }
}
