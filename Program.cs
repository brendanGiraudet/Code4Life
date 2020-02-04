using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Bring data on patient samples from the diagnosis machine to the laboratory with enough molecules to produce medicine!
 **/
class Player
{
    static int maxMolecule = 10;
    static int maxDataFiles = 3;
    static List<DataFile> diagDatafiles = new List<DataFile>();
    static void Main(string[] args)
    {
        string[] inputs;
        int projectCount = int.Parse(Console.ReadLine());
        List<ScientistProject> projectList = new List<ScientistProject>();
        for (int i = 0; i < projectCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var project = new ScientistProject();
            project.Molecules = new List<Molecule>()
            {
                new Molecule { Genre = "A", Quantity = int.Parse(inputs[0]) },
                new Molecule { Genre = "B", Quantity = int.Parse(inputs[1]) },
                new Molecule { Genre = "C", Quantity = int.Parse(inputs[2]) },
                new Molecule { Genre = "D", Quantity = int.Parse(inputs[3]) },
                new Molecule { Genre = "E", Quantity = int.Parse(inputs[4]) }
            };
            projectList.Add(project);
            project.Molecules.ForEach(m =>
                {
                    Console.Error.WriteLine(m.Genre + " : " + m.Quantity);
                }
                );
                Console.Error.WriteLine("-----");
        }
        projectList.OrderBy(p => p.Molecules.Count);

        // game loop
        while (true)
        {
            List<Robot> robots = new List<Robot>();

            for (int i = 0; i < 2; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var robot = new Robot
                {
                    Etat = int.Parse(inputs[1]),
                    Score = int.Parse(inputs[2])
                };
                robot.TargetModule = new Module { Name = inputs[0] };
                robot.Molecules = new List<Molecule>()
                {
                    new Molecule { Genre = "A", Quantity = int.Parse(inputs[3]) },
                    new Molecule { Genre = "B", Quantity = int.Parse(inputs[4]) },
                    new Molecule { Genre = "C", Quantity = int.Parse(inputs[5]) },
                    new Molecule { Genre = "D", Quantity = int.Parse(inputs[6]) },
                    new Molecule { Genre = "E", Quantity = int.Parse(inputs[7]) }
                };
                robot.Molecules.ForEach(m => robot.MoleculeQuantity += m.Quantity);
                robot.ExpertiseList = new List<Expertise>()
                {
                    new Expertise { Genre = "A", Value = int.Parse(inputs[8]) },
                    new Expertise { Genre = "B", Value = int.Parse(inputs[9]) },
                    new Expertise { Genre = "C", Value = int.Parse(inputs[10]) },
                    new Expertise { Genre = "D", Value = int.Parse(inputs[11]) },
                    new Expertise { Genre = "E", Value = int.Parse(inputs[12]) }
                };
                robot.ExpertiseList.ForEach(ex => robot.ExpertisesValue += ex.Value);
                robots.Add(robot);
            }

            inputs = Console.ReadLine().Split(' ');
            List<Molecule> availableMolecules = new List<Molecule>()
            {
                new Molecule { Genre = "A", Quantity = int.Parse(inputs[0]) },
                new Molecule { Genre = "B", Quantity = int.Parse(inputs[1]) },
                new Molecule { Genre = "C", Quantity = int.Parse(inputs[2]) },
                new Molecule { Genre = "D", Quantity = int.Parse(inputs[3]) },
                new Molecule { Genre = "E", Quantity = int.Parse(inputs[4]) }
            };

            List<DataFile> availableDataFiles = new List<DataFile>();
            int sampleCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < sampleCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                DataFile dataFile = new DataFile
                {
                    ID = int.Parse(inputs[0]),
                    CarriedBy = int.Parse(inputs[1]),
                    Rank = int.Parse(inputs[2]),
                    ExpertiseGain = inputs[3],
                    Health = int.Parse(inputs[4])
                };

                dataFile.Molecules = new List<Molecule>()
                {
                    new Molecule { Genre = "A", Quantity = int.Parse(inputs[5]) },
                    new Molecule { Genre = "B", Quantity = int.Parse(inputs[6]) },
                    new Molecule { Genre = "C", Quantity = int.Parse(inputs[7]) },
                    new Molecule { Genre = "D", Quantity = int.Parse(inputs[8]) },
                    new Molecule { Genre = "E", Quantity = int.Parse(inputs[9]) }
                };

                dataFile.Molecules.ForEach(m => dataFile.MoleculeQuantity += m.Quantity);

                availableDataFiles.Add(dataFile);
            }


            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Robot myRobot = robots.First();
            myRobot.DataFiles = availableDataFiles.Where(dt => dt.CarriedBy == 0).ToList();
            string action = string.Empty;
            // prevision des molecules
            int nbMolPrevu = 0;
            myRobot.DataFiles.ForEach(m => nbMolPrevu += m.Molecules.Count);
            Console.Error.WriteLine("eta : " + myRobot.Etat);
            if (myRobot.IsArrviedInModule())
            {
                switch (myRobot.TargetModule.Name)
                {
                    case "SAMPLES":
                        if (myRobot.DataFiles.Count < maxDataFiles)
                        {
                            int rank = 3;
                            if(projectCount != 0)
                            {
                                rank = 1;
                            }
                            action = "CONNECT " + rank;
                        }
                        break;

                    case "DIAGNOSIS":
                        var diagDataFile = myRobot.DataFiles.Find(df => !diagDatafiles.Any(dd => dd.ID == df.ID));
                        if (diagDataFile != null)
                        {
                            action = "CONNECT " + diagDataFile.ID;
                            diagDatafiles.Add(diagDataFile);
                        }
                        else
                        {
                            if(projectCount == 0)
                            {
                                var invalidDataFile = myRobot.DataFiles.Find(df => (df.Molecules.Find(m => m.Quantity - myRobot.ExpertiseList.Find(ex => ex.Genre == m.Genre).Value > 5) != null));
                                if (invalidDataFile != null)
                                {
                                    action = "CONNECT " + invalidDataFile.ID;
                                }
                            }
                            else
                            {
                                
                                var useLessDataFile = myRobot.DataFiles.Find(df => projectList.Find(p => p.Molecules.Find(m => m.Genre == df.ExpertiseGain).Quantity <= myRobot.ExpertiseList.Find(ex => ex.Genre == df.ExpertiseGain).Value) != null);
                                if(useLessDataFile != null)
                                {
                                    action = "CONNECT " + useLessDataFile.ID;
                                }
                            }
                        }
                        break;
                    case "MOLECULES":
                        myRobot.DataFiles.OrderBy(d => d.MoleculeQuantity);
                        var dataFile = myRobot.DataFiles.Find(df => !myRobot.IsMoleculesFull(df));
                        if (dataFile != null && myRobot.MoleculeQuantity < maxMolecule)
                        {
                            var molecule = dataFile.Molecules.Find(m => m.Quantity > myRobot.Molecules.Find(mm => mm.Genre == m.Genre).Quantity + myRobot.ExpertiseList.Find(ex => ex.Genre == m.Genre).Value && availableMolecules.Find(mm => m.Genre == mm.Genre).Quantity > 0);
                            if (molecule != null)
                            {
                                action = "CONNECT " + molecule.Genre;
                            }
                        }
                        break;
                    case "LABORATORY":
                        var dataFileMoleculeFull = myRobot.DataFiles.Find(df => myRobot.IsMoleculesFull(df));
                        if (dataFileMoleculeFull != null)
                        {
                            action = "CONNECT " + dataFileMoleculeFull.ID;
                        }
                        else if (myRobot.DataFiles.Count != 0)
                        {
                            action = "GOTO MOLECULES";
                        }
                        break;
                    default:
                        action = "GOTO SAMPLES";
                        break;
                }

                if (string.IsNullOrEmpty(action))
                {
                    action = "GOTO " + myRobot.MoveToNextModule();
                }

                Console.WriteLine(action);
            }
            else
            {
                Console.WriteLine("GOTO " + myRobot.TargetModule.Name);
            }
        }
    }
    static bool IsMoleculesFull(List<Molecule> moleculeList, DataFile df)
    {
        bool ret = true;
        moleculeList.ForEach(m =>
        {
            if (m.Quantity < df.Molecules.Find(mm => mm.Genre == m.Genre).Quantity)
            {
                ret = false;
            }
        });

        return ret;
    }
}

class ScientistProject
{
    public List<Molecule> Molecules { get; set; } = new List<Molecule>();
}

class Robot
{
    public Module TargetModule { get; set; } = new Module();
    public int Etat { get; set; }
    public int Score { get; set; }
    public List<Molecule> Molecules { get; set; } = new List<Molecule>();
    public List<Expertise> ExpertiseList { get; set; } = new List<Expertise>();
    public List<DataFile> DataFiles { get; set; } = new List<DataFile>();
    public int MoleculeQuantity { get; set; } = 0;
    public int ExpertisesValue { get; set; }

    public Robot()
    {
    }

    public bool IsArrviedInModule()
    {
        return (this.Etat == 0);
    }

    public bool IsMoleculesFull(DataFile df)
    {
        bool ret = true;
        Molecules.ForEach(m =>
        {
            if (m.Quantity + ExpertiseList.Find(ex => ex.Genre == m.Genre).Value < df.Molecules.Find(mm => mm.Genre == m.Genre).Quantity)
            {
                ret = false;
            }
        });

        return ret;
    }


    public string MoveToNextModule()
    {
        string module = this.TargetModule.Name;
        switch (module)
        {
            case "SAMPLES":
                module = "DIAGNOSIS";
                break;
            case "DIAGNOSIS":
                module = "MOLECULES";
                break;
            case "MOLECULES":
                module = "LABORATORY";
                break;
            case "LABORATORY":
                module = "SAMPLES";
                break;
            default:
                module = "SAMPLES";
                break;
        }

        this.TargetModule.Name = module;

        return this.TargetModule.Name;
    }

    public int getNbMoleculeNeededByGenre(string genre)
    {
        int nb = 0;
        this.DataFiles.ForEach(d => nb += d.Molecules.Where(m => genre == m.Genre).First().Quantity - ExpertiseList.Find(ex => ex.Genre == genre).Value);
        return nb;
    }
}

class Expertise
{
    public string Genre { get; set; }
    public int Value { get; set; }
}

class Module
{
    public string Name { get; set; }
}

class Molecule
{
    public string Genre { get; set; }
    public int Quantity { get; set; }
}

class DataFile
{
    public int ID { get; set; }
    public int CarriedBy { get; set; }
    public int Rank { get; set; }
    public string ExpertiseGain { get; set; }
    public int Health { get; set; }
    public List<Molecule> Molecules { get; set; } = new List<Molecule>();
    public bool Traite { get; set; } = false;
    public int MoleculeQuantity { get; set; } = 0;
}