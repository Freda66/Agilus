using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLX.Robot.Kuka.Controller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace KukaAgylus.Models
{
    /// <summary>
    /// Représente une action éxécutable par le robot
    /// </summary>
    public interface IRobotCommand
    {
        /// <summary>
        /// Id de l'action
        /// </summary>
        Guid Id { get; set; }
        /// <summary>
        /// Nom affichable de l'action
        /// </summary>
        string Name { get; set; }
    }

    /// <summary>
    /// Représente un position de passage du robot
    /// </summary>
    public class RobotPosition
    {
        /// <summary>
        /// Id du point
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// Coordonées de la position
        /// </summary>
        public CartesianPosition Position { get; set; }
        /// <summary>
        /// Constructeur de classe vide
        /// </summary>
        public RobotPosition() { }
        /// <summary>
        /// Construit une instance de RobotPosition à partir de la position actuelle du robot
        /// </summary>
        /// <param name="robot"></param>
        public RobotPosition(RobotController robot)
        {
            try
            {
                robot.StopRelativeMovement();
                Position = robot.GetCurrentPosition();
                robot.StartRelativeMovement();
            }
            catch (Exception ex)
            {
                Position = new CartesianPosition();
            }
        }
    }

    /// <summary>
    /// Représente une trajectoire du robot
    /// </summary>
    public class Movement : IRobotCommand
    {
        /// <summary>
        /// Liste de positions de la trajectoire
        /// </summary>
        public List<RobotPosition> Positions { get; set; }
        /// <summary>
        /// Nom de la trajectoire
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Id de la trajectoire
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// Obtient la liste de CartesianPosition du mouvement
        /// </summary>
        /// <returns>Liste de CartesianPosition</returns>
        public List<CartesianPosition> GetCartesianPositions()
        {
            var listPos = new List<CartesianPosition>();
            foreach (var robPos in Positions)
            {
                listPos.Add(robPos.Position);
            }
            return listPos;
        }
    }

    /// <summary>
    /// Représente une commande à envoyer à l'outil Gripper
    /// </summary>
    public class GripperAction : IRobotCommand
    {
        /// <summary>
        /// Actions possibles du gripper
        /// </summary>
        public enum Action { Open, Close }
        /// <summary>
        /// Id de l'action
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// Nom de l'action
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Action à effectuer
        /// </summary>
        public Action Command { get; set; }
    }

    /// <summary>
    /// Représente un processus d'éxécution du robot
    /// </summary>
    public class RobotProcess
    {
        /// <summary>
        /// Contient les commandes à envoyer au robot
        /// </summary>
        public List<IRobotCommand> Commands { get; set; }
        /// <summary>
        /// Représente le nom du processus (et le nom du fichier de sauvegarde)
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="processName">Nom du processus (utilisé pour charger/sauver le process en JSONFile)</param>
        public RobotProcess(string processName)
        {
            Name = processName;
            LoadProcess();
        }
        /// <summary>
        /// Charge le processus depuis un fichier JSON
        /// </summary>
        public void LoadProcess()
        {
            if (Directory.Exists(Environment.CurrentDirectory + @"\robot_process")
                && File.Exists(Environment.CurrentDirectory + @"\robot_process\" + this.Name + ".json"))
            {
                // Si le process existe on le charge
                string json = File.ReadAllText(Environment.CurrentDirectory + @"\robot_process\" + this.Name + ".json");

                // Paramètre de serialisation prennant en compte le "$type" ajouté lors de la deserialisation
                // Indispensable pour la deserialisation des IRobotCommands
                var settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Objects;

                this.Commands = JsonConvert.DeserializeObject<List<IRobotCommand>>(json, settings);
            }
            else
            {
                // Sinon on créé un nouveau process et on le save
                Commands = new List<IRobotCommand>();
                SaveProcess();
            }
        }
        /// <summary>
        /// Save le processus dans un fichier JSON
        /// </summary>
        public void SaveProcess()
        {
            // Création du répertoire robot_process s'il n'existe pas
            if (!Directory.Exists(Environment.CurrentDirectory + @"\robot_process"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\robot_process");

            // Paramètre de serialisation qui ajoute "$type" dans le Json de sortie
            // Indispensable pour la deserialisation des IRobotCommands
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;

            // Sauvegarde du process
            File.WriteAllText(Environment.CurrentDirectory + @"\robot_process\" + this.Name + ".json", JsonConvert.SerializeObject(Commands, Formatting.None, settings), Encoding.UTF8);
        }
    }

    /// <summary>
    /// Permet la gestion et l'execution des RobotProcess
    /// </summary>
    public static class RobotProcessController
    {
        /// <summary>
        /// Teste l'existance d'un processus avec son nom
        /// </summary>
        /// <param name="inputProcessName">Nom du processus</param>
        /// <returns>Vrai si le processus existe</returns>
        public static bool IsExistingProcess(string inputProcessName)
        {
            if (!string.IsNullOrEmpty(inputProcessName))
            {
                foreach (var procName in GetProcessNameList())
                {
                    if (inputProcessName == procName)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Obtient la liste des process enregistrés
        /// </summary>
        /// <returns>Liste des RobotProcess</returns>
        public static List<RobotProcess> GetProcessList()
        {
            var processList = new List<RobotProcess>();
            foreach (var processName in GetProcessNameList())
            {
                processList.Add(new RobotProcess(processName));
            }
            return processList;
        }

        /// <summary>
        /// Obtient la liste des noms de processus enregistré
        /// </summary>
        /// <returns>Liste des noms des RobotProcess</returns>
        public static List<string> GetProcessNameList()
        {
            string processFolder = Environment.CurrentDirectory + @"\robot_process";
            var processNameList = new List<string>();
            if (Directory.Exists(processFolder))
            {
                foreach (string path in Directory.GetFiles(processFolder).ToList<string>())
                {
                    var processName = path.Split('\\').Last().Replace(".json", "");
                    if (processName != string.Empty)
                        processNameList.Add(processName);
                }
            }
            return processNameList;
        }

        /// <summary>
        /// Permet d'inverser l'ordre d'execution de deux commandes
        /// </summary>
        /// <param name="processName">Nom du processus</param>
        /// <param name="guidCommandA">Id de la commande A</param>
        /// <param name="guidCommandB">Id de la commande B</param>
        /// <returns>Indicateur de succès</returns>
        public static bool SwitchCommand(string processName, Guid guidCommandA, Guid guidCommandB)
        {
            try
            {
                var process = new RobotProcess(processName);

                var commandA = (from cmd in process.Commands
                                where cmd.Id.Equals(guidCommandA)
                                select cmd).First();

                var commandB = (from cmd in process.Commands
                                where cmd.Id.Equals(guidCommandB)
                                select cmd).First();

                if (commandA == null || commandB == null) return false;

                var indexCommandA = process.Commands.IndexOf(commandA);
                var indexCommandB = process.Commands.IndexOf(commandB);

                if (!(indexCommandA >= 0 && indexCommandB >= 0)) return false;

                process.Commands[indexCommandA] = commandB;
                process.Commands[indexCommandB] = commandA;

                process.SaveProcess();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Permet l'ajout d'un mouvement dans un processus
        /// </summary>
        /// <param name="processName">Nom du processus</param>
        /// <param name="movementName">Nom du mouvement à créer</param>
        /// <returns>Indicateur de succès</returns>
        public static bool AddMovement(string processName, string movementName)
        {
            try
            {
                var process = new RobotProcess(processName);
                process.Commands.Add(new Movement() { Name = movementName, Positions = new List<RobotPosition>() });
                process.SaveProcess();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Permet l'ajout de la position courante du robot dans un mouvement
        /// </summary>
        /// <param name="processName">Nom du processus</param>
        /// <param name="robot">Controller du robot</param>
        /// <returns>Indicateur de succès</returns>
        public static bool AddCurrentPosition(string processName, Guid idMovement, RobotController robot)
        {
            try
            {
                var process = new RobotProcess(processName);

                var mvt = (from cmd in process.Commands
                           where cmd.Id.Equals(idMovement)
                           select cmd).First() as Movement;

                mvt.Positions.Add(new RobotPosition(robot));
                process.SaveProcess();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Ajoute une action du gripper
        /// </summary>
        /// <param name="processName">Nom du processus</param>
        /// <param name="action">Action de gripper</param>
        /// <param name="robot">Controller du robot</param>
        /// <returns>Indicateur de succès</returns>
        public static bool AddGripperAction(string processName, GripperAction.Action action, RobotController robot)
        {
            var process = new RobotProcess(processName);
            process.Commands.Add(new GripperAction()
            {
                Name = action == GripperAction.Action.Open ? "Open gripper" : "Close gripper",
                Command = action
            });
            process.SaveProcess();
            try
            {
                robot.StopRelativeMovement();
                if (action == GripperAction.Action.Open)
                    robot.OpenGripper();
                else
                    robot.CloseGripper();
                robot.StartRelativeMovement();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Supprime une commande d'un processus
        /// </summary>
        /// <param name="processName">Nom du processus</param>
        /// <param name="idCommand">Guid de la commande à supprimer</param>
        /// <returns>Indicateur de succès</returns>
        public static bool DeleteCommand(string processName, Guid idCommand)
        {
            try
            {
                var process = new RobotProcess(processName);
                var commands = (from cmd in process.Commands
                                where cmd.Id.Equals(idCommand)
                                select cmd);

                if (commands.Count() > 0)
                {
                    process.Commands.Remove(commands.First());
                    process.SaveProcess();
                    return true;
                }
                else
                {
                    foreach (var command in process.Commands)
                    {
                        if (command is Movement)
                        {
                            var mvt = command as Movement;

                            var positions = (from pos in mvt.Positions
                                             where pos.Id.Equals(idCommand)
                                             select pos);
                            if (positions.Count() > 0)
                            {
                                mvt.Positions.Remove(positions.First());
                                process.SaveProcess();
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Permet de démarrer l'éxécution d'un processus sur un robot
        /// </summary>
        /// <param name="robot">Robot éxécutant le processus</param>
        /// <param name="process">Processus à éxécuter</param>
        /// <returns>Indicateur de succès d'éxécution</returns>
        public static bool ExecuteProcess(RobotController robot, string processName, LogManager log = null)
        {
            var process = new RobotProcess(processName);
            try
            {
                //Arrêt des mouvements relatifs du robot
                robot.StopRelativeMovement();
                // Parcours et éxécution des commandes
                foreach (var command in process.Commands)
                {
                    if (command is Movement)
                    {
                        // Commande de mouvement
                        var mvt = command as Movement;
                        //Envoie de la liste de positions au robot
                        if (log != null) log.AddLog("Info", string.Format("Movement start : {0} waypoints" + mvt.Positions.Count()));
                        robot.PlayTrajectory(mvt.GetCartesianPositions());
                        if (log != null) log.AddLog("Info", "Movement end");
                    }
                    else if (command is GripperAction)
                    {
                        // Commande d'action du gripper
                        var action = command as GripperAction;
                        if (action.Command == GripperAction.Action.Open)
                        {
                            //Ouverture pince
                            robot.OpenGripper();
                            if (log != null) log.AddLog("Info", "Open gripper");
                        }
                        else if (action.Command == GripperAction.Action.Close)
                        {
                            //Fermeture pince
                            robot.CloseGripper();
                            if (log != null) log.AddLog("Info", "Close gripper");
                        }
                    }
                    else
                    {
                        //Commande inconnue
                        if (log != null) log.AddLog("Error", "In ExecuteProcess(): Unknow command");
                        return false;
                    }
                }

            }
            catch (Exception e)
            {
                if (log != null) log.AddLog("Error", "Exception throw in ExecuteProcess(): " + e.Data);
                return false;
            }
            return true;
        }

    }
}