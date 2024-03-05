using _24HourSurvival;
using Engine_lib;
using Kronus_Neural.NEAT;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

public class SaveGame
{
    public static void Save(int gameseed, int popmax)
    {
        // ensure data folder exists
        if (!Directory.Exists(Environment.CurrentDirectory + "/sim_data"))
        {
            Directory.CreateDirectory(Environment.CurrentDirectory + "/sim_data");
        }

        // save the actual game parameters
        var save = new GameSaveModel()
        {
            GameSeed = gameseed,
            PopMax = popmax,
            Codons = SimpleSurvival.codons,
            spawnerX = (int)SimpleSurvival.spawner.position.X,
            spawnerY = (int)SimpleSurvival.spawner.position.Y,
            cameraX = (int)Camera.main_camera.position.X,
            cameraY = (int)Camera.main_camera.position.Y,
            TotalDays = CalendarSystem.TotalDays,
            Day = CalendarSystem.Day,
            Month = CalendarSystem.Month,
            Year = CalendarSystem.Year,
            Current_Hour = CalendarSystem.Current_Hour,
            Current_Minute = CalendarSystem.Current_Minute,
            Current_Second = CalendarSystem.Current_Second,
            Divider = CalendarSystem.Divider,
            Multiplyer = CalendarSystem.Multiplyer,
        };

        string json = JsonConvert.SerializeObject(save);

        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + "/sim_data/save_game_data.json"))
        {
            sw.Flush();
            sw.Write(json);
            sw.Close();
            sw.Dispose();
        }

        // save the creatures
        List<CreatureSaveModel> creatures = new List<CreatureSaveModel>();
        foreach (var item in SimpleSurvival.creatures)
        {
            creatures.Add(item.SaveModel());
        }

        json = JsonConvert.SerializeObject(creatures);
        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + "/sim_data/saved_creature_data.json"))
        {
            sw.Flush();
            sw.Write(json);
            sw.Close();
            sw.Dispose();
        }

        // save the neural networks for later use
        List<NeuralNetworkSaveModel> networks = new List<NeuralNetworkSaveModel>();
        foreach (var net in SimpleSurvival.survival_sim.nets)
        {
            networks.Add(net.Value.Save());
        }

        json = JsonConvert.SerializeObject(networks);
        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + "/sim_data/saved_network_date.json"))
        {
            sw.Flush();
            sw.Write(json);
            sw.Close();
            sw.Dispose();
        }

        // save the species data
        List<SpeciesSaveModel> species = new List<SpeciesSaveModel>();
        foreach (var _species in SimpleSurvival.survival_sim.species)
        {
            species.Add(_species.Value.Save());
        }

        json = JsonConvert.SerializeObject(species);
        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + "/sim_data/saved_species_date.json"))
        {
            sw.Flush();
            sw.Write(json);
            sw.Close();
            sw.Dispose();
        }
    }
}

public class CreatureSaveModel
{
    public string id { get; set; }
    public float thisX { get; set; }
    public float thisY { get; set; }
    public string objectName { get; set; }

    // last known food source
    public int lastFoodSourceX { get; set; }
    public int lastFoodSourceY { get; set; }

    public string genesequence { get; set; }

    public int CreatureType { get; set; }
    public int CreatureState { get; set; }

    // current score
    public float score = 0.0f;

    // creature attributes
    public float hunger = 100;
    public float health = 100;
    public float energy = 100;
    public int metabolism_base = 1;
    public int energy_use_base = 1;
    public int regen_rate_base = 2;

    public int actual_metablolism = 1;
    public int actual_energy_use = 1;
    public int actual_regen_rate = 2;

    public float smell_radius = 450;
    public float sight_radius = 250;

    public float base_attribute_value = 100;
    public float calories_from_food = 50;

    // movement parameters
    // last target
    public int targetX { get; set; }
    public int targetY { get; set; }

    public float speed = 60;
    public float stop_distance = 0.8f;
    public float move_radius = 650;
}

public class GameSaveModel
{
    // main paramters
    public int GameSeed { get; set; }
    public int PopMax { get; set; }
    public List<string> Codons { get; set; }
    public int spawnerX { get; set; }
    public int spawnerY { get; set; }

    // camera position
    public int cameraX { get; set; }
    public int cameraY { get; set; }

    // calendar values
    public int TotalDays = 0;
    public int Day = 1;
    public int Month = 1;
    public int Year = 1;

    public int Current_Hour = 0;
    public int Current_Minute = 0;
    public float Current_Second = 0;

    public float Multiplyer = 60;
    public float Divider = 10;
}
